using System;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using TomatoFarm.DataElements;
using TomatoFarm.Tests.TestDoubles;

namespace TomatoFarm.Tests
{
    [TestFixture]
    public class StandardTomatoFarmAppTests : IDateTimeProvider, IMessenger
    {
        private DateTime _currentDate = new DateTime(2015, 1, 7, 8, 0, 0);
        private bool _wasTomatoWarningFired;
        private bool _wasTomatoDueFired;
        private bool _wasBreakWarningFired;
        private bool _wasBreakDueFired;
        private bool _wasLongBreakWarningFired;
        private bool _wasLongBreakDueFired;
        private TomatoFarmSettings _appSettings;

        private StandardTomatoFarmApp _app;
        private TestableTimer _timer;

        [SetUp]
        public void Initialize()
        {
            _wasTomatoWarningFired = false;
            _wasTomatoDueFired = false;
            _wasBreakWarningFired = false;
            _wasBreakDueFired = false;
            _wasLongBreakWarningFired = false;
            _wasLongBreakDueFired = false;
            _appSettings = new TomatoFarmSettings
            {
                RegularHours = TimeSpan.FromHours(2),
                TomatoDueWarning = TimeSpan.FromMinutes(29),
                TomatoSize = TimeSpan.FromMinutes(30),
                BreakDueWarning = TimeSpan.FromMinutes(4.5),
                BreakSize = TimeSpan.FromMinutes(5),
                LongBreakDueWarning = TimeSpan.FromMinutes(14)
            };
            _timer = new TestableTimer();
            _app = new StandardTomatoFarmApp(_appSettings) { DateTimeProvider = this, Timer = _timer, Messenger = this };
        }

        [Test]
        public void StartWorkTest()
        {
            var expectedRegularHoursEndTime = _currentDate.TimeOfDay + _appSettings.RegularHours;

            _app.StartWork();
            var summary = _app.GetSummary();

            summary.Date.ShouldBeEquivalentTo(_currentDate.Date);
            summary.StartTime.ShouldBeEquivalentTo(_currentDate.TimeOfDay);
            summary.RegularHoursEndTime.ShouldBeEquivalentTo(expectedRegularHoursEndTime);
            var tomato = summary.TimeSlots.Single();
            tomato.Type.ShouldBeEquivalentTo(TimeSlotType.Tomato);
            tomato.StartTime.ShouldBeEquivalentTo(_currentDate.TimeOfDay);
        }

        [Test]
        public void WarnWhenInProgressTomatoApproachesDueTime()
        {
            _app.StartWork();
            _timer.RaiseWarnElapsed();

            _wasTomatoWarningFired.Should().BeTrue();
            _timer.WarnTime.ShouldBeEquivalentTo(_appSettings.TomatoDueWarning);
        }

        [Test]
        public void StartBreakWhenTomatoIsDue()
        {
            _currentDate += _appSettings.TomatoSize;

            _app.StartWork();
            var dueTime = _timer.DueTime;
            _timer.RaiseDueTimeElapsed();
            var summary = _app.GetSummary();

            summary.TimeSlots.Should().HaveCount(2);
            var tomato = summary.TimeSlots.First();
            tomato.EndTime.ShouldBeEquivalentTo(_currentDate.TimeOfDay);
            dueTime.ShouldBeEquivalentTo(_appSettings.TomatoSize);
            _wasTomatoDueFired.Should().BeTrue();
        }

        [Test]
        public void WarnWhenInProgressBreakApproachesDueTime()
        {
            _app.StartWork();
            _timer.RaiseDueTimeElapsed();
            _timer.RaiseWarnElapsed();

            _timer.WarnTime.ShouldBeEquivalentTo(_appSettings.BreakDueWarning);
            _wasTomatoWarningFired.Should().BeFalse();
            _wasTomatoDueFired.Should().BeTrue();
            _wasBreakWarningFired.Should().BeTrue();
        }

