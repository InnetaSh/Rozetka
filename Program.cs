using Microsoft.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;
using System;
using static System.Collections.Specialized.BitVector32;
using static Azure.Core.HttpHeader;
using Rozetka;
using System.Reflection.Metadata;
using System.Data;
using System.Diagnostics;

internal class Program
{
    static List<TableInfo> Tables = new List<TableInfo>();
    static async Task Main(string[] args)
    {
        //string connectionString = @"Data Source = DESKTOP-ITRLGSN; Initial Catalog = master; Trusted_Connection=True; TrustServerCertificate = True";

        //using (SqlConnection connection = new SqlConnection(connectionString))
        //{

        //    await connection.OpenAsync();

        //    SqlCommand command = new SqlCommand();

        //    command.CommandText = "CREATE DATABASE Rozetka";

        //    command.Connection = connection;

        //    await command.ExecuteNonQueryAsync();
        //    Console.WriteLine("DB created");


        //}


        Tables =  await GetAllTables();

        await Menu();

        static async Task Menu()
        {

            bool tableExists = await CheckIfTableExistsAsync("Categories");
            if (!tableExists)
            {
                string sqlQuery = "CREATE TABLE Categories (CategoryId  INT PRIMARY KEY IDENTITY, CategoryName  NVARCHAR(40) NOT NULL)";
                await ExecuteCommand(sqlQuery);
                Tables.Add(new TableInfo() { Name = "Categories" });

            }

            //string sqlQuery_DROPProducts = "DROP TABLE Products ";
            //await ExecuteCommand(sqlQuery_DROPProducts);

            tableExists = await CheckIfTableExistsAsync("Products");
            if (!tableExists)
            {
                string sqlQuery = "CREATE TABLE Products (ProductId  INT PRIMARY KEY IDENTITY, ProductName NVARCHAR(40) NOT NULL, Description NVARCHAR(40) NOT NULL,Price DECIMAL(10,2) DEFAULT 0 ,CategoryId INT  REFERENCES Categories(CategoryId ) ON DELETE CASCADE,Stock INT)";
                await ExecuteCommand(sqlQuery);
                //TablesName.Add("Products");
                Tables.Add(new TableInfo() { Name = "Products" });
            }

            //string sqlQuery_DROPProducts = "DROP TABLE CheckIfTableExistsAsync ";
            //await ExecuteCommand(sqlQuery_DROPProducts);


            // sqlQuery_DROPProducts = "DROP TABLE Accounts ";
            //await ExecuteCommand(sqlQuery_DROPProducts);


            tableExists = await CheckIfTableExistsAsync("Accounts");
            if (!tableExists)
            {
                string sqlQuery = "CREATE TABLE Accounts (AccountId  INT PRIMARY KEY IDENTITY, Login NVARCHAR(20) NOT NULL UNIQUE, Password NVARCHAR(20) NOT NULL)";
                await ExecuteCommand(sqlQuery);
            }

            tableExists = await CheckIfTableExistsAsync("ShoppingCart");
            if (!tableExists)
            {
                string sqlQuery = "CREATE TABLE ShoppingCart (AccountId  INT NOT NULL REFERENCES Accounts(AccountId ) ON DELETE CASCADE,ProductId  INT NOT NULL REFERENCES Products(ProductId) ON DELETE CASCADE)";
                await ExecuteCommand(sqlQuery);
            }



            Console.WriteLine("Выберите:");
            int action;


            Console.WriteLine("1 - Войти");
            Console.WriteLine("2 - У вас ещё нет аккаунта? Зарегистрироваться.");

            Console.WriteLine("0 - выход");

            Console.Write("действие - ");
            while (!Int32.TryParse(Console.ReadLine(), out action) || action < 0 || action > 2)
            {
                Console.WriteLine("Не верный ввод.Введите число:");
                Console.Write("действие - ");
            }

            string login = "";
            string password = "";

            switch (action)
            {
                case 0:
                    break;
                case 1:
                    Console.Write("Введите логин\t\t");
                    login = Console.ReadLine();

                    Console.Write("Введите пароль\t\t");
                    password = Console.ReadLine();


                    await Vhod(login, password);

                    Console.Clear();

                    break;
                case 2:


                    Console.WriteLine("Введите логин");
                    login = Console.ReadLine();


                    bool ExistsLogin = await loginExists(login);
                    if (!ExistsLogin)
                    {
                        Console.WriteLine("Введите пароль");
                        password = Console.ReadLine();

                        await Registration(login, password);

                        await Vhod(login, password);
                        Thread.Sleep(2000);
                        Console.Clear();

                    }
                    else
                    {
                        Console.WriteLine("Такой логин уже существует.");
                        Thread.Sleep(2000);
                        Console.Clear();
                        await Menu();
                    }
                    break;
                
            }
        }

        static async Task Vhod(string login, string password)
        {

            string connectionString = @"Data Source = DESKTOP-ITRLGSN; Initial Catalog = Rozetka; Trusted_Connection=True; TrustServerCertificate = True";

            string sqlExpression = "SELECT * FROM Accounts";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = await command.ExecuteReaderAsync();


                if (reader.HasRows)
                {
                    string columnName1 = reader.GetName(0);
                    string columnName2 = reader.GetName(1);
                    string columnName3 = reader.GetName(2);


                    var flag = false;
                    while (await reader.ReadAsync())
                    {

                        object id = reader.GetValue(0);
                        string Login = reader.GetString(1);
                        string Password = reader.GetString(2);


                        bool isEqual = string.Equals(Login, login, StringComparison.OrdinalIgnoreCase);
                        if (isEqual)
                        {
                            if (Password == password)
                            {
                                Console.WriteLine($"Добро пожаловать, {login}!");
                                Thread.Sleep(2000);
                                Console.Clear();
                                Console.WriteLine("-------------------------------------");

                                if (Login == "admin")
                                {
                                    Console.WriteLine("Вам доступно редактирование таблиц.");
                                    Console.WriteLine();
                                    await SubMenuAdmin();
                                    flag = true;
                                    break;
                                }
                                else
                                {
                                    await SubMenuAccount(login);
                                    flag = true;
                                    break;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Неправильный пароль");
                                Console.WriteLine("Введите пароль");
                                password = Console.ReadLine();

                                await passwordValid(login, password);
                                break;
                            }
                        }
                    }

                    if (!flag)
                    {
                        Console.WriteLine("Для входа в систему необходимо зарегистрироваться.");

                        Thread.Sleep(2000);
                        Console.Clear();
                        await Menu();
                    }
                }

                await reader.CloseAsync();
            }
        }

        static async Task passwordValid(string login, string password)
        {

            string connectionString = @"Data Source = DESKTOP-ITRLGSN; Initial Catalog = Rozetka; Trusted_Connection=True; TrustServerCertificate = True";

            string sqlExpression = "SELECT * FROM Accounts";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = await command.ExecuteReaderAsync();


                if (reader.HasRows)
                {
                    string columnName1 = reader.GetName(0);
                    string columnName2 = reader.GetName(1);
                    string columnName3 = reader.GetName(2);



                    while (await reader.ReadAsync())
                    {

                        object id = reader.GetValue(0);
                        string Login = reader.GetString(1);
                        string Password = reader.GetString(2);



                        if (Password == password)
                        {
                            Console.WriteLine($"Добро пожаловать, {login}!");
                            Thread.Sleep(2000);
                            Console.Clear();
                            Console.WriteLine("-------------------------------------");

                            if (Login == "admin")
                            {
                                Console.WriteLine("Вам доступно редактирование таблиц.");
                                Console.WriteLine();
                                await SubMenuAdmin();
                            }
                            else
                            {
                                await SubMenuAccount(login);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Неправильный пароль.");
                            await Menu();
                        }
                    }
                }

                await reader.CloseAsync();
            }
        }

        static async Task Registration(string login, string password)
        {


            string connectionString = @"Data Source = DESKTOP-ITRLGSN; Initial Catalog = Rozetka; Trusted_Connection=True; TrustServerCertificate = True";

            string sqlExpression = @"INSERT INTO Accounts (Login, Password) VALUES (@login, @password)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sqlExpression, connection);

                command.Parameters.AddWithValue("@login", login);
                command.Parameters.AddWithValue("@password", password);

                await command.ExecuteNonQueryAsync();
                Console.WriteLine($"Добавлено в базу данных: {login}");


            }


        }

