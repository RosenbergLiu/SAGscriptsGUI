
using Microsoft.Win32;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ASKOmaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }



        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            btnSetting.BorderBrush = new SolidColorBrush(Colors.Black);
            btnSetting.BorderThickness = new Thickness(1);
            System.Windows.Window w = new Settings();
            w.Show();
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Window w = new PartReturn();
            w.Show();
        }

        private void test_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnTrigger_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Window w = new SetTrigger();
            w.Show();
        }
    }
}
