#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CustomAPITemplate/CustomAPITemplate.csproj", "CustomAPITemplate/"]
COPY ["CustomAPITemplate.Contract/CustomAPITemplate.Contract.csproj", "CustomAPITemplate.Contract/"]
COPY ["CustomAPITemplate.DB/CustomAPITemplate.DB.csproj", "CustomAPITemplate.DB/"]
COPY ["CustomAPITemplate.Core/CustomAPITemplate.Core.csproj", "CustomAPITemplate.Core/"]
RUN dotnet restore "CustomAPITemplate/CustomAPITemplate.csproj"
COPY . .
WORKDIR "/src/CustomAPITemplate"
RUN dotnet build "CustomAPITemplate.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CustomAPITemplate.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CustomAPITemplate.dll"]