FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CerberusSSOApplication_API/ ./API/

WORKDIR /src/API/Presentation

RUN dotnet restore Presentation.csproj
RUN dotnet publish Presentation.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Presentation.dll"]