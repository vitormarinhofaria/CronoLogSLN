FROM mcr.microsoft.com/dotnet/sdk:5.0 as build

WORKDIR /source
COPY ./*.sln ./
COPY ./CronoLog/*.csproj ./CronoLog/

RUN dotnet restore

COPY . .
#RUN dotnet publish -c release -o /app --no-restore
RUN dotnet publish -r linux-musl-x64 -p:PublishTrimmed=true -c release -o /app --self-contained --no-restore

#FROM mcr.microsoft.com/dotnet/aspnet:5.0
FROM alpine:latest
RUN apk add --no-cache curl
WORKDIR /app
COPY --from=build /app .
COPY ping.sh ping.sh
RUN chmod +x ping.sh
#ENTRYPOINT ["./CronoLog"]