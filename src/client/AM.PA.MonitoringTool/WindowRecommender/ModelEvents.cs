using System;
using System.Collections.Generic;
using System.Timers;
using WindowRecommender.Native;

namespace WindowRecommender
{
    internal class ModelEvents
    {
        internal event EventHandler<IntPtr> WindowFocused;
        internal event EventHandler<IntPtr> WindowOpened;
        internal event EventHandler<IntPtr> WindowClosed;
        internal event EventHandler<IntPtr> WindowMinimized;
        internal event EventHandler<IntPtr> WindowRenamed;
        internal event EventHandler MoveStarted;
        internal event EventHandler MoveEnded;

        private delegate void EventFunction(IntPtr windowHandle);

        // ReSharper disable once CollectionNeverQueried.Local
        // Keep reference of delegates to prevent GC from deleting them.
        private readonly List<NativeMethods.Wineventproc> _delegates;

        private IntPtr[] _winEventHooks;
        private bool _isMoving;
        private IntPtr _focusedWindow;

        internal ModelEvents()
        {
            _delegates = new List<NativeMethods.Wineventproc>();
        }

        public void Start()
        {
            _isMoving = false;

            _winEventHooks = new[]
            {
                NativeMethods.SetWinEventHook(WinEventConstant.EVENT_OBJECT_CREATE, EventWrapper(OnWindowCreated)),
                NativeMethods.SetWinEventHook(WinEventConstant.EVENT_SYSTEM_FOREGROUND, OpenWindowEventWrapper(OnWindowFocused)),
                NativeMethods.SetWinEventHook(WinEventConstant.EVENT_SYSTEM_MINIMIZEEND, OpenWindowEventWrapper(OnWindowFocused)),
                NativeMethods.SetWinEventHook(WinEventConstant.EVENT_SYSTEM_MINIMIZESTART, EventWrapper(OnWindowMinimized)),
                NativeMethods.SetWinEventHook(WinEventConstant.EVENT_SYSTEM_MOVESIZEEND, EventWrapper(OnMoveEnded)),
                NativeMethods.SetWinEventHook(WinEventConstant.EVENT_SYSTEM_MOVESIZESTART, EventWrapper(OnMoveStarted)),
                NativeMethods.SetWinEventHook(WinEventConstant.EVENT_OBJECT_LOCATIONCHANGE, OpenWindowEventWrapper(OnWindowMoved)),
                NativeMethods.SetWinEventHook(WinEventConstant.EVENT_OBJECT_DESTROY, EventWrapper(OnWindowClosed)),
                NativeMethods.SetWinEventHook(WinEventConstant.EVENT_OBJECT_NAMECHANGE, OpenWindowEventWrapper(OnWindowRenamed)),
            };
        }

        public void Stop()
        {
            foreach (var winEventHook in _winEventHooks)
            {
                NativeMethods.UnhookWinEvent(winEventHook);
            }
            _winEventHooks = new IntPtr[0];
            _delegates.Clear();
            _focusedWindow = IntPtr.Zero;
        }

        private NativeMethods.Wineventproc EventWrapper(EventFunction eventFunction)
        {
            // ReSharper disable once ConvertToLocalFunction
            NativeMethods.Wineventproc del = (hWinEventHook, @event, hwnd, idObject, idChild, idEventThread, dwmsEventTime) =>
            {
                if (hwnd != IntPtr.Zero && idObject == ObjectIdentifier.OBJID_WINDOW && idChild == NativeMethods.CHILDID_SELF)
                {
                    eventFunction(hwnd);
                }
            };
            _delegates.Add(del);
            return del;
        }

        private NativeMethods.Wineventproc OpenWindowEventWrapper(EventFunction eventFunction)
        {
            return EventWrapper(windowHandle =>
            {
                if (NativeMethods.IsOpenWindow(windowHandle))
                {
                    eventFunction(windowHandle);
                }
            });
        }

        private void OnWindowCreated(IntPtr windowHandle)
        {
            if (NativeMethods.IsOpenWindow(windowHandle))
            {
                WindowOpened?.Invoke(this, windowHandle);
            }
            else
            {
                // Opening windows can take a bit to be recognized as visible.
                // As there is no further event, wait for 1s to see if it becomes visible.
                // Use the Timer's AutoReset once to have two checks at 500ms each.
                var timer = new Timer(500);
                timer.Elapsed += (sender, args) =>
                {
                    var elapsedTimer = (Timer)sender;
                    elapsedTimer.AutoReset = false;
                    if (NativeMethods.IsOpenWindow(windowHandle))
                    {
                        elapsedTimer.Stop();
                        elapsedTimer.Dispose();
                        WindowOpened?.Invoke(this, windowHandle);
                    }
                };
                timer.Start();
            }
        }

        private void OnWindowFocused(IntPtr windowHandle)
        {
            if (!_isMoving && windowHandle != _focusedWindow)
            {
                WindowFocused?.Invoke(this, windowHandle);
                _focusedWindow = windowHandle;
            }
        }

        private void OnWindowMoved(IntPtr windowHandle)
        {
            if (!_isMoving && windowHandle == _focusedWindow)
            {
                MoveEnded?.Invoke(this, null);
            }
        }

        private void OnWindowClosed(IntPtr windowHandle)
        {
            if (!_isMoving)
            {
                WindowClosed?.Invoke(this, windowHandle);
                if (windowHandle == _focusedWindow)
                {
                    _focusedWindow = IntPtr.Zero;
                }
            }
        }

        private void OnWindowMinimized(IntPtr windowHandle)
        {
            if (!_isMoving)
            {
                WindowMinimized?.Invoke(this, windowHandle);
                if (windowHandle == _focusedWindow)
                {
                    _focusedWindow = IntPtr.Zero;
                }
            }
        }

        private void OnMoveStarted(IntPtr windowHandle)
        {
            _isMoving = true;
            MoveStarted?.Invoke(this, null);
        }

        private void OnMoveEnded(IntPtr windowHandle)
        {
            _isMoving = false;
            MoveEnded?.Invoke(this, null);
        }

        private void OnWindowRenamed(IntPtr windowHandle)
        {
            WindowRenamed?.Invoke(this, windowHandle);
        }
    }
}
