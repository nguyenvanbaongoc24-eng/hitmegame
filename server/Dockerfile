FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 10000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish server/HitMe.Server.csproj -c Release -o /out

FROM runtime
COPY --from=build /out .
ENTRYPOINT ["dotnet", "HitMe.Server.dll"]
