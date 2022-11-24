using CsvHelper;
using Npgsql;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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
            PgBar.Value = 100;
        }




        public void GetJobs()
        {
            PgBar.Value = 0;
            IWebDriver driver = new EdgeDriver();
            driver.Navigate().GoToUrl("https://partners.gorenje.com/sagCC/sredina.aspx");
            driver.FindElement(By.Id("usr")).SendKeys(Properties.Settings.Default.Username);
            driver.FindElement(By.Id("pwd")).SendKeys(Properties.Settings.Default.Password);
            driver.FindElement(By.Id("btnPrijava")).Click();
            driver.Navigate().GoToUrl("https://partners.gorenje.com/sagCC/potni_pregled1.aspx");

            PgBar.Value = 10;
            var selectState = new SelectElement(driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_drpCenter")));
            selectState.SelectByValue("0");
            driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_dtDatumFilter_B-1Img")).Click();
            driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_dtDatumFilter_DDD_C_BC")).Click();
            driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_btnPrikazi")).Click();
            driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_lnkBCsv")).Click();
            PgBar.Value = 50;
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";

            bool downloaded = false;
            while (downloaded == false)
            {
                if (File.Exists(folder + @"\ASPxGridView1.csv"))
                {
                    driver.Close();
                    downloaded = true; break;
                }
                else
                {
                    Thread.Sleep(5);
                }
            }
            PgBar.Value = 100;
        }

        public async Task Import()
        {
            PgBar.Value = 0;
            string CSV = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\ASPxGridView1.csv";
            var reader = new StreamReader(CSV);
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var dr = new CsvDataReader(csv);
            var dt = new DataTable();
            dt.Load(dr);
            int RowCount = dt.Rows.Count;
            int progress = 1;
            foreach (DataRow row in dt.Rows)
            {
                string command="";
                try
                {
                    string DATEstr = row["Claim date"].ToString();
                    string[] DATEele = DATEstr.Split('/');
                    if (DATEele[2] == "2022")
                    {
                        string DATE = DATEele[2] + "-" + DATEele[1] + "-" + DATEele[0];

                        string IDOSS = row["ID OSS"].ToString();
                        string WOS = row["Work order status"].ToString().Split(" ")[0];

                        string TECH = row["Service technician"].ToString();
                        TECH = TECH.Replace(@"'", @"''");


                        string PARTstr = row["Ordered material"].ToString();
                        string PARTS;
                        if (PARTstr == "")
                        {
                           PARTS = "null";
                        }
                        else
                        {
                            string[] PARTele = PARTstr.Split(", ");
                            PARTS = "ARRAY["; 
                            foreach (string p in PARTele)
                            {
                                PARTS=PARTS+p.Split(' ')[0]+",";
                            }
                            PARTS=PARTS.Remove(PARTS.Length-1);
                            PARTS = PARTS + "]";
                        }
                        
                        string MODEL = row["Art./model number"].ToString();
                        string ART = row["Code"].ToString();
                        string TYPE = row["Type of Repair"].ToString().Split(" ")[0];

                        

                        await using var conn = new NpgsqlConnection(Properties.Settings.Default.DBstring);
                        conn.Open();
                        command =
                            $"INSERT INTO " +
                            $"public.jobs (idoss,wos,claim_date,tech,parts,model,art,repair_type) " +
                            $"VALUES " +
                            $"({IDOSS},'{WOS}','{DATE}','{TECH}',{PARTS},'{MODEL}','{ART}','{TYPE}') " +
                            $"ON CONFLICT (IDOSS) " +
                            $"DO UPDATE SET " +
                            $"idoss=EXCLUDED.idoss," +
                            $"parts=EXCLUDED.parts," +
                            $"tech=EXCLUDED.tech," +
                            $"wos=EXCLUDED.wos," +
                            $"claim_date=EXCLUDED.claim_date," +
                            $"repair_type=EXCLUDED.repair_type;";
                        var cmd = new NpgsqlCommand(command, conn);
                        await cmd.ExecuteNonQueryAsync();
                    }


                    var ratio = (((double)progress *1000 / RowCount));
                    PgBar.Value= Math.Ceiling(ratio)/10;
                    progress++;



                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message+"======================="+command);
                }



            }
            MessageBox.Show("Import Done");
        }


        public async void update()
        {

            IWebDriver driver1 = new EdgeDriver();
            IWebDriver driver2 = new EdgeDriver();
            await iniJobPage(driver1);
            await iniListPage(driver2);

        }

        public async Task iniJobPage(IWebDriver driver)
        {
            driver.Navigate().GoToUrl("https://partners.gorenje.com/sagCC/sredina.aspx");
            driver.FindElement(By.Id("usr")).SendKeys(Properties.Settings.Default.Username);
            driver.FindElement(By.Id("pwd")).SendKeys(Properties.Settings.Default.Password);
            driver.FindElement(By.Id("btnPrijava")).Click();
            driver.Navigate().GoToUrl("https://partners.gorenje.com/sagCC/potni_pregled1.aspx");

            var selectState = new SelectElement(driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_drpCenter")));
            selectState.SelectByValue("0");
            var selectType = new SelectElement(driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_ddStatusiFilter")));
            selectType.SelectByValue("R");
            driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_dtDatumFilter_B-1Img")).Click();
            driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_dtDatumFilter_DDD_C_BC")).Click();

        }

        public async Task iniListPage(IWebDriver driver)
        {
            driver.Navigate().GoToUrl("https://partners.gorenje.com/sagCC/sredina.aspx");
            driver.FindElement(By.Id("usr")).SendKeys(Properties.Settings.Default.Username);
            driver.FindElement(By.Id("pwd")).SendKeys(Properties.Settings.Default.Password);
            driver.FindElement(By.Id("btnPrijava")).Click();
            driver.Navigate().GoToUrl("https://partners.gorenje.com/sagCC/potni_pregled1.aspx");

            var selectState = new SelectElement(driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_drpCenter")));
            selectState.SelectByValue("0");
            driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_dtDatumFilter_B-1Img")).Click();
            driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_dtDatumFilter_DDD_C_BC")).Click();

        }




        private void GetCSV_Click(object sender, RoutedEventArgs e)
        {
            GetJobs();
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            Import();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
