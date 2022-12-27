using CsvHelper;
using Microsoft.Office.Interop.Excel;
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
    public partial class Daily : System.Windows.Window
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


            Thread.Sleep(3000);



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
                    driver.Quit();
                    downloaded = true; break;
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
            PgBar.Value = 100;
        }

        public async Task Import()
        {
            string CSV = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\ASPxGridView1.csv";
            var reader = new StreamReader(CSV);
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var dr = new CsvDataReader(csv);
            var dt = new System.Data.DataTable();
            dt.Load(dr);
            int RowCount = dt.Rows.Count;
            int progress = 1;
            string command = "";
            await using var conn = new NpgsqlConnection(Properties.Settings.Default.DBstring);
            conn.Open();
            foreach (DataRow row in dt.Rows)
            {

                try
                {
                    string DATEstr = row["Claim date"].ToString();
                    if (DATEstr != null)
                    {
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
                                    PARTS = PARTS + p.Split(' ')[0] + ",";
                                }
                                PARTS = PARTS.Remove(PARTS.Length - 1);
                                PARTS = PARTS + "]";
                            }

                            string MODEL = row["Art./model number"].ToString();
                            string ART = row["Code"].ToString();
                            string TYPE = row["Type of Repair"].ToString().Split(" ")[0];




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
                    }



                    var ratio = (((double)progress * 1000 / RowCount));
                    PgBar.Value = Math.Ceiling(ratio) / 10;
                    progress++;
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message + "=======================" + command);
                }
            }
            conn.Close();
            try
            {
                File.Delete(CSV);
            }
            catch (System.Exception ex) { MessageBox.Show(ex.Message); }

            MessageBox.Show("Import Done");

        }
        public void UpdateDelivery(string idoss, IWebDriver driver, string DN, bool Pickup)
        {
            driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_txt_oss")).Clear();
            driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_txt_oss")).SendKeys(idoss);
            driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_btnPrikazi")).Click();

            if (DN != "None")
            {
                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_ASPxGridView1_cell0_37_tb_opomba_material_I")).Clear();
                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_ASPxGridView1_cell0_37_tb_opomba_material_I")).SendKeys(DN);
            }
            IWebElement DateInput = driver.FindElement(By.XPath($"//*[@id=\"ctl00_ContentPlaceHolder1_ASPxGridView1_cell0_36_txtBoxX{idoss}_I\"]"));

            for (int i = 1; i < 11; i++)
            {
                DateInput.SendKeys(Keys.Backspace);
            }

            if (Pickup)
            {
                DateTime PickupDate = DateTime.Now.AddDays(2);
                string InputDate = DateFormater(PickupDate);
                driver.FindElement(By.XPath($"//*[@id=\"ctl00_ContentPlaceHolder1_ASPxGridView1_cell0_36_txtBoxX{idoss}_I\"]")).SendKeys(InputDate);
                driver.FindElement(By.XPath($"//*[@id=\"ctl00_ContentPlaceHolder1_ASPxGridView1_cell0_36_txtBoxX{idoss}_I\"]")).SendKeys(Keys.Enter);
            }
            else
            {
                DateTime PickupDate = DateTime.Now.AddDays(7);
                string InputDate = DateFormater(PickupDate);
                driver.FindElement(By.XPath($"//*[@id=\"ctl00_ContentPlaceHolder1_ASPxGridView1_cell0_36_txtBoxX{idoss}_I\"]")).SendKeys(InputDate);
                driver.FindElement(By.XPath($"//*[@id=\"ctl00_ContentPlaceHolder1_ASPxGridView1_cell0_36_txtBoxX{idoss}_I\"]")).SendKeys(Keys.Enter);
            }
        }

        public string DateFormater(DateTime TargetDate)
        {
            string day = TargetDate.ToString("d");
            var daySet = day.Split("/");
            if (daySet[0] == "1" | daySet[0] == "2" | daySet[0] == "3")
            {
                daySet[0] = daySet[0] + "/";
            }
            string result = daySet[0] + daySet[1] + daySet[2];

            return result;
        }

        public async void Update()
        {
            PgBar.Value = 0;
            IWebDriver driver1 = new EdgeDriver();
            IWebDriver driver2 = new EdgeDriver();
            await iniJobPage(driver1);
            await iniListPage(driver2);

            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            Workbook wb = excel.Workbooks.Add(1);
            Worksheet ws = wb.Worksheets[1];

            List<List<string?>> output = new List<List<string?>>();

            List<string> JobList = new List<string>();
            string DN = "";
            await using var conn = new NpgsqlConnection(Properties.Settings.Default.DBstring);
            conn.Open();
            string command = @"SELECT idoss,parts,tech FROM public.jobs WHERE wos='W' AND claim_date<CURRENT_DATE;";
            var cmd = new NpgsqlCommand(command, conn);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    string job = reader.GetValue(0).ToString();
                    if (job != null)
                    {
                        JobList.Add(job);
                    }
                }
            }
            conn.Close();
            int JobCount = JobList.Count;
            int JobProgress = 0;
            foreach (string JobNumber in JobList)
            {
                /////For each record of W job//////
                if (JobNumber != null)
                {
                    string WOS = "R";

                    List<string> SAP = getSAP(JobNumber, driver1);
                    foreach (string s in SAP)
                    {
                        string CaseResult = await CheckSAP(s, driver1, output, JobNumber);
                        if (CaseResult == "not in stock")
                        {
                            WOS = "RPOOS";
                        }
                        if (CaseResult.Length == 10)
                        {
                            DN = CaseResult;
                        }

                    }

                    driver1.FindElement(By.Id("ctl00_ContentPlaceHolder1_ASPxComboBox1_I")).Clear();
                    if (WOS == "R")
                    {
                        driver1.FindElement(By.Id("ctl00_ContentPlaceHolder1_ASPxComboBox1_I")).SendKeys($"R - Return parts confirmed");
                    }
                    else
                    {
                        driver1.FindElement(By.Id("ctl00_ContentPlaceHolder1_ASPxComboBox1_I")).SendKeys($"RPOOS - Return parts out of stock");
                    }

                    driver1.FindElement(By.Id("ctl00_ContentPlaceHolder1_ASPxComboBox1_I")).SendKeys(Keys.Enter);

                    ///////////Update Job State in DB////////////////
                    await using var conn2 = new NpgsqlConnection(Properties.Settings.Default.DBstring);
                    conn2.Open();
                    string UpdateCommand = $"UPDATE public.jobs SET wos='{WOS}' WHERE idoss={JobNumber};";
                    var UpdateCmd = new NpgsqlCommand(UpdateCommand, conn2);
                    await UpdateCmd.ExecuteNonQueryAsync();

                    /////////Find Pickup or not//////////////////////
                    string Tech = driver1.FindElement(By.Id("ctl00_ContentPlaceHolder1_txtServiser")).Text;
                    await using var conn3 = new NpgsqlConnection(Properties.Settings.Default.DBstring);
                    conn3.Open();
                    string TechCommand = $"SELECT pick_up FROM public.technicians WHERE service_unit='{Tech}';";
                    var TechCmd = new NpgsqlCommand(TechCommand, conn3);
                    bool PickUp = true;
                    await using (var TechReader = await TechCmd.ExecuteReaderAsync())
                    {

                        while (await TechReader.ReadAsync())
                        {
                            string isPickUp = TechReader.GetValue(0).ToString();
                            if (isPickUp == "False")
                            {
                                PickUp = false;
                            }
                        }
                    }
                    if (WOS == "R")
                    {
                        UpdateDelivery(JobNumber, driver2, DN, PickUp);
                    }
                }
                JobProgress++;
                var ratio = (((double)JobProgress * 1000 / JobCount));
                PgBar.Value = Math.Ceiling(ratio) / 10;
            }


            for (int i = 1; i < output.Count + 1; i++)
            {
                ws.Cells[i, 1] = output[i - 1][0];
                ws.Cells[i, 2] = output[i - 1][1];
                ws.Cells[i, 3] = output[i - 1][2];
            }

            driver1.Close();
            driver2.Close();
            driver1.Quit();
            driver2.Quit();



            string Today = DateTime.Now.ToString("dd-MM-yyyy");
            wb.SaveAs($"{Today} - RPOOS.xlsx");
            wb.Close();

            excel.Quit();
            PgBar.Value = 100;
        }

        public Task iniJobPage(IWebDriver driver)
        {
            driver.Navigate().GoToUrl("https://partners.gorenje.com/sagCC/sredina.aspx");
            driver.FindElement(By.Id("usr")).SendKeys(Properties.Settings.Default.Username);
            driver.FindElement(By.Id("pwd")).SendKeys(Properties.Settings.Default.Password);
            driver.FindElement(By.Id("btnPrijava")).Click();
            return Task.CompletedTask;
        }

        public Task iniListPage(IWebDriver driver)
        {
            driver.Navigate().GoToUrl("https://partners.gorenje.com/sagCC/sredina.aspx");
            driver.FindElement(By.Id("usr")).SendKeys(Properties.Settings.Default.Username);
            driver.FindElement(By.Id("pwd")).SendKeys(Properties.Settings.Default.Password);
            driver.FindElement(By.Id("btnPrijava")).Click();
            driver.Navigate().GoToUrl("https://partners.gorenje.com/sagCC/potni_pregled1.aspx");

            var selectState = new SelectElement(driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_drpCenter")));
            selectState.SelectByValue("0");
            Thread.Sleep(3000);


            //*[@id="ctl00_ContentPlaceHolder1_dtDatumFilter_I"]
            for (int i = 1; i < 11; i++)
            {
                driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_dtDatumFilter_I")).SendKeys(Keys.Backspace);
            }
            driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_dtDatumFilter_I")).SendKeys(Keys.Enter);
            driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_txt_oss")).SendKeys("11111111");
            driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_txt_oss")).Click();
            return Task.CompletedTask;
        }




        private void GetCSV_Click(object sender, RoutedEventArgs e)
        {
            GetJobs();
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            PgBar.Value = 0;
            Import();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            Update();
        }

        public async Task<string> CheckSAP(string SAP, IWebDriver driver, List<List<string?>> output, string idoss)
        {
            string Case = "no record";///no record
            bool HasRecord = false;
            string DN = "";
            await using var conn = new NpgsqlConnection(Properties.Settings.Default.DBstring);
            conn.Open();
            string command = $"SELECT release_date,part,delivery_note FROM public.release WHERE sap={SAP};";
            var cmd = new NpgsqlCommand(command, conn);

            await using (var reader = await cmd.ExecuteReaderAsync())
            {

                while (await reader.ReadAsync())
                {
                    HasRecord = true;
                    DN = reader.GetValue(2).ToString();
                    string part = reader.GetValue(1).ToString();
                    if (DN == "")
                    {
                        Case = "not in stock";
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_tbKomentar_I")).Clear();
                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_tbKomentar_I")).SendKeys($"{part} not in stock, checking Workshop. ");

                        driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_btnKomentarDodaj")).Click();

                        List<string?> record = new List<string?>();
                        record.Add(idoss);
                        record.Add(SAP);
                        record.Add(part);
                        output.Add(record);

                    }
                }
            }

            if (HasRecord == false)
            {
                Case = "no record";
            }
            else
            {
                if (Case != "not in stock")
                {
                    Case = DN;
                }
            }
            return Case;
        }

        public List<string> getSAP(string idoss, IWebDriver driver)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            driver.Navigate().GoToUrl($"https://partners.gorenje.com/sagCC/oss.aspx?id_oss={idoss}&akcija=zakljucek");
            List<string> SAPs = new List<string>();
            bool HaveNextPage = true;
            /////Analyze Each Page/////
            while (HaveNextPage == true)
            {
                IWebElement tbl = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_gvNarocenMat_DXMainTable"));
                IReadOnlyList<IWebElement> rows = tbl.FindElements(By.ClassName("dxgvDataRow_MetropolisBlue"));

                /////Analyze Each Row/////
                foreach (IWebElement row in rows)
                {
                    IReadOnlyList<IWebElement> cols = row.FindElements(By.ClassName("dxgv"));
                    string SAP = cols[7].Text;
                    if (SAPs.Contains(SAP) == false)
                    {
                        SAPs.Add(SAP);
                    }
                }

                /////If have next Page/////
                string[] PageCount = driver.FindElement(By.XPath(@"//*[@id=""ctl00_ContentPlaceHolder1_gvNarocenMat_DXPagerBottom""]/b[1]")).Text.Split(" ");
                if (PageCount[1] != PageCount[3])
                {
                    driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_gvNarocenMat_DXPagerBottom_PBN")).Click();

                    bool Went = false;
                    while (Went == false)
                    {
                        Thread.Sleep(1000);
                        string[] NewPageCount = driver.FindElement(By.XPath(@"//*[@id=""ctl00_ContentPlaceHolder1_gvNarocenMat_DXPagerBottom""]/b[1]")).Text.Split(" ");

                        if (NewPageCount[1] != PageCount[1])
                        {
                            Went = true;
                        }
                    }

                }
                else
                {
                    HaveNextPage = false;
                }
            }
            return SAPs;

        }

        private void test_Click(object sender, RoutedEventArgs e)
        {
            DateTime PickupDate = DateTime.Now.AddDays(28);
            string InputDate = DateFormater(PickupDate);
            MessageBox.Show(InputDate);
        }
    }
}
