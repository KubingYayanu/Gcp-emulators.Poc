# README

# Docker 環境建立

執行 Docker Compose

```bash
$ docker compose -f docker-compose.yaml -p gcp-emulators-poc up -d

# 重新編譯相依服務 Image
$ docker compose -f docker-compose.yaml -p gcp-emulators-poc up -d --build
```

# Resources 管理

## Topics

```bash
# 查詢所有 Topics
$ curl "http://localhost:8085/v1/projects/lets-have-some-fun/topics"

# 查詢指定 Topic 是否存在
$ curl "http://localhost:8085/v1/projects/lets-have-some-fun/topics/something-go-wrong"
```

## Subscriptions

```bash
# 查詢所有 Subscriptions
$ curl "http://localhost:8085/v1/projects/lets-have-some-fun/subscriptions"

# 查詢指定 Subscription 是否存在
$ curl "http://localhost:8085/v1/projects/lets-have-some-fun/subscriptions/regist-something"
```