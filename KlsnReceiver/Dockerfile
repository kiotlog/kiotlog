FROM microsoft/dotnet:2.0-sdk AS build-env
WORKDIR /Sln

#######################################################
# WARINING!
# Remeber to set PROJECT in runtime image section, too.
ARG PROJECT=KlsnReceiver
#######################################################

COPY . .
RUN dotnet restore

# RUN dotnet publish -c Release -r linux-x64 -o out /p:ShowLinkerSizeComparison=true
WORKDIR /Sln/${PROJECT}
RUN dotnet publish -c Release -r linux-x64 -o out /p:LinkDuringPublish=false /p:ShowLinkerSizeComparison=true

# build runtime image
FROM debian:stretch-slim

#######################################################
# WARINING!
# Set PROJECT here, too.
ARG PROJECT=KlsnReceiver
#######################################################

WORKDIR /App

RUN set -ef \
    && apt update \
    && apt install -y --no-install-recommends \
        libicu57 libcurl3 libunwind8 libssl1.0 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build-env /Sln/${PROJECT}/out ./

ENV LC_ALL en_US.UTF-8
ENV LANG en_US.UTF-8

ENTRYPOINT ["/App/KlsnReceiver"]