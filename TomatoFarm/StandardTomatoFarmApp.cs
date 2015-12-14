using System;
using System.Linq;
using TomatoFarm.DataElements;

namespace TomatoFarm
{
    public class StandardTomatoFarmApp
    {
        public IDateTimeProvider DateTimeProvider { private get; set; }
        public ITimer Timer { private get; set; }
        public IMessenger Messenger { private get; set; }

        private static readonly object Locker = new object();
        private WorkDay _workday;
        private readonly TomatoFarmSettings _appSettings;
        private const int TomatoRoundSize = 4;

        public StandardTomatoFarmApp(TomatoFarmSettings appSettings)
        {
            if (appSettings == null) throw new ArgumentNullException(nameof(appSettings));
            _appSettings = appSettings;
        }

        public void StartWork()
        {
            var dateTime = DateTimeProvider.GetCurrentDate();
            _workday = WorkDay.MakeNewDay(dateTime, _appSettings.RegularHours); 
            StartTomatoWithTimer(dateTime.TimeOfDay);
        }

        public void StartNewTomato()
        {
            Timer.Stop();
            StartTomatoWithTimer(DateTimeProvider.GetCurrentTime());
        }

        private void StartTomatoWithTimer(TimeSpan timeStamp)
        {
            _workday.StartTimeSlot(timeStamp, TimeSlotType.Tomato);
            Timer.Start(
                new TimedAction {Action = () => Messenger.TomatoDueTimeWarning(), DueIn = _appSettings.TomatoDueWarning},
                new TimedAction {Action = () => StartBreakWithTimer(), DueIn = _appSettings.TomatoSize});
        }

        public void StartBreak()
        {
            Timer.Stop();
            StartBreakWithTimer();
        }

        private void StartBreakWithTimer()
        {
            lock (Locker)
            {
                if(_workday.GetSummary().TimeSlots.Last().Type == TimeSlotType.Break)
                    return;
                var breakType = GetBreakType();
                var @break = MakeBreak(breakType);

                _workday.StartTimeSlot(DateTimeProvider.GetCurrentTime(), breakType);
                Messenger.TomatoFinished();
                var b = @break;
                Timer.Start(
                    new TimedAction { Action = () => b.Warn(), DueIn = b.WarnTime },
                    new TimedAction { Action = () => FinishBreak(@break), DueIn = @break.DueTime });
            }

        }

        private TimeSlotType GetBreakType()
        {
            const int timeSlotRoundSize = 2*TomatoRoundSize - 1;
            var timeSlotRound = _workday.GetSummary().TimeSlots.OrderByDescending(p => p.StartTime).Take(timeSlotRoundSize).ToList();
            return timeSlotRound.Count == timeSlotRoundSize ? TimeSlotType.LongBreak : TimeSlotType.Break;
        }

        private Break MakeBreak(TimeSlotType type)
        {
            if(type == TimeSlotType.Break)
                return new ShortBreak(Messenger, _appSettings);
            return new LongBreak(Messenger, _appSettings);
        }

        private void FinishBreak(Break @break)
        {
            Timer.Stop();
            @break.Finish();
            _workday.PauseDay(DateTimeProvider.GetCurrentTime());
        }

        public WorkSummary GetSummary()
        {
            return _workday?.GetSummary();
        }

        public void EndWork()
        {
            Timer.Stop();
            _workday.FinishDay(DateTimeProvider.GetCurrentTime());
        }

        public void PauseWork()
        {
            Timer.Stop();
            _workday.PauseDay(DateTimeProvider.GetCurrentTime());
        }
    }
}