namespace TestContainers.Solution2.Support
{
    using BoDi;

    using Microsoft.EntityFrameworkCore;

    using TechTalk.SpecFlow.Assist;
    using TechTalk.SpecFlow.Assist.ValueRetrievers;

    using TestContainers.Shared.Contexts;
    using TestContainers.Shared.Services;

    [Binding]
    internal class Binding
    {
        private readonly IObjectContainer _objectContainer;
        private readonly FeatureContext _featureContext;

        public Binding(
            IObjectContainer objectContainer,
            FeatureContext featureContext)
        {
            _objectContainer = objectContainer ?? throw new ArgumentNullException(nameof(objectContainer));
            _featureContext = featureContext ?? throw new ArgumentNullException(nameof(featureContext));
        }

        [BeforeTestRun]
        public static void BeforeTestRun() => Service.Instance.ValueRetrievers.Register(new NullValueRetriever("<null>"));

        [BeforeFeature]
        public static async Task BeforeFeature(FeatureContext featureContext)
        {
            PostgreSqlFixture postgreSqlFixture = new();

            await postgreSqlFixture.InitializeAsync();
            await postgreSqlFixture.UseBackupFile(await File.ReadAllBytesAsync("Support/db_backup.dump"));

            var optionsBuilder = new DbContextOptionsBuilder<StoreContext>();
            optionsBuilder.UseNpgsql(postgreSqlFixture.Connection!);

            featureContext.Set(postgreSqlFixture);
        }

        [AfterFeature]
        public static async Task AfterFeature(FeatureContext featureContext)
        {
            var postgreSqlFixture = featureContext.Get<PostgreSqlFixture>();

            await postgreSqlFixture.DisposeAsync();
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            var postgreSqlFixture = _featureContext.Get<PostgreSqlFixture>();
            var storeContext = new StoreContext(
                new DbContextOptionsBuilder<StoreContext>()
                .UseNpgsql(postgreSqlFixture.Connection!)
                .Options);

            _objectContainer.RegisterFactoryAs(factory =>
            {
                return new AccountService(storeContext);
            });

            _objectContainer.RegisterFactoryAs(factory =>
            {
                return new OrderService(storeContext);
            });
        }
    }
}
