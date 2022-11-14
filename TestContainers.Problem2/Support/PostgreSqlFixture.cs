namespace TestContainers.Problem2.Support
{
    using System.Threading.Tasks;

    using DotNet.Testcontainers.Builders;
    using DotNet.Testcontainers.Configurations;
    using DotNet.Testcontainers.Containers;

    using Npgsql;

    public sealed class PostgreSqlFixture : IAsyncLifetime
    {
        public PostgreSqlTestcontainer Container { get; private set; }

        public NpgsqlConnection? Connection { get; private set; }

        public PostgreSqlFixture()
        {
            Environment.SetEnvironmentVariable("DOCKER_HOST", "tcp://localhost:2375");

            var testcontainersBuilder =
                new TestcontainersBuilder<PostgreSqlTestcontainer>()
                // 1) Create a new PostgreSql database
                .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
                .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted($"pg_isready -h 'localhost' -p '5432'"));

            Container = testcontainersBuilder.Build();
        }

        public async Task UseBackupFile(byte[] backupFile)
        {
            // 2) Copy the backup file into the container

            var command = "pg_restore --username=db_user --dbname=db -1 /tmp/db_backup.dump";

            await Container.ExecAsync(command.Split(' '));
        }

        public async Task InitializeAsync()
        {
            await Container.StartAsync();

            Connection = new NpgsqlConnection(Container.ConnectionString);
        }

        public async Task DisposeAsync()
        {
            if (Connection is not null)
            {
                await Connection.DisposeAsync();
            }

            await Container.DisposeAsync();
        }
    }
}
