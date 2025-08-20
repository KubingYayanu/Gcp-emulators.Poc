#!/bin/bash

# 給予執行權限
# chmod +x mac-kill-watch.sh

pkill -f "Gcp.PubSub.Poc.Consumer"

pkill -f "Gcp.PubSub.Poc.Producer"
