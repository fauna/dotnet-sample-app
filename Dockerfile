FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

COPY ./SampleApp.sln /app/SampleApp.sln
COPY ./sample-app/DotNetSampleApp.csproj /app/sample-app/DotNetSampleApp.csproj

WORKDIR /app
RUN dotnet restore

COPY . ./

WORKDIR /app/sample-app
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "DotNetSampleApp.dll"]
