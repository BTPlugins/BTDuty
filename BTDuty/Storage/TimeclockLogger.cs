using BTDuty.Modules;
using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTDuty.Storage
{
    public class TimeclockLogger : IDefaultable
    {
        public List<PlayerTimeclock> TimeClock { get; set; }

        public TimeclockLogger()
        {

        }
        public void LoadDefaults()
        {
            TimeClock = new List<PlayerTimeclock>();
        }
        public void AddNewTime(ulong playerId, string dutyName, string dutyGroup, DateTime startDate, DateTime endDate, int totalTime)
        {
            TimeClock.Add(new PlayerTimeclock(playerId, dutyName, dutyGroup, startDate, endDate, totalTime));
        }
    }
}
