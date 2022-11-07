using Npgsql;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.ComponentModel;
using System.Linq;
using SapNwRfc;
using OpenQA.Selenium.DevTools.V106.Overlay;

namespace ASKOmaster
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            if (Properties.Settings.Default.Login == true)
            {
                UsernameInput.Text = Properties.Settings.Default.Username;
                PasswordInput.Text = Properties.Settings.Default.Password;
                LoginTestResult.Text = "Login successful.  " + Properties.Settings.Default.LoginString;
                DriverTestResult.Text = Properties.Settings.Default.EdgeResult;
            }
            if (Properties.Settings.Default.Database)
            {
                DbConnResult.Text = Properties.Settings.Default.DBresult;
                DbString.Text = Properties.Settings.Default.DBstring;
            }

        }
        private void btnCloseSettings_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnTestLogin_Click(object sender, RoutedEventArgs e)
        {
            var driver = new EdgeDriver();
            driver.Navigate().GoToUrl("https://partners.gorenje.com/sagCC/Login.aspx");
            driver.FindElement(By.XPath("//*[@id=\"usr\"]")).SendKeys(UsernameInput.Text);
            driver.FindElement(By.XPath("//*[@id=\"pwd\"]")).SendKeys(PasswordInput.Text);
            driver.FindElement(By.XPath("//*[@id=\"cbRememberMe\"]")).Click();
            driver.FindElement(By.XPath("//*[@id=\"btnPrijava\"]")).Click();
            try
            {
                var ai_user = driver.Manage().Cookies.GetCookieNamed("ai_user");
                var sagAuth = driver.Manage().Cookies.GetCookieNamed(".sagAuth");
                Properties.Settings.Default.ai_user = ai_user.Value;
                Properties.Settings.Default.sagAuth = sagAuth.Value;
                Properties.Settings.Default.Username = UsernameInput.Text;
                Properties.Settings.Default.Password = PasswordInput.Text;
                Properties.Settings.Default.LoginString = driver.FindElement(By.XPath("//*[@id=\"ctl00_sagMenu_I0i0_\"]/span")).Text;
                Properties.Settings.Default.Login = true;
                Properties.Settings.Default.Save();
                driver.Quit();
                LoginTestResult.Text = "Login successful.  " + Properties.Settings.Default.LoginString;
            }
            catch (System.NullReferenceException)
            {
                LoginTestResult.Text = "Login failed.  ";
                Properties.Settings.Default.Reset();
                Properties.Settings.Default.Login = false;
                Properties.Settings.Default.Save();
                driver.Quit();
            }
        }

        private void btnResetLogin_Click(object sender, RoutedEventArgs e)
        {
            UsernameInput.Clear();
            PasswordInput.Clear();
            LoginTestResult.Text = "";
            Properties.Settings.Default.Username = null;
            Properties.Settings.Default.Password = null;
            Properties.Settings.Default.LoginString = null;
            Properties.Settings.Default.Login = false;
            Properties.Settings.Default.Save();
        }

        private void btnTestConn_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DBstring = DbString.Text;
            try
            {
                using var conn = new NpgsqlConnection(DbString.Text);
                conn.Open();
                string command = $"SELECT code,state FROM public.technicians;";
                var cmd = new NpgsqlCommand(command, conn);
                cmd.ExecuteReader();
                DbConnResult.Text = "Database Connected";
                Properties.Settings.Default.DBresult = "Database Connected";
                Properties.Settings.Default.Database = true;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                DbConnResult.Text = ex.Message;
            }
        }

        private void btnTestDriver_Click(object sender, RoutedEventArgs e)
        {

            IWebDriver driver = new EdgeDriver();
        }

        private void btnDownloadDriver_Click(object sender, RoutedEventArgs e)
        {
            bool EdgeInstalled = false;
            string EdgeVersion = "unknown";
            string[] files=Directory.GetDirectories("C:\\Program Files (x86)\\Microsoft\\Edge\\Application");
            foreach (string file in files)
            {
                if ((files[0].Split('\\')[5])[0].Equals('1'))
                {
                    EdgeVersion = files[0].Split('\\')[5];
                    EdgeInstalled = true;
                }
            }
            
            if (EdgeInstalled)
            {
                string remoteUri = "https://msedgedriver.azureedge.net/" + EdgeVersion + "/edgedriver_win64.zip";
                string fileName = "edgedriver_win64.zip";
                WebClient wc = new WebClient();
                wc.DownloadFileTaskAsync(remoteUri, fileName);
                Properties.Settings.Default.EdgeResult= $"WebDriver Version {EdgeVersion} Downloaded";
                DriverTestResult.Text = $"WebDriver Version {EdgeVersion} Downloaded";
                Properties.Settings.Default.Edge = true;
                Properties.Settings.Default.Save();
            }
            else
            {
                DriverTestResult.Text = "Edge not installed";
                Properties.Settings.Default.EdgeResult = "Edge not installed";
                Properties.Settings.Default.Edge = true;
                Properties.Settings.Default.Save();
            }
        }



        private void btnStartSAP_Click(object sender, RoutedEventArgs e)
        {
            bool processExists = Process.GetProcesses().Any(p => p.ProcessName.Contains("SAP Logon for Windows"));
            if (processExists)
            {
                SAPresult.Text = "SAP is running";
            }
            else
            {
                SAPresult.Text = "SAP is not running";
                Process.Start("C:\\Program Files (x86)\\SAP\\FrontEnd\\SAPgui\\saplogon.exe");
                
            }
        }
        private void btnTestSAP_Click(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
