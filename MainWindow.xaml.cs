
using Microsoft.Win32;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Specialized;
using System.Reflection;
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
            VersionNo.Text=Assembly.GetExecutingAssembly().GetName().Version.ToString();
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
            if (isValid())
            {
                System.Windows.Window w = new PartReturn();
                w.Show();
            }
            
        }

        private void test_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnTrigger_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Window w = new SetTrigger();
            w.Show();
        }

        public bool isValid()
        {
            if (Properties.Settings.Default.Database != true)
            {
                MessageBox.Show("Dabatase not connected");
                return false;

            }
            else if (Properties.Settings.Default.Login != true)
            {
                MessageBox.Show("SAG or GSD user didn't setup");
                return false;
            }
            else if (Properties.Settings.Default.Edge != true)
            {
                MessageBox.Show("WebDriver not found");
                return false;
            }
            else
            {
                return true;
            }
        }

        private void btnPreEmpt_Click(object sender, RoutedEventArgs e)
        {
            if (isValid())
            {
                System.Windows.Window w = new PreEmpt();
                w.Show();
            }
        }

        private void btnDailyRelease_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnWorkShop_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnStockTake_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDBLookup_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Window w = new DBLookUp();
            w.Show();
        }

        private void btnDaily_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Window w = new Daily();
            w.Show();
        }
    }
}
