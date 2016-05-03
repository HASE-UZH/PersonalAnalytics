﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Retrospection
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow(string version)
        {
            InitializeComponent();
            TbVersion.Text = "Version: " + version;
        }

        private void Feedback_Clicked(object sender, RoutedEventArgs e)
        {
            Handler.GetInstance().SendFeedback();
        }

        private void Andre_Clicked(object sender, MouseButtonEventArgs e)
        {
            Handler.GetInstance().SendFeedback();
        }

        private void Tom_Clicked(object sender, MouseButtonEventArgs e)
        {
            Handler.GetInstance().SendFeedback();
        }

        private void Thomas_Clicked(object sender, MouseButtonEventArgs e)
        {
            Handler.GetInstance().SendFeedback();
        }

        //private void CheckForUpdates_Clicked(object sender, RoutedEventArgs e)
        //{
        //    // todo: implement
        //}
    }
}
