using System;
using System.Collections.Generic;

namespace KiotlogDB
{
    public partial class Points
    {
        public Guid Id { get; set; }
        public string DeviceDevice { get; set; }
        public DateTime Time { get; set; }
        public string Flags { get; set; }
        public string Data { get; set; }

        public Devices DeviceDeviceNavigation { get; set; }
    }
}
