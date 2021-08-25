FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine as build

WORKDIR /source
COPY ./*.sln ./
COPY ./CronoLog/*.csproj ./CronoLog/

RUN dotnet restore

COPY . .
#RUN dotnet publish -c release -o /app --no-restore
RUN dotnet publish -c release -o /app --no-restore

#FROM mcr.microsoft.com/dotnet/aspnet:5.0
FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine
RUN apk add --no-cache curl
RUN apk add --no-cache tzdata
WORKDIR /app
COPY --from=build /app .

COPY ping.sh ping.sh
RUN chmod +x ping.sh
COPY pause_cards.sh pause_cards.sh
RUN chmod +x pause_cards.sh

#ENTRYPOINT ["./CronoLog"]