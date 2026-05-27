FROM mcr.microsoft.com/devcontainers/base:ubuntu

# Install dependencies
RUN apt-get update && apt-get install -y \
    curl \
    wget \
    apt-transport-https \
    ca-certificates \
    gnupg \
    lsb-release \
    iptables \
    sudo \
    && rm -rf /var/lib/apt/lists/*

# -----------------------------
# Install Docker (DinD)
# -----------------------------
RUN curl -fsSL https://get.docker.com | sh

# -----------------------------
# Install .NET SDK
# -----------------------------
RUN wget https://dot.net/v1/dotnet-install.sh \
    && chmod +x dotnet-install.sh \
    && ./dotnet-install.sh --channel LTS \
    && rm dotnet-install.sh

ENV PATH="$PATH:/root/.dotnet:/root/.dotnet/tools"

# -----------------------------
# Install Node.js (LTS)
# -----------------------------
RUN curl -fsSL https://deb.nodesource.com/setup_lts.x | bash - \
    && apt-get install -y nodejs

# -----------------------------
# Install Dapr CLI
# -----------------------------
RUN wget -q https://raw.githubusercontent.com/dapr/cli/master/install/install.sh -O - | /bin/bash

# -----------------------------
# Non-root user setup
# -----------------------------
RUN useradd -m vscode \
    && echo "vscode ALL=(ALL) NOPASSWD:ALL" >> /etc/sudoers

# Copia script dind
COPY dind-entrypoint.sh /usr/local/bin/dind-entrypoint.sh
RUN chmod +x /usr/local/bin/dind-entrypoint.sh

USER vscode
``
