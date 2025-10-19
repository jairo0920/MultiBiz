# Build Server + Client (hosted)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY MultiBiz.sln ./
COPY MultiBiz.Shared/MultiBiz.Shared.csproj MultiBiz.Shared/
COPY MultiBiz.Server/MultiBiz.Server.csproj MultiBiz.Server/
COPY MultiBiz.Client/MultiBiz.Client.csproj MultiBiz.Client/
RUN dotnet restore
COPY . .
RUN dotnet publish MultiBiz.Server/MultiBiz.Server.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "MultiBiz.Server.dll"]
