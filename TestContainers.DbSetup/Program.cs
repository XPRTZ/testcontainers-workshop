using System.Globalization;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

using Npgsql;

using PeanutButter.RandomGenerators;

using RandomNameGeneratorLibrary;

using TestContainers.DbSetup;

Environment.SetEnvironmentVariable("DOCKER_HOST", "tcp://localhost:2375");

var directoryInfo = Directory.CreateDirectory($"{CommonDirectoryPath.GetProjectDirectory().DirectoryPath}/database-mount");

var testcontainersBuilder =
    new TestcontainersBuilder<PostgreSqlTestcontainer>()
    .WithDatabase(new PostgreSqlTestcontainerConfiguration()
    {
        Database = "db",
        Username = "db_user",
        Password = "db_password",
        Port = 49203
    })
    .WithBindMount(directoryInfo.FullName, "/tmp")
    .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
    .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432));

await using (var testcontainers = testcontainersBuilder.Build())
{
    await testcontainers.StartAsync();

    var command = "pg_dump --username=db_user --if-exists --clean --create --format=c --file=/tmp/db_backup.dump db";

    using var connection = new NpgsqlConnection(testcontainers.ConnectionString);

    var personGenerator = new PersonNameGenerator();
    var persons = personGenerator.GenerateMultipleFirstAndLastNames(1000);
    var random = new Random();

    connection.Open();

    var execresult = await testcontainers.ExecScriptAsync(File.ReadAllText("db_structure.pgsql"));

    var itemList = RandomValueGen.GetRandomCollection(() =>
        new Item
        {
            Description = RandomValueGen.GetRandomWords(1, 3),
            Price = RandomValueGen.GetRandomMoney(),
            Stock = RandomValueGen.GetRandomInt(0, 100),
        },
        minValues: 50,
        maxValues: 50);

    foreach (var item in itemList)
    {
        var sql = $"INSERT INTO items (description, price, stock) VALUES ('{item.Description}', '{item.Price.ToString(CultureInfo.InvariantCulture)}', '{item.Stock}') RETURNING item_id";

        using var cmd = new NpgsqlCommand(sql, connection);

        item.ItemId = (int)cmd.ExecuteScalar()!;
    }

    foreach (var person in persons)
    {
        if (person.Split(' ') is [var firstName, var lastName])
        {
            var now = DateTime.UtcNow;
            var creationDate = RandomValueGen.GetRandomDate(now.AddYears(-5), now.AddDays(-7));
            DateTime? lastLoginDate = RandomValueGen.GetRandomDate(creationDate, now.AddDays(-1));
            var email = $"{RandomValueGen.GetRandomUserName(firstName, lastName)}@{RandomValueGen.GetRandomDomain()}";
            var neverLoggedIn = RandomValueGen.GetRandomInt(0, 5) % 5 == 0;
            var sql = string.Empty;

            if (neverLoggedIn)
            {
                sql = $"INSERT INTO accounts (email, password, first_name, last_name, created_on) VALUES ('{email}', '{RandomValueGen.GetRandomAlphaNumericString(8, 15)}', '{firstName}', '{lastName}', '{creationDate:yyyy-MM-dd HH:mm:ss}') RETURNING user_id";

                using var cmd = new NpgsqlCommand(sql, connection);
                await cmd.ExecuteNonQueryAsync();
            }
            else
            {
                sql = $"INSERT INTO accounts (email, password, first_name, last_name, created_on, last_login) VALUES ('{email}', '{RandomValueGen.GetRandomAlphaNumericString(8, 15)}', '{firstName}', '{lastName}', '{creationDate:yyyy-MM-dd HH:mm:ss}', '{lastLoginDate:yyyy-MM-dd HH:mm:ss}') RETURNING user_id";

                using var cmd = new NpgsqlCommand(sql, connection);
                var user_id = (int)(await cmd.ExecuteScalarAsync())!;

                foreach (var item in RandomValueGen.GetRandomSelectionFrom(itemList, 0, 20))
                {
                    sql = $"INSERT INTO orders (user_id, item_id, number_of_items, order_date) VALUES ('{user_id}', '{item.ItemId}', '{RandomValueGen.GetRandomInt(1, 5)}', '{RandomValueGen.GetRandomDate(creationDate, lastLoginDate):yyyy-MM-dd HH:mm:ss}')";

                    using var cmdOrder = new NpgsqlCommand(sql, connection);
                    await cmdOrder.ExecuteNonQueryAsync();
                }
            }
        }
    }

    await testcontainers.ExecAsync(command.Split(' '));

    connection.Close();
}
