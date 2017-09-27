using System;
using System.ComponentModel;
using Windows.UI.Xaml;

namespace TrivaApp.ViewModels
{
    internal class CountdownTimer : INotifyPropertyChanged
    {
        private DispatcherTimer _timer;

        private DateTime _endTime;
       
        public CountdownTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };

            _endTime = DateTime.UtcNow;

            _timer.Tick += (sender, e) =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimeRemaining"));
            };
        }

        public TimeSpan RemainingTime
        {
            get
            {
                var remainingTime = _endTime - DateTime.UtcNow;
                return remainingTime.TotalMilliseconds < 0 ? TimeSpan.FromMilliseconds(0) : remainingTime;
            }
        }

        public void Start(TimeSpan remainingTime)
        {
            _endTime = DateTime.UtcNow + remainingTime;
            _timer.Start();
        }

        public void Reset()
        {
            _timer.Stop();
            _endTime = DateTime.UtcNow;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
