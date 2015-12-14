using System;
using TomatoFarm.DataElements;

namespace TomatoFarm
{
    internal abstract class Break
    {
        protected readonly IMessenger Messenger;
        protected readonly TomatoFarmSettings Settings;

        protected Break(IMessenger messenger, TomatoFarmSettings settings)
        {
            Settings = settings;
            Messenger = messenger;
        }

        public abstract void Warn();
        public abstract void Finish();
        public abstract TimeSpan WarnTime { get; }
        public abstract TimeSpan DueTime { get; }
    }
}