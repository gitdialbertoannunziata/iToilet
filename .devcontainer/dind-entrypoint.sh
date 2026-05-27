
#!/bin/bash

set -e

echo "🚀 Starting Docker daemon..."

# Avvia Docker daemon in background
sudo dockerd > /tmp/dockerd.log 2>&1 &

# Attendi Docker
timeout=30
while ! docker info >/dev/null 2>&1; do
  sleep 1
  timeout=$((timeout-1))
  if [ $timeout -le 0 ]; then
    echo "❌ Docker failed to start"
    cat /tmp/dockerd.log
    exit 1
  fi
done

echo "✅ Docker started"

# Init Dapr
echo "🚀 Initializing Dapr..."
dapr init

echo "✅ Dev container ready!"
