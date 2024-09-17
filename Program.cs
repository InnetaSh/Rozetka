using Microsoft.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;
using System;

internal class Program
{
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


        await Menu();
        static async Task Menu()
        {

            bool tableExists = await CheckIfTableExistsAsync("Categories");
            if (!tableExists)
            {
                string sqlQuery = "CREATE TABLE Categories (CategoryId  INT PRIMARY KEY IDENTITY, CategoryName  NVARCHAR(40) NOT NULL)";
                await ExecuteCommand(sqlQuery);
            }

            //string sqlQuery_DROPProducts = "DROP TABLE Products ";
            //await ExecuteCommand(sqlQuery_DROPProducts);
            
            tableExists = await CheckIfTableExistsAsync("Products");
            if (!tableExists)
            {
                string sqlQuery = "CREATE TABLE Products (ProductId  INT PRIMARY KEY IDENTITY, ProductName NVARCHAR(40) NOT NULL, Description NVARCHAR(40) NOT NULL,Price DECIMAL(10,2) DEFAULT 0 ,CategoryId INT  REFERENCES Categories(CategoryId ) ON DELETE CASCADE,Stock INT)";
                await ExecuteCommand(sqlQuery);
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
                string sqlQuery = "CREATE TABLE ShoppingCart (AccountId  INT NOT NULL REFERENCES Accounts(AccountId ) ON DELETE CASCADE,ProductId  INT NOT NULL REFERENCES Products(ProductID) ON DELETE CASCADE)";
                await ExecuteCommand(sqlQuery);
            }



            Console.WriteLine("Выберите:");
            int action;


            Console.WriteLine("1 - Войти");
            Console.WriteLine("2 - У вас ещё нет аккаунта? Зарегистрироваться.");

            Console.WriteLine("3 - выход");

            Console.Write("действие - ");
            while (!Int32.TryParse(Console.ReadLine(), out action) || action < 1 || action > 3)
            {
                Console.WriteLine("Не верный ввод.Введите число:");
                Console.Write("действие - ");
            }

            string login = "";
            string password = "";

            switch (action)
            {
                case 1:
                    Console.WriteLine("Введите логин");
                    login = Console.ReadLine();

                    Console.WriteLine("Введите пароль");
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
                        Menu();
                    }
                    break;
                case 3:
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
                                }
                                else
                                {
                                    await SubMenuAccount(login);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Неправильный пароль");
                                Console.WriteLine("Введите пароль");
                                password = Console.ReadLine();

                                await passwordValid(login, password);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Для входа в систему необходимо зарегистрироваться.");

                            Thread.Sleep(2000);
                            Console.Clear();
                            await Menu();
                        }
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

            Console.WriteLine("9 - выход");


            Console.Write("действие - ");
            while (!Int32.TryParse(Console.ReadLine(), out action) || action < 1 || action > 9)
            {
                Console.WriteLine("Не верный ввод.Введите число:");
                Console.Write("действие - ");
            }

