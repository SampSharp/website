FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/SampSharp.Documentation/*.csproj ./
RUN dotnet restore

# Node install
RUN curl -fsSL https://deb.nodesource.com/setup_17.x | bash -
RUN apt-get install -y nodejs

# Copy everything else and build
COPY src/SampSharp.Documentation ./
RUN rm -rf ./data ./wwwroot/dist
RUN dotnet publish -c Release -o out

# Node build
RUN npm install
RUN npm run build-prod && cp -r wwwroot/dist out/wwwroot/dist

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "SampSharp.Documentation.dll"]
