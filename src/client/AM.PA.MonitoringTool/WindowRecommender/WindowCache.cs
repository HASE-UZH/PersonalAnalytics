using System;
using System.Collections.Generic;
using System.Linq;
using WindowRecommender.Native;

namespace WindowRecommender
{
    internal class WindowCache : IWindowEvents
    {
        public event EventHandler<WindowRecord> WindowFocused;
        public event EventHandler<WindowRecord> WindowOpened;
        public event EventHandler<WindowRecord> WindowOpenedOrFocused;
        public event EventHandler<WindowRecord> WindowClosed;
        public event EventHandler<WindowRecord> WindowMinimized;
        public event EventHandler<WindowRecord> WindowClosedOrMinimized;
        public event EventHandler<WindowRecord> WindowRenamed;
        public event EventHandler<List<WindowRecord>> Setup;

        private Dictionary<IntPtr, WindowRecord> _cache;

        internal WindowCache(ModelEvents modelEvents)
        {
            _cache = new Dictionary<IntPtr, WindowRecord>();
            modelEvents.WindowOpened += OnWindowOpened;
            modelEvents.WindowFocused += OnWindowFocused;
            modelEvents.WindowClosed += OnWindowClosed;
            modelEvents.WindowMinimized += OnWindowMinimized;
            modelEvents.WindowRenamed += OnWindowRenamed;
        }

        internal void Start()
        {
            var openWindowRecords = NativeMethods.GetOpenWindows().Select(GetWindowRecord).ToList();
            Setup?.Invoke(this, openWindowRecords);
            _cache = openWindowRecords.ToDictionary(record => record.Handle, record => record);
        }

        private void OnWindowClosed(object sender, IntPtr e)
        {
            var windowHandle = e;
            if (_cache.ContainsKey(windowHandle))
            {
                WindowClosedOrMinimized?.Invoke(this, _cache[windowHandle]);
                WindowClosed?.Invoke(this, _cache[windowHandle]);
                _cache.Remove(windowHandle);
            }
        }

        private void OnWindowFocused(object sender, IntPtr e)
        {
            var windowHandle = e;
            if (!_cache.ContainsKey(windowHandle))
            {
                _cache[windowHandle] = GetWindowRecord(windowHandle);
            }
            WindowOpenedOrFocused?.Invoke(this, _cache[windowHandle]);
            WindowFocused?.Invoke(this, _cache[windowHandle]);
        }

        private void OnWindowOpened(object sender, IntPtr e)
        {
            var windowHandle = e;
            if (!_cache.ContainsKey(windowHandle))
            {
                _cache[windowHandle] = GetWindowRecord(windowHandle);
            }
            WindowOpenedOrFocused?.Invoke(this, _cache[windowHandle]);
            WindowOpened?.Invoke(this, _cache[windowHandle]);
        }

        private void OnWindowMinimized(object sender, IntPtr e)
        {
            var windowHandle = e;
            if (_cache.ContainsKey(windowHandle))
            {
                WindowClosedOrMinimized?.Invoke(this, _cache[windowHandle]);
                WindowMinimized?.Invoke(this, _cache[windowHandle]);
            }
        }

        private void OnWindowRenamed(object sender, IntPtr e)
        {
            var windowHandle = e;
            if (_cache.ContainsKey(windowHandle))
            {
                var windowRecord = _cache[windowHandle];
                windowRecord.Title = NativeMethods.GetWindowTitle(windowHandle);
                _cache[windowHandle] = windowRecord;
                WindowRenamed?.Invoke(this, windowRecord);
            }
        }

        private static WindowRecord GetWindowRecord(IntPtr windowHandle)
        {
            var windowTitle = NativeMethods.GetWindowTitle(windowHandle);
            var processName = NativeMethods.GetProcessName(windowHandle);
            return new WindowRecord(windowHandle, windowTitle, processName);
        }
    }
}
