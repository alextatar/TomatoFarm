using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using TomatoFarm.DataElements;

namespace TomatoFarm.Tests
{
    [TestFixture]
    public class WorkDayTest
    {
        private DateTime _startTime;
        private WorkDay _workday;
        private readonly TimeSpan _regularHours = TimeSpan.FromHours(8);

        private static void CheckFinishedTimeSlot(TimeSlot timeSlot, TimeSpan breakStartTime)
        {
            timeSlot.IsInProgress.Should().BeFalse();
            timeSlot.EndTime.ShouldBeEquivalentTo(breakStartTime);
        }

        [SetUp]
        public void Initialize()
        {
            _startTime = DateTime.Today.AddHours(8);
            _workday = WorkDay.MakeNewDay(_startTime, _regularHours);
        }

        [Test]
        public void StartAndEndTimeAreRecorded()
        {
            var expectedRegularHoursEndTime = _startTime.TimeOfDay + _regularHours;
            var stopTime = _startTime.AddHours(1);
            _workday.FinishDay(stopTime.TimeOfDay);

            var result = _workday.GetSummary();
            
            result.Should().NotBeNull();
            result.Date.ShouldBeEquivalentTo(_startTime.Date);
            result.StartTime.ShouldBeEquivalentTo(_startTime.TimeOfDay);
            result.EndTime.ShouldBeEquivalentTo(stopTime.TimeOfDay);
            result.RegularHoursEndTime.ShouldBeEquivalentTo(expectedRegularHoursEndTime);
        }

        [Test]
        [TestCase(TimeSlotType.Tomato)]
        [TestCase(TimeSlotType.Break)]
        [TestCase(TimeSlotType.LongBreak)]
        public void AddOneTimeSlot(TimeSlotType timeSlotType)
        {
            _workday.StartTimeSlot(_startTime.TimeOfDay, timeSlotType);
            var result = _workday.GetSummary();

            var timeSlot = result.TimeSlots.Single();
            timeSlot.StartTime.ShouldBeEquivalentTo(_startTime.TimeOfDay);
            timeSlot.IsInProgress.Should().BeTrue();
            timeSlot.Type.ShouldBeEquivalentTo(timeSlotType);
        }

        [Test]
        [TestCase(TimeSlotType.Tomato)]
        [TestCase(TimeSlotType.Break)]
        [TestCase(TimeSlotType.LongBreak)]
        public void AddingTimeSlotWithInvalidStartTimeThrowsException(TimeSlotType timeSlotType)
        {
            var start = DateTime.Today.AddHours(1);
            var workday = WorkDay.MakeNewDay(start, _regularHours);

            Action<WorkDay> action = p => p.StartTimeSlot(TimeSpan.Zero, timeSlotType);

            workday.Invoking(action).ShouldThrow<WorkDay.InvalidStartTimeException>();
        }

        [Test]
        public void AddBreakAfterTomato()
        {
            var breakStartTime = _startTime.AddMinutes(30).TimeOfDay;

            _workday.StartTimeSlot(_startTime.TimeOfDay, TimeSlotType.Tomato);
            _workday.StartTimeSlot(breakStartTime, TimeSlotType.Break);
            var result = _workday.GetSummary();

            result.TimeSlots.Should().HaveCount(2);
            CheckFinishedTimeSlot(result.TimeSlots.Last(), breakStartTime);
            result.TimeSlots.First().IsInProgress.Should().BeTrue();
        }

        [Test]
        public void TwoTomatoesWithABreakInBetween()
        {
            var breakStartTime = _startTime.AddMinutes(30).TimeOfDay;
            var secondTomatoStartTime = breakStartTime + TimeSpan.FromMinutes(5);

            _workday.StartTimeSlot(_startTime.TimeOfDay, TimeSlotType.Tomato);
            _workday.StartTimeSlot(breakStartTime, TimeSlotType.Break);
            _workday.StartTimeSlot(secondTomatoStartTime, TimeSlotType.Tomato);
            var result = _workday.GetSummary();

            var timeSlots = result.TimeSlots.ToArray();
            timeSlots.Should().HaveCount(3);
            CheckFinishedTimeSlot(timeSlots[2], breakStartTime);
            CheckFinishedTimeSlot(timeSlots[1], secondTomatoStartTime);
            timeSlots[0].IsInProgress.Should().BeTrue();
        }

