# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["QuizSystem.Api/QuizSystem.Api.csproj", "QuizSystem.Api/"]
RUN dotnet restore "QuizSystem.Api/QuizSystem.Api.csproj"

COPY . .
WORKDIR "/src/QuizSystem.Api"
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:5014
EXPOSE 5014

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "QuizSystem.Api.dll"]