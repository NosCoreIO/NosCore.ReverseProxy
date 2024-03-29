ARG REPO=mcr.microsoft.com/dotnet/runtime-deps
FROM $REPO:6.0.0-alpine3.14-amd64

RUN apk add --no-cache icu-libs	# Install .NET
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Install .NET
ENV DOTNET_VERSION 6.0.0

RUN wget -O dotnet.tar.gz https://dotnetcli.azureedge.net/dotnet/Runtime/$DOTNET_VERSION/dotnet-runtime-$DOTNET_VERSION-linux-musl-x64.tar.gz \
    && dotnet_sha512='1b6b5346426e53afd7ea4344e79b29a903b36bb1dfbc88d68f3a17a88b42ca9563d8af7c086cc0d455cb344c7d11896d585667c76e424b2e2760e7421018c1c7' \
    && echo "$dotnet_sha512  dotnet.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -C /usr/share/dotnet -oxzf dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm dotnet.tar.gz

COPY ./configuration /app/configuration
COPY ./build/net6.0 /app/build/net6.0

WORKDIR /app/build/net6.0

EXPOSE 4000 1337

ENTRYPOINT ["dotnet", "NosCore.ReverseProxy.dll"]
