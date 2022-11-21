FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AWSSDK.BuildSystem.MessageProcessor/AWSSDK.BuildSystem.MessageProcessor.csproj", "AWSSDK.BuildSystem.MessageProcessor/"]
COPY ["AWSSDK.BuildSystem.ConsoleClient/AWSSDK.BuildSystem.ConsoleClient.csproj", "AWSSDK.BuildSystem.ConsoleClient/"]
COPY ["AWSSDK.BuildSystem.Common/AWSSDK.BuildSystem.Common.csproj", "AWSSDK.BuildSystem.Common/"]
COPY ["AWSSDK.BuildSystem.MessageProcessor.Deployment/AWSSDK.BuildSystem.MessageProcessor.Deployment.csproj", "AWSSDK.BuildSystem.MessageProcessor.Deployment/"]
COPY ["AWSSDK.BuildSystem.LambdaClient/AWSSDK.BuildSystem.LambdaClient.csproj", "AWSSDK.BuildSystem.LambdaClient/"]
COPY ["AWSSDK.BuildSystem.LambdaFailedBuildNotifier/AWSSDK.BuildSystem.LambdaFailedBuildNotifier.csproj", "AWSSDK.BuildSystem.LambdaFailedBuildNotifier/"]
COPY ["AWSSDK.Orchestrator.EventBus/AWSSDK.Orchestrator.EventBus.csproj", "AWSSDK.Orchestrator.EventBus/"]
COPY ["AWSSDK.Orchestrator.EventBus.EventGenerator/AWSSDK.Orchestrator.EventBus.EventGenerator.csproj", "AWSSDK.Orchestrator.EventBus.EventGenerator/"]
RUN dotnet restore "AWSSDK.Orchestrator.EventBus.EventGenerator/AWSSDK.Orchestrator.EventBus.EventGenerator.csproj"
COPY . .
WORKDIR "/src/AWSSDK.Orchestrator.EventBus.EventGenerator"
RUN dotnet build "AWSSDK.Orchestrator.EventBus.EventGenerator.csproj" -c Release -o /app/build

FROM build AS publish
RUN apt-get update -yq \
    && apt-get install curl gnupg -yq \
    && curl -sL https://deb.nodesource.com/setup_14.x | bash \
    && apt-get install nodejs -yq
RUN dotnet publish "AWSSDK.Orchestrator.EventBus.EventGenerator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AWSSDK.Orchestrator.EventBus.EventGenerator.dll"]
