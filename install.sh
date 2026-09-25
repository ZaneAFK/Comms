#!/bin/bash
# Comms installer.
#
# Downloads compose.yml and nginx.conf from a GitHub release
# and starts the stack with Docker Compose. Safe to re-run: it reuses an
# existing .env (secrets included) and just refreshes the version + configs.
#
# Installs to /etc/comms by default (requires root/sudo). Use --dir to install
# elsewhere without elevated privileges.
#
# Usage:
#   curl -fsSL https://raw.githubusercontent.com/ZaneAFK/Comms/master/install.sh | sudo bash
#   curl -fsSL https://raw.githubusercontent.com/ZaneAFK/Comms/master/install.sh | bash -s -- --version 1.2.0 --dir /opt/comms

set -euo pipefail

cat <<'EOF'
  ______
  / ____/___  ____ ___  ____ ___  _____
 / /   / __ \/ __ `__ \/ __ `__ \/ ___/
/ /___/ /_/ / / / / / / / / / / (__  )
\____/\____/_/ /_/ /_/_/ /_/ /_/____/

EOF

REPO="ZaneAFK/Comms"
INSTALL_DIR="/etc/comms"
VERSION=""

while [ $# -gt 0 ]; do
    case "$1" in
        --version) VERSION="$2"; shift 2 ;;
        --dir) INSTALL_DIR="$2"; shift 2 ;;
        *) echo "Unknown option: $1" >&2; exit 1 ;;
    esac
done

command -v curl >/dev/null 2>&1 || { echo "Error: curl is required." >&2; exit 1; }
command -v docker >/dev/null 2>&1 || { echo "Error: docker is required. See https://docs.docker.com/get-docker/" >&2; exit 1; }
docker compose version >/dev/null 2>&1 || { echo "Error: the 'docker compose' plugin is required." >&2; exit 1; }

if [ -z "$VERSION" ]; then
    echo "Looking up latest release..."
    VERSION=$(curl -fsSL "https://api.github.com/repos/$REPO/releases/latest" \
        | grep -m1 '"tag_name"' \
        | sed -E 's/.*"tag_name": *"v?([^"]+)".*/\1/') || true
    if [ -z "$VERSION" ]; then
        echo "Error: could not determine the latest release. Pass --version explicitly." >&2
        exit 1
    fi
fi

TAG="v$VERSION"
BASE_URL="https://github.com/$REPO/releases/download/$TAG"

echo "Deploying Comms $TAG to $INSTALL_DIR"
mkdir -p "$INSTALL_DIR" 2>/dev/null || { echo "Error: cannot create $INSTALL_DIR (permission denied). Re-run with sudo, or pick a different location with --dir." >&2; exit 1; }
cd "$INSTALL_DIR"
INSTALL_DIR="$(pwd)"

for f in compose.yml nginx.conf; do
    curl -fsSL "$BASE_URL/$f" -o "$f.new"
    mv "$f.new" "$f"
done

gen_secret() {
    if command -v openssl >/dev/null 2>&1; then
        openssl rand -base64 24
    else
        head -c 32 /dev/urandom | base64
    fi
}

if [ -f .env ]; then
    echo "Existing .env found — keeping your secrets, just updating COMMS_VERSION."
    if grep -q '^COMMS_VERSION=' .env; then
        sed -i.bak "s/^COMMS_VERSION=.*/COMMS_VERSION=$VERSION/" .env && rm -f .env.bak
    else
        echo "COMMS_VERSION=$VERSION" >> .env
    fi
else
    echo "No .env found — generating one with fresh secrets."
    {
        echo "DATABASE_USER=comms"
        echo "DATABASE_PASSWORD=$(gen_secret)"
        echo "JWT_KEY=$(gen_secret)"
        echo "COMMS_VERSION=$VERSION"
    } > .env
    chmod 600 .env
    echo "Generated secrets saved to $INSTALL_DIR/.env — back this file up, it will not be shown again."
fi

echo "Pulling images and starting the stack..."
docker compose pull
docker compose up -d

echo ""
echo "Comms $TAG is starting. It will be available at http://<this-host>:8002 once healthy."
echo "Check status with: docker compose -f $INSTALL_DIR/compose.yml ps"
echo "To update later, re-run this same command."