        [Test]
        [TestCase(TimeSlotType.Tomato, TimeSlotType.Tomato)]
        [TestCase(TimeSlotType.Break, TimeSlotType.Break)]
        [TestCase(TimeSlotType.LongBreak, TimeSlotType.LongBreak)]
        [TestCase(TimeSlotType.Break, TimeSlotType.LongBreak)]
        [TestCase(TimeSlotType.LongBreak, TimeSlotType.Break)]
        public void TwoTimeSlotsOfTheSameTypeCannotBeAddedConsecutively(TimeSlotType firstType, TimeSlotType nextType)
        {
            _workday.StartTimeSlot(_startTime.TimeOfDay, firstType);
            Action<WorkDay> action = p => p.StartTimeSlot(_startTime.AddMinutes(30).TimeOfDay, nextType);

            _workday.Invoking(action).ShouldThrow<WorkDay.TimeSlotTypeAlreadyInProgressException>();
        }

        [Test]
        public void AddingNewTimeSlotWithStartTimeEarlierAsTheCurrentTimeSlotIsNotAllowed()
        {
            _workday.StartTimeSlot(_startTime.TimeOfDay, TimeSlotType.Tomato);
            _workday.StartTimeSlot(_startTime.AddMinutes(30).TimeOfDay, TimeSlotType.Break);
            Action<WorkDay> action = p => p.StartTimeSlot(_startTime.AddMinutes(29).TimeOfDay, TimeSlotType.Tomato);

            _workday.Invoking(action).ShouldThrow<WorkDay.InvalidStartTimeException>();
        }

        [Test]
        public void PausingDayWillEndTheCurrentTimeSlot()
        {
            var pauseTime = _startTime.AddHours(1).TimeOfDay;
            _workday.StartTimeSlot(_startTime.TimeOfDay, TimeSlotType.Break);
            _workday.PauseDay(pauseTime);

            var result = _workday.GetSummary();

            var timeSlot = result.TimeSlots.Single();
            timeSlot.IsInProgress.Should().BeFalse();
            result.EndTime.ShouldBeEquivalentTo(null);
        }

        [Test]
        public void EndingDayWillAlsoEndTheCurrentTimeSlot()
        {
            var endTime = _startTime.AddHours(1).TimeOfDay;
            _workday.StartTimeSlot(_startTime.TimeOfDay, TimeSlotType.Break);
            _workday.FinishDay(endTime);

            var result = _workday.GetSummary();

            CheckFinishedTimeSlot(result.TimeSlots.Single(), endTime);
            result.EndTime.ShouldBeEquivalentTo(endTime);
        }

        [Test]
        public void DayCannotBeFinishedEarlierThanItWasStarted()
        {
            Action<WorkDay> action = p => p.FinishDay(_startTime.AddHours(-1).TimeOfDay);

            _workday.Invoking(action).ShouldThrow<WorkDay.InvalidStartTimeException>();
        }

        [Test]
        public void DayCannotBePausedEarlierThanItWasStarted()
        {
            Action<WorkDay> action = p => p.PauseDay(_startTime.AddHours(-1).TimeOfDay);

            _workday.Invoking(action).ShouldThrow<WorkDay.InvalidStartTimeException>();
        }

        [Test]
        public void CannotResumeWorkDayWithABreak()
        {
            _workday.StartTimeSlot(_startTime.TimeOfDay, TimeSlotType.Tomato);
            _workday.PauseDay(_startTime.AddMinutes(30).TimeOfDay);
            Action<WorkDay> action = p => p.StartTimeSlot(_startTime.AddHours(1).TimeOfDay, TimeSlotType.Break);

            _workday.Invoking(action).ShouldThrow<WorkDay.ResumeWorkDayWithBreakException>();
        }
    }
}
