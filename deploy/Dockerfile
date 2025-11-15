ARG REPO=mcr.microsoft.com/dotnet/runtime-deps
FROM $REPO:10.0-alpine

RUN apk add --no-cache icu-libs	# Install .NET
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Install .NET
ENV DOTNET_VERSION 10.0.0

RUN wget -O dotnet.tar.gz https://dotnetcli.azureedge.net/dotnet/Runtime/$DOTNET_VERSION/dotnet-runtime-$DOTNET_VERSION-linux-musl-x64.tar.gz \
    && mkdir -p /usr/share/dotnet \
    && tar -C /usr/share/dotnet -oxzf dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet.tar.gz

COPY ./configuration /app/configuration
COPY ./build/net10.0 /app/build/net10.0

WORKDIR /app/build/net10.0

EXPOSE 4000 1337

ENTRYPOINT ["dotnet", "NosCore.ReverseProxy.dll"]
