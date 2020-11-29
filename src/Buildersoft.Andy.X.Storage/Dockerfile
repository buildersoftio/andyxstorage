FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/Buildersoft.Andy.X.Storage/Buildersoft.Andy.X.Storage.csproj", "src/Buildersoft.Andy.X.Storage/"]
COPY ["src/Buildersoft.Andy.X.Storage.Logic/Buildersoft.Andy.X.Storage.Logic.csproj", "src/Buildersoft.Andy.X.Storage.Logic/"]
COPY ["src/Buildersoft.Andy.X.Storage.Data.Model/Buildersoft.Andy.X.Storage.Data.Model.csproj", "src/Buildersoft.Andy.X.Storage.Data.Model/"]
COPY ["src/Buildersoft.Andy.X.Storage.FileConfig/Buildersoft.Andy.X.Storage.FileConfig.csproj", "src/Buildersoft.Andy.X.Storage.FileConfig/"]
COPY ["src/Buildersoft.Andy.X.Storage.Utilities/Buildersoft.Andy.X.Storage.Utilities.csproj", "src/Buildersoft.Andy.X.Storage.Utilities/"]
RUN dotnet restore "src/Buildersoft.Andy.X.Storage/Buildersoft.Andy.X.Storage.csproj"
COPY . .
WORKDIR "/src/src/Buildersoft.Andy.X.Storage"
RUN dotnet build "Buildersoft.Andy.X.Storage.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Buildersoft.Andy.X.Storage.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Buildersoft.Andy.X.Storage.dll"]