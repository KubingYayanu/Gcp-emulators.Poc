# README

# Docker 環境建立

執行 Docker Compose

```bash
$ docker compose -f docker-compose.yaml -p gcp-emulators-poc up -d

# 重新編譯相依服務 Image
$ docker compose -f docker-compose.yaml -p gcp-emulators-poc up -d --build
```

# Resources 管理

- 需要先建立 Topic，並且建立對應的 Subscription，才能進行訊息的發布與拉取

## Topics

```bash
# 建立 Topic
$ curl -X PUT http://localhost:8085/v1/projects/test-project/topics/test-topic

# 查詢所有 Topics
$ curl -s "http://localhost:8085/v1/projects/test-project/topics" | jq

# 查詢指定 Topic 是否存在
$ curl -s "http://localhost:8085/v1/projects/test-project/topics/test-topic" | jq

# 發布訊息
$ curl -X POST http://localhost:8085/v1/projects/test-project/topics/test-topic:publish \
  -H "Content-Type: application/json" \
  -d '{
        "messages": [
          { "data": "'$(echo -n "hello emulator" | base64)'" }
        ]
      }'
```

## Subscriptions

```bash
# 建立 Subscription, 並指定 ackDeadlineSeconds
$ curl -X PUT http://localhost:8085/v1/projects/test-project/subscriptions/test-subscription \
  -H "Content-Type: application/json" \
  -d '{
        "topic": "projects/test-project/topics/test-topic",
        "ackDeadlineSeconds": 60
      }'

# 刪除 Subscription
$ curl -X DELETE http://localhost:8085/v1/projects/test-project/subscriptions/test-subscription

# 查詢所有 Subscriptions
$ curl -s "http://localhost:8085/v1/projects/test-project/subscriptions" | jq

# 查詢指定 Subscription 是否存在
$ curl -s "http://localhost:8085/v1/projects/test-project/subscriptions/test-subscription" | jq

# 拉取訊息
$ curl -s -X POST http://localhost:8085/v1/projects/test-project/subscriptions/test-subscription:pull \
  -H "Content-Type: application/json" \
  -d '{"maxMessages":1}' | jq
```
