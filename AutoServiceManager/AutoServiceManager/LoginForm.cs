using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutoServiceManager
{
    public partial class LoginForm : Form
    {
        // Статическая переменная для хранения текущего пользователя
        public static string CurrentUser { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
            CreateLogoImage(); // Создаем логотип программно
        }

        private void CreateLogoImage()
        {
            // Создаем логотип программно
            Bitmap logo = new Bitmap(150, 150);
            using (Graphics g = Graphics.FromImage(logo))
            {
                g.Clear(Color.Transparent);

                // Рисуем круг
                g.FillEllipse(new SolidBrush(Color.FromArgb(255, 255, 255, 255)), 10, 10, 130, 130);
                g.DrawEllipse(new Pen(Color.FromArgb(200, 255, 255, 255), 3), 10, 10, 130, 130);

                // Рисуем автомобиль
                using (Font font = new Font("Arial", 40, FontStyle.Bold))
                {
                    g.DrawString("🚗", font, Brushes.White, 35, 40);
                }

                // Текст
                using (Font font = new Font("Arial", 12, FontStyle.Bold))
                {
                    g.DrawString("АВТОСЕРВИС", font, Brushes.White, 20, 110);
                }
            }

            pictureBox1.Image = logo;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtLogin.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Универсальная проверка
            bool isAuthenticated = CheckCredentials(username, password);

            if (isAuthenticated)
            {
                // Сохраняем имя пользователя
                CurrentUser = username;

                this.Hide();
                var mainForm = new MainForm();
                mainForm.Show();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!\n\n" +
                    "Попробуйте:\n" +
                    "• Логин: admin / Пароль: admin123\n" +
                    "• Логин: admin / Пароль: 123456\n" +
                    "• Логин: user / Пароль: user123\n" +
                    "• Логин: user / Пароль: 123456",
                    "Ошибка входа",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }

        private bool CheckCredentials(string username, string password)
        {
            // Все возможные комбинации
            return (username == "admin" && (password == "admin123" || password == "123456")) ||
                   (username == "user" && (password == "user123" || password == "123456")) ||
                   (username == "manager" && password == "123456") ||
                   (username == "mechanic" && password == "123456");
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            // Автозаполнение для тестирования (только в Debug режиме)
#if DEBUG
            txtLogin.Text = "admin";
            txtPassword.Text = "admin123";
            txtPassword.SelectAll(); // Выделяем пароль для быстрой замены
#endif
        }

        private void linkLabelForgot_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("📋 Доступные учетные записи:\n\n" +
                "👑 АДМИНИСТРАТОР:\n" +
                "Логин: admin\n" +
                "Пароль: admin123 или 123456\n\n" +
                "👤 ПОЛЬЗОВАТЕЛЬ:\n" +
                "Логин: user\n" +
                "Пароль: user123 или 123456\n\n" +
                "👨‍💼 МЕНЕДЖЕР:\n" +
                "Логин: manager\n" +
                "Пароль: 123456\n\n" +
                "🔧 МЕХАНИК:\n" +
                "Логин: mechanic\n" +
                "Пароль: 123456\n\n" +
                "💡 Для быстрого входа нажмите кнопку 'ДЕМО-РЕЖИМ'",
                "Справка по учетным записям",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnDemo_Click(object sender, EventArgs e)
        {
            // Автовход для демо-режима
            txtLogin.Text = "admin";
            txtPassword.Text = "admin123";
            btnLogin_Click(sender, e);
        }
    }
}