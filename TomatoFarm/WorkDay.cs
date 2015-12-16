using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TomatoFarm.DataElements;

namespace TomatoFarm
{
    public class WorkDay    
    {
        private readonly WorkSummary _summary;
        private bool _isPaused;

        private TimeSlot Current => _summary.TimeSlots.FirstOrDefault();

        private WorkDay(DateTime date, TimeSpan regularHours)
        {
            _summary = new WorkSummary
            {
                Date = date.Date,
                StartTime = date.TimeOfDay,
                RegularHoursEndTime = date.TimeOfDay + regularHours,
                TimeSlots = new Stack<TimeSlot>()
            };
        }

        public static WorkDay MakeNewDay(DateTime date, TimeSpan regularHours)
        {
            return new WorkDay(date, regularHours);
        }

        public void FinishDay(TimeSpan stopTime)
        {
            PauseDay(stopTime);
            _summary.EndTime = stopTime;
        }

        public void PauseDay(TimeSpan pauseTime)
        {
            _isPaused = true;
            CheckTimeStamp(pauseTime);
            CloseCurrentSlot(pauseTime);
        }

        public WorkSummary GetSummary()
        {
            return _summary;
        }

        public void StartTimeSlot(TimeSpan timeStamp, TimeSlotType type)
        {
            CheckTimeStamp(timeStamp);
            CheckTimeSlotType(type);
            CloseCurrentSlot(timeStamp);

            _isPaused = false;
            _summary.TimeSlots.Push(new TimeSlot { StartTime = timeStamp, Type = type, IsInProgress = true });
        }

        private void CheckTimeSlotType(TimeSlotType type)
        {
            if(type == TimeSlotType.Break && _isPaused) throw new ResumeWorkDayWithBreakException();
            if (Current == null) return;
            if (Current.Type == type || AreBothTypesBreaks(Current.Type, type)) throw new TimeSlotTypeAlreadyInProgressException();
        }

        private bool AreBothTypesBreaks(TimeSlotType type1, TimeSlotType type2)
        {
            var breakTypes = new[] {TimeSlotType.Break, TimeSlotType.LongBreak};
            return breakTypes.Contains(type1) && breakTypes.Contains(type2);
        }

        private void CheckTimeStamp(TimeSpan timeStamp)
        {
            if (timeStamp < _summary.StartTime)
                throw new InvalidStartTimeException();
            if (Current != null && Current.StartTime > timeStamp)
                throw new InvalidStartTimeException();
        }

        private void CloseCurrentSlot(TimeSpan timeStamp)
        {
            if (Current == null) return;

            Current.IsInProgress = false;
            Current.EndTime = timeStamp;
        }

        public class InvalidStartTimeException : Exception
        {
        }

        public class TimeSlotTypeAlreadyInProgressException : Exception
        {
        }

        public class ResumeWorkDayWithBreakException : Exception
        {
        }
    }
}