        [Test]
        public void WarnAndPauseWhenInProgressBreakIsDue()
        {
            _currentDate += _appSettings.TomatoSize + _appSettings.BreakSize;

            _app.StartWork();
            _timer.RaiseDueTimeElapsed();
            _wasTomatoDueFired = false;
            _timer.RaiseDueTimeElapsed();
            var summary = _app.GetSummary();

            _wasTomatoDueFired.Should().BeFalse();
            _wasBreakDueFired.Should().BeTrue();
            _timer.DueTime.ShouldBeEquivalentTo(_appSettings.BreakSize);
            _timer.WasStoped.Should().BeTrue();
            summary.TimeSlots.Should().HaveCount(2);
            var @break = summary.TimeSlots.Last();
            @break.IsInProgress.Should().BeFalse();
            @break.EndTime.ShouldBeEquivalentTo(_currentDate.TimeOfDay);
        }

        [Test]
        public void CanStartANewTomatoWhileInBreak()
        {
            _currentDate += _appSettings.TomatoSize + _appSettings.BreakSize;

            BuildTomatoAndBreakPairs(1);
            _app.StartNewTomato();
            var summary = _app.GetSummary();

            _timer.WasStoped.Should().BeTrue();
            summary.TimeSlots.Should().HaveCount(3);
            var tomato = summary.TimeSlots.Last();
            tomato.StartTime.ShouldBeEquivalentTo(_currentDate.TimeOfDay);
        }

        [Test]
        public void ANewBreakIsStartedWhenANewTomatoIsDue()
        {
             _currentDate += _appSettings.TomatoSize + _appSettings.BreakSize + _appSettings.TomatoSize;

            BuildTomatoAndBreakPairs(1);
            _app.StartNewTomato();
            var warnTime = _timer.WarnTime;
            var dueTime = _timer.DueTime;
            _wasTomatoDueFired = false;
            _timer.RaiseWarnElapsed();
            _timer.RaiseDueTimeElapsed();
            var summary = _app.GetSummary();

            _wasTomatoWarningFired.Should().BeTrue();
            warnTime.ShouldBeEquivalentTo(_appSettings.TomatoDueWarning);
            _wasTomatoDueFired.Should().BeTrue();
            dueTime.ShouldBeEquivalentTo(_appSettings.TomatoSize);
            summary.TimeSlots.Should().HaveCount(4);
            summary.TimeSlots.Last().StartTime.ShouldBeEquivalentTo(_currentDate.TimeOfDay);
        }

        [Test]
        public void CanStartBreakBeforeTomatoIsDue()
        {
            _currentDate += TimeSpan.FromMinutes(20);

            _app.StartWork();
            _app.StartBreak();
            _timer.RaiseWarnElapsed();
            _timer.RaiseDueTimeElapsed();
            var summary = _app.GetSummary();

            _timer.WasStoped.Should().BeTrue();
            _wasBreakWarningFired.Should().BeTrue();
            _wasBreakDueFired.Should().BeTrue();
            _timer.WarnTime.ShouldBeEquivalentTo(_appSettings.BreakDueWarning);
            _timer.DueTime.ShouldBeEquivalentTo(_appSettings.BreakSize);
            summary.TimeSlots.Should().HaveCount(2);
            summary.TimeSlots.Last().StartTime.ShouldBeEquivalentTo(_currentDate.TimeOfDay);
        }

        [Test]
        public void ASecondStartBreakRequestIsIgnored()
        {
            _app.StartWork();
            _app.StartBreak();
            _app.StartBreak();
            var summary = _app.GetSummary();

            summary.TimeSlots.Should().HaveCount(2);
            summary.TimeSlots.First().Type.ShouldBeEquivalentTo(TimeSlotType.Tomato);
            summary.TimeSlots.Last().Type.ShouldBeEquivalentTo(TimeSlotType.Break);
        }

        [Test]
        public void CanEndWorkDay()
        {
            _currentDate += TimeSpan.FromMinutes(10);

            _app.StartWork();
            _app.EndWork();
            var summary = _app.GetSummary();

            _timer.WasStoped.Should().BeTrue();
            summary.EndTime.ShouldBeEquivalentTo(_currentDate.TimeOfDay);
            summary.TimeSlots.Should().HaveCount(1);
            summary.TimeSlots.First().EndTime.ShouldBeEquivalentTo(_currentDate.TimeOfDay);
        }

