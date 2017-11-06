#FROM microsoft/dotnet:2.0-runtime-deps
FROM debian:stretch-slim
RUN apt update && apt install -y --no-install-recommends libunwind8 curl ca-certificates
WORKDIR /opt/app
COPY out/linux-x64 ./
COPY runtimeconfig.json DecoderDotnet.runtimeconfig.json
CMD ["/opt/app/DecoderDotnet"]