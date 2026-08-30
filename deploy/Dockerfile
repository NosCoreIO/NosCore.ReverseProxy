FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY NuGet.config Directory.Build.props* ./
COPY src/ src/
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish src/NosCore.ReverseProxy -c Release -o /out/app

FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine

# runtime-deps sets DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true and ships no ICU data.
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --no-cache icu-libs

COPY configuration /app/configuration
WORKDIR /app/build/net10.0
COPY --from=build /out/app .
USER $APP_UID

EXPOSE 4000 1337

ENTRYPOINT ["dotnet", "NosCore.ReverseProxy.dll"]
