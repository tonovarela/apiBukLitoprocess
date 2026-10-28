---
description: Construye la imagen Docker para Linux (amd64) y la publica en Docker Hub con el tag indicado y latest
argument-hint: <tag>  (ej. 8.0.7)
allowed-tools: Bash(./scripts/docker-publish.sh:*)
---

Publica la imagen con el tag `$ARGUMENTS`.

1. Si `$ARGUMENTS` está vacío, pide el tag al usuario y no continúes.
2. Ejecuta `./scripts/docker-publish.sh $ARGUMENTS` (timeout de 10 minutos).
3. Reporta el resultado: tags publicados, digest y plataforma. Si falla, muestra el error tal cual.
4. Si el tag en `docker-compose.yaml` (`image:`) es distinto de `$ARGUMENTS`, menciónalo y pregunta si se actualiza.
