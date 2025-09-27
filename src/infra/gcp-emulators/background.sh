#!/bin/bash
# 在背景執行 init-pubsub.sh 初始化腳本
# init-pubsub.sh 包含內建的等待機制，會等待 Pub/Sub 服務啟動後再建立 topic/subscription
# ===
# 開發環境: Docker 初始化自動建立
# 測試/正式環境: 手動建立

echo "🚀 啟動 Pub/Sub 初始化程序..."

# 執行初始化腳本
/config/init-pubsub.sh

echo "✅ Pub/Sub 初始化程序已完成"