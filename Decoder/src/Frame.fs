module Trmpln.Frame

open FSharp.Data
open Units

type TTNFrame = JsonProvider<""" {
    "app_id":"trmpln-test",
    "dev_id":"lopy-shawill",
    "hardware_serial":"70B3D5499A5101FA",
    "port":2,
    "counter":11,
    "payload_raw":"AQCzD+IA0CY=",
    "metadata": {
        "time":"2017-03-28T13:28:58.109690635Z",
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
            } ] } }""">

type DataV1 = {
    Battery: float<V>
    Temperature: float<C>
    Pressure: float<hPa>
}

type Frame<'data> = {
    Version: byte
    Data: 'data
    Meta: TTNFrame.Root
}
