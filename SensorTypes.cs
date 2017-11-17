using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;

namespace KiotlogDB
{
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

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string _Meta { get; set; }

        [NotMapped]
        public JsonBMeta Meta
        {
            get { return _Meta == null ? null : JsonConvert.DeserializeObject<JsonBMeta>(_Meta, JsonSettings.snakeSettings); }
            set { _Meta = value == null ? null : JsonConvert.SerializeObject(value, JsonSettings.snakeSettings); }
        }
        
        public string Type { get; set; }

        public ICollection<Sensors> Sensors { get; set; }
    }
}
