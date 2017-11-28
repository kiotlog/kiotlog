using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;

namespace KiotlogDB
{
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

        public Guid Id { get; set; }
        public string Device { get; set; }
        public string Meta { get; set; }

        internal string _Auth { get; set; }
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

        public ICollection<Points> Points { get; set; }
        public ICollection<Sensors> Sensors { get; set; }
    }
}
