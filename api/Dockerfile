
# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.csproj .
RUN dotnet clean
RUN dotnet restore

# copy everything else and build app
COPY . .
RUN dotnet publish -c Release -o out

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "aspnetapp.dll"]