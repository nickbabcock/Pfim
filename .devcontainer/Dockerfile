FROM mcr.microsoft.com/vscode/devcontainers/dotnet:1.5.4-8.0

RUN apt-get update && apt-get install -y \
        apt-transport-https \
        ca-certificates \
        dirmngr \
        gnupg \
    && \
    # Install Mono
    apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && \
    echo "deb https://download.mono-project.com/repo/debian stable-buster main" | tee /etc/apt/sources.list.d/mono-official-stable.list && \
    apt update && \
    apt-get install -y mono-complete && \
    rm -rf /var/lib/apt/lists/*
