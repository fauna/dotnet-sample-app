FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

COPY ./SampleApp.sln /app/SampleApp.sln
COPY ./DotNetSampleApp/DotNetSampleApp.csproj /app/DotNetSampleApp/DotNetSampleApp.csproj
COPY ./DotNetSampleApp.Tests/DotNetSampleApp.Tests.csproj /app/DotNetSampleApp.Tests/DotNetSampleApp.Tests.csproj

WORKDIR /app
RUN dotnet restore

COPY . ./

WORKDIR /app/DotNetSampleApp
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "DotNetSampleApp.dll"]
