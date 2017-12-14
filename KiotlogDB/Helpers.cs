using System;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace KiotlogDB
{
    public class Helpers
    {
        public static async Task<Devices> loadDeviceAsync (KiotlogDBContext context,  Guid id) {
            return await context.Devices
                                .Include(x => x.Sensors)
                                    .ThenInclude(x => x.SensorType)
                                .SingleOrDefaultAsync(x => x.Id.Equals(id));
        }
    }
}