        static async Task SubMenuAdmin()
        {
            Console.WriteLine("Выберите:");
            int action;


            Console.WriteLine("1 - Добавить категорию.");
            Console.WriteLine("2 - удалить категорию");

            Console.WriteLine("3 - Добавить продукт.");
            Console.WriteLine("4 - Удалить продукт.");

            Console.WriteLine("5 - Изменить описание продукта.");
            Console.WriteLine("6 - изменить цены в выбранной категории товаров");


            Console.WriteLine("7 - посмотреть все категории");
            Console.WriteLine("8 - посмотреть все продукты");

            Console.WriteLine("9 - добавить новую таблицу");
            Console.WriteLine("10 - заполнить новую таблицу");

            Console.WriteLine("11 - посмотреть все таблицы");
            Console.WriteLine("12 - удалить таблицу");

            Console.WriteLine("0 - выход");


            Console.Write("действие - ");
            while (!Int32.TryParse(Console.ReadLine(), out action) || action < 0 || action > 12)
            {
                Console.WriteLine("Не верный ввод.Введите число:");
                Console.Write("действие - ");
            }
            Thread.Sleep(2000);
            Console.Clear();

            switch (action)
            {
                case 1:
                    Console.WriteLine("Введите название категории:");
                    string Category = Console.ReadLine();


                    string sqlQuery = $@"INSERT INTO Categories (CategoryName) VALUES ('{Category}')";
                    await ExecuteCommand(sqlQuery);
                    Console.WriteLine("Категория добавлена.");

                    Thread.Sleep(2000);
                    Console.Clear();
                    await SubMenuAdmin();
                    break;

                case 2:
                    Console.WriteLine("Введите название категории:");
                    Category = Console.ReadLine();
                    bool categoryExists = await CategoryExists(Category);

                    if (categoryExists)
                    {

                        sqlQuery = $@"UPDATE Products SET CategoryId = NULL  WHERE CategoryId IN (SELECT CategoryId FROM Categories WHERE CategoryName = ('{Category}'))";
                        await ExecuteCommand(sqlQuery);
                         sqlQuery = $@"DELETE FROM Categories FROM Categories WHERE CategoryName = ('{Category}')";
                        await ExecuteCommand(sqlQuery);
                        Console.WriteLine("Категория удалена.");
                    }
                    else
                    {
                        Console.WriteLine("Категория не существует. Пожалуйста, введите существующую категорию.");
                    }
                    Thread.Sleep(2000);
                    Console.Clear();
                    await SubMenuAdmin();
                    break;


                case 3:
                    Console.WriteLine("Введите название категории:");
                    Category = Console.ReadLine();

                    categoryExists = await CategoryExists(Category);

                    if (categoryExists)
                    {
                        Console.WriteLine("Введите название продукта:");
                        string nameProd = Console.ReadLine();



                        Console.WriteLine("Введите описание продукта:");
                        string DescriptionProd = Console.ReadLine();

                        Console.WriteLine("Введите цену продукта:");

                        decimal PriceProd;
                        while (!decimal.TryParse(Console.ReadLine(), out PriceProd) || PriceProd < 0)
                        {
                            Console.WriteLine("Не верный ввод.Введите число:");
                            Console.Write("действие - ");
                        }

                        Console.WriteLine("Введите количество продукта на складе:");
                        int StockProd;
                        while (!Int32.TryParse(Console.ReadLine(), out StockProd) || StockProd < 0)
                        {
                            Console.WriteLine("Не верный ввод.Введите число:");
                            Console.Write("действие - ");
                        }



                        sqlQuery = $@"INSERT INTO Products (ProductName, Description, Price, CategoryId, Stock) 
                            VALUES (('{nameProd}'), ('{DescriptionProd}'), ({PriceProd}), 
                            (SELECT CategoryId FROM Categories WHERE CategoryName = ('{Category}')), ({StockProd}))";

                        await ExecuteCommand(sqlQuery);
                        Console.WriteLine("Продукт успешно добавлен.");
                    }
                    else
                    {
                        Console.WriteLine("Категория не существует. Пожалуйста, введите существующую категорию.");
                    }

                    Thread.Sleep(2000);
                    Console.Clear();
                    await SubMenuAdmin();
                    break;

                case 4:

                    Console.WriteLine("Введите название продукта:");
                    string nameProduct = Console.ReadLine();

                    sqlQuery = $@"DELETE FROM Products WHERE ProductName = ('{nameProduct}')";
                    await ExecuteCommand(sqlQuery);


                    Console.WriteLine("Продукт удален.");
                    Thread.Sleep(2000);
                    Console.Clear();
                    await SubMenuAdmin();
                    break;



                case 5:
                    Console.WriteLine("Введите название категории:");
                    Category = Console.ReadLine();

                    categoryExists = await CategoryExists(Category);

                    if (categoryExists)
                    {
                        Console.WriteLine("Введите название продукта,данные о котором нужно изменить:");
                        nameProduct = Console.ReadLine();

                        Console.WriteLine("Введите новое описание продукта:");
                        string DescriptionProduct = Console.ReadLine();
                        decimal PriceProduct;
                        Console.WriteLine("Введите новую цену продукта:");
                        while (!decimal.TryParse(Console.ReadLine(), out PriceProduct) || PriceProduct < 0)
                        {
                            Console.WriteLine("Не верный ввод.Введите число:");
                            Console.Write("действие - ");
                        }
                        int StockProduct;
                        Console.WriteLine("Введите новое количество продукта на складе:");
                        while (!Int32.TryParse(Console.ReadLine(), out StockProduct) || StockProduct < 0)
                        {
                            Console.WriteLine("Не верный ввод.Введите число:");
                            Console.Write("действие - ");
                        }


                        sqlQuery = $@"UPDATE Products 
                                        SET 
                                            ProductName = '{nameProduct}', 
                                            Description = '{DescriptionProduct}', 
                                            Price = {PriceProduct}, 
                                            CategoryId = (SELECT CategoryId FROM Categories WHERE CategoryName = '{Category}'), 
                                            Stock = {StockProduct} 
                                        WHERE 
                                            ProductName = '{nameProduct}'";
                        await ExecuteCommand(sqlQuery);


                        Console.WriteLine("Данные о продукте успешно обновлены.");
                    }
                    else
                    {
                        Console.WriteLine("Категория не существует. Пожалуйста, введите существующую категорию.");
                    }
                    Thread.Sleep(2000);
                    Console.Clear();
                    await SubMenuAdmin();
                    break;

                case 6:
                    Console.WriteLine("Введите название категории:");
                    Category = Console.ReadLine();

                    categoryExists = await CategoryExists(Category);

                    if (categoryExists)
                    {

                        Console.WriteLine("Введите процент,на который поднять цену:");
                        int percent;
                        while (!Int32.TryParse(Console.ReadLine(), out percent))
                        {
                            Console.WriteLine("Не верный ввод.Введите число:");
                            Console.Write($"введите процент: ");
                        }

                        sqlQuery = $@"UPDATE Products SET Price = Price * (100 + {percent})/100 FROM Products  JOIN Categories ON Categories.CategoryId = Products.CategoryId  WHERE CategoryName = '{Category}' ";
                        await ExecuteCommand(sqlQuery);



                        Console.WriteLine("Цены успешно обновлены.");
                    }
                    else
                    {
                        Console.WriteLine("Категория не существует. Пожалуйста, введите существующую категорию.");
                    }
                    Thread.Sleep(2000);
                    Console.Clear();
                    await SubMenuAdmin();
                    break;



                case 7:
                    Console.WriteLine("Категории");
                    Console.WriteLine("-----------------------------------------------");
                    await ShowCategories();

                    Console.WriteLine("Нажмите любую клавишу для выхода в меню...");
                    Console.ReadKey();
                    Console.Clear();
                    await SubMenuAdmin();
                    break;

                case 8:
                    Console.WriteLine("Продукты");
                    Console.WriteLine("-----------------------------------------------");
                    string sqlExpression = "SELECT * FROM Products";

                    await ShowProducts(sqlExpression);

                    Console.WriteLine("Нажмите любую клавишу для выхода в меню...");
                    Console.ReadKey();
                    Console.Clear();
                    await SubMenuAdmin();
                    break;

                case 9:
                    await AddNewTable();

                    await SubMenuAdmin();
                    break;

                case 10:
                    Console.WriteLine("Введите название таблицы для заполнения:");
                    string TableName = Console.ReadLine();
                    var fitTable = Tables.FirstOrDefault(x => x.Name == TableName);
                    if (fitTable == null)
                        Console.WriteLine("Такой таблицы не существует");
                    else
                        await InsertIntoNewTable(Tables.FirstOrDefault(x => x.Name == TableName));
                    break;
                case 11:

                    for (var i = 0; i < Tables.Count; i++)
                    {
                        Console.WriteLine(i + "-" + Tables[i].Name);
                    }

                    Console.WriteLine("Вывести данные таблицы? (Y/N)");

                    string flag = Console.ReadLine().ToUpper();
                    while (flag.ToUpper() != "Y" && flag.ToUpper() != "N")
                    {
                        Console.WriteLine("Не верный ввод.Вывести данные таблицы? (Y/N)");
                        flag = Console.ReadLine().ToUpper();
                    }

                    if (flag == "Y")
                    {

                        Console.WriteLine("Введите название таблицы:");
                        string TablName = Console.ReadLine();
                        var fitTbl = Tables.FirstOrDefault(x => x.Name == TablName);
                        if (fitTbl == null)
                            Console.WriteLine("Такой таблицы не существует");
                        else
                            ShowTable(TablName);
                    }
                    await SubMenuAdmin();
                    break;

                case 12:
                    Console.WriteLine("Введите название таблицы:");
                    string TableNameDelete = Console.ReadLine();

                    var fitTableDelete = Tables.FirstOrDefault(x => x.Name == TableNameDelete);
                    if (fitTableDelete == null)
                        Console.WriteLine("Такой таблицы не существует");
                    else
                    {
                        sqlQuery = $@"DROP TABLE {TableNameDelete}";
                        await ExecuteCommand(sqlQuery);
                        Tables.Remove(fitTableDelete);
                        Console.WriteLine($" Таблица {TableNameDelete} успешно удалена.");
                    }
                    Console.WriteLine("------------------------------------");
                    await SubMenuAdmin();
                    break;
                case 0:
                    await Menu();
                    break;
            }
        }



