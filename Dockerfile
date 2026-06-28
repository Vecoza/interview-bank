FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/InterviewBank.API/InterviewBank.API.csproj ./InterviewBank.API/
RUN dotnet restore ./InterviewBank.API/InterviewBank.API.csproj

COPY src/InterviewBank.API/. ./InterviewBank.API/
RUN dotnet publish ./InterviewBank.API/InterviewBank.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "InterviewBank.API.dll"]
