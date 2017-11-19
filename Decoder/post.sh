#!/usr/bin/env bash

set -ef

URL=${1:-"localhost:8080"}
CHANNEL=${2:-sigfox}
APPLICATION=${3:-59021d713c878907da9ebf3e}
DEVICE=${4:-18B8D6}

curl -X POST "http://${URL}/${CHANNEL}/${APPLICATION}/devices/${DEVICE}/up" \
    --user 18B8D6:fb2e2658-c6cd-498d-91e6-37ad92bbe89b \
    -H "Content-type: application/json" \
    -d @- <<JSONDATA
{
    "app_id": "app1234",
    "dev_id": "18B8D6",
    "payload_raw": "01c3112671771700",
    "metadata": {
        "time": $(date +%s),
        "frequency":867.5,
        "modulation":"LORA",
        "data_rate":"SF7BW125",
        "coding_rate":"4/5",
        "gateways": [ {
                "gtw_id":"eui-b827ebfffecc08ee",
                "timestamp":148308084,
                "time":"2017-03-28T13:28:57.970053Z",
                "channel":5,
                "rssi":-3,
                "snr":7.2,
                "latitude":45.050232,
                "longitude":7.668766
            }
        ]
    }
}
JSONDATA