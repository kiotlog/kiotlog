using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;

namespace KiotlogDB
{
    [Table("sensors")]
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

        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("meta", TypeName = "jsonb")]
        internal string _Meta { get; set; }

        [Required]
        [Column("fmt", TypeName = "jsonb")]
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

        [JsonIgnore]
        [Column("conversion_id")]
        public Guid? ConversionId { get; set; }

        [JsonIgnore]
        [Column("sensor_type_id")]
        public Guid? SensorTypeId { get; set; }

        [JsonIgnore]
        [Column("device_id")]
        public Guid DeviceId { get; set; }

        [ForeignKey("ConversionId")]
        [InverseProperty("Sensors")]
        public Conversions Conversion { get; set; }

        [ForeignKey("DeviceId")]
        [InverseProperty("Sensors")]
        public Devices Device { get; set; }

        [ForeignKey("SensorTypeId")]
        [InverseProperty("Sensors")]
        public SensorTypes SensorType { get; set; }
    }
}
