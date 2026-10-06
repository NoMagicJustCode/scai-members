FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
RUN mkdir -p /app/uploads

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/ScaiMembers.Api/ScaiMembers.Api.csproj", "ScaiMembers.Api/"]
RUN dotnet restore "ScaiMembers.Api/ScaiMembers.Api.csproj"
COPY src/ScaiMembers.Api/ ScaiMembers.Api/
WORKDIR /src/ScaiMembers.Api
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "ScaiMembers.Api.dll"]
