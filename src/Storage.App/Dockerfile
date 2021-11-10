#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Storage.App/Storage.App.csproj", "src/Storage.App/"]
COPY ["src/Storage.Core/Storage.Core.csproj", "src/Storage.Core/"]
COPY ["src/Storage.Model/Storage.Model.csproj", "src/Storage.Model/"]
COPY ["src/Storage.IO/Storage.IO.csproj", "src/Storage.IO/"]
COPY ["src/Storage.Utility/Storage.Utility.csproj", "src/Storage.Utility/"]
RUN dotnet restore "src/Storage.App/Storage.App.csproj"
COPY . .
WORKDIR "/src/src/Storage.App"
RUN dotnet build "Storage.App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Storage.App.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Storage.App.dll"]