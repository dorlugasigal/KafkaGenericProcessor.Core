FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["KafkaFlowProcessor/KafkaFlowProcessor.csproj", "KafkaFlowProcessor/"]
RUN dotnet restore "KafkaFlowProcessor/KafkaFlowProcessor.csproj"
COPY . .
WORKDIR "/src/KafkaFlowProcessor"
RUN dotnet build "KafkaFlowProcessor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KafkaFlowProcessor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KafkaFlowProcessor.dll"]