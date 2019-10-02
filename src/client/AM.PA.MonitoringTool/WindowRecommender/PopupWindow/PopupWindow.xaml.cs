using System.Windows;

namespace WindowRecommender.PopupWindow
{
    /// <summary>
    /// Interaction logic for the WindowRecommender DebugWindow
    /// </summary>
    /// <inheritdoc cref="System.Windows.Window" />
    public partial class PopupWindow
    {
        public PopupWindow()
        {
            Topmost = true;
            ShowActivated = true;
            ShowInTaskbar = true;
            ResizeMode = ResizeMode.NoResize;
            InitializeComponent();
        }

        private void SubmitClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void SkipClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
