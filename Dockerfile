FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
#  dsdsd
COPY publish/ ./

RUN apt-get update && apt-get install -y curl

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "PROXY_TODB.dll"]