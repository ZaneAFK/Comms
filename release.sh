#!/bin/bash
set -euo pipefail

cd "$(git rev-parse --show-toplevel)"

if [ -z "${1:-}" ]; then
    echo "Usage: ./release.sh <version>  (e.g. ./release.sh 1.0.0)"
    exit 1
fi

if ! echo "$1" | grep -qE '^[0-9]+\.[0-9]+\.[0-9]+$'; then
    echo "Error: version must be in the form X.Y.Z (e.g. 1.0.0)" >&2
    exit 1
fi

VERSION=$1
TAG="v$VERSION"

BRANCH=$(git rev-parse --abbrev-ref HEAD)
if [ "$BRANCH" != "master" ]; then
    echo "Error: releases must be cut from master (currently on '$BRANCH')." >&2
    exit 1
fi

if [ -n "$(git status --porcelain)" ]; then
    echo "Error: working tree is not clean. Commit or stash changes first." >&2
    exit 1
fi

echo "Fetching origin..."
git fetch origin master --tags

BEHIND=$(git rev-list --count HEAD..origin/master)
if [ "$BEHIND" -gt 0 ]; then
    echo "Error: local master is behind origin/master by $BEHIND commit(s). Pull first." >&2
    exit 1
fi

if git rev-parse "$TAG" >/dev/null 2>&1; then
    echo "Error: tag $TAG already exists." >&2
    exit 1
fi

# Bump package.json (Comms-Server's version is derived from this tag by MinVer, no file to bump)
npm version --no-git-tag-version "$VERSION" --prefix Comms-Client

git add Comms-Client/package.json Comms-Client/package-lock.json
git commit -m "Bump version to $VERSION"
git tag "$TAG"

echo ""
echo "Version bumped to $VERSION and tagged $TAG."
echo "Run: git push origin master $TAG"