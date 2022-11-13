#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AWSSDK.BuildSystem.MessageProcessor/AWSSDK.BuildSystem.MessageProcessor.csproj", "AWSSDK.BuildSystem.MessageProcessor/"]
COPY ["AWSSDK.BuildSystem.Common/AWSSDK.BuildSystem.Common.csproj", "AWSSDK.BuildSystem.Common/"]
RUN dotnet restore "AWSSDK.BuildSystem.MessageProcessor/AWSSDK.BuildSystem.MessageProcessor.csproj"
COPY . .
WORKDIR "/src/AWSSDK.BuildSystem.MessageProcessor"
RUN dotnet build "AWSSDK.BuildSystem.MessageProcessor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AWSSDK.BuildSystem.MessageProcessor.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AWSSDK.BuildSystem.MessageProcessor.dll"]