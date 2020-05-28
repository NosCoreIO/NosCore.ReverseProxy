ARG REPO=mcr.microsoft.com/dotnet/core/runtime
FROM $REPO:3.1-alpine

# Install ASP.NET Core
ENV ASPNET_VERSION 3.1.4

RUN wget -O aspnetcore.tar.gz https://dotnetcli.azureedge.net/dotnet/aspnetcore/Runtime/$ASPNET_VERSION/aspnetcore-runtime-$ASPNET_VERSION-linux-musl-arm64.tar.gz \
    && aspnetcore_sha512='5bd3e7faa380809d67df58a218805c719e9141c19f599a149d2edd76f629d2fd96ae5dce79ec65f118bb95c6916ae9cc4fbe375aa58f910692893ab5ade2445b' \
    && echo "$aspnetcore_sha512  aspnetcore.tar.gz" | sha512sum -c - \
    && tar -ozxf aspnetcore.tar.gz -C /usr/share/dotnet ./shared/Microsoft.AspNetCore.App \
    && rm aspnetcore.tar.gz

COPY . /app

WORKDIR /app/build/bin

EXPOSE 4000 1337

ENTRYPOINT ["dotnet", "NosCore.ReverseProxy.dll"]