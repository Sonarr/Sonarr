#!/usr/bin/env bash
set -e

CONTAINER="dockge"
BUILD_DIR="/tmp/sonarr-build"
IMAGE="sonarr-local:latest"

echo "==> Syncing source to $CONTAINER..."
incus exec "$CONTAINER" -- rm -rf "$BUILD_DIR"
incus exec "$CONTAINER" -- mkdir -p "$BUILD_DIR"

tar -C /home/matt/sonarr-fork -cf - \
    --exclude='.git' \
    --exclude='node_modules' \
    --exclude='_output' \
    package.json yarn.lock tsconfig.json frontend Dockerfile \
  | incus exec "$CONTAINER" -- tar -xf - -C "$BUILD_DIR"

echo "==> Building image..."
incus exec "$CONTAINER" -- docker build -t "$IMAGE" "$BUILD_DIR"

echo "==> Restarting Sonarr..."
incus exec "$CONTAINER" -- docker compose -f /opt/stacks/sonarr/compose.yaml up -d --force-recreate

echo "==> Cleaning up..."
incus exec "$CONTAINER" -- rm -rf "$BUILD_DIR"

echo "==> Done. Sonarr is running with your latest changes."
