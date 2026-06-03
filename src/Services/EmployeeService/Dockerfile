FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/Services/EmployeeService/EmployeeService.csproj"
RUN dotnet build "src/Services/EmployeeService/EmployeeService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Services/EmployeeService/EmployeeService.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmployeeService.dll"]