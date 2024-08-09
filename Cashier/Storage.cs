using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using QRCoder;
using Newtonsoft.Json;

namespace Cashier
{
    public partial class Storage : Form
    {
        string connectionString = "server=localhost;database=net_cashier;uid=root;pwd=;";

        public Storage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM barang";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dataGridView1.DataSource = dataTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Pastikan baris yang diklik valid
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                // Ambil nilai dari baris yang diklik
                string name = row.Cells["nama"].Value?.ToString();
                string code = row.Cells["kode"].Value?.ToString();
                string price = row.Cells["harga"].Value?.ToString();
                string stock = row.Cells["stok"].Value?.ToString();

                txtNamaBarang.Text = name;
                txtKodeBarang.Text = code;
                numericHargaBarang.Value = decimal.TryParse(price, out decimal parsedPrice) ? parsedPrice : 0;
                numericStok.Value = decimal.TryParse(stock, out decimal parsedStock) ? parsedStock : 0;

                // Generate QR Code untuk barang yang dipilih
                GenerateQRCode(name, code, price, stock);
            }
        }


        private void GenerateQRCode(string name, string code, string price, string stock)
        {
            var item = new
            {
                Name = name,
                Code = code,
                Price = price,
                Stock = stock
            };

            string qrText = JsonConvert.SerializeObject(item);

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(10);

            pictureBoxQRCode.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBoxQRCode.Image = qrCodeImage;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                string name = txtNamaBarang.Text;
                string code = txtKodeBarang.Text;
                string price = numericHargaBarang.Value.ToString();
                string stock = numericStok.Value.ToString();

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO barang (nama, kode, harga, stok) VALUES (@name, @code, @price, @stock)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@code", code);
                        cmd.Parameters.AddWithValue("@price", price);
                        cmd.Parameters.AddWithValue("@stock", stock);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Data successfully inserted!");
                            LoadData();
                        }
                        else
                        {
                            MessageBox.Show("Data insertion failed.");
                        }
                    }
                }

                GenerateQRCode(name, code, price, stock);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    string selectedCode = dataGridView1.SelectedRows[0].Cells["kode"].Value.ToString();
                    string name = txtNamaBarang.Text;
                    string code = txtKodeBarang.Text;
                    string price = numericHargaBarang.Value.ToString();
                    string stock = numericStok.Value.ToString();

                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "UPDATE barang SET nama = @name, kode = @code, harga = @price, stok = @stock WHERE kode = @selectedCode";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@name", name);
                            cmd.Parameters.AddWithValue("@code", code);
                            cmd.Parameters.AddWithValue("@price", price);
                            cmd.Parameters.AddWithValue("@stock", stock);
                            cmd.Parameters.AddWithValue("@selectedCode", selectedCode);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Data successfully updated!");
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("Data update failed.");
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select a record to update.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void buttonDelet_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    string selectedCode = dataGridView1.SelectedRows[0].Cells["kode"].Value.ToString();

                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "DELETE FROM barang WHERE kode = @selectedCode";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@selectedCode", selectedCode);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Data successfully deleted!");
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("Data deletion failed.");
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select a record to delete.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }
    }


}
