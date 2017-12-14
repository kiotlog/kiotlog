using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;

namespace KiotlogDB
{
    [Table("sensor_types")]
    public partial class SensorTypes
    {
        public class JsonBMeta
        {
            public int Max { get; set;}
            public int Min { get; set;}
        }
        public SensorTypes()
        {
            Sensors = new HashSet<Sensors>();
        }

        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; }

        [Column("meta", TypeName = "jsonb")]
        internal string _Meta { get; set; }

        [NotMapped]
        public JsonBMeta Meta
        {
            get { return _Meta == null ? null : JsonConvert.DeserializeObject<JsonBMeta>(_Meta, JsonSettings.snakeSettings); }
            set { _Meta = value == null ? null : JsonConvert.SerializeObject(value, JsonSettings.snakeSettings); }
        }
        
        [Required]
        [Column("type")]
        public string Type { get; set; }

        [InverseProperty("SensorType")]
        public ICollection<Sensors> Sensors { get; set; }
    }
}
