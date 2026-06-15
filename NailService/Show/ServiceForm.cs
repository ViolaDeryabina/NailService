using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NailService
{
    public partial class ServiceForm : Form
    {
        private string _connection;
        private int _roleID;
        private string _userName;
        private int _userId;
        private EditUserClass _editUserClass;
        private ImageService _imageService;

        private DataTable _originalDataTable; // Храним оригинальные данные для фильтрации
        private string _currentSearchText = "";
        private int _currentCategoryFilter = 0; // 0 - все категории
        private string _currentSortOrder = ""; // "ASC", "DESC" или ""

        public ServiceForm(string FIO, int RoleID, int userId, string login = null)
        {
            InitializeComponent();
            _roleID = RoleID;
            _userName = FIO;

            if (RoleID == 2)
            {
                txtFIO.Text = $"Админ: {FIO}";
            }
            else if (RoleID == 4)
            {
                txtFIO.Text = $"Менеджер: {FIO}";
                button2.Visible = false;
            }

            _userId = userId;
            _connection = Connection.ConnectionString;
            _editUserClass = new EditUserClass();
            _imageService = new ImageService();

            ConfigureDataGridView();
            ConfigureByRole();
            InitializeSearchAndFilterControls();
            LoadServicesData();
        }

        private MySqlConnection GetNewConnection() => new MySqlConnection(_connection);

        /// <summary>
        /// Инициализация элементов поиска, фильтра и сортировки
        /// </summary>
        private void InitializeSearchAndFilterControls()
        {
            // Поиск по названию
            txtSearch.TextChanged += TxtSearch_TextChanged;

            // Фильтр по категории
            LoadCategoriesToComboBox();
            cmbCategoryFilter.SelectedIndexChanged += CmbCategoryFilter_SelectedIndexChanged;

            // Сортировка по цене
            cmbSortPrice.Items.Add("Без сортировки");
            cmbSortPrice.Items.Add("Цена: по возрастанию");
            cmbSortPrice.Items.Add("Цена: по убыванию");
            cmbSortPrice.SelectedIndex = 0;
            cmbSortPrice.SelectedIndexChanged += CmbSortPrice_SelectedIndexChanged;
        }

        /// <summary>
        /// Загрузка категорий в комбобокс фильтра
        /// </summary>
        private void LoadCategoriesToComboBox()
        {
            try
            {
                using (var connection = GetNewConnection())
                {
                    connection.Open();
                    string query = "SELECT IDCategory, CategoryName FROM Category WHERE IsActive = 1 ORDER BY CategoryName";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Добавляем пункт "Все категории" в начало DataTable
                    DataRow allRow = dt.NewRow();
                    allRow["IDCategory"] = 0;
                    allRow["CategoryName"] = "Все категории";
                    dt.Rows.InsertAt(allRow, 0);

                    cmbCategoryFilter.DisplayMember = "CategoryName";
                    cmbCategoryFilter.ValueMember = "IDCategory";
                    cmbCategoryFilter.DataSource = dt;
                    cmbCategoryFilter.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
            }
        }

        /// <summary>
        /// Поиск по названию услуги
        /// </summary>
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            _currentSearchText = txtSearch.Text.Trim();
            ApplyFiltersAndSort();
        }

        /// <summary>
        /// Фильтр по категории
        /// </summary>
        private void CmbCategoryFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCategoryFilter.SelectedValue != null)
            {
                if (cmbCategoryFilter.SelectedValue.ToString() == "Все категории" ||
                    cmbCategoryFilter.SelectedValue is string && (string)cmbCategoryFilter.SelectedValue == "Все категории")
                {
                    _currentCategoryFilter = 0;
                }
                else
                {
                    _currentCategoryFilter = Convert.ToInt32(cmbCategoryFilter.SelectedValue);
                }
                ApplyFiltersAndSort();
            }
        }

        /// <summary>
        /// Сортировка по цене
        /// </summary>
        private void CmbSortPrice_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbSortPrice.SelectedIndex)
            {
                case 1:
                    _currentSortOrder = "ASC";
                    break;
                case 2:
                    _currentSortOrder = "DESC";
                    break;
                default:
                    _currentSortOrder = "";
                    break;
            }
            ApplyFiltersAndSort();
        }

        /// <summary>
        /// Применение всех фильтров и сортировки
        /// </summary>
        private void ApplyFiltersAndSort()
        {
            if (_originalDataTable == null) return;

            DataTable filteredDt = _originalDataTable.Copy();

            // Фильтр по названию услуги
            if (!string.IsNullOrEmpty(_currentSearchText))
            {
                for (int i = filteredDt.Rows.Count - 1; i >= 0; i--)
                {
                    string serviceName = filteredDt.Rows[i]["Название услуги"].ToString();
                    if (!serviceName.ToLower().Contains(_currentSearchText.ToLower()))
                    {
                        filteredDt.Rows.RemoveAt(i);
                    }
                }
            }

            // Фильтр по категории
            if (_currentCategoryFilter > 0)
            {
                for (int i = filteredDt.Rows.Count - 1; i >= 0; i--)
                {
                    int categoryId = Convert.ToInt32(filteredDt.Rows[i]["CategoryID"]);
                    if (categoryId != _currentCategoryFilter)
                    {
                        filteredDt.Rows.RemoveAt(i);
                    }
                }
            }

            // Сортировка по цене (создаём отсортированную копию)
            DataTable sortedDt;
            if (!string.IsNullOrEmpty(_currentSortOrder))
            {
                sortedDt = filteredDt.Copy();
                sortedDt.DefaultView.Sort = $"Цена {_currentSortOrder}";
                sortedDt = sortedDt.DefaultView.ToTable();
            }
            else
            {
                sortedDt = filteredDt;
            }

            DisplayDataGridView(sortedDt);
        }

        /// <summary>
        /// Отображение данных в DataGridView с миниатюрами
        /// </summary>
        private void DisplayDataGridView(DataTable dataTable)
        {
            DataTable maskedDt = new DataTable();
            maskedDt.Columns.Add("ID", typeof(int));
            maskedDt.Columns.Add("Название услуги", typeof(string));
            maskedDt.Columns.Add("Описание", typeof(string));
            maskedDt.Columns.Add("Цена", typeof(decimal));
            maskedDt.Columns.Add("Категория", typeof(string));
            maskedDt.Columns.Add("Миниатюра", typeof(Image));
            maskedDt.Columns.Add("CategoryID", typeof(int));

            // Увеличиваем размер миниатюр (было 80, стало 120)
            Size thumbSize = new Size(100, 100); // Фиксированный размер вместо автоматического

            foreach (DataRow row in dataTable.Rows)
            {
                Image thumb = null;
                if (row["Фото"] != DBNull.Value)
                {
                    byte[] bytes = (byte[])row["Фото"];
                    if (bytes?.Length > 0)
                    {
                        using (var ms = new MemoryStream(bytes))
                        using (var img = Image.FromStream(ms))
                            thumb = _imageService.ScaleImage(new Bitmap(img), thumbSize.Width, thumbSize.Height);
                    }
                }
                if (thumb == null)
                    thumb = _imageService.CreateDefaultThumbnail(thumbSize.Width, thumbSize.Height);

                maskedDt.Rows.Add(
                    Convert.ToInt32(row["ID"]),
                    row["Название услуги"]?.ToString() ?? "",
                    row["Описание"]?.ToString() ?? "",
                    Convert.ToDecimal(row["Цена"]),
                    row["Категория"]?.ToString() ?? "",
                    thumb,
                    Convert.ToInt32(row["CategoryID"])
                );
            }

            int selectedIndex = (dataGridViewServices.SelectedRows.Count > 0) ? dataGridViewServices.SelectedRows[0].Index : -1;
            dataGridViewServices.Columns.Clear();
            dataGridViewServices.DataSource = maskedDt;

            ConfigureDataGridViewColumns();

            if (selectedIndex >= 0 && selectedIndex < dataGridViewServices.Rows.Count)
                dataGridViewServices.Rows[selectedIndex].Selected = true;
        }

        /// <summary>
        /// Настройка колонок DataGridView
        /// </summary>
        private void ConfigureDataGridViewColumns()
        {
            dataGridViewServices.Columns["ID"].Visible = false;
            dataGridViewServices.Columns["CategoryID"].Visible = false;

            // Увеличиваем FillWeight для картинки
            dataGridViewServices.Columns["Название услуги"].FillWeight = 25;
            dataGridViewServices.Columns["Описание"].FillWeight = 35;
            dataGridViewServices.Columns["Цена"].FillWeight = 10;
            dataGridViewServices.Columns["Категория"].FillWeight = 15;
            dataGridViewServices.Columns["Миниатюра"].FillWeight = 8;

            dataGridViewServices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Устанавливаем высоту строк
            dataGridViewServices.RowTemplate.Height = 100;
            dataGridViewServices.RowTemplate.MinimumHeight = 100;

            // Настройка отображения картинок по центру
            dataGridViewServices.Columns["Миниатюра"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        /// <summary>
        /// Настройка интерфейса в зависимости от роли
        /// </summary>
        private void ConfigureByRole()
        {
            if (_roleID == 4) // менеджер
            {
                button2.Enabled = false;
            }
            else
            {
                button2.Enabled = true;
            }
        }

        private void ConfigureDataGridView()
        {
            dataGridViewServices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewServices.MultiSelect = false;
            dataGridViewServices.RowHeadersVisible = false;
            dataGridViewServices.ReadOnly = true;
            dataGridViewServices.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 203, 219);
            dataGridViewServices.DefaultCellStyle.SelectionForeColor = Color.Black;
            dataGridViewServices.MouseClick += DataGridViewServices_MouseClick;
        }

        private void DataGridViewServices_MouseClick(object sender, MouseEventArgs e)
        {
            if (_roleID == 4) return;

            if (e.Button == MouseButtons.Right)
            {
                var hitTest = dataGridViewServices.HitTest(e.X, e.Y);
                if (hitTest.RowIndex >= 0 && hitTest.RowIndex < dataGridViewServices.Rows.Count)
                {
                    dataGridViewServices.ClearSelection();
                    dataGridViewServices.Rows[hitTest.RowIndex].Selected = true;

                    var contextMenu = new ContextMenuStrip();
                    var editMenuItem = new ToolStripMenuItem("Редактировать");
                    editMenuItem.Image = Properties.Resources.edit_icon;
                    editMenuItem.Click += (s, args) => EditSelectedService();

                    var deleteMenuItem = new ToolStripMenuItem("Удалить");
                    deleteMenuItem.Image = Properties.Resources.delete_icon;
                    deleteMenuItem.Click += (s, args) => DeleteSelectedService();

                    contextMenu.Items.Add(editMenuItem);
                    contextMenu.Items.Add(deleteMenuItem);
                    contextMenu.Show(dataGridViewServices, e.Location);
                }
            }
        }

        private void EditSelectedService()
        {
            if (_roleID == 4) { ShowInfo("Нет прав на редактирование"); return; }
            if (dataGridViewServices.SelectedRows.Count == 0) { ShowInfo("Выберите услугу"); return; }
            OpenEditFormService(dataGridViewServices.SelectedRows[0]);
        }

        private void OpenEditFormService(DataGridViewRow row)
        {
            if (_roleID == 4) { ShowInfo("Недостаточно прав"); return; }

            try
            {
                int serviceId = Convert.ToInt32(row.Cells["ID"].Value);
                var serviceModel = _editUserClass.LoadServiceById(serviceId);
                if (serviceModel != null)
                {
                    using (var connection = GetNewConnection())
                    {
                        connection.Open();
                        string query = "SELECT Photo FROM services WHERE IDServices = @ServiceId";
                        MySqlCommand cmd = new MySqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                        byte[] imageBytes = cmd.ExecuteScalar() as byte[];
                        if (imageBytes != null && imageBytes.Length > 0)
                        {
                            using (var ms = new MemoryStream(imageBytes))
                            {
                                serviceModel.ServiceImage = new Bitmap(Image.FromStream(ms));
                            }
                            serviceModel.PhotoBytes = imageBytes;
                        }
                    }

                    var editForm = new EditServiceForm(serviceModel);
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        _editUserClass.UpdateServiceInDatabase(editForm.Service);
                        LoadServicesData();
                        ShowInfo("Услуга обновлена");
                    }
                }
                else ShowInfo("Не удалось загрузить данные");
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}"); }
        }

        private void DeleteSelectedService()
        {
            if (_roleID == 4) { ShowInfo("Нет прав на удаление"); return; }
            if (dataGridViewServices.SelectedRows.Count == 0) { ShowInfo("Выберите услугу"); return; }

            var row = dataGridViewServices.SelectedRows[0];
            string serviceName = row.Cells["Название услуги"].Value?.ToString();
            int serviceId = Convert.ToInt32(row.Cells["ID"].Value);

            if (HasDependencies(serviceId))
            {
                MessageBox.Show($"Невозможно удалить '{serviceName}'. Есть связанные записи.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show($"Удалить '{serviceName}'? (будет отключена)", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                SoftDeleteService(serviceId);
            }
        }

        private bool HasDependencies(int serviceId)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM Record WHERE Service = @ServiceId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
                catch { return true; }
            }
        }

        private void SoftDeleteService(int serviceId)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    string checkQuery = "SELECT IsActive FROM services WHERE IDServices = @ServiceId";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@ServiceId", serviceId);
                    object result = checkCmd.ExecuteScalar();
                    if (result != null && !Convert.ToBoolean(result))
                    {
                        ShowInfo("Услуга уже отключена");
                        return;
                    }

                    string query = "UPDATE services SET IsActive = 0 WHERE IDServices = @ServiceId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        ShowInfo("Услуга отключена");
                        LoadServicesData();
                    }
                    else ShowInfo("Услуга не найдена");
                }
                catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}"); }
            }
        }

        private void LoadServicesData()
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                        s.IDServices as 'ID',
                        s.ServiceName as 'Название услуги',
                        s.Description as 'Описание',
                        s.Price as 'Цена',
                        s.Category as 'CategoryID',
                        s.Photo as 'Фото',
                        c.CategoryName as 'Категория'
                    FROM Services s
                    INNER JOIN Category c ON s.Category = c.IDCategory
                    WHERE s.IsActive = 1 AND c.IsActive = 1
                    ORDER BY c.CategoryName, s.ServiceName";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                    _originalDataTable = new DataTable();
                    adapter.Fill(_originalDataTable);

                    ApplyFiltersAndSort();
                }
                catch (Exception ex) { MessageBox.Show($"Ошибка загрузки: {ex.Message}"); }
            }
        }

        private void ShowInfo(string msg) => MessageBox.Show(msg, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

        private void button2_Click(object sender, EventArgs e)
        {
            if (_roleID == 4) { ShowInfo("Добавление доступно только администратору"); return; }
            if (new AddServiceForm(this).ShowDialog() == DialogResult.OK)
            {
                LoadServicesData();
                ShowInfo("Услуга добавлена");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_roleID == 4)
            {
                Form menu = new MenuManager(_userName, _userId);
                menu.Show();
                this.Close();
            }
            else if (_roleID == 2)
            {
                Form menu = new MenuAdmin(_userName);
                menu.Show();
                this.Close();
            }
        }
    }
}