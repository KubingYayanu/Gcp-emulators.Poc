#!/bin/bash
# 透過迴圈嘗試建立 topic/subscription, 避免 pub/sub 尚未完成啟動就呼叫 API
# ===
# 開發環境: Docker 初始化自動建立
# 測試/正式環境: 手動建立
set -e

PUBSUB_HOST=${PUBSUB_HOST:-localhost:8086}
PROJECT_ID=${PROJECT_ID:-test-project}
MAX_RETRIES=${MAX_RETRIES:-30}
RETRY_INTERVAL=${RETRY_INTERVAL:-2}

echo "正在等待 Pub/Sub emulator 在 $PUBSUB_HOST 啟動..."

# 等待 Pub/Sub 服務啟動的函式
wait_for_pubsub() {
    local retry_count=0
    
    while [ $retry_count -lt $MAX_RETRIES ]; do
        echo "嘗試連接 Pub/Sub emulator... (第 $((retry_count + 1)) 次)"
        
        # 檢查 Pub/Sub 服務是否可用
        if curl -s --connect-timeout 3 --max-time 5 -f "http://$PUBSUB_HOST" > /dev/null 2>&1; then
            echo "✅ Pub/Sub emulator 已啟動並可接受連接"
            return 0
        fi
        
        retry_count=$((retry_count + 1))
        if [ $retry_count -lt $MAX_RETRIES ]; then
            echo "⏳ Pub/Sub emulator 尚未準備就緒，等待 $RETRY_INTERVAL 秒後重試..."
            sleep $RETRY_INTERVAL
        fi
    done
    
    echo "❌ 錯誤：在 $((MAX_RETRIES * RETRY_INTERVAL)) 秒內無法連接到 Pub/Sub emulator"
    exit 1
}

# 建立 Topic 的函式
create_topic() {
    local topic_name=$1
    local retry_count=0
    local max_api_retries=5
    
    while [ $retry_count -lt $max_api_retries ]; do
        echo "正在建立 topic: $topic_name (第 $((retry_count + 1)) 次嘗試)"
        
        if curl -s -X PUT "http://$PUBSUB_HOST/v1/projects/$PROJECT_ID/topics/$topic_name" \
            --write-out "%{http_code}" --output /tmp/topic_response.txt | grep -q "^20[0-9]$"; then
            echo "✅ Topic '$topic_name' 建立成功"
            return 0
        fi
        
        retry_count=$((retry_count + 1))
        if [ $retry_count -lt $max_api_retries ]; then
            echo "⏳ Topic 建立失敗，等待 2 秒後重試..."
            sleep 2
        fi
    done
    
    echo "❌ 錯誤：無法建立 topic '$topic_name'"
    cat /tmp/topic_response.txt 2>/dev/null || true
    return 1
}

# 建立 Subscription 的函式
create_subscription() {
    local subscription_name=$1
    local topic_name=$2
    local retry_count=0
    local max_api_retries=5
    
    while [ $retry_count -lt $max_api_retries ]; do
        echo "正在建立 subscription: $subscription_name (第 $((retry_count + 1)) 次嘗試)"
        
        if curl -s -X PUT "http://$PUBSUB_HOST/v1/projects/$PROJECT_ID/subscriptions/$subscription_name" \
            -H "Content-Type: application/json" \
            -d "{
                \"topic\": \"projects/$PROJECT_ID/topics/$topic_name\",
                \"ackDeadlineSeconds\": 60
            }" \
            --write-out "%{http_code}" --output /tmp/subscription_response.txt | grep -q "^20[0-9]$"; then
            echo "✅ Subscription '$subscription_name' 建立成功"
            return 0
        fi
        
        retry_count=$((retry_count + 1))
        if [ $retry_count -lt $max_api_retries ]; then
            echo "⏳ Subscription 建立失敗，等待 2 秒後重試..."
            sleep 2
        fi
    done
    
    echo "❌ 錯誤：無法建立 subscription '$subscription_name'"
    cat /tmp/subscription_response.txt 2>/dev/null || true
    return 1
}

echo "🚀 開始初始化 Pub/Sub emulator (專案: $PROJECT_ID)..."

# 等待 Pub/Sub 服務啟動
wait_for_pubsub

# 建立 topic
if create_topic "test-topic"; then
    # 建立 subscription
    if create_subscription "test-subscription" "test-topic"; then
        echo "🎉 Pub/Sub emulator 初始化完成！"
        
        # 清理暫存檔案
        rm -f /tmp/topic_response.txt /tmp/subscription_response.txt
        
        # 顯示建立的資源摘要
        echo ""
        echo "📋 已建立的資源摘要："
        echo "   - 專案 ID: $PROJECT_ID"
        echo "   - Topic: test-topic"
        echo "   - Subscription: test-subscription"
        echo "   - Pub/Sub 端點: $PUBSUB_HOST"
        echo ""
    else
        echo "❌ Subscription 建立失敗"
        exit 1
    fi
else
    echo "❌ Topic 建立失敗"
    exit 1
fi