        static async Task SubMenuAccount(string login)
        {
            Console.WriteLine("Выберите:");
            int action;

            Console.WriteLine("1 - посмотреть все продукты из категории");
            Console.WriteLine("2 - посмотреть все продукты");
            Console.WriteLine("3 - добавить товар в корзину");
            Console.WriteLine("4 - посмотреть товары из корзины");

            Console.WriteLine("0 - выход");


            Console.Write("действие - ");
            while (!Int32.TryParse(Console.ReadLine(), out action) || action < 0 || action > 4)
            {
                Console.WriteLine("Не верный ввод.Введите число:");
                Console.Write("действие - ");
            }

            switch (action)
            {
                case 1:
                    Console.WriteLine("Введите название категории:");
                    string Category = Console.ReadLine();

                    bool categoryExists = await CategoryExists(Category);

                    if (categoryExists)
                    {
                        Console.WriteLine("Продукты из категории:");
                        Console.WriteLine("-----------------------------------------------");
                        string sqlExpression1 = $@"SELECT * FROM Products WHERE CategoryId IN (SELECT CategoryId FROM Categories WHERE CategoryName ='{Category}')";
                        await ShowProducts(sqlExpression1);

                    }
                    else
                    {
                        Console.WriteLine("Категория не существует. Пожалуйста, введите существующую категорию.");
                    }
                    Console.WriteLine("Нажмите любую клавишу для выхода в меню...");
                    Console.ReadKey();
                    Console.Clear();
                    await SubMenuAccount(login);
                    break;

                case 2:
                    Console.WriteLine("Продукты");
                    Console.WriteLine("-----------------------------------------------");
                    
                    string sqlExpression = "SELECT * FROM Products";

                    await ShowProducts(sqlExpression);

                    Console.WriteLine("Нажмите любую клавишу для выхода в меню...");
                    Console.ReadKey();
                    Console.Clear();
                    await SubMenuAccount(login);
                    break;


                case 3:
                    Console.WriteLine("Введите название продукта:");
                    string ProductName = Console.ReadLine();


                    string sqlQuery = @"INSERT INTO ShoppingCart (AccountId,ProductId) VALUES ((SELECT AccountId FROM Accounts WHERE Login = @login),(SELECT ProductId FROM Products WHERE ProductName = @ProductName))";
                    await ExecuteCommand_Shopping(sqlQuery, login, ProductName);
                    Console.WriteLine("Категория добавлена.");

                    Thread.Sleep(2000);
                    Console.Clear();
                    await SubMenuAccount(login);
                    break;


                case 4:
                    Console.WriteLine("Продукты из корзины:");
                    Console.WriteLine("-----------------------------------------------");
                    sqlExpression = $@"SELECT * FROM Products JOIN ShoppingCart ON Products.ProductId = ShoppingCart.ProductId JOIN Accounts ON ShoppingCart.AccountId = Accounts.AccountId WHERE Accounts.Login = '{login}'";

                    await ShowProducts(sqlExpression);
                    

                    Console.WriteLine("Нажмите любую клавишу для выхода в меню...");
                    Console.ReadKey();
                    Console.Clear();
                    await SubMenuAccount(login);
                    break;


                case 0:
                    await Menu();
                    break;
            }



        }

