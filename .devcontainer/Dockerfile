
FROM mcr.microsoft.com/devcontainers/base:ubuntu

# -----------------------------
# Base dependencies
# -----------------------------
RUN apt-get update && apt-get install -y \
    curl \
    wget \
    apt-transport-https \
    ca-certificates \
    gnupg \
    lsb-release \
    iptables \
    sudo \
    software-properties-common \
    && rm -rf /var/lib/apt/lists/*

# -----------------------------
# INSTALL DOCKER (DinD)
# -----------------------------
RUN curl -fsSL https://get.docker.com | sh

# -----------------------------
# INSTALL .NET SDK (OFFICIAL WAY ✅)
# -----------------------------
RUN wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    && sudo dpkg -i packages-microsoft-prod.deb \
    && rm packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y dotnet-sdk-8.0

# Optional: global tools
RUN dotnet tool install -g dotnet-ef \
    && dotnet tool install -g Microsoft.dotnet-httprepl

ENV PATH="$PATH:/root/.dotnet/tools"

# -----------------------------
# INSTALL NODE (LTS)
# -----------------------------
RUN curl -fsSL https://deb.nodesource.com/setup_lts.x | bash - \
    && apt-get install -y nodejs

# -----------------------------
# INSTALL DAPR
# -----------------------------
RUN wget -q https://raw.githubusercontent.com/dapr/cli/master/install/install.sh -O - | /bin/bash


# -----------------------------
# DIND SCRIPT
# -----------------------------
COPY dind-entrypoint.sh /usr/local/bin/dind-entrypoint.sh
RUN chmod +x /usr/local/bin/dind-entrypoint.sh
