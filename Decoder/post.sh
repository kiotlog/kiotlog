#!/usr/bin/env bash

set -ef

CHANNEL=${1:-sigfox}
APPLICATION=${2:-59021d713c878907da9ebf3e}
DEVICE=${3:-18B8D6}

curl -X POST "http://localhost:8080/${CHANNEL}/${APPLICATION}/devices/${DEVICE}/up" \
    --user 18B8D6:fb2e2658-c6cd-498d-91e6-37ad92bbe89b \
    -H "Content-type: application/json" \
    -d @- <<JSONDATA
{
    "app_id": "app1234",
    "dev_id": "18B8D6",
    "payload_raw": "01c3112671771700",
    "metadata": {
        "time": $(date +%s)
    }
}
JSONDATA