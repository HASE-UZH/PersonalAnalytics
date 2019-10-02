using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WindowRecommender.Data;
using WindowRecommender.Native;
using WindowRecommender.Properties;

namespace WindowRecommender.PopupWindow
{
    internal class PopupWindowManager
    {
        private const int TimerIntervalMinutes = 1;
        private const int MinPopupIntervalMinutes = 40;
        private const int MaxPopupIntervalMinutes = 50;

        private readonly DispatcherTimer _timer;

        private int _minutesUntilNextPopup;
        private List<(WindowRecord windowRecord, bool show)> _windows;

        internal PopupWindowManager()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(TimerIntervalMinutes)
            };
            _timer.Tick += OnTimerTick;
        }

        internal void Start()
        {
            _minutesUntilNextPopup = GetMinutesUntilNextPopup();
            _timer.Start();
        }

        internal void Stop()
        {
            _timer.Stop();
        }

        internal void SetWindows(List<(WindowRecord windowRecord, bool show)> windows)
        {
            _windows = windows;
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            _minutesUntilNextPopup -= TimerIntervalMinutes;
            if (_minutesUntilNextPopup < TimerIntervalMinutes)
            {
                OpenPopup();
                _minutesUntilNextPopup = GetMinutesUntilNextPopup();
            }
        }

        private void OpenPopup()
        {
            var popupWindow = new PopupWindow();
            var popupWindowDataContext = (PopupWindowDataContext)popupWindow.DataContext;
            popupWindowDataContext.SetWindows(_windows.Select(GetOpenWindowsDataSource));
            var dialogResult = popupWindow.ShowDialog();
            if (dialogResult == true)
            {
                var responses = popupWindowDataContext.OpenWindows
                    .Select(source => (source.Handle, source.Relevant, source.Hazed));
                Queries.SavePopupResponses(responses);
            }
        }

        private static int GetMinutesUntilNextPopup()
        {
            var random = new Random();
            return random.Next(MinPopupIntervalMinutes, MaxPopupIntervalMinutes);
        }

        private static OpenWindowsDataSource GetOpenWindowsDataSource((WindowRecord windowRecord, bool show) windowTuple)
        {
            var (windowRecord, show) = windowTuple;
            var iconHandle = NativeMethods.GetWindowIconPointer(windowRecord.Handle);
            BitmapSource bitmapSource;
            if (iconHandle == IntPtr.Zero)
            {
                bitmapSource = GetDefaultIcon();
            }
            else
            {
                bitmapSource = Imaging.CreateBitmapSourceFromHIcon(iconHandle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            return new OpenWindowsDataSource(windowRecord.Handle, windowRecord.Title, bitmapSource, !show);
        }

        private static BitmapSource GetDefaultIcon()
        {
            var bitmapHandle = Resources.DefaultIcon.GetHbitmap();
            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmapHandle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            bitmapSource.Freeze();
            NativeMethods.DeleteObject(bitmapHandle);
            return bitmapSource;
        }
    }
}
