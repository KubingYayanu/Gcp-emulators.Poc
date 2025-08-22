#!/bin/bash

# 給予執行權限
# chmod +x mac-start-producer.sh

dotnet run --environment "ASPNETCORE_ENVIRONMENT=Local" --no-build --project "./src/services/Gcp.PubSub.Poc.Producer/Gcp.PubSub.Poc.Producer.csproj"