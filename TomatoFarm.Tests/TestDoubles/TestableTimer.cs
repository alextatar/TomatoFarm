using System;
using System.Linq;
using TomatoFarm.DataElements;

namespace TomatoFarm.Tests.TestDoubles
{
    public class TestableTimer : ITimer
    {
        private Action _warnAction;
        private Action _dueAction;

        public TimeSpan WarnTime { get; private set; }
        public TimeSpan DueTime { get; private set; }
        public bool WasStoped { get; private set; }

        public void Start(params TimedAction[] actions)
        {
            if(actions.Length != 2)
                throw new Exception("Unexpected number of actions");

            var warn = actions.First();
            var due = actions.Last();

            _warnAction = warn.Action;
            WarnTime = warn.DueIn;

            _dueAction = due.Action;
            DueTime = due.DueIn;
        }

        public void Stop()
        {
            WasStoped = true;
        }

        public void RaiseWarnElapsed()
        {
            _warnAction();
        }

        public void RaiseDueTimeElapsed()
        {
            _dueAction();
        }
    }
}