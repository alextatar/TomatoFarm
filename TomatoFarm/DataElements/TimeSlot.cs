using System;
using System.Diagnostics.Contracts;

namespace TomatoFarm.DataElements
{
    public class TimeSlot
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public TimeSlotType Type { get; set; }
        public bool IsInProgress { get; set; }
    }
}