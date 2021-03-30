FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

# copy fsproj and restore as distinct layers
COPY *.fsproj .
RUN dotnet restore

# copy everything else and build app
COPY . .
WORKDIR /app
RUN dotnet build -c Release


FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app
COPY --from=build /app/ ./
ENTRYPOINT ["dotnet", "naja-haje.dll", "--urls", "http://*:5000;http://*:5001"]
    MAINTAINER Saxon Jensen <saxon.jensen@gmail.com>

