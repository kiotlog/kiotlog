using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;

namespace KiotlogDB
{
    
    public partial class Sensors
    {
        public class JsonBMeta
        {
            public string Name { get; set; }
        }

        public class JsonBFmt
        {
            public int Index { get; set; }
            public string FmtChr { get; set; }
        }

        public Guid Id { get; set; }

        internal string _Meta { get; set; }
        internal string _Fmt { get; set; }

        [NotMapped]
        public JsonBMeta Meta
        {
            get { return _Meta == null ? null : JsonConvert.DeserializeObject<JsonBMeta>(_Meta, JsonSettings.snakeSettings); }
            set { _Meta = value == null ? null : JsonConvert.SerializeObject(value, JsonSettings.snakeSettings); }
        }

        [NotMapped]
        public JsonBFmt Fmt
        {
            get { return _Fmt == null ? null : JsonConvert.DeserializeObject<JsonBFmt>(_Fmt, JsonSettings.snakeSettings); }
            set { _Fmt = value == null ? null : JsonConvert.SerializeObject(value, JsonSettings.snakeSettings); }
        }

        public Guid? ConversionId { get; set; }
        public Guid? SensorTypeId { get; set; }
        public Guid DeviceId { get; set; }

        public Conversions Conversion { get; set; }
        public Devices Device { get; set; }
        public SensorTypes SensorType { get; set; }
    }
}
