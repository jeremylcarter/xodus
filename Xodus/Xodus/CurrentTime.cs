using System;
using System.ComponentModel;
using Windows.UI.Xaml;

namespace Xodus
{
    public class CurrentTime : INotifyPropertyChanged
    {
        private DateTime _now;

        public CurrentTime()
        {
            _now = DateTime.Now;
            var dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromMilliseconds(1000);
            dt.Tick += Dt_Tick;
            dt.Start();
        }

        public DateTime Now
        {
            get => _now;
            set
            {
                _now = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Now"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Dt_Tick(object sender, object e)
        {
            Now = DateTime.Now;
        }
    }
}