            switch (action)
            {
                case 1:
                    Console.WriteLine("Введите название категории:");
                    string Category = Console.ReadLine();


                    string sqlQuery = @"INSERT INTO Categories (CategoryName) VALUES (@Category)";
                    await ExecuteCommand_Categories(sqlQuery, Category);
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

                        string sqlExpressionUPDATE_Categories_1 = "UPDATE Products SET CategoryId = NULL  WHERE CategoryId IN (SELECT CategoryId FROM Categories WHERE CategoryName = @Category)";
                        string sqlExpressionDELETE_Categories_2 = "DELETE FROM Categories FROM Categories WHERE CategoryName = @Category";
                        await ExecuteCommand_DeleteCategories(sqlExpressionUPDATE_Categories_1, sqlExpressionDELETE_Categories_2, Category);

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



                        sqlQuery = @"INSERT INTO Products (ProductName, Description, Price, CategoryId, Stock) 
                            VALUES (@nameProduct, @DescriptionProduct, @PriceProduct, 
                            (SELECT CategoryId FROM Categories WHERE CategoryName = @Category), @StockProduct)";

                        await ExecuteCommand_Products(sqlQuery, Category, nameProd, DescriptionProd, PriceProd, StockProd);
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

                    sqlQuery = @"DELETE FROM Products WHERE ProductName = @nameProduct";
                    await ExecuteCommand_Prod(sqlQuery, nameProduct);


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
                        Console.WriteLine("Введите название продукта:");
                        nameProduct = Console.ReadLine();

                        Console.WriteLine("Введите описание продукта:");
                        string DescriptionProduct = Console.ReadLine();
                        decimal PriceProduct;
                        Console.WriteLine("Введите цену продукта:");
                        while (!decimal.TryParse(Console.ReadLine(), out PriceProduct) || PriceProduct < 0)
                        {
                            Console.WriteLine("Не верный ввод.Введите число:");
                            Console.Write("действие - ");
                        }
                        int StockProduct;
                        Console.WriteLine("Введите количество продукта на складе:");
                        while (!Int32.TryParse(Console.ReadLine(), out StockProduct) || StockProduct < 0)
                        {
                            Console.WriteLine("Не верный ввод.Введите число:");
                            Console.Write("действие - ");
                        }


                        sqlQuery = @"UPDATE  Products SET ProductName = nameProduct,Description = DescriptionProduct,Price = PriceProduct,CategoryId IN (SELECT CategoryId FROM Categories  WHERE CategoryName = @Category), Stock = StockProduct) WHERE WHERE ProductName = @nameProduct";
                        await ExecuteCommand_Products(sqlQuery, Category, nameProduct, DescriptionProduct, PriceProduct, StockProduct);


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

                        sqlQuery = @"UPDATE Products SET Price = Price * (100 + @percent)/100 FROM Products  JOIN Categories ON Categories.CategoryId = Products.CategoryId  WHERE CategoryName = @Category ";
                        await ExecuteCommand_percentPrice(sqlQuery, Category, percent);



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
                    await ShowProducts();

                    Console.WriteLine("Нажмите любую клавишу для выхода в меню...");
                    Console.ReadKey();
                    Console.Clear();
                    await SubMenuAdmin();
                    break;

                case 9:
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

            Console.WriteLine("5 - выход");


            Console.Write("действие - ");
            while (!Int32.TryParse(Console.ReadLine(), out action) || action < 1 || action > 5)
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
                        ShowProductsByCategories(Category);

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
                    await ShowProducts();

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
                    await ShowProducts();

                    Console.WriteLine("Нажмите любую клавишу для выхода в меню...");
                    Console.ReadKey();
                    Console.Clear();
                    await SubMenuAccount(login);
                    break;


                case 5:
                    await Menu();
                    break;
            }



        }



        static async Task<bool> CheckIfTableExistsAsync(string tableName)
        {
            string query = @"IF EXISTS( SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName ) SELECT CAST(1 AS BIT) ELSE SELECT CAST(0 AS BIT)";

            return await ExecuteQueryAsync(query, new SqlParameter("@TableName", tableName));
        }

        static async Task<bool> ExecuteQueryAsync(string query, SqlParameter parameter)
        {
            string connectionString = @"Data Source=DESKTOP-ITRLGSN; Initial Catalog=Rozetka; Trusted_Connection=True; TrustServerCertificate=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
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

        static async Task ExecuteCommand_Categories(string sqlQuery,string Category)
        {
            string connectionString = @"Data Source = DESKTOP-ITRLGSN; Initial Catalog = Rozetka; Trusted_Connection=True; TrustServerCertificate = True";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sqlQuery, connection);

                command.Parameters.AddWithValue("@Category", Category);
               
                await command.ExecuteNonQueryAsync();


            }

        }

        static async Task ExecuteCommand_DeleteCategories(string sqlQuery, string sqlQuery2, string Category)
        {
            string connectionString = @"Data Source = DESKTOP-ITRLGSN; Initial Catalog = Rozetka; Trusted_Connection=True; TrustServerCertificate = True";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                await connection.OpenAsync();


                SqlCommand command = new SqlCommand(sqlQuery, connection);
                command.Parameters.AddWithValue("@Category", Category);
                int number = await command.ExecuteNonQueryAsync();


                SqlCommand command_2 = new SqlCommand(sqlQuery2, connection);
                command_2.Parameters.AddWithValue("@Category", Category);
                await command_2.ExecuteNonQueryAsync();

                

                await command.ExecuteNonQueryAsync();


            }

        }

        static async Task ExecuteCommand_Prod(string sqlQuery, string Product)
        {
            string connectionString = @"Data Source = DESKTOP-ITRLGSN; Initial Catalog = Rozetka; Trusted_Connection=True; TrustServerCertificate = True";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sqlQuery, connection);

                command.Parameters.AddWithValue("@nameProduct", Product);

                await command.ExecuteNonQueryAsync();


            }

        }

        static async Task ExecuteCommand_Products(string sqlQuery, string Category,string nameProduct, string DescriptionProduct, decimal PriceProduct, int StockProduct )
        {
            
            string connectionString = @"Data Source = DESKTOP-ITRLGSN; Initial Catalog = Rozetka; Trusted_Connection=True; TrustServerCertificate = True";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sqlQuery, connection);

                command.Parameters.AddWithValue("@Category", Category);
                command.Parameters.AddWithValue("@nameProduct", nameProduct);
                command.Parameters.AddWithValue("@DescriptionProduct", DescriptionProduct);
                command.Parameters.AddWithValue("@PriceProduct", PriceProduct);
                command.Parameters.AddWithValue("@StockProduct", StockProduct);

                await command.ExecuteNonQueryAsync();

            }
        }

        static async Task ExecuteCommand_percentPrice(string sqlQuery, string Category, int percent)
        {

            string connectionString = @"Data Source = DESKTOP-ITRLGSN; Initial Catalog = Rozetka; Trusted_Connection=True; TrustServerCertificate = True";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sqlQuery, connection);

                command.Parameters.AddWithValue("@Category", Category);
                command.Parameters.AddWithValue("@percent", percent);

                await command.ExecuteNonQueryAsync();


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
                Console.WriteLine("Table created");


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

        static async Task ShowProducts()
        {
            string connectionString = @"Data Source = DESKTOP-ITRLGSN; Initial Catalog = Rozetka; Trusted_Connection=True; TrustServerCertificate = True";

            string sqlExpression = "SELECT * FROM Products";


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
                    string columnName4 = reader.GetName(3);
                    string columnName5 = reader.GetName(4);
                    string columnName6 = reader.GetName(5);



                    Console.WriteLine($"{columnName1,-10}\t{columnName2,-20}\t{columnName4,-10}\t{columnName5,-10}\t{columnName6,-10}\t{columnName3,-40}");


                    while (await reader.ReadAsync())
                    {

                        int ProductId = reader.GetInt32(0);
                        string nameProduct = reader.GetString(1);
                        string DescriptionProduct = reader.GetString(2);

                        object Price = reader.GetValue(3);
                        object CategoryId = reader.GetValue(4);
                        object Stock = reader.GetValue(5);



                        Console.WriteLine($"{ProductId,-9} \t{nameProduct,-20} \t{Price,-10} \t{CategoryId,-10} \t{Stock,-10} \t{DescriptionProduct,-40}");
                    }
                }

                await reader.CloseAsync();


            }

        }

        static async Task ShowProductsByCategories(string Category)
        {
            string connectionString = @"Data Source = DESKTOP-ITRLGSN; Initial Catalog = Rozetka; Trusted_Connection=True; TrustServerCertificate = True";

            string sqlExpression = "SELECT * FROM Products WHERE CategoryId IN (SELECT CategoryId FROM Categories WHERE CategoryName = @Category";


            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.Parameters.AddWithValue("@Category", Category);
                SqlDataReader reader = await command.ExecuteReaderAsync();


                if (reader.HasRows)
                {
                    string columnName1 = reader.GetName(0);
                    string columnName2 = reader.GetName(1);
                    string columnName3 = reader.GetName(2);
                    string columnName4 = reader.GetName(3);
                    string columnName5 = reader.GetName(4);
                    string columnName6 = reader.GetName(5);



                    Console.WriteLine($"{columnName1,-10}\t{columnName2,-20}\t{columnName3,-40}\t{columnName4,-10}\t{columnName5,-10}\t{columnName6,-10}");


                    while (await reader.ReadAsync())
                    {

                        int ProductId = reader.GetInt32(0);
                        string nameProduct = reader.GetString(1);
                        string DescriptionProduct = reader.GetString(2);

                        object Price = reader.GetValue(3);
                        object CategoryId = reader.GetValue(4);
                        object Stock = reader.GetValue(5);



                        Console.WriteLine($"{ProductId,-9} \t{nameProduct,-20} \t{DescriptionProduct,-40} \t{Price,-10} \t{CategoryId,-10} \t{Stock,-10}");
                    }
                }

                await reader.CloseAsync();


            }

        }
    }
}
