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
    public partial class Form1 : Form
    {
        string connectionString = "server=localhost;database=net_cashier;uid=root;pwd=;";

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
            StartHttpServer();
        }

        private void InitializeCustomComponents()
        {
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.textBoxCode = new System.Windows.Forms.TextBox();
            this.textBoxPrice = new System.Windows.Forms.TextBox();
            this.textBoxStock = new System.Windows.Forms.TextBox();
            this.buttonSave = new System.Windows.Forms.Button();
            this.pictureBoxQRCode = new System.Windows.Forms.PictureBox();
            this.dataGridViewScannedItems = new System.Windows.Forms.DataGridView();

            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxQRCode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewScannedItems)).BeginInit();
            this.SuspendLayout();

            // textBoxName
            this.textBoxName.Location = new System.Drawing.Point(12, 12);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(260, 20);
            this.textBoxName.TabIndex = 0;

            // textBoxCode
            this.textBoxCode.Location = new System.Drawing.Point(12, 38);
            this.textBoxCode.Name = "textBoxCode";
            this.textBoxCode.Size = new System.Drawing.Size(260, 20);
            this.textBoxCode.TabIndex = 1;

            // textBoxPrice
            this.textBoxPrice.Location = new System.Drawing.Point(12, 64);
            this.textBoxPrice.Name = "textBoxPrice";
            this.textBoxPrice.Size = new System.Drawing.Size(260, 20);
            this.textBoxPrice.TabIndex = 2;

            // textBoxStock
            this.textBoxStock.Location = new System.Drawing.Point(12, 90);
            this.textBoxStock.Name = "textBoxStock";
            this.textBoxStock.Size = new System.Drawing.Size(260, 20);
            this.textBoxStock.TabIndex = 3;

            // buttonSave
            this.buttonSave.Location = new System.Drawing.Point(12, 116);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(260, 23);
            this.buttonSave.TabIndex = 4;
            this.buttonSave.Text = "Save and Generate QR Code";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);

            // pictureBoxQRCode
            this.pictureBoxQRCode.Location = new System.Drawing.Point(12, 145);
            this.pictureBoxQRCode.Name = "pictureBoxQRCode";
            this.pictureBoxQRCode.Size = new System.Drawing.Size(260, 260);
            this.pictureBoxQRCode.TabIndex = 5;
            this.pictureBoxQRCode.TabStop = false;

            // dataGridViewScannedItems
            this.dataGridViewScannedItems.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewScannedItems.Location = new System.Drawing.Point(12, 420);
            this.dataGridViewScannedItems.Name = "dataGridViewScannedItems";
            this.dataGridViewScannedItems.Size = new System.Drawing.Size(260, 150);
            this.dataGridViewScannedItems.TabIndex = 6;

            // Add columns to the DataGridView
            this.dataGridViewScannedItems.ColumnCount = 4;
            this.dataGridViewScannedItems.Columns[0].Name = "Name";
            this.dataGridViewScannedItems.Columns[1].Name = "Code";
            this.dataGridViewScannedItems.Columns[2].Name = "Price";
            this.dataGridViewScannedItems.Columns[3].Name = "Stock";

            // Optional: Set column width mode to fill
            this.dataGridViewScannedItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Form1
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 600);
            this.Controls.Add(this.dataGridViewScannedItems);
            this.Controls.Add(this.pictureBoxQRCode);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.textBoxStock);
            this.Controls.Add(this.textBoxPrice);
            this.Controls.Add(this.textBoxCode);
            this.Controls.Add(this.textBoxName);
            this.Name = "Form1";
            this.Text = "Net Cashier";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxQRCode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewScannedItems)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            string name = textBoxName.Text;
            string code = textBoxCode.Text;
            string price = textBoxPrice.Text;
            string stock = textBoxStock.Text;

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
                    cmd.ExecuteNonQuery();
                }
            }

            GenerateQRCode(name, code, price, stock);
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

            // Serialize the object to JSON format
            string qrText = JsonConvert.SerializeObject(item);

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(10);

            pictureBoxQRCode.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBoxQRCode.Image = qrCodeImage;
        }


        private void StartHttpServer()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://192.168.1.179:8000/");
            listener.Start();
            Console.WriteLine("Server started, listening on http://192.168.1.179:8000/");
            listenerThread = new Thread(Listen);
            listenerThread.Start();
        }

        private void Listen()
        {
            while (listener.IsListening)
            {
                var context = listener.GetContext();
                var request = context.Request;
                var response = context.Response;

                // Add CORS headers
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 204;
                    response.Close();
                    continue;
                }

                if (request.HttpMethod == "POST" && request.HasEntityBody)
                {
                    using (var body = request.InputStream)
                    using (var reader = new StreamReader(body, request.ContentEncoding))
                    {
                        var json = reader.ReadToEnd();
                        var item = JsonConvert.DeserializeObject<ScannedItem>(json);

                        // Update the DataGridView
                        Invoke(new Action(() =>
                        {
                            dataGridViewScannedItems.Rows.Add(item.Name, item.Code, item.Price, item.Stock);
                        }));
                    }
                }

                response.StatusCode = 200;
                response.Close();
            }
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            listener.Stop();
            listenerThread.Abort();
        }

        public class ScannedItem
        {
            public string Name { get; set; }
            public string Code { get; set; }
            public string Price { get; set; }
            public string Stock { get; set; }
        }

        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.TextBox textBoxCode;
        private System.Windows.Forms.TextBox textBoxPrice;
        private System.Windows.Forms.TextBox textBoxStock;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.PictureBox pictureBoxQRCode;
        private System.Windows.Forms.DataGridView dataGridViewScannedItems;
        private HttpListener listener;
        private Thread listenerThread;
    }
}
