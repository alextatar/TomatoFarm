using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TomatoFarm.Utils
{
    public class CurrentDateTimeProvider : IDateTimeProvider 
    {
        public DateTime GetCurrentDate()
        {
            return DateTime.Now;
        }

        public TimeSpan GetCurrentTime()
        {
            return DateTime.Now.TimeOfDay;
        }
    }
}
