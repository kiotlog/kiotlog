using System;
using System.Collections.Generic;

namespace KiotlogDB
{
    public partial class Devices
    {
        public Devices()
        {
            Points = new HashSet<Points>();
            Sensors = new HashSet<Sensors>();
        }

        public Guid Id { get; set; }
        public string Device { get; set; }
        public string Meta { get; set; }
        public string Auth { get; set; }
        public string Frame { get; set; }

        public ICollection<Points> Points { get; set; }
        public ICollection<Sensors> Sensors { get; set; }
    }
}
