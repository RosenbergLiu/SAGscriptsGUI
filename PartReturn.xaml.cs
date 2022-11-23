using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using Npgsql;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Specialized;
using System.Transactions;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Reflection.Metadata;
using System.ComponentModel;
using System.Media;
using System.IO;
using System.Reflection;

namespace ASKOmaster
{
    /// <summary>
    /// Interaction logic for PartReturn.xaml
    /// </summary>
    public partial class PartReturn : System.Windows.Window
    {
        public PartReturn()
        {
            InitializeComponent();
            PartNum.IsEnabled = false;
            btnUpdate.IsEnabled = false;
            btnCancelU.IsEnabled = false;
            btnAdd.IsDefault = true;
            btnRun.IsEnabled = false;
            iniListAsync();

        }
        private async void iniListAsync()
        {
            StringCollection prParts = new StringCollection();
            prParts = Properties.Settings.Default.PRparts;
            if (prParts != null)
            {
                foreach (var p in prParts)
                {
                    PartsList.Items.Add(p);
                }
            }
            BarCode.Focus();
            await using var conn = new NpgsqlConnection(Properties.Settings.Default.DBstring);
            conn.Open();
            string command = $"SELECT code,state FROM public.technicians;";
            var cmd = new NpgsqlCommand(command, conn);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    string Tech = reader.GetValue(0).ToString();
                    Technicians.Items.Add(Tech);
                }
            }
        }

        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (BarCode.Text != "")
            {
                if (isBarCode.IsChecked == true)
                {
                    await AddToListAsync(BarCode.Text);
                    BarCode.Clear();
                }
                else
                {
                    if (BarCode.Text.Length > 6)
                    {

                        string Part = "";
                        await using var conn = new NpgsqlConnection(Properties.Settings.Default.DBstring);
                        conn.Open();
                        string barcode = BarCode.Text;
                        string command = $"SELECT part FROM public.parts where barcode={barcode};";

                        var cmd = new NpgsqlCommand(command, conn);
                        await using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Part = reader.GetValue(0).ToString();

                            }
                        }


                        if (Part != null && Part != "")
                        {

                            await AddToListAsync(Part);
                            BarCode.Clear();
                        }
                        else
                        {
                            if (MessageBox.Show("Do you want to manual input part?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                PartNum.IsEnabled = true;
                                btnUpdate.IsEnabled = true;
                                btnCancelU.IsEnabled = true;
                                PartNum.Focus();
                                btnAdd.IsDefault = false;
                                btnUpdate.IsDefault = true;
                                btnAdd.IsEnabled = false;
                                BarCode.IsEnabled = false;
                            }
                            else
                            {
                                BarCode.Clear();
                                BarCode.Focus();
                            }
                        }
                    }
                    else
                    {
                        await AddToListAsync(BarCode.Text);
                        BarCode.Clear();
                    }
                }
                using (FileStream stream = File.Open(@"AddSound.wav", FileMode.Open))
                {
                    SoundPlayer myNewSound = new SoundPlayer(stream);
                    myNewSound.Load();
                    myNewSound.Play();
                }
            }
        }

        public Task AddToListAsync(string Part)
        {

            PartsList.Items.Insert(0, Part);
            StringCollection prParts = new StringCollection();
            foreach (string p in PartsList.Items)
            {
                prParts.Add(p);
            }
            Properties.Settings.Default.PRparts = prParts;
            Properties.Settings.Default.Save();
            return Task.CompletedTask;
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            PartsList.Items.Remove(PartsList.SelectedItem);
            StringCollection prParts = new StringCollection();
            foreach (string p in PartsList.Items)
            {
                prParts.Add(p);
            }
            Properties.Settings.Default.PRparts = prParts;
            Properties.Settings.Default.Save();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you want to clear the list?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                PartsList.Items.Clear();
                Properties.Settings.Default.PRparts = new StringCollection();
            }
        }

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (PartNum.Text != "")
            {
                await AddToListAsync(PartNum.Text);
                await using var conn = new NpgsqlConnection(Properties.Settings.Default.DBstring);
                conn.Open();
                string partNum = PartNum.Text;
                string command = $"INSERT INTO public.parts(part, barcode) VALUES ({partNum},{BarCode.Text})";
                var cmd = new NpgsqlCommand(command, conn);
                await cmd.ExecuteNonQueryAsync();
            }
            PartNum.Clear();
            PartNum.IsEnabled = false;
            btnUpdate.IsEnabled = false;
            btnCancelU.IsEnabled = false;
            btnAdd.IsEnabled = true;
            btnAdd.IsDefault = true;
            BarCode.IsEnabled = true;
            BarCode.Clear();
            BarCode.Focus();
            btnUpdate.IsDefault = false;

        }

        private void btnCancelU_Click(object sender, RoutedEventArgs e)
        {
            PartNum.IsEnabled = false;
            btnUpdate.IsEnabled = false;
            btnCancelU.IsEnabled = false;
            btnAdd.IsDefault = true;
            BarCode.Focus();
            btnUpdate.IsDefault = false;
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



            ////////////////////////Open Page////////////////////////////////
            IWebDriver driver = new EdgeDriver();
            driver.Navigate().GoToUrl("https://partners.gorenje.com/sagCC/vracilo_vnos.aspx");
            driver.FindElement(By.Id("usr")).SendKeys(Properties.Settings.Default.Username);
            driver.FindElement(By.Id("pwd")).SendKeys(Properties.Settings.Default.Password);

            driver.FindElement(By.Id("btnPrijava")).Click();

            driver.Navigate().GoToUrl("https://partners.gorenje.com/sagCC/vracilo_vnos.aspx");
            //select State
            var selectState = new SelectElement(driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_drpCenter")));
            selectState.SelectByText(State);

            var selectUnit = new SelectElement(driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_drpEnota")));
            selectUnit.SelectByText(serviceUnit);

            var selectTechnician = new SelectElement(driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_DropDownList5")));
            selectTechnician.SelectByText(serviceTechnician);

            //click "create" button
            driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_btnPrikazi")).Click();

            ////////////////////////////Start Input////////////////////////////
            foreach (var p in PartsList.Items)
            {
                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_txt_material")).Clear();
                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_txt_material")).SendKeys(p.ToString());
                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_txt_min")).Click();
                if (driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_btnShrani0")).Enabled == true) {
                    driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_btnShrani0")).Click();
                }
                    
            }
            MessageBox.Show("Input Done");
        }


        private void Technicians_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            btnRun.IsEnabled = true;
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            string excelPath;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".xlsm";
            Nullable<bool> dialogOK = openFileDialog.ShowDialog();
            if (dialogOK == true)
            {
                string path = openFileDialog.FileName;
                excelPath = path;
                if (path.Substring(path.Length - 5, 5) != ".xlsx")
                {
                    MessageBox.Show("Invalid file selected");
                }
                else
                {
                    Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
                    Workbook wb = excel.Workbooks.Open(excelPath);
                    Worksheet ws = wb.Worksheets[1];
                    int limit = ws.UsedRange.Rows.Count;
                    int i = 1;
                    StringCollection prParts = new StringCollection();
                    while (ws.Cells[i, 1].Value2 != null)
                    {
                        PartsList.Items.Add(ws.Cells[i, 1].Value2);
                        prParts.Add(ws.Cells[i, 1].Value2.ToString());
                        i++;
                    }
                    Properties.Settings.Default.PRparts = prParts;
                    Properties.Settings.Default.Save();
                    wb.Save();
                    wb.Close();
                    excel.Quit();
                }
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            Workbook wb = excel.Workbooks.Add(1);
            Worksheet ws = wb.Worksheets[1];
            int i = 1;
            foreach(string part in PartsList.Items)
            {
                ws.Cells[i,1].Value2 = part;
                i++;
            }
            wb.SaveAs("CPartReturnExport.xlsx");

            wb.Close();
            excel.Quit();
            MessageBox.Show("File exported to Documents");
            
        }

        private void btnStockTake_Click(object sender, RoutedEventArgs e)
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

            IWebDriver driver = new EdgeDriver();
            driver.Navigate().GoToUrl("https://partners.gorenje.com/sagCC/Sredina.aspx");
            driver.FindElement(By.Id("usr")).SendKeys(Properties.Settings.Default.Username);
            driver.FindElement(By.Id("pwd")).SendKeys(Properties.Settings.Default.Password);

            driver.FindElement(By.Id("btnPrijava")).Click();

            driver.Navigate().GoToUrl("https://partners.gorenje.com/sagCC/inventura_vnos.aspx");


            var selectState = new SelectElement(driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_drpCenter")));
            selectState.SelectByText(State);


            if(serviceUnit!="Nick De Poilly") {
                var selectUnit = new SelectElement(driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_drpEnota")));
                selectUnit.SelectByText(serviceUnit);

                var selectTechnician = new SelectElement(driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_DropDownList5")));
                selectTechnician.SelectByText(serviceTechnician);
            }
            else
            {
                var selectUnit = new SelectElement(driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_drpEnota")));
                selectUnit.SelectByValue("22404");
                var selectTechnician = new SelectElement(driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_DropDownList5")));
                selectTechnician.SelectByValue("54557");
            }
            



            foreach(var p in PartsList.Items)
            {
                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_txt_material")).Clear();
                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_txt_material")).SendKeys(p.ToString());
                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_txt_min")).Click();
                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_btnShrani0")).Click();
            }
            MessageBox.Show("Input Done");
        }
    }
}
