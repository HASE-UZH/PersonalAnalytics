using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowRecommender;
using WindowRecommender.Fakes;
using WindowRecommender.Native.Fakes;

namespace WindowRecommenderTests
{
    [TestClass]
    public class WindowCacheTest
    {
        [TestMethod]
        public async Task TestStart()
        {
            using (ShimsContext.Create())
            {
                ShimNativeMethods.GetOpenWindows = () => new List<IntPtr>
                {
                    new IntPtr(1)
                };
                ShimNativeMethods.GetWindowTitleIntPtr = _ => "test_title";
                ShimNativeMethods.GetProcessNameIntPtr = _ => "test_process";
                var tsc = new TaskCompletionSource<List<WindowRecord>>();
                var windowCache = new WindowCache(new ShimModelEvents());
                windowCache.Setup += (sender, e) => tsc.SetResult(e);
                windowCache.Start();
                await tsc.Task;
                CollectionAssert.AreEqual(new List<WindowRecord>
                {
                    new WindowRecord(new IntPtr(1), "test_title", "test_process")
                }, tsc.Task.Result);
            }
        }

        [TestMethod]
        public async Task TestStart_Empty()
        {
            using (ShimsContext.Create())
            {
                ShimNativeMethods.GetOpenWindows = () => new List<IntPtr>();
                var tsc = new TaskCompletionSource<List<WindowRecord>>();
                var windowCache = new WindowCache(new ShimModelEvents());
                windowCache.Setup += (sender, e) => tsc.SetResult(e);
                windowCache.Start();
                await tsc.Task;
                CollectionAssert.AreEqual(new List<WindowRecord>(), tsc.Task.Result);
            }
        }

        [TestMethod]
        public async Task TestWindowOpen()
        {
            using (ShimsContext.Create())
            {
                ShimNativeMethods.GetWindowTitleIntPtr = _ => "test_title";
                ShimNativeMethods.GetProcessNameIntPtr = _ => "test_process";
                var openTcs = new TaskCompletionSource<WindowRecord>();
                var openFocusTcs = new TaskCompletionSource<WindowRecord>();
                EventHandler<IntPtr> windowOpenedHandler = null;
                var modelEvents = new ShimModelEvents
                {
                    WindowOpenedAddEventHandlerOfIntPtr = handler => windowOpenedHandler = handler
                };
                var windowCache = new WindowCache(modelEvents);
                windowCache.WindowOpened += (sender, e) => openTcs.SetResult(e);
                windowCache.WindowOpenedOrFocused += (sender, e) => openFocusTcs.SetResult(e);
                windowOpenedHandler.Invoke(modelEvents, new IntPtr(1));
                await openTcs.Task;
                await openFocusTcs.Task;
                Assert.AreEqual(new IntPtr(1), openTcs.Task.Result.Handle);
                Assert.AreEqual("test_title", openTcs.Task.Result.Title);
                Assert.AreEqual("test_process", openTcs.Task.Result.ProcessName);
                Assert.AreEqual(new IntPtr(1), openFocusTcs.Task.Result.Handle);
                Assert.AreEqual("test_title", openFocusTcs.Task.Result.Title);
                Assert.AreEqual("test_process", openFocusTcs.Task.Result.ProcessName);
            }
        }

