namespace TomatoFarm
{
    public interface IMessenger
    {
        void TomatoDueTimeWarning();
        void TomatoFinished();
        void BreakDueTimeWarning();
        void BreakFinished();
        void LongBreakDueTimeWarning();
        void LongBreakFinished();
    }
}