FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CompSci.csproj", "."]
RUN dotnet restore "CompSci.csproj"
COPY . .
RUN dotnet build "CompSci.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CompSci.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p wwwroot/uploads/assignments wwwroot/uploads/notes wwwroot/uploads/pastquestions
ENTRYPOINT ["dotnet", "CompSci.dll"]