        [TestMethod]
        public async Task TestWindowFocus()
        {
            using (ShimsContext.Create())
            {
                ShimNativeMethods.GetOpenWindows = () => new List<IntPtr>
                {
                    new IntPtr(1)
                };
                ShimNativeMethods.GetWindowTitleIntPtr = _ => "test_title";
                ShimNativeMethods.GetProcessNameIntPtr = _ => "test_process";
                var focusTcs = new TaskCompletionSource<WindowRecord>();
                var openFocusTcs = new TaskCompletionSource<WindowRecord>();
                EventHandler<IntPtr> windowFocusedHandler = null;
                var modelEvents = new ShimModelEvents
                {
                    WindowFocusedAddEventHandlerOfIntPtr = handler => windowFocusedHandler = handler
                };
                var windowCache = new WindowCache(modelEvents);
                windowCache.Start();
                ShimNativeMethods.GetWindowTitleIntPtr = _ =>
                {
                    Assert.Fail();
                    return "";
                };
                ShimNativeMethods.GetProcessNameIntPtr = _ =>
                {
                    Assert.Fail();
                    return "";
                };
                windowCache.WindowFocused += (sender, e) => focusTcs.SetResult(e);
                windowCache.WindowOpenedOrFocused += (sender, e) => openFocusTcs.SetResult(e);
                windowFocusedHandler.Invoke(modelEvents, new IntPtr(1));
                await focusTcs.Task;
                await openFocusTcs.Task;
                Assert.AreEqual(new IntPtr(1), focusTcs.Task.Result.Handle);
                Assert.AreEqual("test_title", focusTcs.Task.Result.Title);
                Assert.AreEqual("test_process", focusTcs.Task.Result.ProcessName);
                Assert.AreEqual(new IntPtr(1), openFocusTcs.Task.Result.Handle);
                Assert.AreEqual("test_title", openFocusTcs.Task.Result.Title);
                Assert.AreEqual("test_process", openFocusTcs.Task.Result.ProcessName);
            }
        }

        [TestMethod]
        public async Task TestWindowFocus_New()
        {
            using (ShimsContext.Create())
            {
                ShimNativeMethods.GetWindowTitleIntPtr = _ => "test_title";
                ShimNativeMethods.GetProcessNameIntPtr = _ => "test_process";
                var focusTcs = new TaskCompletionSource<WindowRecord>();
                var openFocusTcs = new TaskCompletionSource<WindowRecord>();
                EventHandler<IntPtr> windowFocusedHandler = null;
                var modelEvents = new ShimModelEvents
                {
                    WindowFocusedAddEventHandlerOfIntPtr = handler => windowFocusedHandler = handler
                };
                var windowCache = new WindowCache(modelEvents);
                windowCache.WindowFocused += (sender, e) => focusTcs.SetResult(e);
                windowCache.WindowOpenedOrFocused += (sender, e) => openFocusTcs.SetResult(e);
                windowFocusedHandler.Invoke(modelEvents, new IntPtr(1));
                await focusTcs.Task;
                await openFocusTcs.Task;
                Assert.AreEqual(new IntPtr(1), focusTcs.Task.Result.Handle);
                Assert.AreEqual("test_title", focusTcs.Task.Result.Title);
                Assert.AreEqual("test_process", focusTcs.Task.Result.ProcessName);
                Assert.AreEqual(new IntPtr(1), openFocusTcs.Task.Result.Handle);
                Assert.AreEqual("test_title", openFocusTcs.Task.Result.Title);
                Assert.AreEqual("test_process", openFocusTcs.Task.Result.ProcessName);
            }
        }

