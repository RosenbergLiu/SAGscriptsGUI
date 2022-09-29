using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using System.Collections.Specialized;
using System.Windows;

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
            catch (OpenQA.Selenium.NoSuchElementException)
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
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Login = false;
            Properties.Settings.Default.Save();
        }

    }
}
