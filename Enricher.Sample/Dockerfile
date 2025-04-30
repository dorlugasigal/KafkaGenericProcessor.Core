FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["Enricher.Sample/Enricher.Sample.csproj", "Enricher.Sample/"]
COPY ["KafkaGenericProcessor.Core/KafkaGenericProcessor.Core.csproj", "KafkaGenericProcessor.Core/"]
RUN dotnet restore "Enricher.Sample/Enricher.Sample.csproj"

# Copy everything else and build
COPY Enricher.Sample/. Enricher.Sample/
COPY KafkaGenericProcessor.Core/. KafkaGenericProcessor.Core/
WORKDIR "/src/Enricher.Sample"
RUN dotnet build "Enricher.Sample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Enricher.Sample.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Enricher.Sample.dll"]