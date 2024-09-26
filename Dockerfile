FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

COPY ./SampleApp.sln /app/SampleApp.sln
COPY ./fauna-dotnet/Fauna/Fauna.csproj /app/fauna-dotnet/Fauna/Fauna.csproj
COPY ./sample-app/dotnet-sample-app.csproj /app/sample-app/dotnet-sample-app.csproj

WORKDIR /app
RUN dotnet restore

COPY . ./

WORKDIR /app/sample-app
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "dotnet-sample-app.dll"]
