# Use the official .NET SDK 10 image as base
FROM mcr.microsoft.com/dotnet/sdk:10.0

ENV DEBIAN_FRONTEND=noninteractive

# Create a vscode user (uid 1000) and install some small useful tools
RUN useradd -m -s /bin/bash vscode && \
    apt-get update && apt-get install -y --no-install-recommends \n      ca-certificates curl git wget unzip && \
    rm -rf /var/lib/apt/lists/*

# Switch to non-root user
USER vscode
WORKDIR /home/vscode

# Ensure dotnet global tools are available for the vscode user
ENV PATH="$PATH:/home/vscode/.dotnet/tools"

EXPOSE 9222
