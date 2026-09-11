FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/JCA.WorkSpace.Service.API/JCA.WorkSpace.Service.API.csproj", "src/JCA.WorkSpace.Service.API/"]
RUN dotnet restore "src/JCA.WorkSpace.Service.API/JCA.WorkSpace.Service.API.csproj"

COPY . .
RUN dotnet publish "src/JCA.WorkSpace.Service.API/JCA.WorkSpace.Service.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

USER $APP_UID

EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "JCA.WorkSpace.Service.API.dll"]