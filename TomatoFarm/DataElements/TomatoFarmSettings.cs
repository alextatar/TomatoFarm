using System;

namespace TomatoFarm.DataElements
{
    public class TomatoFarmSettings
    {
        public TimeSpan RegularHours { get; set; }

        public TimeSpan TomatoSize { get; set; }
        public TimeSpan TomatoDueWarning { get; set; }

        public TimeSpan BreakDueWarning { get; set; }
        public TimeSpan BreakSize { get; set; }

        public TimeSpan LongBreakDueWarning { get; set; }
        public TimeSpan LongBreakSize { get; set; }
    }
}