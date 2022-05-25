FROM mcr.microsoft.com/dotnet/sdk:6.0 AS src
COPY . .
WORKDIR /src

RUN dotnet build MiniBank.Web -c Release -r linux-x64
RUN dotnet test MiniBank.Core.Tests --no-build
RUN dotnet publish MiniBank.Web -c Release -r linux-x64 --no-build -o /dist

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as final
WORKDIR /app
COPY --from=src /dist .
ENV ASPNETCORE_URLS=http://+:5000;http://+5001
EXPOSE 5000 5001
ENTRYPOINT ["dotnet", "MiniBank.Web.dll"]