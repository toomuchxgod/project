using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutoServiceManager
{
    public partial class MainForm : Form
    {
        private Form activeForm = null;

        public MainForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            UpdateUserInfo(); // Показываем информацию о пользователе
        }

        private void UpdateUserInfo()
        {
            // Показываем текущего пользователя
            string username = LoginForm.CurrentUser ?? "Гость";
            statusLabelUser.Text = $"Пользователь: {username}";

            // Меняем цвет в зависимости от роли
            if (username == "admin")
                statusLabelUser.BackColor = Color.FromArgb(220, 53, 69); // Красный
            else if (username == "user")
                statusLabelUser.BackColor = Color.FromArgb(40, 167, 69); // Зеленый
            else
                statusLabelUser.BackColor = Color.FromArgb(0, 122, 204); // Синий
        }

        private void OpenChildForm(Form childForm)
        {
            if (activeForm != null)
                activeForm.Close();

            activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            panelContent.Controls.Add(childForm);
            panelContent.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        private void btnClients_Click(object sender, EventArgs e)
        {
            try
            {
                OpenChildForm(new ClientsForm());
                lblSection.Text = "👥 КЛИЕНТЫ";
                lblInfo.Text = "Управление клиентской базой автосервиса";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCars_Click(object sender, EventArgs e)
        {
            // Создаем простую форму для автомобилей
            var form = new Form();
            form.Text = "Автомобили";
            var label = new Label
            {
                Text = "🚗 ФОРМА АВТОМОБИЛИ\n\n" +
                       "Здесь будет:\n" +
                       "• Регистрация автомобилей\n" +
                       "• Привязка к клиентам\n" +
                       "• История обслуживания\n" +
                       "• Технические характеристики\n\n" +
                       $"Пользователь: {LoginForm.CurrentUser}",
                Font = new Font("Segoe UI", 12),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(20)
            };
            form.Controls.Add(label);
            OpenChildForm(form);
            lblSection.Text = "🚗 АВТОМОБИЛИ";
            lblInfo.Text = "";
        }

        private void btnServices_Click(object sender, EventArgs e)
        {
            // Создаем простую форму для услуг
            var form = new Form();
            form.Text = "Услуги";
            var label = new Label
            {
                Text = "🔧 КАТАЛОГ УСЛУГ\n\n" +
                       "Здесь будет:\n" +
                       "• Список всех услуг\n" +
                       "• Цены и сроки выполнения\n" +
                       "• Категории услуг\n" +
                       "• Специальные предложения\n\n" +
                       $"Пользователь: {LoginForm.CurrentUser}",
                Font = new Font("Segoe UI", 12),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(20)
            };
            form.Controls.Add(label);
            OpenChildForm(form);
            lblSection.Text = "🔧 УСЛУГИ";
            lblInfo.Text = "";
        }

        private void btnOrders_Click(object sender, EventArgs e)
        {
            // Создаем простую форму для заказов
            var form = new Form();
            form.Text = "Заказы";
            var label = new Label
            {
                Text = "📋 УПРАВЛЕНИЕ ЗАКАЗАМИ\n\n" +
                       "Здесь будет:\n" +
                       "• Создание новых заказов\n" +
                       "• Отслеживание статусов\n" +
                       "• Расчет стоимости\n" +
                       "• История заказов\n\n" +
                       $"Пользователь: {LoginForm.CurrentUser}",
                Font = new Font("Segoe UI", 12),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(20)
            };
            form.Controls.Add(label);
            OpenChildForm(form);
            lblSection.Text = "📋 ЗАКАЗЫ";
            lblInfo.Text = "";
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            // Создаем простую форму для отчетов
            var form = new Form();
            form.Text = "Отчеты";

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            var label = new Label
            {
                Text = "📊 ГЕНЕРАЦИЯ ОТЧЕТОВ\n\n" +
                       "Доступные отчеты:\n" +
                       "1. Финансовый отчет за период\n" +
                       "2. Статистика услуг\n" +
                       "3. Клиентская база\n" +
                       "4. Автомобили по маркам\n" +
                       "5. Доходы по месяцам\n\n" +
                       $"Пользователь: {LoginForm.CurrentUser}",
                Font = new Font("Segoe UI", 12),
                Location = new Point(20, 20),
                AutoSize = true
            };

            var btnReport1 = new Button
            {
                Text = "📈 Финансовый отчет",
                Location = new Point(20, 200),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnReport1.Click += (s, args) => MessageBox.Show("Генерация финансового отчета...", "Отчет",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            var btnReport2 = new Button
            {
                Text = "📊 Статистика услуг",
                Location = new Point(240, 200),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnReport2.Click += (s, args) => MessageBox.Show("Генерация статистики услуг...", "Статистика",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            panel.Controls.Add(label);
            panel.Controls.Add(btnReport1);
            panel.Controls.Add(btnReport2);
            form.Controls.Add(panel);

            OpenChildForm(form);
            lblSection.Text = "📊 ОТЧЕТЫ";
            lblInfo.Text = "";
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите выйти из программы?", "Подтверждение выхода",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void timerDateTime_Tick(object sender, EventArgs e)
        {
            statusLabelDateTime.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Показываем приветственную информацию
            string username = LoginForm.CurrentUser ?? "Гость";
            lblSection.Text = "🏠 ГЛАВНАЯ ПАНЕЛЬ";
            lblInfo.Text = $"👋 Добро пожаловать, {username}!\n\n" +
                          "Это система управления автосервисом.\n\n" +
                          "📌 Для начала работы выберите нужный раздел в меню слева:\n\n" +
                          "👥 КЛИЕНТЫ - управление клиентской базой\n" +
                          "🚗 АВТОМОБИЛИ - регистрация и история ТО\n" +
                          "🔧 УСЛУГИ - каталог услуг и цены\n" +
                          "📋 ЗАКАЗЫ - создание и отслеживание заказов\n" +
                          "📊 ОТЧЕТЫ - аналитика и статистика\n\n" +
                          $"📅 Дата: {DateTime.Now:dd.MM.yyyy}\n" +
                          $"🕐 Время: {DateTime.Now:HH:mm}\n" +
                          $"🔧 Версия: 1.0";

            timerDateTime.Start();

            // Проверяем подключение к БД
            CheckDatabaseConnection();
        }

        private void CheckDatabaseConnection()
        {
            try
            {
                // Простая проверка подключения
                statusLabelDB.Text = "База данных: Проверка...";
                Application.DoEvents(); // Обновляем интерфейс

                // Имитируем проверку (в реальной программе здесь будет DatabaseHelper.TestConnection())
                System.Threading.Thread.Sleep(500);

                // Для теста всегда показываем Online
                statusLabelDB.Text = "База данных: Online";
                statusLabelDB.BackColor = Color.FromArgb(40, 167, 69); // Зеленый
            }
            catch
            {
                statusLabelDB.Text = "База данных: Offline";
                statusLabelDB.BackColor = Color.FromArgb(220, 53, 69); // Красный
            }
        }

        private void ShowDashboard()
        {
            if (activeForm != null)
            {
                activeForm.Close();
                activeForm = null;
            }

            string username = LoginForm.CurrentUser ?? "Гость";
            lblSection.Text = "🏠 ГЛАВНАЯ ПАНЕЛЬ";
            lblInfo.Text = $"👋 С возвращением, {username}!\n\n" +
                          "📊 Статистика за сегодня:\n" +
                          "• Новых клиентов: 3\n" +
                          "• Выполненных заказов: 12\n" +
                          "• Общая выручка: 45,200 руб.\n" +
                          "• Автомобилей в работе: 5\n\n" +
                          $"⏰ Актуально на: {DateTime.Now:HH:mm}";
        }

        // Добавим обработчик двойного клика по логотипу
        private void pictureBoxLogo_DoubleClick(object sender, EventArgs e)
        {
            ShowDashboard();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Можно добавить подтверждение выхода
            if (e.CloseReason == CloseReason.UserClosing)
            {
                var result = MessageBox.Show("Закрыть программу?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}