using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomatoFarm.DataElements;

namespace TomatoFarm.Utils
{
    public class TomatoTimer : ITimer, IDisposable
    {
        private CancellationTokenSource _cancellationToken;
        private Stopwatch _stopwatch; 

        public TimeSpan Elapsed { get { return _stopwatch.Elapsed; } }

        public TomatoTimer()
        {
            _stopwatch = new Stopwatch();
        }

        public void Start(params TimedAction[] actions)
        {
            _cancellationToken = new CancellationTokenSource();
            _stopwatch.Restart();
            var longestAction = actions.Max(p => p.DueIn);
            foreach (var timedAction in actions)
            {
                Task.Delay(timedAction.DueIn, _cancellationToken.Token)
                    .ContinueWith((t, s) => {
                        if (timedAction.DueIn == longestAction)
                            _stopwatch.Stop();
                        timedAction.Action.Invoke();
                    }, null, CancellationToken.None,
                        TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
                        TaskScheduler.Default);
            }

        }

        public void Stop()
        {
            _stopwatch.Stop();
            _cancellationToken.Cancel();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _cancellationToken.Cancel();
        }
    }
}