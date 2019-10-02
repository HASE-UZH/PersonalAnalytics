using System;
using System.Collections.Generic;
using System.Linq;
using GameOverlay.Drawing;
using GameOverlay.Windows;

namespace WindowRecommender.Graphics
{
    internal class HazeOverlayWindow: IDisposable
    {
        private readonly GraphicsWindow _window;

        private SolidBrush _brush;
        private Rectangle[] _rectangles;
        private bool _needsRedraw;

        internal HazeOverlayWindow(Rectangle screenRectangle)
        {
            _rectangles = new Rectangle[0];
            _needsRedraw = true;

            _window = new GraphicsWindow
            {
                IsTopmost = true,
                FPS = Settings.FramesPerSecond,
                X = screenRectangle.Left,
                Y = screenRectangle.Top,
                Width = screenRectangle.Right - screenRectangle.Left,
                Height = screenRectangle.Bottom - screenRectangle.Top
            };
            _window.SetupGraphics += OnSetupGraphics;
            _window.DrawGraphics += OnDrawGraphics;
            _window.DestroyGraphics += OnDestroyGraphics;
        }

        ~HazeOverlayWindow()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Start()
        {
            _window.IsVisible = true;
            _window.StartThread();
        }

        public void Stop()
        {
            _rectangles = new Rectangle[0];
            _needsRedraw = true;
            _window.IsVisible = false;
            _window.StopThread();
        }

        internal void Show(IEnumerable<Rectangle> rectangles)
        {
            _rectangles = rectangles.ToArray();
            _needsRedraw = true;
        }

        internal void Hide()
        {
            _rectangles = new Rectangle[0];
            _needsRedraw = true;
        }

        private void OnSetupGraphics(object sender, SetupGraphicsEventArgs e)
        {
            var graphics = e.Graphics;
            _brush = graphics.CreateSolidBrush(0, 0, 0, Settings.OverlayAlpha);
        }

        private void OnDrawGraphics(object sender, DrawGraphicsEventArgs e)
        {
            if (_needsRedraw)
            {
                var graphics = e.Graphics;

                graphics.ClearScene();
                foreach (var rectangle in _rectangles)
                {
                    graphics.FillRectangle(_brush, rectangle);
                }

                _needsRedraw = false;
            }
        }

        private void OnDestroyGraphics(object sender, DestroyGraphicsEventArgs e)
        {
            _brush.Dispose();
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _window?.Dispose();
                _brush?.Dispose();
            }
        }
    }
}
