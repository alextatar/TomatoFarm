using System;
using TomatoFarm.DataElements;

namespace TomatoFarm
{
    internal class LongBreak : Break
    {
        public override TimeSpan WarnTime => Settings.LongBreakDueWarning;
        public override TimeSpan DueTime => Settings.LongBreakSize;

        public LongBreak(IMessenger messenger, TomatoFarmSettings settings) : base(messenger, settings)
        {
        }

        public override void Warn()
        {
            Messenger.LongBreakDueTimeWarning();
        }

        public override void Finish()
        {
            Messenger.LongBreakFinished();
        }
    }
}