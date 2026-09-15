#! /bin/bash

set -e

PUSH=0
TEST=0
BUILD=0
DOCKER_BUILD=0
DOCKER_PUSH=0
INSTALL_PORTER=0
ALLOWED_RUNTIMES=("linux-x64" "win-x64" "linux-arm64" "win-arm64")
RUNTIME="linux-x64"
HASH=""

while [ -n "$1" ]; do 
    case "$1" in
    --push|-p) PUSH=1 ;;
    --test|-t) TEST=1 ;;
    --build|-b) BUILD=1 ;;
    --docker|-d) DOCKER_BUILD=1 ;;
    --dockerpush) DOCKER_PUSH=1 ;;
    --install|-i) INSTALL_PORTER=1 ;;
    --runtime|-r) RUNTIME="${2#*=}" ;;
    --tag) TAG="${2#*=}" ;;
    --hash) HASH="${2#*=}" ;;
    esac 
    shift
done

if [ -z "$TAG" ]; then
    TAG=$(git describe --tags --abbrev=0)
fi

if [ -z "$HASH" ]; then
    HASH=$(git rev-parse --short HEAD)
fi

if [ -z "$TAG" ]; then
    echo "Could not read a tag from git history, exiting. You can force set a tag with --tag <TAG>"
    exit 1;
fi

echo "tag: ${TAG}"
echo "hash: ${HASH}"
echo "runtime: ${RUNTIME}"
echo "build: ${BUILD}"
echo "test: ${TEST}"
echo "push: ${PUSH}"

IS_IN_ARRAY=$(echo ${ALLOWED_RUNTIMES[@]} | grep -o $RUNTIME | wc -w)
if [ $IS_IN_ARRAY -eq 0 ]; then
    echo "runtime ${RUNTIME} is not supported"
    exit 1;
fi

if [ $INSTALL_PORTER -eq 1 ]; then
    wget https://github.com/shukriadams/porter/releases/download/0.0.2/porter_linux-x64 -O ~/porter
    chmod +x ~/porter
fi

if [ $BUILD -eq 1 ]; then

    # write hash + tag to currentVersion.txt in source, this will be displayed by web ui
    echo "$TAG (${HASH})" > ./../src/Krono/currentVersion.txt 

    ~/porter --install ./../src/Krono

    dotnet restore ./../src/Krono

    dotnet publish ./../src/Krono/Krono.csproj \
        --configuration Release \
        --runtime $RUNTIME \
        -o ./publish \
        -p:PublishReadyToRun=true \
        -p:PublishSingleFile=true \
        -p:PublishTrimmed=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        --self-contained true 
fi

if [ $TEST -eq 1 ]; then
    echo "testing ... "
    
    ./publish/Krono -v

    echo "test passed"
fi

if [ $DOCKER_BUILD -eq 1 ]; then
    docker build -t shukriadams/krono .

    STATUS=$(docker run shukriadams/krono:latest krono --version) 
    if [[ "$STATUS" == *"version :"* ]]; then
        echo "container test passed with string ${STATUS}"
    else
        echo "container test returned unexpected value ${STATUS}"
        exit 1
    fi
fi

if [ $DOCKER_PUSH -eq 1 ]; then
    echo "uploading docker image"
    docker login -u $DOCKER_USER -p $DOCKER_PASS 

    docker tag shukriadams/krono:latest shukriadams/krono:$TAG  
    docker push shukriadams/krono:$TAG
fi

if [ $PUSH -eq 1 ]; then

    # at time of writing, access token had permissions:
    # actions : read (unsure)
    # artefact metadata : read (unsure)
    # contents : write (confirmed)
    
    echo "uploading binaries to github"

    if [ $RUNTIME = "linux-x64" ] ; then
        filename=./publish/Krono
        EXTENSION=""    
    fi

    NAME="krono_${RUNTIME}${EXTENSION}"

    repo="shukriadams/krono"
    GH_REPO="https://api.github.com/repos/$repo"
    GH_TAGS="$GH_REPO/releases/tags/$TAG"
    AUTH="Authorization: token $GH_TOKEN"
    WGET_ARGS="--content-disposition --auth-no-challenge --no-cookie"
    CURL_ARGS="-LJO#"

    # Validate token.
    curl -o /dev/null -sH "$GH_TOKEN" $GH_REPO || { echo "Error : token validation failed";  exit 1; }

    # Read asset tags.
    RESPONSE=$(curl -sH "$GH_TOKEN" $GH_TAGS)

    # Get ID of the asset based on given filename.
    eval $(echo "$RESPONSE" | grep -m 1 "id.:" | grep -w id | tr : = | tr -cd '[[:alnum:]]=')
    [ "$id" ] || { echo "Error : Failed to get release id for tag: $TAG"; echo "$RESPONSE" | awk 'length($0)<100' >&2; exit 1; }

    # upload file to github
    GH_ASSET="https://uploads.github.com/repos/$repo/releases/$id/assets?name=$(basename $NAME)"
    curl --data-binary @"$filename" -H "Authorization: token $GH_TOKEN" -H "Content-Type: application/octet-stream" $GH_ASSET

fi

