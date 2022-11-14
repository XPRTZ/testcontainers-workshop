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
                .WithDatabase(new PostgreSqlTestcontainerConfiguration()
                {
                    Database = "db",
                    Username = "db_user",
                    Password = "db_password",
                })
                .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
                .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted($"pg_isready -h 'localhost' -p '5432'"));

            await using var container = testcontainersBuilder.Build();

            await container.StartAsync()
                .ConfigureAwait(false);

            await container.CopyFileAsync("/tmp/db_backup.dump", await File.ReadAllBytesAsync("db_backup.dump"));

            var command = "pg_restore --username=db_user --dbname=db -1 /tmp/db_backup.dump";

            await container.ExecAsync(command.Split(' '));

            var storeContext = new StoreContext(
                new DbContextOptionsBuilder<StoreContext>()
                .UseNpgsql(container.ConnectionString)
                .Options);

            var service = new AccountService(storeContext);

            // Act
            var numberOfAccounts = service.GetAllAccounts().Count();
            var numberOfRemovedAccounts = service.RemoveAllStaleAccounts();

            // Assert
            (numberOfAccounts - numberOfRemovedAccounts).Should().Be(852);
        }
    }
}