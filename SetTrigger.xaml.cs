using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using Npgsql;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;


namespace ASKOmaster
{
    /// <summary>
    /// Interaction logic for SetTrigger.xaml
    /// </summary>
    public partial class SetTrigger : System.Windows.Window
    {
        public SetTrigger()
        {
            InitializeComponent();
            btnRun.IsEnabled = false;
            using var conn = new NpgsqlConnection(Properties.Settings.Default.DBstring);
            try
            {
                conn.Open();
            }
            catch (System.Net.Sockets.SocketException)
            {
                MessageBox.Show("Cannot connect to the Database");
            }
            
            
            string command = $"SELECT code,state FROM public.technicians;";
            var cmd = new NpgsqlCommand(command, conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string Tech = reader.GetValue(0).ToString();
                Technicians.Items.Add(Tech);
            }
        }

        private void Technicians_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnRun.IsEnabled = true;
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            var conn = new NpgsqlConnection(Properties.Settings.Default.DBstring);
            conn.Open();

            string State = "";
            string serviceUnit = "";
            string serviceTechnician = "";
            string command = $"SELECT state, service_unit,service_technician FROM public.technicians where code='{Technicians.Text}';";
            using var cmd = new NpgsqlCommand(command, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                State = reader.GetValue(0).ToString();
                serviceUnit = reader.GetValue(1).ToString();
                serviceTechnician = reader.GetValue(2).ToString();
            }



            ////////////////////////Open Page////////////////////////
            IWebDriver driver = new EdgeDriver();
            driver.Navigate().GoToUrl("https://partners.gorenje.com/sagCC/se_seti_serviserji.aspx");
            driver.FindElement(By.Id("usr")).SendKeys(Properties.Settings.Default.Username);
            driver.FindElement(By.Id("pwd")).SendKeys(Properties.Settings.Default.Password);

            driver.FindElement(By.Id("btnPrijava")).Click();
            driver.Navigate().GoToUrl("https://partners.gorenje.com/sagCC/se_seti_serviserji.aspx");

            ////////////////////////Open Excel////////////////////////
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            Workbook wb;
            Worksheet ws;
            wb = excel.Workbooks.Open(ExcelPath.Text);
            ws = wb.Worksheets[1];

            int rowCount = ws.Rows.Count;

            int row = 1;
            while (ws.Cells[row, 1].Value2 != null)
            {

                string part = ws.Cells[row, 1].Value2.ToString();
                string min = ws.Cells[row, 2].Value2.ToString();
                string max = ws.Cells[row, 3].Value2.ToString();

                //Input Part
                for (int i = 0; i < 7; i++)
                {
                    driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_dd_izd_sifra_I")).SendKeys(Keys.Backspace);
                }
                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_dd_izd_sifra_I")).SendKeys(part);
                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_dd_izd_sifra_I")).SendKeys(Keys.Enter);
                
                try
                {
                    new WebDriverWait(driver, TimeSpan.FromSeconds(20)).Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id=\"ctl00_ContentPlaceHolder1_dd_izd_sifra_DDD_L_LBI0T0\"]")));
                }
                catch (NoSuchElementException)
                {
                    driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_dd_izd_sifra_I")).Click();
                }
                
                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_dd_izd_sifra_I")).SendKeys(Keys.Enter);
                //Input Quantity
                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_spMin_I")).SendKeys(Keys.Backspace);
                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_spMin_I")).SendKeys(min);
                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_spMax_I")).SendKeys(Keys.Backspace);
                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_spMax_I")).SendKeys(max);

                //Enter Technician

                driver.FindElement(By.XPath($"//*[@id=\"ctl00_ContentPlaceHolder1_ASPxDropDownEdit1_I\"]")).Clear();
                driver.FindElement(By.XPath($"//*[@id=\"ctl00_ContentPlaceHolder1_ASPxDropDownEdit1_I\"]")).SendKeys(serviceTechnician);
                driver.FindElement(By.XPath("//*[@id=\"ctl00_ContentPlaceHolder1_ASPxDropDownEdit1_B-1Img\"]")).Click();
                driver.FindElement(By.XPath("//*[@id=\"ctl00_ContentPlaceHolder1_ASPxDropDownEdit1_B-1Img\"]")).Click();
                //Submit
                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_btnDodaj")).Click();
                row++;
            }
            wb.Close();
            
            driver.Close();

        }

        private void btnExcelSelect_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".xlsm";
            Nullable<bool> dialogOK = openFileDialog.ShowDialog();
            if (dialogOK == true)
            {
                string path = openFileDialog.FileName;
                ExcelPath.Text = path;
            }
        }
    }
}
