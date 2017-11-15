using System;
using System.Collections.Generic;

namespace KiotlogDB
{
    public partial class SensorTypes
    {
        public SensorTypes()
        {
            Sensors = new HashSet<Sensors>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Meta { get; set; }
        public string Type { get; set; }

        public ICollection<Sensors> Sensors { get; set; }
    }
}
