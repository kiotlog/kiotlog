using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;

namespace KiotlogDB
{
    [Table("devices")]
    public partial class Devices
    {
        public Devices()
        {
            Points = new HashSet<Points>();
            Sensors = new HashSet<Sensors>();
        }

        public class JsonBAuth
        {
            public BasicAuth Basic { get; set; }
            public class BasicAuth {
                public string Token { get; set; }
            }
            public KlsnAuth Klsn {get; set; }
            public class KlsnAuth {
                public string Key { get; set; }
            }
        }

        public class JsonBFrame
        {
            public bool Bigendian { get; set; }
            public string Bitfields { get; set; }
        }

        [Column("id")]
        public Guid Id { get; set; }

        [Required(ErrorMessage="The Device field is required")]
        [Column("device")]
        public string Device { get; set; }

        [Required]
        [Column("meta", TypeName = "jsonb")]
        public string Meta { get; set; }

        [Required]
        [Column("auth", TypeName = "jsonb")]
        internal string _Auth { get; set; }

        [Required]
        [Column("frame", TypeName = "jsonb")]
        internal string _Frame { get; set; }

        [NotMapped]
        public JsonBAuth Auth
        {
            get { return _Auth == null ? null : JsonConvert.DeserializeObject<JsonBAuth>(_Auth); }
            set { _Auth = value == null ? null : JsonConvert.SerializeObject(value); }
        }

        [NotMapped]
        public JsonBFrame Frame
        {
            get { return _Frame == null ? null : JsonConvert.DeserializeObject<JsonBFrame>(_Frame); }
            set { _Frame = value == null ? null : JsonConvert.SerializeObject(value); }
        }

        [InverseProperty("Device")]
        public ICollection<Points> Points { get; set; }
        
        [NotNullOrEmptyCollection(ErrorMessage="We need at leat one Sensor")]
        [InverseProperty("Device")]
        public ICollection<Sensors> Sensors { get; set; }

        public bool ShouldSerializePoints()
        {
            return Points != null && Points.Count > 0;
        }

        public bool ShouldSerializeSensors()
        {
            return Sensors != null && Sensors.Count > 0;
        }
    }
}
