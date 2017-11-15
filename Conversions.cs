using System;
using System.Collections.Generic;

namespace KiotlogDB
{
    public partial class Conversions
    {
        public Conversions()
        {
            Sensors = new HashSet<Sensors>();
        }

        public Guid Id { get; set; }
        public string Fun { get; set; }

        public ICollection<Sensors> Sensors { get; set; }
    }
}
