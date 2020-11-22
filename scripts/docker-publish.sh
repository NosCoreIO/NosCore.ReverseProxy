#!/usr/bin/env bash

echo "$DOCKER_PASSWORD" | docker login -u "$DOCKER_USERNAME" --password-stdin
dotnet test --runtime linux-musl-x64 --filter TestCategory=SomethingAsDotnetBuildDoesntWorkAnymore

DOCKER_ENV=''
DOCKER_TAG=''
export PATH=$PATH:$HOME/.local/bin

case "$TRAVIS_BRANCH" in
  "master")
    DOCKER_ENV=production
    DOCKER_TAG=latest
    ;;  
esac

docker build -f ./deploy/Dockerfile -t noscore.reverseproxy:$DOCKER_TAG --no-cache .
docker tag noscore.reverseproxy:$DOCKER_TAG noscoreio/noscore.reverseproxy:$DOCKER_TAG
docker push noscoreio/noscore.reverseproxy:$DOCKER_TAG