        [Test]
        public void CanPauseWork()
        {
            _currentDate += TimeSpan.FromMinutes(10);

            _app.StartWork();
            _app.PauseWork();
            var summary = _app.GetSummary();

            _timer.WasStoped.Should().BeTrue();
            summary.EndTime.ShouldBeEquivalentTo(null);
            summary.TimeSlots.Should().HaveCount(1);
            summary.TimeSlots.First().EndTime.ShouldBeEquivalentTo(_currentDate.TimeOfDay);
        }

        [Test]
        public void ALongBreakIsStartedWhen4TomatoesAreDone()
        {
            BuildTomatoAndBreakPairs(4);
            var summary = _app.GetSummary();

            summary.TimeSlots.Should().HaveCount(8);
            summary.TimeSlots.Last().Type.ShouldBeEquivalentTo(TimeSlotType.LongBreak);
        }

        [Test]
        public void ALongBreakIsStartedEachTimeANewRoundIsFinished()
        {
            BuildTomatoAndBreakPairs(8);
            var summary = _app.GetSummary();

            summary.TimeSlots.Should().HaveCount(16);
            summary.TimeSlots[7].Type.ShouldBeEquivalentTo(TimeSlotType.LongBreak);
            summary.TimeSlots.Last().Type.ShouldBeEquivalentTo(TimeSlotType.LongBreak);
        }

        [Test]
        public void WarnWhenInProgressLongBreakApproachesDueTime()
        {
            BuildTomatoAndBreakPairs(4);
            _timer.RaiseWarnElapsed();

            _timer.WarnTime.ShouldBeEquivalentTo(_appSettings.LongBreakDueWarning);
            _wasLongBreakWarningFired.Should().BeTrue();
        }

        [Test]
        public void WarnAndPauseWhenInProgressLongBreakIsDue()
        {
            var expectedEndTime = CalculateTimeForOneRoundPlusOneLongBreak();

            BuildTomatoAndBreakPairs(4);
            _timer.RaiseDueTimeElapsed();
            var summary = _app.GetSummary();

            _wasLongBreakDueFired.Should().BeTrue();
            _timer.DueTime.ShouldBeEquivalentTo(_appSettings.LongBreakSize);
            _timer.WasStoped.Should().BeTrue();
            var @break = summary.TimeSlots.Last();
            @break.IsInProgress.Should().BeFalse();
            @break.EndTime.ShouldBeEquivalentTo(expectedEndTime);
        }

        private TimeSpan CalculateTimeForOneRoundPlusOneLongBreak()
        {
            for (var i = 0; i < 4; i++) _currentDate += _appSettings.TomatoSize;
            for (var i = 0; i < 3; i++) _currentDate += _appSettings.BreakSize;
            _currentDate += _appSettings.LongBreakSize;
            return _currentDate.TimeOfDay;
        }

        private void BuildTomatoAndBreakPairs(int count)
        {
            _app.StartWork();
            RaiseDueTimeForOneTomatoAndBreak();

            for (var i = 0; i < count - 1; i++)
            {
                _app.StartNewTomato();
                RaiseDueTimeForOneTomatoAndBreak();
            }
        }

        private void RaiseDueTimeForOneTomatoAndBreak()
        {
            _timer.RaiseDueTimeElapsed();
            _timer.RaiseDueTimeElapsed();
        }

        public DateTime GetCurrentDate()
        {
            return _currentDate;
        }

        public TimeSpan GetCurrentTime()
        {
            return _currentDate.TimeOfDay;
        }

        public void TomatoDueTimeWarning()
        {
            _wasTomatoWarningFired = true;
        }

        public void TomatoFinished()
        {
            _wasTomatoDueFired = true;
        }

        public void BreakDueTimeWarning()
        {
            _wasBreakWarningFired = true;
        }

        public void BreakFinished()
        {
            _wasBreakDueFired = true;
        }

        public void LongBreakDueTimeWarning()
        {
            _wasLongBreakWarningFired = true;
        }

        public void LongBreakFinished()
        {
            _wasLongBreakDueFired = true;
        }
    }
}
