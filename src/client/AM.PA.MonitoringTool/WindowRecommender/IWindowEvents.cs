using System;
using System.Collections.Generic;

namespace WindowRecommender
{
    internal interface IWindowEvents
    {
        event EventHandler<WindowRecord> WindowFocused;
        event EventHandler<WindowRecord> WindowOpened;
        event EventHandler<WindowRecord> WindowOpenedOrFocused;
        event EventHandler<WindowRecord> WindowClosed;
        event EventHandler<WindowRecord> WindowMinimized;
        event EventHandler<WindowRecord> WindowClosedOrMinimized;
        event EventHandler<WindowRecord> WindowRenamed;
        event EventHandler<List<WindowRecord>> Setup;
    }
}