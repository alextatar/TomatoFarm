using System;
using TomatoFarm.DataElements;

namespace TomatoFarm
{
    internal class ShortBreak : Break
    {
        public override TimeSpan WarnTime => Settings.BreakDueWarning;
        public override TimeSpan DueTime => Settings.BreakSize;

        public ShortBreak(IMessenger messenger, TomatoFarmSettings settings) : base(messenger, settings)
        {
        }

        public override void Warn()
        {
            Messenger.BreakDueTimeWarning();
        }

        public override void Finish()
        {
            Messenger.BreakFinished();
        }
    }
}