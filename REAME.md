# README

# Docker 環境建立

執行 Docker Compose

```bash
$ docker compose -f docker-compose.yaml -p gcp-emulators-poc up -d

# 重新編譯相依服務 Image
$ docker compose -f docker-compose.yaml -p gcp-emulators-poc up -d --build
```

# Resources 管理

-   需要先建立 Topic，並且建立對應的 Subscription，才能進行訊息的發布與拉取

## Topics

```bash
# 建立 Topic
$ curl -X PUT http://localhost:8085/v1/projects/test-project/topics/test-topic

# 查詢所有 Topics
$ curl -s "http://localhost:8085/v1/projects/test-project/topics" | jq

# 查詢指定 Topic 是否存在
$ curl -s "http://localhost:8085/v1/projects/test-project/topics/test-topic" | jq

# 刪除 Topic

$ curl -X DELETE http://localhost:8085/v1/projects/test-project/topics/test-topic

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

# 查詢所有 Subscriptions
$ curl -s "http://localhost:8085/v1/projects/test-project/subscriptions" | jq

# 查詢指定 Subscription 是否存在
$ curl -s "http://localhost:8085/v1/projects/test-project/subscriptions/test-subscription" | jq

# 刪除 Subscription
$ curl -X DELETE http://localhost:8085/v1/projects/test-project/subscriptions/test-subscription

# 拉取訊息
$ curl -s -X POST http://localhost:8085/v1/projects/test-project/subscriptions/test-subscription:pull \
  -H "Content-Type: application/json" \
  -d '{
        "maxMessages": 1
      }'

#
$ curl -X POST http://localhost:8085/v1/projects/test-project/subscriptions/test-subscription:35
```

# Publisher/Subscriber

## 架構概覽

以下是本專案中實作的三種 Publisher/Subscriber 模式：

```mermaid
graph TD
  subgraph "一對一模式"
      P1[PublisherOneToOne] --> T1[Topic: one-to-one]
      T1 --> S1[Subscription: one-to-one-sub]
      S1 --> C1[SubscriberOneToOne]
  end

  %% style P1 fill:#e1f5fe
  %% style T1 fill:#f3e5f5
  %% style S1 fill:#e8f5e8
  %% style C1 fill:#fff3e0
```

```mermaid
graph TD
  subgraph "一對多模式"
      P2[PublisherOneToMany] --> T2[Topic: one-to-many]
      T2 --> S2[Subscription: one-to-many-sub]
      S2 --> C2[SubscriberOneToMany1]
      S2 --> C3[SubscriberOneToMany2]
      note2["隨機分配訊息"]
  end

  %% style P2 fill:#e1f5fe
  %% style T2 fill:#f3e5f5
  %% style S2 fill:#e8f5e8
  %% style C2 fill:#fff3e0
  %% style C3 fill:#fff3e0
```

```mermaid
graph TD
    subgraph "屬性過濾模式"
        P3[PublisherFilterByAttribute] --> T3[Topic: filter-by-attribute]
        T3 --> S3[Subscription: filter-by-attribute-sub-dog]
        T3 --> S4[Subscription: filter-by-attribute-sub-cat]
        S3 --> C4[SubscriberFilterByAttributeDog]
        S4 --> C5[SubscriberFilterByAttributeCat]
        note3["⚠️ Emulator 不支援 filter"]
    end

    %% style P3 fill:#e1f5fe
    %% style T3 fill:#f3e5f5
    %% style S3 fill:#e8f5e8
    %% style S4 fill:#e8f5e8
    %% style C4 fill:#fff3e0
    %% style C5 fill:#fff3e0
```

## 一對一

**模式說明：** 一個發布者對應一個訂閱者，訊息直接傳遞

```mermaid
sequenceDiagram
    participant PA as PublisherOneToOne
    participant T1 as Topic: one-to-one
    participant S1 as Subscription: one-to-one-sub
    participant SA as SubscriberOneToOne

    PA->>T1: 發布訊息
    T1->>S1: 儲存訊息
    SA->>S1: 拉取訊息
    S1->>SA: 返回訊息
    SA->>S1: 確認訊息 (ACK)
```

-   PublisherOneToOne
-   SubscriberOneToOne
    -   subscription: `one-to-one-sub`

