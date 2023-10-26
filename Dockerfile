FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["BankingPoc/BankingPoc.csproj", "BankingPoc/"]
COPY ["BankingPoc.Services/BankingPoc.Services.csproj", "BankingPoc.Services/"]
COPY ["BankingPoc.Data/BankingPoc.Data.csproj", "BankingPoc.Data/"]
RUN dotnet restore "BankingPoc/BankingPoc.csproj"
COPY . .
WORKDIR "/src/BankingPoc"
RUN dotnet build "BankingPoc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BankingPoc.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BankingPoc.dll"]
