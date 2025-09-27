#!/bin/bash
# 測試 Pub/Sub 初始化腳本的驗證工具
# 用於檢查 topic 和 subscription 是否正確建立

PUBSUB_HOST=${PUBSUB_HOST:-localhost:8086}
PROJECT_ID=${PROJECT_ID:-test-project}

echo "🔍 正在驗證 Pub/Sub emulator 設定..."
echo "📍 主機: $PUBSUB_HOST"
echo "📁 專案 ID: $PROJECT_ID"
echo ""

# 檢查 Pub/Sub 服務狀態
echo "1️⃣ 檢查 Pub/Sub 服務連接狀態..."
if curl -s --connect-timeout 3 --max-time 5 -f "http://$PUBSUB_HOST" > /dev/null 2>&1; then
    echo "✅ Pub/Sub emulator 服務正常運行"
else
    echo "❌ 無法連接到 Pub/Sub emulator"
    exit 1
fi

# 檢查 Topic
echo ""
echo "2️⃣ 檢查 Topic 'test-topic'..."
TOPIC_RESPONSE=$(curl -s -w "%{http_code}" "http://$PUBSUB_HOST/v1/projects/$PROJECT_ID/topics/test-topic")
TOPIC_HTTP_CODE="${TOPIC_RESPONSE: -3}"

if [ "$TOPIC_HTTP_CODE" = "200" ]; then
    echo "✅ Topic 'test-topic' 存在且可訪問"
else
    echo "❌ Topic 'test-topic' 不存在或無法訪問 (HTTP: $TOPIC_HTTP_CODE)"
fi

# 檢查 Subscription
echo ""
echo "3️⃣ 檢查 Subscription 'test-subscription'..."
SUB_RESPONSE=$(curl -s -w "%{http_code}" "http://$PUBSUB_HOST/v1/projects/$PROJECT_ID/subscriptions/test-subscription")
SUB_HTTP_CODE="${SUB_RESPONSE: -3}"

if [ "$SUB_HTTP_CODE" = "200" ]; then
    echo "✅ Subscription 'test-subscription' 存在且可訪問"
else
    echo "❌ Subscription 'test-subscription' 不存在或無法訪問 (HTTP: $SUB_HTTP_CODE)"
fi

# 列出所有 Topics
echo ""
echo "4️⃣ 列出所有 Topics..."
ALL_TOPICS=$(curl -s "http://$PUBSUB_HOST/v1/projects/$PROJECT_ID/topics")
echo "📋 Topics 清單:"
echo "$ALL_TOPICS" | jq -r '.topics[]?.name // "無 topics 找到"' 2>/dev/null || echo "$ALL_TOPICS"

# 列出所有 Subscriptions
echo ""
echo "5️⃣ 列出所有 Subscriptions..."
ALL_SUBS=$(curl -s "http://$PUBSUB_HOST/v1/projects/$PROJECT_ID/subscriptions")
echo "📋 Subscriptions 清單:"
echo "$ALL_SUBS" | jq -r '.subscriptions[]?.name // "無 subscriptions 找到"' 2>/dev/null || echo "$ALL_SUBS"

echo ""
echo "🏁 驗證完成！"