using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomatoFarm.DataElements;

namespace TomatoFarm.Utils
{
    public class TomatoTimer : ITimer, IDisposable
    {
        private CancellationTokenSource _cancellationToken;

        public void Start(params TimedAction[] actions)
        {
            _cancellationToken = new CancellationTokenSource();
            foreach (var timedAction in actions)
            {
                Task.Delay(timedAction.DueIn, _cancellationToken.Token)
                    .ContinueWith((t, s) => timedAction.Action.Invoke(), null, CancellationToken.None,
                        TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
                        TaskScheduler.Default);
            }

        }

        public void Stop()
        {
            _cancellationToken.Cancel();
        }

        public void Dispose()
        {
            _cancellationToken.Cancel();
        }
    }
}