        [TestMethod]
        public async Task TestWindowClose()
        {
            using (ShimsContext.Create())
            {
                ShimNativeMethods.GetOpenWindows = () => new List<IntPtr>
                {
                    new IntPtr(1)
                };
                ShimNativeMethods.GetWindowTitleIntPtr = _ => "test_title";
                ShimNativeMethods.GetProcessNameIntPtr = _ => "test_process";
                var closeTcs = new TaskCompletionSource<WindowRecord>();
                var closeMinimizeTcs = new TaskCompletionSource<WindowRecord>();
                EventHandler<IntPtr> windowClosedHandler = null;
                var modelEvents = new ShimModelEvents
                {
                    WindowClosedAddEventHandlerOfIntPtr = handler => windowClosedHandler = handler
                };
                var windowCache = new WindowCache(modelEvents);
                windowCache.Start();
                ShimNativeMethods.GetWindowTitleIntPtr = _ =>
                {
                    Assert.Fail();
                    return "";
                };
                ShimNativeMethods.GetProcessNameIntPtr = _ =>
                {
                    Assert.Fail();
                    return "";
                };
                windowCache.WindowClosed += (sender, e) => closeTcs.SetResult(e);
                windowCache.WindowClosedOrMinimized += (sender, e) => closeMinimizeTcs.SetResult(e);
                windowClosedHandler.Invoke(modelEvents, new IntPtr(1));
                await closeTcs.Task;
                await closeMinimizeTcs.Task;
                Assert.AreEqual(new IntPtr(1), closeTcs.Task.Result.Handle);
                Assert.AreEqual("test_title", closeTcs.Task.Result.Title);
                Assert.AreEqual("test_process", closeTcs.Task.Result.ProcessName);
                Assert.AreEqual(new IntPtr(1), closeMinimizeTcs.Task.Result.Handle);
                Assert.AreEqual("test_title", closeMinimizeTcs.Task.Result.Title);
                Assert.AreEqual("test_process", closeMinimizeTcs.Task.Result.ProcessName);
            }
        }

        [TestMethod]
        public void TestWindowClose_NonExistent()
        {
            using (ShimsContext.Create())
            {
                var called = false;
                EventHandler<IntPtr> windowClosedHandler = null;
                var modelEvents = new ShimModelEvents
                {
                    WindowClosedAddEventHandlerOfIntPtr = handler => windowClosedHandler = handler
                };
                var windowCache = new WindowCache(modelEvents);
                windowCache.WindowClosed += (sender, e) => called = true;
                windowCache.WindowClosedOrMinimized += (sender, e) => called = true;
                windowClosedHandler.Invoke(modelEvents, new IntPtr(1));
                Assert.IsFalse(called);
            }
        }

        [TestMethod]
        public async Task TestWindowMinimize()
        {
            using (ShimsContext.Create())
            {
                ShimNativeMethods.GetOpenWindows = () => new List<IntPtr>
                {
                    new IntPtr(1)
                };
                ShimNativeMethods.GetWindowTitleIntPtr = _ => "test_title";
                ShimNativeMethods.GetProcessNameIntPtr = _ => "test_process";
                var minimizeTcs = new TaskCompletionSource<WindowRecord>();
                var closeMinimizeTcs = new TaskCompletionSource<WindowRecord>();
                EventHandler<IntPtr> windowClosedHandler = null;
                var modelEvents = new ShimModelEvents
                {
                    WindowMinimizedAddEventHandlerOfIntPtr = handler => windowClosedHandler = handler
                };
                var windowCache = new WindowCache(modelEvents);
                windowCache.Start();
                ShimNativeMethods.GetWindowTitleIntPtr = _ =>
                {
                    Assert.Fail();
                    return "";
                };
                ShimNativeMethods.GetProcessNameIntPtr = _ =>
                {
                    Assert.Fail();
                    return "";
                };
                windowCache.WindowMinimized += (sender, e) => minimizeTcs.SetResult(e);
                windowCache.WindowClosedOrMinimized += (sender, e) => closeMinimizeTcs.SetResult(e);
                windowClosedHandler.Invoke(modelEvents, new IntPtr(1));
                await minimizeTcs.Task;
                await closeMinimizeTcs.Task;
                Assert.AreEqual(new IntPtr(1), minimizeTcs.Task.Result.Handle);
                Assert.AreEqual("test_title", minimizeTcs.Task.Result.Title);
                Assert.AreEqual("test_process", minimizeTcs.Task.Result.ProcessName);
                Assert.AreEqual(new IntPtr(1), closeMinimizeTcs.Task.Result.Handle);
                Assert.AreEqual("test_title", closeMinimizeTcs.Task.Result.Title);
                Assert.AreEqual("test_process", closeMinimizeTcs.Task.Result.ProcessName);
            }
        }

