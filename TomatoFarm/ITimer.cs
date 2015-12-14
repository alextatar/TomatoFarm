using TomatoFarm.DataElements;

namespace TomatoFarm
{
    public interface ITimer
    {
        void Start(params TimedAction[] actions);
        void Stop();
    }
}
