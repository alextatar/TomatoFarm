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
            _timer.Start(new TimedAction { Action = () => ExecuteAction(), DueIn = TimeSpan.FromSeconds(2) });
            Thread.Sleep(TimeSpan.FromSeconds(3.2));

            _wasExecuted.Should().BeTrue();
            _times.ShouldBeEquivalentTo(1);
        }

        [Test]
        public void EachActionIsExecutedWhenCorespondingTimeElapses()
        {
            _timer.Start(
                new TimedAction { Action = () => ExecuteAction(), DueIn = TimeSpan.FromSeconds(2) },
                new TimedAction { Action = () => ExecuteAction(), DueIn = TimeSpan.FromSeconds(4) });
            Thread.Sleep(TimeSpan.FromSeconds(5));

            _wasExecuted.Should().BeTrue();
            _times.ShouldBeEquivalentTo(2);
        }

        [Test]
        public void GivenThatTimerIsStoppedNoActionExecuted()
        {
            _timer.Start(new TimedAction { Action = () => ExecuteAction(), DueIn = TimeSpan.FromSeconds(2) },
                new TimedAction { Action = () => ExecuteAction(), DueIn = TimeSpan.FromSeconds(4) });
            Thread.Sleep(TimeSpan.FromSeconds(1));
            _timer.Stop();
            Thread.Sleep(TimeSpan.FromSeconds(5));

            _wasExecuted.Should().BeFalse();
            _times.ShouldBeEquivalentTo(0);
        }


        [Test]
        public void DisposingTimerCancelsAlTasks()
        {
            _timer.Start(new TimedAction { Action = () => ExecuteAction(), DueIn = TimeSpan.FromSeconds(2) },
                new TimedAction { Action = () => ExecuteAction(), DueIn = TimeSpan.FromSeconds(4) });
            Thread.Sleep(TimeSpan.FromSeconds(1));
            _timer.Dispose();
            Thread.Sleep(TimeSpan.FromSeconds(5));

            _wasExecuted.Should().BeFalse();
            _times.ShouldBeEquivalentTo(0);
        }

        private void ExecuteAction()
        {
            _wasExecuted = true;
            _times++;
        }
    }
}
