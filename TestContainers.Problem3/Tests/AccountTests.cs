namespace TestContainers.Problem3.Tests
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using DotNet.Testcontainers.Builders;

    using FluentAssertions;

    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;

    using TestContainers.Problem3.Fixtures;

    using Xunit;

    public sealed class AccountTests : IClassFixture<DemoAppContainer>
    {
        private static readonly ChromeOptions _chromeOptions = new();

        private readonly DemoAppContainer _demoAppContainer;

        static AccountTests()
        {
            _chromeOptions.AddArgument("headless");
            _chromeOptions.AddArgument("ignore-certificate-errors");
        }

        public AccountTests(DemoAppContainer demoAppContainer)
        {
            _demoAppContainer = demoAppContainer;
            _demoAppContainer.SetBaseAddress();
        }

        [Fact]
        [Trait("Category", nameof(AccountTests))]
        public async Task Get_Accounts_Should_Return_100_Pages_Of_Accounts()
        {
            // Arrange
            string ScreenshotFileName() => $"{nameof(Get_Accounts_Should_Return_100_Pages_Of_Accounts)}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.png";

            using var chrome = new ChromeDriver(_chromeOptions);

            // Act
            chrome.Navigate().GoToUrl(_demoAppContainer.BaseAddress);

            chrome.GetScreenshot().SaveAsFile(Path.Combine(CommonDirectoryPath.GetProjectDirectory().DirectoryPath, ScreenshotFileName()));

            chrome.FindElement(By.Id("accounts_link")).Click();

            await Task.Delay(TimeSpan.FromSeconds(5));

            chrome.GetScreenshot().SaveAsFile(Path.Combine(CommonDirectoryPath.GetProjectDirectory().DirectoryPath, ScreenshotFileName()));

            // Assert
            var span = chrome.FindElement(By.ClassName("pager-display")).FindElement(By.TagName("span"));

            span.Text.Should().Be("1 of 100");
        }
    }
}
