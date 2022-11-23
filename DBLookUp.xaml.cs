using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using Npgsql;
using System;
using System.IO;
using System.Media;
using System.Windows;

namespace ASKOmaster
{
    /// <summary>
    /// Interaction logic for ExcelLookUp.xaml
    /// </summary>
    public partial class DBLookUp : System.Windows.Window
    {
        public DBLookUp()
        {
            InitializeComponent();
            LookupText.Focus();
        }



        private async void btnLookUp_Click(object sender, RoutedEventArgs e)
        {
            bool freighted = false;
            await using var conn = new NpgsqlConnection(Properties.Settings.Default.DBstring);
            conn.Open();
            string barcode = LookupText.Text;
            string command = $"SELECT * FROM public.freight f LEFT JOIN public.parts p ON f.part=p.part WHERE barcode={barcode};";

            var cmd = new NpgsqlCommand(command, conn);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Object[] values = new Object[reader.FieldCount];
                    int fieldCount = reader.GetValues(values);
                    for (int i = 1; i < fieldCount-4; i++)
                    {
                        if (Convert.ToInt32(values[i]) > 0)
                        {
                            freighted = true;
                            using (FileStream stream = File.Open(@"AddSound.wav", FileMode.Open))
                            {
                                SoundPlayer myNewSound = new SoundPlayer(stream);
                                myNewSound.Load();
                                myNewSound.Play();
                            }
                            break;

                        }
                    }
                    if (freighted==false)
                    {
                        
                    }

                    ResultBlock.Text = freighted.ToString();
                    
                }
            }
            LookupText.Clear();
            LookupText.Focus();
        }
    }
}
