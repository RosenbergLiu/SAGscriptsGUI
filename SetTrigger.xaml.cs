using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using Npgsql;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using System;
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
            conn.Open();
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
            driver.Navigate().GoToUrl("https://partners.gorenje.com/sagCC/vracilo_vnos.aspx");
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
