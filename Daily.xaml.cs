using OpenQA.Selenium.Edge;
using OpenQA.Selenium;
using System;
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
using System.Runtime.CompilerServices;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;
using System.Threading;

namespace ASKOmaster
{
    /// <summary>
    /// Interaction logic for Daily.xaml
    /// </summary>
    public partial class Daily : Window
    {
        public Daily()
        {
            InitializeComponent();
        }




        public void GetJobs()
        {
            IWebDriver driver = new EdgeDriver();
            driver.Navigate().GoToUrl("https://partners.gorenje.com/sagCC/sredina.aspx");
            driver.FindElement(By.Id("usr")).SendKeys(Properties.Settings.Default.Username);
            driver.FindElement(By.Id("pwd")).SendKeys(Properties.Settings.Default.Password);
            driver.FindElement(By.Id("btnPrijava")).Click();
            driver.Navigate().GoToUrl("https://partners.gorenje.com/sagCC/potni_pregled1.aspx");
            

            var selectState = new SelectElement(driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_drpCenter")));
            selectState.SelectByValue("0");
            driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_dtDatumFilter_B-1Img")).Click();
            driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_dtDatumFilter_DDD_C_BC")).Click();
            driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_btnPrikazi")).Click();
            driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_lnkBCsv")).Click();

        }


        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => GetJobs());
        }
    }
}
