FROM debian:stretch-slim
WORKDIR /app
RUN apt update && apt install -y --no-install-recommends libunwind8 libssl1.0 && rm -rf /var/lib/apt/lists/*
COPY out ./
COPY runtimeconfig.json ./HttpReceiver.runtimeconfig.json
CMD ["./HttpReceiver"]

