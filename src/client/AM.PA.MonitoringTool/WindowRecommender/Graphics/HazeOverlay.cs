using System.Collections.Generic;
using System.Linq;
using WindowRecommender.Data;
using WindowRecommender.Native;

namespace WindowRecommender.Graphics
{
    internal class HazeOverlay
    {
        private Rectangle[] _monitorRectangles;
        private (HazeOverlayWindow window, Rectangle rectangle)[] _windows;
        private bool _isRunning;

        internal HazeOverlay()
        {
            _monitorRectangles = new Rectangle[0];
            _windows = new (HazeOverlayWindow window, Rectangle rectangle)[0];
            _isRunning = false;
            RefreshMonitorRectangles();
        }

        public void Start()
        {
            if (_windows.Length ==0)
            {
                CreateMonitorWindows();
            }
            _isRunning = true;
            foreach (var (window, _) in _windows)
            {
                window.Start();
            }
        }

        public void Stop()
        {
            _isRunning = false;
            foreach (var (window, _) in _windows)
            {
                window.Stop();
            }
        }

        internal void Show(IEnumerable<(Rectangle rectangle, bool show)> windowInfo)
        {
            var windowList = windowInfo.ToList();
            foreach (var (window, screenRectangle) in _windows)
            {
                var rectangles = Mask.Cut(screenRectangle, windowList);
                var transformedRectangles = rectangles.Select(rectangle => Rectangle.TranslatedRelative(screenRectangle, rectangle));
                window.Show(transformedRectangles);
            }
        }

        internal void Hide()
        {
            foreach (var (window, _) in _windows)
            {
                window.Hide();
            }
        }

        internal void RefreshMonitorRectangles()
        {
            var monitorRectangles = NativeMethods.GetMonitorWorkingAreas().Select(rect => (Rectangle) rect).ToArray();
            if (!_monitorRectangles.SequenceEqual(monitorRectangles))
            {
                _monitorRectangles = monitorRectangles;
                Queries.SaveScreenEvents(_monitorRectangles);

                foreach (var (window, _) in _windows)
                {
                    window.Dispose();
                }
                _windows = new (HazeOverlayWindow window, Rectangle rectangle)[0];

                if (_isRunning)
                {
                    CreateMonitorWindows();
                    foreach (var (window, _) in _windows)
                    {
                        window.Start();
                    }
                }
            }
        }

        private void CreateMonitorWindows()
        {
            _windows = _monitorRectangles.Select(screenRect =>
            {
                var screenRectangle = screenRect;
                var hazeOverlayWindow = new HazeOverlayWindow(screenRectangle);

                return (window: hazeOverlayWindow, rectangle: screenRectangle);
            }).ToArray();
        }
    }
}
