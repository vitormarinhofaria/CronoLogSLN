FROM mcr.microsoft.com/dotnet/sdk:5.0 as build

WORKDIR /source
COPY ./*.sln ./
COPY ./CronoLog/*.csproj ./CronoLog/

RUN dotnet restore

COPY . .
RUN dotnet publish -c release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:5.0
COPY ping.sh ping.sh
RUN chmod +x ping.sh
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["./CronoLog"]