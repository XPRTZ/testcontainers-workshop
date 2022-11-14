namespace TestContainers.Solution3.Fixtures;

using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

using Xunit;

public sealed class DemoAppContainer : HttpClient, IAsyncLifetime
{
    private static readonly X509Certificate Certificate = new X509Certificate2(DemoAppImage.CertificateFilePath, DemoAppImage.CertificatePassword);

    private static readonly DemoAppImage Image = new();

    private readonly IDockerNetwork _demoAppNetwork;

    private readonly IDockerContainer _postgresqlContainer;

    private readonly IDockerContainer _demoAppContainer;

    public DemoAppContainer()
      : base(new HttpClientHandler
      {
          // Trust the development certificate.
          ServerCertificateCustomValidationCallback = (_, certificate, _, _) => Certificate.Equals(certificate)
      })
    {
        Environment.SetEnvironmentVariable("DOCKER_HOST", "tcp://localhost:2375");

        const string demoAppStorage = "demoAppStorage";

        var postgreSqlConfiguration = new PostgreSqlTestcontainerConfiguration()
        {
            Database = "db",
            Username = "db_user",
            Password = "db_password",
        };

        var connectionString = $"Server={demoAppStorage};Database={postgreSqlConfiguration.Database};User Id={postgreSqlConfiguration.Username};Password={postgreSqlConfiguration.Password};";

        _demoAppNetwork = new TestcontainersNetworkBuilder()
          .WithName(Guid.NewGuid().ToString("D"))
          .Build();

        _postgresqlContainer =
            new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(postgreSqlConfiguration)
            .WithNetwork(_demoAppNetwork)
            .WithNetworkAliases(demoAppStorage)
            .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
            .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted($"pg_isready -h 'localhost' -p '5432'"))
            .Build();

        _demoAppContainer = new TestcontainersBuilder<TestcontainersContainer>()
          .WithImage(Image)
          .WithNetwork(_demoAppNetwork)
          .WithPortBinding(DemoAppImage.HttpsPort, true)
          .WithEnvironment("ASPNETCORE_URLS", "https://+")
          .WithEnvironment("ASPNETCORE_Kestrel__Certificates__Default__Path", DemoAppImage.CertificateFilePath)
          .WithEnvironment("ASPNETCORE_Kestrel__Certificates__Default__Password", DemoAppImage.CertificatePassword)
          .WithEnvironment("ConnectionStrings__StoreConnectionString", connectionString)
          .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(DemoAppImage.HttpsPort))
          .Build();
    }

    public async Task InitializeAsync()
    {
        // It is not necessary to clean up resources immediately (still good practice). The Resource Reaper will take care of orphaned resources.
        await Image.InitializeAsync();

        await _demoAppNetwork.CreateAsync();

        await _postgresqlContainer.StartAsync();

        await UseDatabaseBackup();

        await _demoAppContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Image.DisposeAsync();

        await _demoAppContainer.DisposeAsync();

        await _postgresqlContainer.DisposeAsync();

        await _demoAppNetwork.DeleteAsync();
    }

    public void SetBaseAddress()
    {
        try
        {
            var uriBuilder = new UriBuilder("https", _demoAppContainer.Hostname, _demoAppContainer.GetMappedPublicPort(DemoAppImage.HttpsPort));
            BaseAddress = uriBuilder.Uri;
        }
        catch
        {
            // Set the base address only once.
        }
    }

    public async Task UseDatabaseBackup()
    {
        await _postgresqlContainer.CopyFileAsync("/tmp/db_backup.dump", await File.ReadAllBytesAsync("Fixtures/db_backup.dump"));

        var command = "pg_restore --username=db_user --dbname=db -1 /tmp/db_backup.dump";

        await _postgresqlContainer.ExecAsync(command.Split(' '));
    }
}
