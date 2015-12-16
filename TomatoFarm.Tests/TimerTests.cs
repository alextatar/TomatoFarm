using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using TomatoFarm.DataElements;
using TomatoFarm.Utils;

namespace TomatoFarm.Tests
{
    [TestFixture]
    //[Ignore("Long running! Should be moved to an IntegrationTest Assembly")]
    public class TimerTests
    {
        private TomatoTimer _timer;

        private bool _wasExecuted;
        private int _times;

        private void CheckElapsedTime(TimeSpan dueIn)
        {
            Math.Round(_timer.Elapsed.TotalSeconds).ShouldBeEquivalentTo(dueIn.TotalSeconds);
        }

        [SetUp]
        public void Initialize()
        {
            _wasExecuted = false;
            _times = 0;
            _timer = new TomatoTimer();
        }

        [Test]
        public void OneActionIsExecutedWhenCorespondingTimeElapses()
        {
            var dueIn = TimeSpan.FromSeconds(2);
            _timer.Start(new TimedAction { Action = () => ExecuteAction(), DueIn = dueIn });
            Thread.Sleep(TimeSpan.FromSeconds(3.2));

            _wasExecuted.Should().BeTrue();
            _times.ShouldBeEquivalentTo(1);
            CheckElapsedTime(dueIn);
        }

        [Test]
        public void EachActionIsExecutedWhenCorespondingTimeElapses()
        {
            var dueIn = TimeSpan.FromSeconds(4);
            _timer.Start(
                new TimedAction { Action = () => ExecuteAction(), DueIn = TimeSpan.FromSeconds(2) },
                new TimedAction { Action = () => ExecuteAction(), DueIn = dueIn });
            Thread.Sleep(TimeSpan.FromSeconds(5));

            _wasExecuted.Should().BeTrue();
            _times.ShouldBeEquivalentTo(2);
            CheckElapsedTime(dueIn);
        }

        [Test]
        public void GivenThatTimerIsStoppedNoActionExecuted()
        {
            var stopAfter = TimeSpan.FromSeconds(1);

            _timer.Start(new TimedAction { Action = () => ExecuteAction(), DueIn = TimeSpan.FromSeconds(2) },
                new TimedAction { Action = () => ExecuteAction(), DueIn = TimeSpan.FromSeconds(4) });
            Thread.Sleep(stopAfter);
            _timer.Stop();
            Thread.Sleep(TimeSpan.FromSeconds(5));

            _wasExecuted.Should().BeFalse();
            _times.ShouldBeEquivalentTo(0);
            CheckElapsedTime(stopAfter);
        }


        [Test]
        public void DisposingTimerCancelsAlTasks()
        {
            var stopAfter = TimeSpan.FromSeconds(1);

            _timer.Start(new TimedAction { Action = () => ExecuteAction(), DueIn = TimeSpan.FromSeconds(2) },
                new TimedAction { Action = () => ExecuteAction(), DueIn = TimeSpan.FromSeconds(4) });
            Thread.Sleep(stopAfter);
            _timer.Dispose();
            Thread.Sleep(TimeSpan.FromSeconds(5));

            _wasExecuted.Should().BeFalse();
            _times.ShouldBeEquivalentTo(0);
            CheckElapsedTime(stopAfter);
        }

        [Test]
        public void StopWatchIsResetEveryWithEachStart()
        {
            var firstDueIn = TimeSpan.FromSeconds(2);
            var secondDuedIn = TimeSpan.FromSeconds(4);

            _timer.Start(new TimedAction { Action = () => ExecuteAction(), DueIn = firstDueIn });
            Thread.Sleep(firstDueIn);
            Thread.Sleep(TimeSpan.FromSeconds(0.5));
            _timer.Start(new TimedAction { Action = () => ExecuteAction(), DueIn = secondDuedIn });
            Thread.Sleep(secondDuedIn);
            Thread.Sleep(TimeSpan.FromSeconds(0.5));

            CheckElapsedTime(secondDuedIn);
        }

        private void ExecuteAction()
        {
            _wasExecuted = true;
            _times++;
        }
    }
}
