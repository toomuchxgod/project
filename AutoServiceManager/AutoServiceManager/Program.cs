using System;
using System.Windows.Forms;
using System.IO;

namespace AutoServiceManager
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Проверяем, установлен ли SQL Server LocalDB
                if (!IsLocalDBInstalled())
                {
                    var result = MessageBox.Show(
                        "SQL Server LocalDB не установлен или не запущен.\n\n" +
                        "Для работы программы необходимо:\n" +
                        "1. Установить Microsoft SQL Server Express LocalDB\n" +
                        "2. Или использовать существующий SQL Server\n\n" +
                        "Хотите продолжить без базы данных? (функциональность будет ограничена)",
                        "Предупреждение",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.No)
                    {
                        ShowInstallationInstructions();
                        return;
                    }
                }

                // Проверка подключения к БД
                bool dbConnected = DatabaseHelper.TestConnection();

                if (!dbConnected)
                {
                    var result = MessageBox.Show(
                        "Не удалось подключиться к базе данных.\n\n" +
                        "Возможные причины:\n" +
                        "1. База данных не создана\n" +
                        "2. SQL Server не запущен\n" +
                        "3. Неправильная строка подключения\n\n" +
                        "Хотите создать базу данных автоматически?",
                        "Ошибка подключения",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Error);

                    if (result == DialogResult.Yes)
                    {
                        CreateDatabase();
                        dbConnected = DatabaseHelper.TestConnection();
                    }
                }

                // Показываем окно настроек если БД не подключена
                if (!dbConnected)
                {
                    ShowConnectionSettings();
                }
                else
                {
                    // Запускаем форму входа
                    Application.Run(new LoginForm());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Критическая ошибка при запуске программы:\n\n{ex.Message}\n\n" +
                    $"Детали: {ex.StackTrace}",
                    "Ошибка запуска",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static bool IsLocalDBInstalled()
        {
            try
            {
                // Проверяем наличие LocalDB через командную строку
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c sqllocaldb info";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output.Contains("MSSQLLocalDB") || output.Contains("v11.0") || output.Contains("v12.0");
            }
            catch
            {
                return false;
            }
        }

        private static void ShowInstallationInstructions()
        {
            string message =
                "Установка SQL Server LocalDB:\n\n" +
                "1. Скачайте и установите Microsoft SQL Server Express с LocalDB:\n" +
                "   https://go.microsoft.com/fwlink/?linkid=866658\n\n" +
                "2. Или установите через Visual Studio Installer:\n" +
                "   - Откройте Visual Studio Installer\n" +
                "   - Нажмите 'Modify' рядом с вашей версией Visual Studio\n" +
                "   - Во вкладке 'Individual Components' найдите 'SQL Server Express LocalDB'\n" +
                "   - Установите галочку и нажмите 'Modify'\n\n" +
                "3. После установки перезапустите компьютер";

            MessageBox.Show(message, "Инструкция по установке",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static void CreateDatabase()
        {
            try
            {
                // Создаем простой SQL скрипт для создания базы данных
                string createScript = @"
-- Создание базы данных
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'AutoServiceDB')
BEGIN
    CREATE DATABASE AutoServiceDB;
    PRINT 'База данных AutoServiceDB создана.';
END
GO

USE AutoServiceDB;
GO

-- Таблица пользователей
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        UserID INT PRIMARY KEY IDENTITY(1,1),
        Username NVARCHAR(50) NOT NULL UNIQUE,
        Password NVARCHAR(100) NOT NULL,
        FullName NVARCHAR(100) NOT NULL,
        Role NVARCHAR(50) NOT NULL,
        CreatedDate DATETIME DEFAULT GETDATE()
    );
    
    -- Вставляем тестового пользователя (пароль: 123456 в MD5)
    INSERT INTO Users (Username, Password, FullName, Role) 
    VALUES ('admin', 'E10ADC3949BA59ABBE56E057F20F883E', 'Администратор', 'Admin');
    
    PRINT 'Таблица Users создана.';
END
GO

-- Таблица клиентов
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Clients')
BEGIN
    CREATE TABLE Clients (
        ClientID INT PRIMARY KEY IDENTITY(1,1),
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        Phone NVARCHAR(20) NOT NULL,
        Email NVARCHAR(100),
        Address NVARCHAR(200),
        RegistrationDate DATETIME DEFAULT GETDATE()
    );
    
    -- Тестовые клиенты
    INSERT INTO Clients (FirstName, LastName, Phone, Email, Address) VALUES
    ('Иван', 'Петров', '+79123456789', 'ivan@mail.ru', 'ул. Ленина, 10'),
    ('Мария', 'Сидорова', '+79234567890', 'maria@gmail.com', 'пр. Мира, 25');
    
    PRINT 'Таблица Clients создана.';
END
GO

-- Таблица автомобилей
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Cars')
BEGIN
    CREATE TABLE Cars (
        CarID INT PRIMARY KEY IDENTITY(1,1),
        ClientID INT FOREIGN KEY REFERENCES Clients(ClientID),
        Brand NVARCHAR(50) NOT NULL,
        Model NVARCHAR(50) NOT NULL,
        Year INT,
        VIN NVARCHAR(50),
        LicensePlate NVARCHAR(20),
        LastServiceDate DATETIME
    );
    
    PRINT 'Таблица Cars создана.';
END
GO

-- Таблица услуг
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Services')
BEGIN
    CREATE TABLE Services (
        ServiceID INT PRIMARY KEY IDENTITY(1,1),
        ServiceName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500),
        Price DECIMAL(10,2) NOT NULL,
        Duration INT
    );
    
    -- Тестовые услуги
    INSERT INTO Services (ServiceName, Description, Price, Duration) VALUES
    ('Замена масла', 'Замена моторного масла и фильтра', 2500.00, 60),
    ('Замена тормозных колодок', 'Замена передних/задних тормозных колодок', 4000.00, 90);
    
    PRINT 'Таблица Services создана.';
END
GO";

                // Выполняем скрипт
                using (var connection = new System.Data.SqlClient.SqlConnection(
                    @"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;"))
                {
                    connection.Open();

                    // Разделяем скрипт на отдельные команды
                    string[] commands = createScript.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string commandText in commands)
                    {
                        if (!string.IsNullOrWhiteSpace(commandText))
                        {
                            using (var command = new System.Data.SqlClient.SqlCommand(commandText, connection))
                            {
                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    // Игнорируем ошибки типа "уже существует"
                                    if (!ex.Message.Contains("уже существует") &&
                                        !ex.Message.Contains("already exists"))
                                    {
                                        throw;
                                    }
                                }
                            }
                        }
                    }
                }

                MessageBox.Show("База данных успешно создана!\n\n" +
                    "Данные для входа:\n" +
                    "Логин: admin\n" +
                    "Пароль: 123456",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания базы данных: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void ShowConnectionSettings()
        {
            using (var settingsForm = new ConnectionSettingsForm())
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    // Перепроверяем подключение
                    if (DatabaseHelper.TestConnection())
                    {
                        Application.Run(new LoginForm());
                    }
                    else
                    {
                        var result = MessageBox.Show(
                            "Не удалось подключиться с новыми настройками.\n" +
                            "Хотите продолжить в демо-режиме?",
                            "Ошибка подключения",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            Application.Run(new LoginForm());
                        }
                    }
                }
                else
                {
                    // Пользователь отменил настройки
                    var result = MessageBox.Show(
                        "Без базы данных программа будет работать в ограниченном режиме.\n" +
                        "Продолжить?",
                        "Подтверждение",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        Application.Run(new LoginForm());
                    }
                }
            }
        }
    }

    // Простая форма настроек подключения (если понадобится)
    public class ConnectionSettingsForm : Form
    {
        private TextBox txtServer;
        private TextBox txtDatabase;
        private Button btnTest;
        private Button btnOK;
        private Button btnCancel;

        public ConnectionSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки подключения";
            this.Size = new System.Drawing.Size(400, 250);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Метки и поля
            var lblServer = new Label { Text = "Сервер:", Location = new System.Drawing.Point(20, 20), Width = 100 };
            txtServer = new TextBox { Location = new System.Drawing.Point(120, 20), Width = 250 };

            var lblDatabase = new Label { Text = "База данных:", Location = new System.Drawing.Point(20, 60), Width = 100 };
            txtDatabase = new TextBox { Location = new System.Drawing.Point(120, 60), Width = 250 };

            // Кнопки
            btnTest = new Button { Text = "Тест подключения", Location = new System.Drawing.Point(20, 100), Width = 150 };
            btnTest.Click += BtnTest_Click;

            btnOK = new Button { Text = "OK", Location = new System.Drawing.Point(200, 150), Width = 80 };
            btnOK.Click += BtnOK_Click;

            btnCancel = new Button { Text = "Отмена", Location = new System.Drawing.Point(290, 150), Width = 80 };
            btnCancel.Click += BtnCancel_Click;

            // Добавление контролов
            this.Controls.AddRange(new Control[] { lblServer, txtServer, lblDatabase, txtDatabase, btnTest, btnOK, btnCancel });
        }

        private void LoadSettings()
        {
            // Можно загружать из файла конфигурации или реестра
            txtServer.Text = "(localdb)\\MSSQLLocalDB";
            txtDatabase.Text = "AutoServiceDB";
        }

        private void BtnTest_Click(object sender, EventArgs e)
        {
            try
            {
                string connectionString = $"Data Source={txtServer.Text};Initial Catalog={txtDatabase.Text};Integrated Security=True";

                using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    connection.Open();
                    MessageBox.Show("Подключение успешно!", "Тест подключения",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            // Сохраняем настройки
            string connectionString = $"Data Source={txtServer.Text};Initial Catalog={txtDatabase.Text};Integrated Security=True";
            DatabaseHelper.SetConnectionString(connectionString);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}