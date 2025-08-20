#!/bin/bash

# 給予執行權限
# chmod +x mac-watch.sh

(cd ./src/services/Gcp.PubSub.Poc.Consumer && dotnet run --no-build --project "Gcp.PubSub.Poc.Consumer.csproj") &

# (cd ./src/services/Gcp.PubSub.Poc.Producer && dotnet run --no-build --project "Gcp.PubSub.Poc.Producer.csproj") &
