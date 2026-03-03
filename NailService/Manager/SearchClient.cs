using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NailService
{
    /// <summary>
    /// Форма для поиска и выбора клиента по номеру телефона
    /// Используется при создании новой записи для быстрого поиска существующего клиента
    /// </summary>
    public partial class SearchClient : Form
    {
        private ClientItem _selectedClient = null;
        private int _currentSelectedId = 0;
        private bool _isUpdatingFromComboBox = false;

        /// <summary>
        /// Конструктор формы поиска клиента
        /// </summary>
        public SearchClient()
        {
            InitializeComponent();
            SetupDataGridView();
            LoadAllClients();
            SetupEventHandlers();
        }

        #region Настройка интерфейса

        /// <summary>
        /// Настройка DataGridView для отображения списка клиентов
        /// </summary>
        private void SetupDataGridView()
        {
            dataGridViewClient.AllowUserToAddRows = false;
            dataGridViewClient.AllowUserToDeleteRows = false;
            dataGridViewClient.ReadOnly = true;
            dataGridViewClient.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewClient.MultiSelect = false;
            dataGridViewClient.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewClient.RowHeadersVisible = false;

            dataGridViewClient.Columns.Add("ID", "ID");
            dataGridViewClient.Columns.Add("LastName", "Фамилия");
            dataGridViewClient.Columns.Add("FirstName", "Имя");
            dataGridViewClient.Columns.Add("MiddleName", "Отчество");
            dataGridViewClient.Columns.Add("Phone", "Телефон");

            dataGridViewClient.Columns["ID"].Visible = false;

            dataGridViewClient.Columns["LastName"].FillWeight = 25;
            dataGridViewClient.Columns["FirstName"].FillWeight = 25;
            dataGridViewClient.Columns["MiddleName"].FillWeight = 25;
            dataGridViewClient.Columns["Phone"].FillWeight = 25;

            StyleManager.ApplyGridStyles(dataGridViewClient);
        }

        /// <summary>
        /// Подписка на события формы
        /// </summary>
        private void SetupEventHandlers()
        {
            txtPhone.TextChanged += TxtPhone_TextChanged;
            dataGridViewClient.CellDoubleClick += DataGridViewClient_CellDoubleClick;
        }

        #endregion

        #region Загрузка и поиск данных

        /// <summary>
        /// Загрузка всех клиентов из базы данных
        /// </summary>
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

        /// <summary>
        /// Обработчик изменения текста в поле поиска
        /// Запускает поиск при вводе 3 и более символов
        /// </summary>
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

        /// <summary>
        /// Поиск клиентов по номеру телефона с ранжированием результатов
        /// </summary>
        /// <param name="phone">Введенный номер телефона</param>
        private void SearchClientsByPhone(string phone)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();

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
                    UpdateResultCountMessage(dt.Rows.Count);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска клиентов: {ex.Message}");
            }
        }

        /// <summary>
        /// Обновление сообщения о количестве найденных клиентов
        /// </summary>
        private void UpdateResultCountMessage(int count)
        {
            if (count == 1)
            {
                lblResultCount.Text = "Найден 1 клиент";
            }
            else if (count >= 2 && count <= 4)
            {
                lblResultCount.Text = $"Найдено {count} клиента";
            }
            else
            {
                lblResultCount.Text = $"Найдено {count} клиентов";
            }
        }

        /// <summary>
        /// Заполнение DataGridView данными из DataTable
        /// </summary>
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

            // Восстановление выделения ранее выбранного клиента
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

        #endregion

        #region Обработка выбора клиента

        /// <summary>
        /// Обработчик двойного клика по строке клиента
        /// </summary>
        private void DataGridViewClient_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Можно реализовать автоматический выбор при двойном клике
            }
        }

        /// <summary>
        /// Получение выбранного клиента
        /// </summary>
        public ClientItem GetSelectedClient()
        {
            return _selectedClient;
        }

        /// <summary>
        /// Обработчик кнопки "Выбрать" - сохранение выбранного клиента
        /// </summary>
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

                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Выберите клиента из списка!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Обработчик кнопки "Отмена" - закрытие формы без выбора
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Обработчик кнопки "Очистить" - сброс поиска и загрузка всех клиентов
        /// </summary>
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtPhone.Clear();
            LoadAllClients();
        }

        #endregion
    }
}