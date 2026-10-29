FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["InitializeDB/InitializeDB.csproj", "InitializeDB/"]
RUN dotnet restore "InitializeDB/InitializeDB.csproj"
COPY . .
WORKDIR "/src/InitializeDB"
RUN dotnet build "InitializeDB.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InitializeDB.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
RUN apt-get update \
 && apt-get install -y --no-install-recommends \
      libgssapi-krb5-2 \
      libkrb5-3 \
      krb5-locales \
 && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InitializeDB.dll"]