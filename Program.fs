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

// ---------------------------------
// Models
// ---------------------------------

type User =
    { Uid: String
      Name: String
      Email: String
      Count: int
      Updated: DateTime }

let makeUser (map: Map<string, string>) : User =
    { Uid = map.Item "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
      Name = map.Item "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
      Email = map.Item "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"
      Updated = DateTime.Now
      Count = 0 }

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
    let board = "/board"

module Database =
    let connectionString (ctx: HttpContext) =
        let config = ctx.GetService<IConfiguration>()
        config.["DB_CONN_STR"]

    // Sql.host config.["DB_HOST"]
    // |> Sql.database config.["DB_DATABASE"]
    // |> Sql.username config.["DB_USERNAME"]
    // |> Sql.password config.["DB_PASSWORD"]
    // |> Sql.sslMode SslMode.Require
    // |> Sql.port (int config.["DB_PORT"])
    // |> Sql.formatConnectionString

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
                  Count = read.int "count"
                  Updated = read.dateTime "updated" })
        |> List.tryHead

    let listUsers (connectionString: String) : User list =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM users"
        |> Sql.execute
            (fun read ->
                { Uid = read.string "uid"
                  Name = read.string "name"
                  Email = read.string "email"
                  Count = read.int "count"
                  Updated = read.dateTime "updated" })

    let upsertUser (connectionString: String) (user: User) =
        connectionString
        |> Sql.connect
        |> Sql.query
            "INSERT INTO users (uid, name, email, count, updated) VALUES (@uid, @name, @email, @count, @updated) ON CONFLICT (uid) DO UPDATE SET count = @count, updated = @updated"
        |> Sql.parameters [ ("uid", Sql.string user.Uid)
                            ("name", Sql.string user.Name)
                            ("email", Sql.string user.Email)
                            ("count", Sql.int user.Count)
                            ("updated", Sql.timestamp user.Updated) ]
        |> Sql.executeNonQuery

    let incrementOrCreate (connectionString: String) (authUser: User) =
        let user = getUser connectionString authUser.Uid

        match user with
        | Some u ->
            if (DateTime.Now - u.Updated).Minutes > 5 then
                upsertUser
                    connectionString
                    { u with
                          Count = u.Count + 1
                          Updated = DateTime.Now }
            else
                0
        | None -> upsertUser connectionString { authUser with Count = 1 }


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
                link [ _rel "preconnect"
                       _href "https://fonts.gstatic.com" ]
                link [ _rel "stylesheet"
                       _href "https://fonts.googleapis.com/css2?family=New+Tegomin&display=swap" ]
            ]
            body [] content
        ]

    let partial () = h1 [] [ encodedText "Cheesed" ]

    let login =
        [ h1 [] [ str "Cheese" ]
          img [ _src "./cheese.png" ]
          a [ _href Urls.googleAuth ] [
              str "Cheese this fool"
          ]
          a [ _href Urls.board ] [
              str "Cheese Board"
          ] ]
        |> layout

    let index = login

    let user (user: User) =
        let times =
            if user.Count > 1 then
                $"{user.Count} times"
            else
                "once... keep it that way"

        [ h1 [] [
            str $"Get cheesed, {user.Name}"
          ]
          img [ _src "/cheese.png" ]
          h2 [] [
              str $"You've been cheesed {times}"
          ]
          a [ _href Urls.logout ] [
              str "The Cheese Board"
          ] ]
        |> layout

    let notFound =
        [ h1 [] [ str "Not Found" ]
          a [ _href Urls.index ] [ str "Home" ] ]
        |> layout

    let board (users: User list) =
        [ h1 [] [ str "The Cheese Board" ]
          img [ _src "/cheese.png" ]
          div [ _class "cheeseBoard" ] [
              yield!
                  users
                  |> List.map
                      (fun user ->
                          div [ _class "row" ] [
                              div [] [ str user.Name ]
                              div [] [ str (string user.Count) ]
                          ])
          ]
          a [ _href Urls.login ] [ str "Home" ] ]
        |> layout


// ---------------------------------
// Web app
// ---------------------------------

module Handlers =
    let index : HttpHandler = Views.login |> htmlView
    let login : HttpHandler = Views.login |> htmlView

    let user : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            let authUser =
                ctx.User.Claims
                |> Seq.map (fun c -> (c.Type, c.Value))
                |> Map.ofSeq
                |> makeUser

            let connStr = Database.connectionString ctx

            let allowedDomain =
                ctx.GetService<IConfiguration>().["ALLOWED_DOMAIN"]

            if authUser.Email.Contains(allowedDomain) then
                Database.incrementOrCreate connStr authUser
                |> ignore
            else
                ()

            (authUser |> Views.user |> htmlView) next ctx

    let board : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            let connStr = Database.connectionString ctx
            let users = Database.listUsers connStr

            (users |> Views.board |> htmlView) next ctx

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
                              route Urls.board >=> board
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
