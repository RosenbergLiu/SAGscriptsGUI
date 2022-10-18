using Npgsql;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System.Collections.Specialized;
using System.Windows;

namespace ASKOmaster
{
    /// <summary>
    /// Interaction logic for PartReturn.xaml
    /// </summary>
    public partial class PartReturn : Window
    {
        public PartReturn()
        {
            InitializeComponent();
            PartNum.IsEnabled = false;
            btnUpdate.IsEnabled = false;
            btnCancelU.IsEnabled = false;
            btnAdd.IsDefault = true;
            btnRun.IsEnabled = false;
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

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (BarCode.Text != "")
            {
                if (BarCode.Text.Length > 6)
                {
                    string Part = "";
                    using var conn = new NpgsqlConnection(Properties.Settings.Default.DBstring);
                    conn.Open();
                    string barcode = BarCode.Text;
                    string command = $"SELECT part FROM public.parts where barcode={barcode};";
                    var cmd = new NpgsqlCommand(command, conn);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Part = reader.GetValue(0).ToString();
                    }
                    if (Part != "")
                    {
                        AddToList(Part);
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
                    AddToList(BarCode.Text);
                }

            }
        }

        public void AddToList(string Part)
        {
            PartsList.Items.Insert(0, Part);
            StringCollection prParts = new StringCollection();
            foreach (string p in PartsList.Items)
            {
                prParts.Add(p);
            }
            Properties.Settings.Default.PRparts = prParts;
            Properties.Settings.Default.Save();
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

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (PartNum.Text != "")
            {
                AddToList(PartNum.Text);
                using var conn = new NpgsqlConnection(Properties.Settings.Default.DBstring);
                conn.Open();
                string partNum = PartNum.Text;
                string command = $"INSERT INTO public.parts(part, barcode) VALUES ({partNum},{BarCode.Text})";
                var cmd = new NpgsqlCommand(command, conn);
                cmd.ExecuteNonQuery();
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
                if (driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_btnShrani0")).Enabled == true)
                    driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_btnShrani0")).Click();
            }
            MessageBox.Show("Input Done");
        }


        private void Technicians_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            btnRun.IsEnabled = true;
        }
    }
}
