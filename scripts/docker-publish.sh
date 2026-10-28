#!/usr/bin/env bash
# Construye la imagen para Linux y la publica en Docker Hub con el tag indicado y "latest".
# Uso: ./scripts/docker-publish.sh <tag>        ej. ./scripts/docker-publish.sh 8.0.7
# Variables opcionales: IMAGE (default tonovarela/apibuklitoprocess), PLATFORM (default linux/amd64)
set -euo pipefail

TAG="${1:-}"
IMAGE="${IMAGE:-tonovarela/apibuklitoprocess}"
PLATFORM="${PLATFORM:-linux/amd64}"

if [[ -z "$TAG" ]]; then
  echo "Uso: $0 <tag>   (ej. $0 8.0.7)" >&2
  exit 1
fi

if [[ "$TAG" == "latest" ]]; then
  echo "Indica un tag de versión; 'latest' se asigna automáticamente." >&2
  exit 1
fi

if ! [[ "$TAG" =~ ^[A-Za-z0-9_][A-Za-z0-9_.-]{0,127}$ ]]; then
  echo "Tag inválido: '$TAG'" >&2
  exit 1
fi

cd "$(dirname "$0")/.."

echo ">> Construyendo y publicando $IMAGE:$TAG y $IMAGE:latest ($PLATFORM)"
docker buildx build \
  --platform "$PLATFORM" \
  -t "$IMAGE:$TAG" \
  -t "$IMAGE:latest" \
  --push \
  .

echo ">> Verificando en Docker Hub"
docker buildx imagetools inspect "$IMAGE:$TAG" | grep -E "Name:|Digest:|Platform:" | head -4

echo ">> Listo: $IMAGE:$TAG (y latest)"
