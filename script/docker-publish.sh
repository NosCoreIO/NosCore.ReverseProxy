#!/usr/bin/env bash

echo "$DOCKER_PASSWORD" | docker login -u "$DOCKER_USERNAME" --password-stdin
dotnet publish ./src/NosCoreBot -c Release -o ./bin/Docker

DOCKER_ENV=''
DOCKER_TAG=''

case "$TRAVIS_BRANCH" in
  "master")
    DOCKER_ENV=production
    DOCKER_TAG=latest
    ;;  
esac

docker build -f ./deploy/Dockerfile -t noscore.reverseproxy:$DOCKER_TAG --no-cache .
docker tag noscorebot:$DOCKER_TAG noscoreio/noscore.reverseproxy:$DOCKER_TAG
docker push noscoreio/noscore.reverseproxy:$DOCKER_TAG