        static async Task<bool> CheckIfTableExistsAsync(string tableName)
        {
            string query = @"IF EXISTS( SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName ) SELECT CAST(1 AS BIT) ELSE SELECT CAST(0 AS BIT)";

            return await ExecuteQueryAsync(query, new SqlParameter("@TableName", tableName));
        }

        static async Task<List<TableInfo>> GetAllTables()
        {
            string query = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";

            string connectionString = @"Data Source=DESKTOP-ITRLGSN; Initial Catalog=Rozetka; Trusted_Connection=True; TrustServerCertificate=True";

            List<TableInfo> tables = new List<TableInfo>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        tables.Add(new TableInfo { Name = reader["TABLE_NAME"].ToString() });
                    }
                }
            }

            return tables;

        }

        static async Task<List<ColumnInfo>> GetColumnsByTableName(string tableName)
        {
            string query = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName";

            string connectionString = @"Data Source=DESKTOP-ITRLGSN; Initial Catalog=Rozetka; Trusted_Connection=True; TrustServerCertificate=True";

            List<ColumnInfo> columns = new List<ColumnInfo>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableName", tableName);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            columns.Add(new ColumnInfo
                            {
                                Name = reader["COLUMN_NAME"].ToString()
                            });
                        }
                    }
                }
            }

            return columns;
        }

        static async Task<bool> ExecuteQueryAsync(string query, SqlParameter parameter)
        {
            string connectionString = @"Data Source=DESKTOP-ITRLGSN; Initial Catalog=Rozetka; Trusted_Connection=True; TrustServerCertificate=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameter != null)
                        command.Parameters.Add(parameter);

                    object result = await command.ExecuteScalarAsync();
                    return Convert.ToBoolean(result);
                }
            }
        }

        static async Task<bool> CategoryExists(string categoryName)
        {
            string connectionString = @"Data Source=DESKTOP-ITRLGSN; Initial Catalog=Rozetka; Trusted_Connection=True; TrustServerCertificate=True";
            string sqlQuery = @"SELECT COUNT(1) FROM Categories WHERE CategoryName = @CategoryName";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@CategoryName", categoryName);
                    int count = (int)await command.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

        static async Task<bool> loginExists(string login)
        {
            string connectionString = @"Data Source=DESKTOP-ITRLGSN; Initial Catalog=Rozetka; Trusted_Connection=True; TrustServerCertificate=True";
            string sqlQuery = @"SELECT COUNT(1) FROM Accounts WHERE Login = @login";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    int count = (int)await command.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

        static async Task ExecuteCommand(string sqlQuery)
        {
            string connectionString = @"Data Source = DESKTOP-ITRLGSN; Initial Catalog = Rozetka; Trusted_Connection=True; TrustServerCertificate = True";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sqlQuery, connection);

                await command.ExecuteNonQueryAsync();
              
            }

        }

        static async Task ExecuteCommand_Shopping(string sqlQuery, string login, string ProductName)
        {

            string connectionString = @"Data Source = DESKTOP-ITRLGSN; Initial Catalog = Rozetka; Trusted_Connection=True; TrustServerCertificate = True";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sqlQuery, connection);

                command.Parameters.AddWithValue("@login", login);
                command.Parameters.AddWithValue("@ProductName", ProductName);

                await command.ExecuteNonQueryAsync();


            }

        }

        static async Task ShowCategories()
        {
            string connectionString = @"Data Source = DESKTOP-ITRLGSN; Initial Catalog = Rozetka; Trusted_Connection=True; TrustServerCertificate = True";

            string sqlExpression = "SELECT * FROM Categories";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sqlExpression, connection);
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        string columnName1 = reader.GetName(0);
                        string columnName2 = reader.GetName(1);


                        Console.WriteLine($"{columnName1}\t{columnName2}");

                        while (await reader.ReadAsync())
                        {
                            object id = reader.GetValue(0);
                            object CategoryName = reader.GetValue(1);

                            Console.WriteLine($"{id,-10} \t{CategoryName} ");
                        }
                    }
                }
            }
            Console.Read();
        }

        static async Task ShowProducts(string sqlExpression)
        {
            string connectionString = @"Data Source = DESKTOP-ITRLGSN; Initial Catalog = Rozetka; Trusted_Connection=True; TrustServerCertificate = True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = await command.ExecuteReaderAsync();

                if (!reader.HasRows)
                {
                    Console.WriteLine("Данных нет");
                }
                else
                {
                    string columnName1 = reader.GetName(0);
                    string columnName2 = reader.GetName(1);
                    string columnName3 = reader.GetName(2);
                    string columnName4 = reader.GetName(3);
                    string columnName5 = reader.GetName(4);
                    string columnName6 = reader.GetName(5);



                    Console.WriteLine($"{columnName1,-10}\t{columnName2,-30}\t{columnName4,-10}\t{columnName5,-10}\t{columnName6,-10}\t{columnName3,-40}");


                    while (await reader.ReadAsync())
                    {

                        int ProductId = reader.GetInt32(0);
                        string nameProduct = reader.GetString(1);
                        string DescriptionProduct = reader.GetString(2);

                        object Price = reader.GetValue(3);
                        object CategoryId = reader.GetValue(4);
                        object Stock = reader.GetValue(5);



                        Console.WriteLine($"{ProductId,-10} \t{nameProduct,-30} \t{Price,-10} \t{CategoryId,-10} \t{Stock,-10} \t{DescriptionProduct,-40}");
                    }
                }

                await reader.CloseAsync();


            }

        }

        static async void ShowTable(string tableName)
        {
            var dt = await GetTableAsDataTable(tableName);
            var header = "";
            for (var i = 0; i < dt.Columns.Count; i++)
                header += $"{dt.Columns[i].ColumnName} \t";
            Console.WriteLine(header);
            Console.WriteLine();

            var row = "";
            foreach (DataRow r in dt.Rows)
            {
                foreach (var cell in r.ItemArray)
                    Console.Write("{0}\t", cell);
                Console.WriteLine();
            }
        }

        static async Task<DataTable> GetTableAsDataTable(string tableName)
        {
            string query = $"SELECT * FROM {tableName}";

            string connectionString = @"Data Source=DESKTOP-ITRLGSN; Initial Catalog=Rozetka; Trusted_Connection=True; TrustServerCertificate=True";

            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(dataTable);
                }
            }

            return dataTable;
        }

        
        static async Task AddNewTable()
        {
            Console.WriteLine("Введите название новой таблицы:");
            var NewTableName = Console.ReadLine();
            string flag;
            bool NewtableExists = await CheckIfTableExistsAsync(NewTableName);
            if (!NewtableExists)
            {
                Tables.Add(new TableInfo() { Name = NewTableName });

                int action;

                string sqlQuery = $@"CREATE TABLE {NewTableName} (Id INT  PRIMARY KEY IDENTITY)";
                await ExecuteCommand(sqlQuery);


                Console.WriteLine("Добавить добавить поле ProductId? (Y/N)");
                flag = Console.ReadLine().ToUpper();
                while (flag.ToUpper() != "Y" && flag.ToUpper() != "N")
                {
                    Console.WriteLine("Не верный ввод.Добавить добавить поле ProductId? (Y/N)");
                    flag = Console.ReadLine().ToUpper();
                }

                if (flag == "Y")
                {
                    sqlQuery = $@"ALTER TABLE {NewTableName} ADD ProductId INT  REFERENCES Products(ProductId) ON DELETE NO ACTION";
                    await ExecuteCommand(sqlQuery);
                    Console.WriteLine($"Столбец ProductId успешно добавлен в таблицу {NewTableName}.");
                    Tables.Last().Columns.Add(new ColumnInfo() { Name = "ProductId", DataType = "INT" });
                }


                Console.WriteLine("Добавить CategoryId ? (Y / N)");
                flag = Console.ReadLine().ToUpper();

                while (flag.ToUpper() != "Y" && flag.ToUpper() != "N")
                {
                    Console.WriteLine("Не верный ввод.Добавить CategoryId ? (Y/N)");
                    flag = Console.ReadLine().ToUpper();
                }
                if (flag == "Y")
                {
                    sqlQuery = $@"ALTER TABLE {NewTableName} ADD CategoryId INT  REFERENCES Categories(CategoryId) ON DELETE NO ACTION";
                    await ExecuteCommand(sqlQuery);
                    Console.WriteLine($"Столбец CategoryId успешно добавлен в таблицу {NewTableName}.");
                    Tables.Last().Columns.Add(new ColumnInfo() { Name = "CategoryId", DataType = "INT" });
                }

                Console.WriteLine("Добавить еще поле? (Y/N)");
                flag = Console.ReadLine().ToUpper();

                while (flag.ToUpper() != "Y" && flag.ToUpper() != "N")
                {
                    Console.WriteLine("Не верный ввод.Добавить еще поле? (Y/N)");
                    flag = Console.ReadLine().ToUpper();
                }

                while (flag == "Y")
                {
                    Console.WriteLine("Введите название поля?");
                    string fieldName = Console.ReadLine();

                    Console.WriteLine("Выберете тип поля?");
                    Console.WriteLine("1 - число (int)");
                    Console.WriteLine("2 - строка (string)");
                    Console.WriteLine("3 - дата (date)");
                    Console.WriteLine("4 - деньги (money)");



                    Console.Write("Ваш выбор - ");
                    while (!Int32.TryParse(Console.ReadLine(), out action) || action < 1 || action > 4)
                    {
                        Console.WriteLine("Не верный ввод.Введите число:");
                        Console.Write("Ваш выбор - ");
                    }


                    string type = action switch
                    {
                        1 => "INT",
                        2 => "NVARCHAR(200)",
                        3 => "DATETIME",
                        4 => "MONEY",
                        _ => throw new InvalidOperationException()
                    };

                    string alterQuery = $@"ALTER TABLE {NewTableName} ADD {fieldName} {type}";
                    await ExecuteCommand(alterQuery);
                    Console.WriteLine($"Столбец {fieldName} успешно добавлен в таблицу {NewTableName}.");

                    Tables.Last().Columns.Add(new ColumnInfo() { Name = fieldName, DataType = type });

                    Console.WriteLine("Добавить еще поле? (Y/N)");
                    while (flag.ToUpper() != "Y" && flag.ToUpper() != "N")
                    {
                        Console.WriteLine("Не верный ввод.Добавить еще поле? (Y/N)");
                        flag = Console.ReadLine().ToUpper();
                    }
                    flag = Console.ReadLine().ToUpper();
                }

            }
            Console.WriteLine("Заполнить таблицу данными? (Y / N)");
            flag = Console.ReadLine().ToUpper();
            while (flag.ToUpper() != "Y" && flag.ToUpper() != "N")
            {
                Console.WriteLine("Не верный ввод.Вывести данные таблицы? (Y/N)");
                flag = Console.ReadLine().ToUpper();
            }

            if (flag == "Y")
            {
                await InsertIntoNewTable(Tables.Last());
            }
        }


        static async Task InsertIntoNewTable(TableInfo table)
        {
            for (var i = 0; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].Name == "ProductId")
                {
                    Console.WriteLine("Укажите название продукта для добавления его Id");
                    string ProdName = Console.ReadLine();

                    string sqlQuery = $@"INSERT INTO {table.Name} ({table.Columns[i].Name}) 
                                        SELECT ProductId FROM Products WHERE ProductName = '{ProdName}'";
                    await ExecuteCommand(sqlQuery);
                }
                else if (table.Columns[i].Name == "CategoryId")
                {

                    Console.WriteLine("Укажите название категории для добавления ее Id");
                    string CategorName = Console.ReadLine();
                    string sqlQuery = $@"INSERT INTO {table.Name} (CategoryId) SELECT CategoryId FROM Categories WHERE CategoryName = '{CategorName}'";
                    await ExecuteCommand(sqlQuery);
                }
                else
                {
                    int num = 0;
                    string str = "";
                    DateTime date;
                    decimal money;
                    {
                        if (table.Columns[i].DataType == "INT")
                        {
                            Console.WriteLine($"Укажите числовое значение поля {table.Name}:");

                            while (!Int32.TryParse(Console.ReadLine(), out num))
                            {
                                Console.WriteLine("Не верный ввод.Введите число:");
                            }
                            string sqlQuery = $@"INSERT INTO {table.Name} ({table.Columns[i].Name}) VALUES ({num})";
                            await ExecuteCommand(sqlQuery);
                        }
                        else if (table.Columns[i].DataType == "NVARCHAR(200)")
                        {
                            Console.WriteLine("Укажите строчное значение поля:");
                            str = Console.ReadLine();
                            string sqlQuery = $@"INSERT INTO {table.Name} ({table.Columns[i].Name}) VALUES ('{str}')";
                            await ExecuteCommand(sqlQuery);
                           
                        }
                        else if (table.Columns[i].DataType == "DATETIME")
                        {
                            Console.WriteLine("Укажите дату:");
                            str = Console.ReadLine();
                            if (DateTime.TryParse(str, out date))
                            {
                                Console.WriteLine($"Вы ввели: {date}");
                                string sqlQuery = $@"INSERT INTO {table.Name} ({table.Columns[i].Name}) VALUES ('{date}')";
                                await ExecuteCommand(sqlQuery);
                               
                            }
                            else
                            {
                                Console.WriteLine("Некорректный ввод даты и времени.");
                            }

                        }
                        else if (table.Columns[i].DataType == "MONEY")
                        {
                            Console.WriteLine("Укажите строчное значение поля:");
                            str = Console.ReadLine();
                            if (decimal.TryParse(str, out money))
                            {
                                Console.WriteLine($"Вы ввели: {money:C}");
                                string sqlQuery = $@"INSERT INTO {table.Name} ({table.Columns[i].Name}) VALUES ({money})";
                                await ExecuteCommand(sqlQuery);
                                
                            }
                            else
                            {
                                Console.WriteLine("Некорректный ввод суммы.");
                            }

                        }

                    }
                }

            }
        }

        
    }
}
    

