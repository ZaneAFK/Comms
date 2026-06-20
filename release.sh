#!/bin/bash
set -e

if [ -z "$1" ]; then
    echo "Usage: ./release.sh <version>  (e.g. ./release.sh 1.0.0)"
    exit 1
fi

if ! echo "$1" | grep -qE '^[0-9]+\.[0-9]+\.[0-9]+$'; then
    echo "Error: version must be in the form X.Y.Z (e.g. 1.0.0)" >&2
    exit 1
fi

VERSION=$1
TAG="v$VERSION"

# Bump package.json
npm version --no-git-tag-version "$VERSION" --prefix Comms-Client

# Bump .csproj
sed -i.bak "s|<Version>.*</Version>|<Version>$VERSION</Version>|" Comms-Server/Comms-Server/Comms-Server.csproj && rm Comms-Server/Comms-Server/Comms-Server.csproj.bak

git add Comms-Client/package.json Comms-Client/package-lock.json Comms-Server/Comms-Server/Comms-Server.csproj
git commit -m "Bump version to $VERSION"
git tag "$TAG"

echo ""
echo "Version bumped to $VERSION and tagged $TAG."
echo "Run: git push origin master $TAG"