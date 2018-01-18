/*
    Copyright (C) 2017 Giampaolo Mancini, Trampoline SRL.
    Copyright (C) 2017 Francesco Varano, Trampoline SRL.

    This file is part of Kiotlog.

    Kiotlog is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Kiotlog is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

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

        public class JsonBMeta
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Kind { get; set; }
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
            public bool Bitfields { get; set; }
        }

        [Column("id")]
        public Guid Id { get; set; }

        [Required(ErrorMessage="The Device field is required")]
        [Column("device")]
        public string Device { get; set; }

        [Required]
        [Column("meta", TypeName = "jsonb")]
        internal string _Meta { get; set; }

        [Required]
        [Column("auth", TypeName = "jsonb")]
        internal string _Auth { get; set; }

        [Required]
        [Column("frame", TypeName = "jsonb")]
        internal string _Frame { get; set; }

        [NotMapped]
        public JsonBMeta Meta
        {
            get { return _Meta == null ? null : JsonConvert.DeserializeObject<JsonBMeta>(_Meta); }
            set { _Meta = value == null ? null : JsonConvert.SerializeObject(value); }
        }

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
