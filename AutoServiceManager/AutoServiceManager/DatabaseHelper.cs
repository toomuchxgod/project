using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace AutoServiceManager
{
    public static class DatabaseHelper
    {
        // Статическая строка подключения
        private static string _connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=AutoServiceDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False";

        public static void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static string GetConnectionString()
        {
            return _connectionString;
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public static bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    // Проверяем существование нужных таблиц
                    string checkQuery = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_NAME IN ('Users', 'Clients', 'Cars', 'Services')";

                    using (var command = new SqlCommand(checkQuery, connection))
                    {
                        int tableCount = (int)command.ExecuteScalar();

                        if (tableCount >= 2) // Хотя бы Users и Clients должны существовать
                        {
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("В базе данных отсутствуют необходимые таблицы.\n" +
                                "Создайте таблицы через SQL скрипт.",
                                "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMessage = GetSqlErrorMessage(sqlEx);
                MessageBox.Show(errorMessage, "Ошибка SQL Server",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}\n\n" +
                    $"Строка подключения: {_connectionString}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private static string GetSqlErrorMessage(SqlException ex)
        {
            switch (ex.Number)
            {
                case -1:  // Connection Error
                    return "Ошибка подключения к серверу. Проверьте:\n" +
                           "1. Запущен ли SQL Server\n" +
                           "2. Правильно ли указано имя сервера\n" +
                           $"Ошибка: {ex.Message}";

                case 2:   // Timeout
                    return "Таймаут подключения. Сервер не отвечает.\n" +
                           $"Ошибка: {ex.Message}";

                case 53:  // Network Error
                    return "Сетевая ошибка. Не удалось найти сервер.\n" +
                           $"Ошибка: {ex.Message}";

                case 4060: // Cannot open database
                    return $"База данных не найдена.\n" +
                           $"Создайте базу данных '{GetDatabaseName()}' на сервере.\n" +
                           $"Ошибка: {ex.Message}";

                case 18456: // Login failed
                    return "Ошибка авторизации.\n" +
                           "Проверьте логин и пароль.\n" +
                           $"Ошибка: {ex.Message}";

                case 5120: // Cannot open database file
                    return "Нет доступа к файлу базы данных.\n" +
                           "Проверьте права доступа.\n" +
                           $"Ошибка: {ex.Message}";

                default:
                    return $"Ошибка SQL Server ({ex.Number}): {ex.Message}";
            }
        }

        private static string GetDatabaseName()
        {
            // Извлекаем имя базы данных из строки подключения
            if (_connectionString.Contains("Initial Catalog="))
            {
                int start = _connectionString.IndexOf("Initial Catalog=") + 16;
                int end = _connectionString.IndexOf(";", start);
                if (end == -1) end = _connectionString.Length;
                return _connectionString.Substring(start, end - start);
            }
            return "AutoServiceDB";
        }

        public static string ComputeMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString().ToUpper();
            }
        }

        public static bool ValidateUser(string username, string password)
        {
            try
            {
                // Временная проверка для тестирования
                if (username == "admin" && password == "123456")
                    return true;
                if (username == "user" && password == "123456")
                    return true;

                string hashedPassword = ComputeMD5Hash(password);

                using (var connection = GetConnection())
                {
                    string query = "SELECT COUNT(*) FROM Users WHERE Username = @username AND Password = @password";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", hashedPassword);

                        connection.Open();
                        int count = (int)command.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Если БД недоступна, используем временную проверку
                if (ex.Message.Contains("не удалось") || ex.Message.Contains("timeout") ||
                    ex.Message.Contains("open") || ex.Message.Contains("connect"))
                {
                    MessageBox.Show("База данных недоступна. Используется временная авторизация.\n\n" +
                        "Доступные логины:\n" +
                        "admin / 123456\n" +
                        "user / 123456",
                        "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    return (username == "admin" && password == "123456") ||
                           (username == "user" && password == "123456");
                }

                MessageBox.Show($"Ошибка авторизации: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static DataTable GetClients()
        {
            try
            {
                return ExecuteQuery("SELECT * FROM Clients ORDER BY LastName, FirstName");
            }
            catch
            {
                // Возвращаем пустую таблицу если БД недоступна
                DataTable dt = new DataTable();
                dt.Columns.Add("ClientID", typeof(int));
                dt.Columns.Add("FirstName", typeof(string));
                dt.Columns.Add("LastName", typeof(string));
                dt.Columns.Add("Phone", typeof(string));
                return dt;
            }
        }

        public static DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (var connection = GetConnection())
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка выполнения запроса: {ex.Message}\n\nЗапрос: {query}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dataTable;
        }

        public static int ExecuteNonQuery(string query, params SqlParameter[] parameters)
        {
            try
            {
                using (var connection = GetConnection())
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка выполнения команды: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        public static object ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            try
            {
                using (var connection = GetConnection())
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    return command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка выполнения скалярного запроса: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        public static bool BackupDatabase(string backupPath)
        {
            try
            {
                string query = $"BACKUP DATABASE AutoServiceDB TO DISK = '{backupPath}'";
                ExecuteNonQuery(query);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания резервной копии: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool RestoreDatabase(string backupPath)
        {
            try
            {
                string query = $@"
                    USE master;
                    ALTER DATABASE AutoServiceDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    RESTORE DATABASE AutoServiceDB FROM DISK = '{backupPath}' WITH REPLACE;
                    ALTER DATABASE AutoServiceDB SET MULTI_USER;";

                ExecuteNonQuery(query);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка восстановления из резервной копии: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}