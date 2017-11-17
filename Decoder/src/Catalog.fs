namespace Decoder

open Microsoft.EntityFrameworkCore

open KiotlogDB

module Catalog =

    let getFormatString cs deviceDevice =
        
        use ctx = new KiotlogDBContext(cs)

        let device =
            let devices =
                ctx.Devices
                    .Include("Sensors")
                    .Include("Sensors.SensorType")
                    .Include("Sensors.Conversion")

            query {
                for d in devices do
                where (d.Device = deviceDevice)
                select d
                exactlyOne
            }    

        let endianness = if device.Frame.Bigendian then ">" else "<"

        let fmtString =
            device.Sensors
            |> Seq.sortBy (fun sensor -> sensor.Fmt.Index)
            |> Seq.map (fun sensor -> sensor.Fmt.FmtChr)
            |> Seq.reduce (+)
        
        endianness + fmtString