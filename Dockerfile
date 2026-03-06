FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/EfMicroservice.Api/EfMicroservice.Api.csproj", "src/EfMicroservice.Api/"]
COPY ["src/EfMicroservice.Application/EfMicroservice.Application.csproj", "src/EfMicroservice.Application/"]
COPY ["src/EfMicroservice.Common/EfMicroservice.Common.csproj", "src/EfMicroservice.Common/"]
COPY ["src/EfMicroservice.Domain/EfMicroservice.Domain.csproj", "src/EfMicroservice.Domain/"]
COPY ["src/EfMicroservice.ExternalData/EfMicroservice.ExternalData.csproj", "src/EfMicroservice.ExternalData/"]
COPY ["src/EfMicroservice.Persistence/EfMicroservice.Persistence.csproj", "src/EfMicroservice.Persistence/"]

RUN dotnet restore "src/EfMicroservice.Api/EfMicroservice.Api.csproj"

COPY . .
WORKDIR "/src/src/EfMicroservice.Api"
RUN dotnet build "EfMicroservice.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EfMicroservice.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EfMicroservice.Api.dll"]
