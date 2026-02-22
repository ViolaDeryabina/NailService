using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NailService
{
    public partial class SearchClient : Form
    {
        private ClientItem _selectedClient = null;
        private int _currentSelectedId = 0;
        private bool _isUpdatingFromComboBox = false;

        public SearchClient()
        {
            InitializeComponent();
            SetupDataGridView();
            LoadAllClients();
            SetupEventHandlers();
        }

        private void SetupDataGridView()
        {
            // Настройка DataGridView
            dataGridViewClient.AllowUserToAddRows = false;
            dataGridViewClient.AllowUserToDeleteRows = false;
            dataGridViewClient.ReadOnly = true;
            dataGridViewClient.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewClient.MultiSelect = false;
            dataGridViewClient.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewClient.RowHeadersVisible = false;

            // Добавляем колонки
            dataGridViewClient.Columns.Add("ID", "ID");
            dataGridViewClient.Columns.Add("LastName", "Фамилия");
            dataGridViewClient.Columns.Add("FirstName", "Имя");
            dataGridViewClient.Columns.Add("MiddleName", "Отчество");
            dataGridViewClient.Columns.Add("Phone", "Телефон");

            // Скрываем колонку ID
            dataGridViewClient.Columns["ID"].Visible = false;

            // Настройка ширины колонок
            dataGridViewClient.Columns["LastName"].FillWeight = 25;
            dataGridViewClient.Columns["FirstName"].FillWeight = 25;
            dataGridViewClient.Columns["MiddleName"].FillWeight = 25;
            dataGridViewClient.Columns["Phone"].FillWeight = 25;

            // Применяем стили
            StyleManager.ApplyGridStyles(dataGridViewClient);
        }

        private void SetupEventHandlers()
        {
            txtPhone.TextChanged += TxtPhone_TextChanged;
            dataGridViewClient.CellDoubleClick += DataGridViewClient_CellDoubleClick;
        }

        private void LoadAllClients()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT IDClient, LastName, FirstName, MiddleName, Phone 
                        FROM Client 
                        ORDER BY LastName, FirstName";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());

                    FillDataGridView(dt);
                    lblResultCount.Text = $"Всего клиентов: {dt.Rows.Count}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}");
            }
        }

        private void TxtPhone_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingFromComboBox) return;

            string phoneText = txtPhone.Text.Trim();

            if (phoneText.Length >= 3)
            {
                SearchClientsByPhone(phoneText);
            }
            else if (phoneText.Length == 0)
            {
                LoadAllClients();
            }
        }

        private void SearchClientsByPhone(string phone)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();

                    // Очищаем телефон от форматирования для поиска
                    string cleanPhone = InputValidator.GetCleanPhoneNumber(phone);

                    string query = @"
                        SELECT IDClient, LastName, FirstName, MiddleName, Phone 
                        FROM Client 
                        WHERE REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Phone, ' ', ''), '-', ''), '(', ''), ')', ''), '+', '') LIKE @Phone
                        ORDER BY 
                            CASE 
                                WHEN REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Phone, ' ', ''), '-', ''), '(', ''), ')', ''), '+', '') = @ExactPhone THEN 0
                                WHEN REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Phone, ' ', ''), '-', ''), '(', ''), ')', ''), '+', '') LIKE @PhoneStart THEN 1
                                WHEN REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Phone, ' ', ''), '-', ''), '(', ''), ')', ''), '+', '') LIKE @PhoneEnd THEN 2
                                ELSE 3
                            END,
                            LastName, FirstName";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Phone", "%" + cleanPhone + "%");
                    cmd.Parameters.AddWithValue("@ExactPhone", cleanPhone);
                    cmd.Parameters.AddWithValue("@PhoneStart", cleanPhone + "%");
                    cmd.Parameters.AddWithValue("@PhoneEnd", "%" + cleanPhone);

                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());

                    FillDataGridView(dt);

                    if (dt.Rows.Count == 1)
                    {
                        lblResultCount.Text = "Найден 1 клиент";
                    }
                    else if (dt.Rows.Count >= 2 && dt.Rows.Count <= 4)
                    {
                        lblResultCount.Text = $"Найдено {dt.Rows.Count} клиента";
                    }
                    else
                    {
                        lblResultCount.Text = $"Найдено {dt.Rows.Count} клиентов";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска клиентов: {ex.Message}");
            }
        }

        private void FillDataGridView(DataTable dt)
        {
            dataGridViewClient.Rows.Clear();

            foreach (DataRow row in dt.Rows)
            {
                dataGridViewClient.Rows.Add(
                    row["IDClient"],
                    row["LastName"],
                    row["FirstName"],
                    row["MiddleName"],
                    row["Phone"]
                );
            }

            // Выделяем ранее выбранного клиента, если он есть в списке
            if (_currentSelectedId != 0)
            {
                foreach (DataGridViewRow row in dataGridViewClient.Rows)
                {
                    if (Convert.ToInt32(row.Cells["ID"].Value) == _currentSelectedId)
                    {
                        row.Selected = true;
                        dataGridViewClient.FirstDisplayedScrollingRowIndex = row.Index;
                        break;
                    }
                }
            }
        }

        private void DataGridViewClient_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
               // SelectCurrentClient();
            }
        }

        

        public ClientItem GetSelectedClient()
        {
            return _selectedClient;
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (dataGridViewClient.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridViewClient.SelectedRows[0];

                _selectedClient = new ClientItem
                {
                    ID = Convert.ToInt32(selectedRow.Cells["ID"].Value),
                    LastName = selectedRow.Cells["LastName"].Value?.ToString() ?? "",
                    FirstName = selectedRow.Cells["FirstName"].Value?.ToString() ?? "",
                    MiddleName = selectedRow.Cells["MiddleName"].Value?.ToString() ?? "",
                    Phone = selectedRow.Cells["Phone"].Value?.ToString() ?? "",
                    FullName = NameFormatter.FormatToShortName(
                        selectedRow.Cells["LastName"].Value?.ToString() ?? "",
                        selectedRow.Cells["FirstName"].Value?.ToString() ?? "",
                        selectedRow.Cells["MiddleName"].Value?.ToString() ?? ""
                    )
                };

                // Только устанавливаем DialogResult - форма закроется сама
                this.DialogResult = DialogResult.OK;
                // Не вызываем Close()!
            }
            else
            {
                MessageBox.Show("Выберите клиента из списка!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }



        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtPhone.Clear();
            LoadAllClients();
        }
    }
}