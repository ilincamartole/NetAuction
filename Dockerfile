# 1. Etapa de Build: Folosim imaginea .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Instalăm Node.js (necesar pentru a compila proiectul React / netauction-react)
RUN apt-get update && \
    apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

# Copiem fișierul .csproj și restaurăm pachetele NuGet
COPY ["WebApplication1/WebApplication1.csproj", "WebApplication1/"]
RUN dotnet restore "WebApplication1/WebApplication1.csproj"

# Copiem restul soluției
COPY . .

# Mergem în folderul proiectului și facem Publish
# (Acest pas va compila atât C#-ul cât și codul React, dacă .csproj e configurat corect)
WORKDIR "/src/WebApplication1"
RUN dotnet publish "WebApplication1.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Etapa de Runtime: Folosim imaginea mult mai ușoară de ASP.NET
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Setăm portul pe care îl va folosi Render (Render expune automat porturile setate aici)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Aducem fișierele compilate din etapa de build
COPY --from=build /app/publish .

# Punctul de pornire al aplicației
ENTRYPOINT ["dotnet", "WebApplication1.dll"]