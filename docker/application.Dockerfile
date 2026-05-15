FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY HireHub.sln ./
COPY src ./src
COPY ApiGateway ./ApiGateway
RUN dotnet publish src/Services/ApplicationService/ApplicationService.Api/ApplicationService.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ApplicationService.Api.dll"]
