namespace TestContainers.Problem3.Tests
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;

    using DotNet.Testcontainers.Builders;

    using FluentAssertions;

    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;

    using TestContainers.Problem3.Fixtures;

    using Xunit;

    public sealed class OrderTests : IClassFixture<DemoAppContainer>
    {
        private static readonly ChromeOptions _chromeOptions = new();

        private readonly DemoAppContainer _demoAppContainer;

        static OrderTests()
        {
            _chromeOptions.AddArgument("headless");
            _chromeOptions.AddArgument("ignore-certificate-errors");
        }

        public OrderTests(DemoAppContainer demoAppContainer)
        {
            _demoAppContainer = demoAppContainer;
            _demoAppContainer.SetBaseAddress();
        }

        [Fact]
        [Trait("Category", nameof(OrderTests))]
        public async Task Get_Orders_Should_Return_626_Pages_Of_Orders()
        {
            // Arrange
            string ScreenshotFileName() => $"{nameof(Get_Orders_Should_Return_626_Pages_Of_Orders)}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.png";

            using var chrome = new ChromeDriver(_chromeOptions);

            // Act
            chrome.Navigate().GoToUrl(_demoAppContainer.BaseAddress);

            chrome.GetScreenshot().SaveAsFile(Path.Combine(CommonDirectoryPath.GetProjectDirectory().DirectoryPath, ScreenshotFileName()));

            chrome.FindElement(By.Id("orders_link")).Click();

            await Task.Delay(TimeSpan.FromSeconds(5));

            chrome.GetScreenshot().SaveAsFile(Path.Combine(CommonDirectoryPath.GetProjectDirectory().DirectoryPath, ScreenshotFileName()));

            // Assert
            var span = chrome.FindElement(By.ClassName("pager-display")).FindElement(By.TagName("span"));

            span.Text.Should().Be("1 of 626");
        }
    }
}
