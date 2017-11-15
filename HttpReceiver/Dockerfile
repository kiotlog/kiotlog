FROM microsoft/dotnet:2.0-sdk AS build-env
WORKDIR /app

COPY nuget.config ./
COPY *.*sproj ./
RUN dotnet restore

# copy everything else and build
COPY src ./src
RUN dotnet publish -c Release -r linux-x64 -o out /p:ShowLinkerSizeComparison=true

# build runtime image
FROM debian:stretch-slim
WORKDIR /app
RUN apt update && apt install -y --no-install-recommends libunwind8 libssl1.0 && rm -rf /var/lib/apt/lists/*
COPY --from=build-env /app/out ./
# COPY runtimeconfig.json ./HttpReceiver.runtimeconfig.json
ENTRYPOINT ["./HttpReceiver"]

# ./HttpReceiver --host 0.0.0.0 --mqttbroker emqtt 1883 --postgres postgres 5432 kl_grafana KlGr4f4n4 trmpln