using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace WindowRecommender.PopupWindow
{
    internal class PopupWindowDataContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<OpenWindowsDataSource> OpenWindows { get; private set; }

        internal PopupWindowDataContext()
        {
            OpenWindows = new ObservableCollection<OpenWindowsDataSource>();
        }

        public void SetWindows(IEnumerable<OpenWindowsDataSource> openWindows)
        {
            OpenWindows = new ObservableCollection<OpenWindowsDataSource>(openWindows);
            OnPropertyChanged(nameof(OpenWindows));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class OpenWindowsDataSource
    {
        public bool Relevant { get; set; }
        public string Handle { get; }
        public string Title { get; }
        public BitmapSource Icon { get; }
        public bool Hazed { get; }

        public OpenWindowsDataSource(IntPtr handle, string title, BitmapSource icon, bool hazed)
        {
            Relevant = false;
            Handle = handle.ToString();
            Title = title;
            Icon = icon;
            Hazed = hazed;
        }
    }
}
