using System;
using System.Collections.Generic;

namespace TomatoFarm.DataElements
{
    public class WorkSummary
    {
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan RegularHoursEndTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public IList<TimeSlot> TimeSlots { get; set; }
    }
}