using System;

namespace TomatoFarm.DataElements
{
    public class TimedAction
    {
        public Action Action { get; set; }
        public TimeSpan DueIn { get; set; }
    }
}