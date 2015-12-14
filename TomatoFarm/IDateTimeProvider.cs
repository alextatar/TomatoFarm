using System;

namespace TomatoFarm
{
    public interface IDateTimeProvider
    {
        DateTime GetCurrentDate();
        TimeSpan GetCurrentTime();
    }
}