module cheesed2.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Authentication
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Npgsql.FSharp
open Microsoft.Extensions.Configuration.EnvironmentVariables
open Microsoft.Extensions.Configuration
open System

// ---------------------------------
// Models
// ---------------------------------

type User =
    { Uid: String
      Name: String
      Email: String
      Count: int }

module AuthSchemes =
    let cookie = "Cookies"
    let google = "Google"

module Urls =
    let index = "/"
    let login = "/login"
    let googleAuth = "/google-auth"
    let user = "/user"
    let logout = "/logout"
    let missing = "/missing"

module Database =
    let connectionString (ctx: HttpContext) =
        let config = ctx.GetService<IConfiguration>()

        Sql.host config.["DB_HOST"]
        |> Sql.database config.["DB_DATABASE"]
        |> Sql.username config.["DB_USERNAME"]
        |> Sql.password config.["DB_PASSWORD"]
        |> Sql.port 5432
        |> Sql.formatConnectionString

    let getUser (connectionString: String) (uid: String) : User option =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM users WHERE uid = @uid"
        |> Sql.parameters [ "uid", Sql.string uid ]
        |> Sql.execute
            (fun read ->
                { Uid = read.string "uid"
                  Name = read.string "name"
                  Email = read.string "email"
                  Count = read.int "count" })
        |> List.tryHead

    let upsertUser (connectionString: String) (user: User) =
        connectionString
        |> Sql.connect
        |> Sql.query
            "INSERT INTO users (uid, name, email, count) VALUES (@uid, @name, @email, @count) ON CONFLICT (uid) DO UPDATE SET count = @count"
        |> Sql.parameters [ ("uid", Sql.string user.Uid)
                            ("name", Sql.string user.Name)
                            ("email", Sql.string user.Email)
                            ("count", Sql.int user.Count) ]
        |> Sql.executeNonQuery


// ---------------------------------
// Views
// ---------------------------------

module Views =
    open Giraffe.ViewEngine

    let layout (content: XmlNode list) =
        html [] [
            head [] [
                title [] [ encodedText "Cheesed" ]
                link [ _rel "stylesheet"
                       _type "text/css"
                       _href "/main.css" ]
            ]
            body [] content
        ]

    let partial () = h1 [] [ encodedText "cheesed2" ]

    let index =
        [ partial ()
          ul [] [
              li [] [
                  a [ _href Urls.login ] [ str "Login" ]
              ]
              li [] [
                  a [ _href Urls.user ] [ str "Profile" ]
              ]
          ] ]
        |> layout

    let login =
        [ h1 [] [ str "Login" ]
          a [ _href Urls.googleAuth ] [
              str "Google"
          ]
          a [ _href Urls.index ] [ str "Home" ] ]
        |> layout

    let user (claims: (string * string) seq) =
        [ h1 [] [ str "Details:" ]
          h2 [] [ str "claims:" ]
          ul [] [
              yield!
                  claims
                  |> Seq.map
                      (fun (key, value) ->
                          li [] [
                              sprintf "%s: %s" key value |> str
                          ])
          ]
          p [] [
              a [ _href Urls.logout ] [ str "Logout" ]
          ] ]
        |> layout

    let notFound =
        [ h1 [] [ str "Not Found" ]
          a [ _href Urls.index ] [ str "Home" ] ]
        |> layout


// ---------------------------------
// Web app
// ---------------------------------

module Handlers =
    let index : HttpHandler = Views.index |> htmlView
    let login : HttpHandler = Views.login |> htmlView

    let user : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            (ctx.User.Claims
             |> Seq.map (fun c -> (c.Type, c.Value))
             |> Views.user
             |> htmlView)
                next
                ctx

    let logout : HttpHandler =
        signOut AuthSchemes.cookie
        >=> redirectTo false Urls.index

    let challenge (scheme: string) (redirectUri: string) : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                do! ctx.ChallengeAsync(scheme, AuthenticationProperties(RedirectUri = redirectUri))
                return! next ctx
            }

    let googleAuth = challenge AuthSchemes.google Urls.user

    let authenticate : HttpHandler = requiresAuthentication login

    let notFound : HttpHandler =
        setStatusCode 404 >=> (Views.notFound |> htmlView)

    let webApp : HttpHandler =
        choose [ GET
                 >=> choose [ route Urls.index >=> index
                              route Urls.login >=> login
                              route Urls.user >=> authenticate >=> user
                              route Urls.logout >=> logout
                              route Urls.googleAuth >=> googleAuth ]
                 notFound ]

    let error (ex: Exception) (logger: ILogger) =
        logger.LogError(EventId(), ex, "Unhandled exception")

        clearResponse
        >=> setStatusCode 500
        >=> text ex.Message


// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder: CorsPolicyBuilder) =
    builder
        .WithOrigins("http://localhost:5000", "https://localhost:5001")
        .AllowAnyMethod()
        .AllowAnyHeader()
    |> ignore

let configureApp (app: IApplicationBuilder) =
    let env =
        app.ApplicationServices.GetService<IWebHostEnvironment>()

    app.UseAuthentication() |> ignore

    (match env.IsDevelopment() with
     | true -> app.UseDeveloperExceptionPage()
     | false ->
         app
             .UseGiraffeErrorHandler(Handlers.error)
             .UseHttpsRedirection())
        .UseCors(configureCors)
        .UseStaticFiles()
        .UseGiraffe(Handlers.webApp)

let configureServices (services: IServiceCollection) =
    services.AddCors() |> ignore

    services
        .AddAuthentication(fun o -> o.DefaultScheme <- AuthSchemes.cookie)
        .AddCookie(AuthSchemes.cookie,
                   fun o ->
                       o.LoginPath <- PathString Urls.login
                       o.LogoutPath <- PathString Urls.logout)
        .AddGoogle(AuthSchemes.google,
                   fun o ->
                       let id =
                           Environment.GetEnvironmentVariable "GOOGLE_CLIENT_ID"

                       let secret =
                           Environment.GetEnvironmentVariable "GOOGLE_SECRET"

                       o.ClientId <- id
                       o.ClientSecret <- secret)
    |> ignore

    services.AddGiraffe() |> ignore

let configureLogging (builder: ILoggingBuilder) =
    builder.AddConsole().AddDebug() |> ignore

let configureConfiguration (context: WebHostBuilderContext) (config: IConfigurationBuilder) =
    config.AddEnvironmentVariables() |> ignore

[<EntryPoint>]
let main args =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot = Path.Combine(contentRoot, "WebRoot")

    Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .UseContentRoot(contentRoot)
                .UseWebRoot(webRoot)
                .ConfigureAppConfiguration(configureConfiguration)
                .Configure(Action<IApplicationBuilder> configureApp)
                .ConfigureServices(configureServices)
                .ConfigureLogging(configureLogging)
            |> ignore)
        .Build()
        .Run()

    0