```bash
# 建立 Topic
$ curl -X PUT http://localhost:8085/v1/projects/test-project/topics/one-to-one

# 建立 Subscription
$ curl -X PUT http://localhost:8085/v1/projects/test-project/subscriptions/one-to-one-sub \
  -H "Content-Type: application/json" \
  -d '{
        "topic": "projects/test-project/topics/one-to-one"
      }'
```

## 一對多

**模式說明：** 一個發布者對應多個訂閱者，同一個訂閱共享相同的 Subscription，訊息會隨機分配

```mermaid
sequenceDiagram
    participant PM as PublisherOneToMany
    participant T2 as Topic: one-to-many
    participant S2 as Subscription: one-to-many-sub
    participant SM1 as SubscriberOneToMany1
    participant SM2 as SubscriberOneToMany2

    PM->>T2: 發布訊息A
    T2->>S2: 儲存訊息A
    PM->>T2: 發布訊息B
    T2->>S2: 儲存訊息B

    par 隨機分配訊息
        SM1->>S2: 拉取訊息
        S2->>SM1: 返回訊息A
        SM1->>S2: 確認訊息A (ACK)
    and
        SM2->>S2: 拉取訊息
        S2->>SM2: 返回訊息B
        SM2->>S2: 確認訊息B (ACK)
    end
```

-   PublisherOneToMany
-   SubscriberOneToMany1
    -   subscription: `one-to-many-sub`
-   SubscriberOneToMany2
    -   subscription: `one-to-many-sub`
-   隨機分配訊息給 Subscriber

```bash
# 建立 Topic
$ curl -X PUT http://localhost:8085/v1/projects/test-project/topics/one-to-many

# 建立 Subscription
$ curl -X PUT http://localhost:8085/v1/projects/test-project/subscriptions/one-to-many-sub \
  -H "Content-Type: application/json" \
  -d '{
        "topic": "projects/test-project/topics/one-to-many"
      }'
```

## Filter by attribute

**模式說明：** 基於訊息屬性進行過濾，不同的訂閱者只接收符合條件的訊息

```mermaid
sequenceDiagram
    participant PF as PublisherFilterByAttribute
    participant T3 as Topic: filter-by-attribute
    participant SD as Subscription: filter-by-attribute-sub-dog
    participant SC as Subscription: filter-by-attribute-sub-cat
    participant SFD as SubscriberFilterByAttributeDog
    participant SFC as SubscriberFilterByAttributeCat

    PF->>T3: 發布訊息 (attribute: dog)
    T3->>SD: 過濾並儲存訊息
    T3-->>SC: 不符合過濾條件 (略過)

    PF->>T3: 發布訊息 (attribute: cat)
    T3-->>SD: 不符合過濾條件 (略過)
    T3->>SC: 過濾並儲存訊息

    SFD->>SD: 拉取訊息
    SD->>SFD: 返回 dog 訊息
    SFD->>SD: 確認訊息 (ACK)

    SFC->>SC: 拉取訊息
    SC->>SFC: 返回 cat 訊息
    SFC->>SC: 確認訊息 (ACK)

    Note over T3: ⚠️ Pub/Sub Emulator 不支援 filter<br/>實際上兩個 Subscriber 都會收到所有訊息
```

-   PublisherFilterByAttribute
-   SubscriberFilterByAttributeDog
    -   subscription: `filter-by-attribute-sub-dog`
-   SubscriberFilterByAttributeCat
    -   subscription: `filter-by-attribute-sub-cat`
-   Pub/Sub Emulator 不支援 filter 功能，導致兩個 Subscriber 都會收到所有訊息

```bash
# 建立 Topic
$ curl -X PUT http://localhost:8085/v1/projects/test-project/topics/filter-by-attribute

# 建立 Subscription (filter by attributes.dog)
$ curl -X PUT http://localhost:8085/v1/projects/test-project/subscriptions/filter-by-attribute-sub-dog \
  -H "Content-Type: application/json" \
  -d '{
        "topic": "projects/test-project/topics/filter-by-attribute",
        "filter": "attributes.partition_key = \"dog\""
      }'

# 建立 Subscription (filter by attributes.cat)
$ curl -X PUT http://localhost:8085/v1/projects/test-project/subscriptions/filter-by-attribute-sub-cat \
  -H "Content-Type: application/json" \
  -d '{
        "topic": "projects/test-project/topics/filter-by-attribute",
        "filter": "attributes.partition_key = \"cat\""
      }'
```
