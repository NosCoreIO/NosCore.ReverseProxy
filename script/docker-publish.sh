#!/usr/bin/env bash

echo "$DOCKER_PASSWORD" | docker login -u "$DOCKER_USERNAME" --password-stdin
dotnet publish ./src/NosCore.ReverseProxy -c Release -o ./bin/Docker --runtime linux-musl-x64 --nologo

DOCKER_ENV=''
DOCKER_TAG=''

case "$TRAVIS_BRANCH" in
  "master")
    DOCKER_ENV=production
    DOCKER_TAG=latest
    ;;  
esac

docker build -f ./deploy/Dockerfile -t noscore.reverseproxy:$DOCKER_TAG --no-cache .
docker tag noscore.reverseproxy:$DOCKER_TAG noscoreio/noscore.reverseproxy:$DOCKER_TAG
docker push noscoreio/noscore.reverseproxy:$DOCKER_TAG