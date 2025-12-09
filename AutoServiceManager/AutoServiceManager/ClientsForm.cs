using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace AutoServiceManager
{
    public partial class ClientsForm : Form
    {
        private DataTable clientsTable;
        private SqlDataAdapter dataAdapter;

        public ClientsForm()
        {
            InitializeComponent();
            LoadClients();
            SetupDataGridView();
        }

        private void LoadClients()
        {
            try
            {
                string query = @"
                    SELECT 
                        ClientID,
                        FirstName,
                        LastName,
                        Phone,
                        ISNULL(Email, '') as Email,
                        ISNULL(Address, '') as Address,
                        RegistrationDate
                    FROM Clients 
                    ORDER BY LastName, FirstName";

                dataAdapter = new SqlDataAdapter(query, DatabaseHelper.GetConnection());

                // Настраиваем команды для обновления
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

                clientsTable = new DataTable();
                dataAdapter.Fill(clientsTable);

                dataGridViewClients.DataSource = clientsTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupDataGridView()
        {
            // Отключаем автогенерацию колонок
            dataGridViewClients.AutoGenerateColumns = false;

            // Очищаем существующие колонки
            dataGridViewClients.Columns.Clear();

            // Создаем колонки вручную
            DataGridViewTextBoxColumn colID = new DataGridViewTextBoxColumn();
            colID.DataPropertyName = "ClientID";
            colID.HeaderText = "ID";
            colID.Width = 50;
            colID.ReadOnly = true;

            DataGridViewTextBoxColumn colFirstName = new DataGridViewTextBoxColumn();
            colFirstName.DataPropertyName = "FirstName";
            colFirstName.HeaderText = "Имя";
            colFirstName.Width = 120;

            DataGridViewTextBoxColumn colLastName = new DataGridViewTextBoxColumn();
            colLastName.DataPropertyName = "LastName";
            colLastName.HeaderText = "Фамилия";
            colLastName.Width = 120;

            DataGridViewTextBoxColumn colPhone = new DataGridViewTextBoxColumn();
            colPhone.DataPropertyName = "Phone";
            colPhone.HeaderText = "Телефон";
            colPhone.Width = 150;

            DataGridViewTextBoxColumn colEmail = new DataGridViewTextBoxColumn();
            colEmail.DataPropertyName = "Email";
            colEmail.HeaderText = "Email";
            colEmail.Width = 180;

            DataGridViewTextBoxColumn colAddress = new DataGridViewTextBoxColumn();
            colAddress.DataPropertyName = "Address";
            colAddress.HeaderText = "Адрес";
            colAddress.Width = 200;
            colAddress.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            DataGridViewTextBoxColumn colRegDate = new DataGridViewTextBoxColumn();
            colRegDate.DataPropertyName = "RegistrationDate";
            colRegDate.HeaderText = "Дата регистрации";
            colRegDate.Width = 120;
            colRegDate.DefaultCellStyle.Format = "dd.MM.yyyy";

            // Добавляем колонки в DataGridView
            dataGridViewClients.Columns.AddRange(new DataGridViewColumn[] {
                colID, colFirstName, colLastName, colPhone,
                colEmail, colAddress, colRegDate
            });

            // Настраиваем внешний вид
            dataGridViewClients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dataGridViewClients.AllowUserToAddRows = false;
            dataGridViewClients.AllowUserToDeleteRows = false;
            dataGridViewClients.RowHeadersVisible = false;
            dataGridViewClients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewClients.MultiSelect = false;

            // Стилизация
            dataGridViewClients.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            dataGridViewClients.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dataGridViewClients.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dataGridViewClients.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 122, 204);
            dataGridViewClients.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridViewClients.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewClients.EnableHeadersVisualStyles = false;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Имя и фамилия обязательны для заполнения!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Создаем новую строку
                DataRow newRow = clientsTable.NewRow();
                newRow["FirstName"] = txtFirstName.Text;
                newRow["LastName"] = txtLastName.Text;
                newRow["Phone"] = txtPhone.Text;
                newRow["Email"] = txtEmail.Text;
                newRow["Address"] = txtAddress.Text;
                newRow["RegistrationDate"] = DateTime.Now;

                clientsTable.Rows.Add(newRow);

                // Сохраняем в базу
                dataAdapter.Update(clientsTable);

                ClearFields();
                MessageBox.Show("Клиент успешно добавлен!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dataGridViewClients.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите клиента для редактирования!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Сохраняем изменения в DataTable
                DataRowView selectedRow = (DataRowView)dataGridViewClients.SelectedRows[0].DataBoundItem;
                if (selectedRow != null)
                {
                    selectedRow["FirstName"] = txtFirstName.Text;
                    selectedRow["LastName"] = txtLastName.Text;
                    selectedRow["Phone"] = txtPhone.Text;
                    selectedRow["Email"] = txtEmail.Text;
                    selectedRow["Address"] = txtAddress.Text;
                }

                // Сохраняем в базу
                dataAdapter.Update(clientsTable);

                MessageBox.Show("Данные успешно сохранены!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewClients.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите клиента для удаления!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Удалить выбранного клиента?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    foreach (DataGridViewRow row in dataGridViewClients.SelectedRows)
                    {
                        DataRowView rowView = (DataRowView)row.DataBoundItem;
                        if (rowView != null)
                        {
                            rowView.Delete();
                        }
                    }

                    // Сохраняем изменения в базу
                    dataAdapter.Update(clientsTable);

                    ClearFields();
                    MessageBox.Show("Клиент удален!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchText) || searchText == "поиск клиентов...")
            {
                // Показываем все строки
                foreach (DataGridViewRow row in dataGridViewClients.Rows)
                {
                    row.Visible = true;
                }
                return;
            }

            foreach (DataGridViewRow row in dataGridViewClients.Rows)
            {
                bool visible = false;

                // Проверяем все ячейки в строке
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value != null && cell.Value.ToString().ToLower().Contains(searchText))
                    {
                        visible = true;
                        break;
                    }
                }

                row.Visible = visible;
            }
        }

        private void dataGridViewClients_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewClients.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridViewClients.SelectedRows[0];

                // Получаем данные из DataBoundItem
                if (row.DataBoundItem is DataRowView rowView)
                {
                    txtFirstName.Text = rowView["FirstName"]?.ToString() ?? "";
                    txtLastName.Text = rowView["LastName"]?.ToString() ?? "";
                    txtPhone.Text = rowView["Phone"]?.ToString() ?? "";
                    txtEmail.Text = rowView["Email"]?.ToString() ?? "";
                    txtAddress.Text = rowView["Address"]?.ToString() ?? "";
                }
                else
                {
                    // Альтернативный способ
                    txtFirstName.Text = row.Cells["FirstName"]?.Value?.ToString() ?? "";
                    txtLastName.Text = row.Cells["LastName"]?.Value?.ToString() ?? "";
                    txtPhone.Text = row.Cells["Phone"]?.Value?.ToString() ?? "";
                    txtEmail.Text = row.Cells["Email"]?.Value?.ToString() ?? "";
                    txtAddress.Text = row.Cells["Address"]?.Value?.ToString() ?? "";
                }
            }
        }

        private void ClearFields()
        {
            txtFirstName.Clear();
            txtLastName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtAddress.Clear();

            // Сбрасываем поиск
            txtSearch.Text = "Поиск клиентов...";
            txtSearch.ForeColor = Color.Gray;

            // Показываем все строки
            foreach (DataGridViewRow row in dataGridViewClients.Rows)
            {
                row.Visible = true;
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Функция печати будет реализована в следующей версии", "Информация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadClients();
            ClearFields();
        }

        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnSearch_Click(sender, e);
            }
        }

        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == "Поиск клиентов...")
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.Black;
            }
        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "Поиск клиентов...";
                txtSearch.ForeColor = Color.Gray;
            }
        }

        private void ClientsForm_Load(object sender, EventArgs e)
        {
            // Настройка плейсхолдера
            txtSearch.Text = "Поиск клиентов...";
            txtSearch.ForeColor = Color.Gray;
            txtSearch.Enter += txtSearch_Enter;
            txtSearch.Leave += txtSearch_Leave;
        }
    }
}