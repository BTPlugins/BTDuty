using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTDuty.Modules
{
    public class PlayerTimeclock
    {
        public ulong playerID { get; set; }
        public string DutyName { get; set; }
        public string DutyGroup { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalTime { get; set; }
        public PlayerTimeclock()
        {

        }
        public PlayerTimeclock(ulong playerId, string dutyName, string dutyGroup, DateTime startDate, DateTime endDate, int totalTime)
        {
            AddTime(playerId, dutyName, dutyGroup, startDate, endDate, totalTime);
        }
        public void AddTime(ulong playerId, string dutyName, string dutyGroup, DateTime startDate, DateTime endDate, int totalTime)
        {
            playerID = playerId;
            DutyName = dutyName;
            DutyGroup = dutyGroup;
            StartDate = startDate;
            EndDate = endDate;
            TotalTime = totalTime;
        }
    }
}
