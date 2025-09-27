# GCP Emulators - Pub/Sub 初始化腳本

這個目錄包含用於初始化 Google Cloud Pub/Sub emulator 的腳本和配置檔。

## 檔案說明

### 🐳 Docker 相關
- **`Dockerfile`**: 建構 GCP emulators 容器映像
- **`background.sh`**: 在背景執行初始化腳本的包裝器
- **`init-pubsub.sh`**: 主要初始化腳本，等待 Pub/Sub 服務啟動並建立 topic/subscription
- **`verify-pubsub.sh`**: 驗證 Pub/Sub 設定的測試腳本

## 功能特色

### 🔄 智能等待機制
`init-pubsub.sh` 包含以下功能：
- **健康檢查**: 自動等待 Pub/Sub emulator 啟動
- **重試機制**: 建立 topic/subscription 失敗時自動重試
- **錯誤處理**: 詳細的錯誤訊息和狀態回報
- **可配置參數**: 透過環境變數自訂設定

### 📊 驗證工具
`verify-pubsub.sh` 提供：
- 服務連接狀態檢查
- Topic 和 Subscription 存在性驗證
- 資源清單顯示
- 詳細的狀態報告

## 環境變數

| 變數名稱 | 預設值 | 說明 |
|---------|--------|------|
| `PUBSUB_HOST` | `localhost:8086` | Pub/Sub emulator 主機和連接埠 |
| `PROJECT_ID` | `test-project` | GCP 專案 ID |
| `MAX_RETRIES` | `30` | 最大重試次數 |
| `RETRY_INTERVAL` | `2` | 重試間隔（秒） |

## 使用方式

### 透過 Docker Compose 啟動
```bash
# 啟動所有服務（包括 Pub/Sub emulator）
docker-compose up -d

# 查看初始化日誌
docker-compose logs gcp-emulators
```

### 手動執行初始化腳本
```bash
# 在容器內執行
docker-compose exec gcp-emulators /config/init-pubsub.sh

# 或直接在主機上執行（需要設定環境變數）
export PUBSUB_HOST=localhost:8086
export PROJECT_ID=test-project
./init-pubsub.sh
```

### 驗證設定
```bash
# 在容器內執行驗證
docker-compose exec gcp-emulators /config/verify-pubsub.sh

# 或在主機上執行
export PUBSUB_HOST=localhost:8086
export PROJECT_ID=test-project
./verify-pubsub.sh
```

## 建立的資源

初始化完成後，會建立以下資源：

- **Topic**: `projects/{PROJECT_ID}/topics/test-topic`
- **Subscription**: `projects/{PROJECT_ID}/subscriptions/test-subscription`
  - 連結到 `test-topic`
  - Ack deadline: 60 秒

## 故障排除

### 常見問題

1. **連接超時**
   - 檢查 `PUBSUB_HOST` 是否正確
   - 確認 Pub/Sub emulator 已啟動
   - 檢查防火牆設定

2. **權限錯誤**
   ```bash
   # 確保腳本有執行權限
   chmod +x *.sh
   ```

3. **API 呼叫失敗**
   - 檢查 `PROJECT_ID` 是否正確
   - 查看容器日誌獲取詳細錯誤訊息

### 日誌查看
```bash
# 查看初始化日誌
docker-compose logs gcp-emulators

# 即時監控日誌
docker-compose logs -f gcp-emulators
```

## 開發注意事項

- 🔧 **開發環境**: 透過 Docker 自動初始化
- 🧪 **測試環境**: 可能需要手動執行初始化
- 🚀 **正式環境**: 應使用實際的 GCP Pub/Sub 服務

## 自訂配置

若需要建立其他 topic/subscription，可以修改 `init-pubsub.sh` 中的相關函式呼叫：

```bash
# 建立額外的 topic
create_topic "my-custom-topic"

# 建立額外的 subscription
create_subscription "my-custom-subscription" "my-custom-topic"
```