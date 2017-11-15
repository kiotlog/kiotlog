using System;
using System.Collections.Generic;

namespace KiotlogDB
{
    public partial class Sensors
    {
        public Guid Id { get; set; }
        public string Meta { get; set; }
        public string Fmt { get; set; }
        public Guid? ConversionId { get; set; }
        public Guid? SensorTypeId { get; set; }
        public Guid DeviceId { get; set; }

        public Conversions Conversion { get; set; }
        public Devices Device { get; set; }
        public SensorTypes SensorType { get; set; }
    }
}
