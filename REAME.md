# README

# Docker 環境建立

執行 Docker Compose

```bash
$ docker compose -f docker-compose.yaml -p gcp-emulators-poc up -d

# 重新編譯相依服務 Image
$ docker compose -f docker-compose.yaml -p gcp-emulators-poc up -d --build
```