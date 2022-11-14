namespace Testcontainers.Solution2_5
{
    using DotNet.Testcontainers.Builders;
    using DotNet.Testcontainers.Configurations;
    using DotNet.Testcontainers.Containers;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using TestContainers.Shared.Contexts;
    using TestContainers.Shared.Services;

    public class TestcontainersTest
    {
        [Fact]
        public async Task Removing_Stale_Accounts_Should_Remove_All_Stale_Accounts()
        {
            // Arrange
            Environment.SetEnvironmentVariable("DOCKER_HOST", "tcp://localhost:2375");

            var testcontainersBuilder =
                new TestcontainersBuilder<PostgreSqlTestcontainer>()
                // 1) Create a new PostgreSql database
                .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
                .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted($"pg_isready -h 'localhost' -p '5432'"));

            await using var container = testcontainersBuilder.Build();

            await container.StartAsync()
                .ConfigureAwait(false);

            // 2) Copy the backup file into the container

            var command = "pg_restore --username=db_user --dbname=db -1 /tmp/db_backup.dump";

            await container.ExecAsync(command.Split(' '));

            var storeContext = new StoreContext(
                new DbContextOptionsBuilder<StoreContext>()
                .UseNpgsql(container.ConnectionString)
                .Options);

            var service = new AccountService(storeContext);

            // Act
            // 3) Retrieve all the acounts from the accountService or using a raw query from the StoreContext:
            // _storeContext.Database.ExecuteSqlInterpolated($"SELECT TOP {5} FROM accounts");

            // 4) Remove all stale accounts. A stale account is an account where the field last_login is null
            // the result of either the service call or the SQL query is the number of rows affected

            // Assert
            (numberOfAccounts - numberOfRemovedAccounts).Should().Be(852);
        }
    }
}