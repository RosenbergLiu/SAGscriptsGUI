using CsvHelper;
using Microsoft.Win32;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows;

namespace ASKOmaster
{
    /// <summary>
    /// Interaction logic for Import.xaml
    /// </summary>
    public partial class Import : System.Windows.Window
    {
        public Import()
        {
            InitializeComponent();
            dbSelector.Items.Add("Release");
            dbSelector.Items.Add("Jobs");

        }

        private void SelectCSV_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".xlsm";
            Nullable<bool> dialogOK = openFileDialog.ShowDialog();
            if (dialogOK == true)
            {
                CSVpath.Text = openFileDialog.FileName;
            }

        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {

            string CSV = CSVpath.Text;
            var reader = new StreamReader(CSV);
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var dr = new CsvDataReader(csv);
            var dt = new System.Data.DataTable();
            MessageBox.Show(dt.Rows.Count.ToString());
            foreach (DataRow row in dt.Rows)
            {
                using (var db = new SparePartsContext())
                {
                    MessageBox.Show("aa");
                    var record = new Release()
                    {
                        sap = row["sap"].ToString(),
                        date = DateTime.Parse(row["sap"].ToString()),
                        part = row["part"].ToString(),
                        quantity = int.Parse(row["quantity"].ToString()),
                        dn = row["dn"].ToString()

                    };
                    db.Release.Add(record);
                    db.SaveChanges();
                }
            }
        }
    }
}