        [TestMethod]
        public void TestWindowMinimize_NonExistent()
        {
            using (ShimsContext.Create())
            {
                var called = false;
                EventHandler<IntPtr> windowMinimizedHandler = null;
                var modelEvents = new ShimModelEvents
                {
                    WindowMinimizedAddEventHandlerOfIntPtr = handler => windowMinimizedHandler = handler
                };
                var windowCache = new WindowCache(modelEvents);
                windowCache.WindowMinimized += (sender, e) => called = true;
                windowCache.WindowClosedOrMinimized += (sender, e) => called = true;
                windowMinimizedHandler.Invoke(modelEvents, new IntPtr(1));
                Assert.IsFalse(called);
            }
        }

        [TestMethod]
        public async Task TestWindowRenamed()
        {
            using (ShimsContext.Create())
            {
                ShimNativeMethods.GetOpenWindows = () => new List<IntPtr>
                {
                    new IntPtr(1)
                };
                ShimNativeMethods.GetWindowTitleIntPtr = _ => "test_title";
                ShimNativeMethods.GetProcessNameIntPtr = _ => "test_process";
                EventHandler<IntPtr> windowRenamedHandler = null;
                var modelEvents = new ShimModelEvents
                {
                    WindowRenamedAddEventHandlerOfIntPtr = handler => windowRenamedHandler = handler
                };
                var windowCache = new WindowCache(modelEvents);
                var called = false;
                windowCache.WindowRenamed += (sender, e) => called = true;
                Assert.IsFalse(called);

                var setupTcs = new TaskCompletionSource<List<WindowRecord>>();
                windowCache.Setup += (sender, e) => setupTcs.SetResult(e);
                windowCache.Start();
                await setupTcs.Task;
                Assert.AreEqual(new IntPtr(1), setupTcs.Task.Result[0].Handle);
                Assert.AreEqual("test_title", setupTcs.Task.Result[0].Title);
                Assert.AreEqual("test_process", setupTcs.Task.Result[0].ProcessName);

                ShimNativeMethods.GetWindowTitleIntPtr = _ => "new_title";
                var renameTcs = new TaskCompletionSource<WindowRecord>();
                windowCache.WindowRenamed += (sender, e) => renameTcs.SetResult(e);
                windowRenamedHandler.Invoke(modelEvents, new IntPtr(1));
                await renameTcs.Task;
                Assert.AreEqual(new IntPtr(1), renameTcs.Task.Result.Handle);
                Assert.AreEqual("new_title", renameTcs.Task.Result.Title);
                Assert.AreEqual("test_process", renameTcs.Task.Result.ProcessName);
            }
        }

        [TestMethod]
        public void TestEvents()
        {
            using (ShimsContext.Create())
            {
                ShimNativeMethods.GetOpenWindows = () => new List<IntPtr>();
                ShimNativeMethods.GetWindowTitleIntPtr = _ => "test_title";
                ShimNativeMethods.GetProcessNameIntPtr = _ => "test_process";
                var eventHandlers = new EventHandler<IntPtr>[5];
                var modelEvents = new ShimModelEvents
                {
                    WindowOpenedAddEventHandlerOfIntPtr = handler => eventHandlers[0] = handler,
                    WindowFocusedAddEventHandlerOfIntPtr = handler => eventHandlers[1] = handler,
                    WindowRenamedAddEventHandlerOfIntPtr = handler => eventHandlers[2] = handler,
                    WindowMinimizedAddEventHandlerOfIntPtr = handler => eventHandlers[3] = handler,
                    WindowClosedAddEventHandlerOfIntPtr = handler => eventHandlers[4] = handler,
                };
                var windowCache = new WindowCache(modelEvents);
                windowCache.Start();
                foreach (var eventHandler in eventHandlers)
                {
                    eventHandler.Invoke(modelEvents, new IntPtr(1));
                }
            }
        }
    }
}
