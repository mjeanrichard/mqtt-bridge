FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MqttBridge/MqttBridge.csproj", "MqttBridge/"]
RUN dotnet restore "MqttBridge/MqttBridge.csproj"
COPY . .
WORKDIR "/src/MqttBridge"
RUN dotnet build "MqttBridge.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MqttBridge.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MqttBridge.dll"]