namespace TestContainers.Solution3.Tests
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;

    using DotNet.Testcontainers.Builders;
    using FluentAssertions;

    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Support.Extensions;
    using OpenQA.Selenium.Support.UI;

    using TestContainers.Solution3.Fixtures;

    using Xunit;

    public sealed class OrderTests : IClassFixture<DemoAppContainer>
    {
        private static readonly ChromeOptions _chromeOptions = new();

        private readonly DemoAppContainer _demoAppContainer;

        static OrderTests()
        {
            //_chromeOptions.AddArgument("headless");
            _chromeOptions.AddArgument("ignore-certificate-errors");
        }

        public OrderTests(DemoAppContainer demoAppContainer)
        {
            _demoAppContainer = demoAppContainer;
            _demoAppContainer.SetBaseAddress();
        }

        [Fact]
        [Trait("Category", nameof(OrderTests))]
        public void Get_Orders_Should_Return_626_Pages_Of_Orders()
        {
            // Arrange
            string ScreenshotFileName() => $"{nameof(Get_Orders_Should_Return_626_Pages_Of_Orders)}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.png";

            using var chrome = new ChromeDriver(_chromeOptions);

            // Act
            chrome.Navigate().GoToUrl(_demoAppContainer.BaseAddress);

            chrome.TakeScreenshot().SaveAsFile(Path.Combine(CommonDirectoryPath.GetProjectDirectory().DirectoryPath, ScreenshotFileName()));

            chrome.FindElement(By.Id("orders_link")).Click();

            var webDriverWait = new WebDriverWait(chrome, TimeSpan.FromSeconds(10));
            var span = webDriverWait.Until(x => x.FindElement(By.ClassName("pager-display")).FindElement(By.TagName("span")));

            chrome.TakeScreenshot().SaveAsFile(Path.Combine(CommonDirectoryPath.GetProjectDirectory().DirectoryPath, ScreenshotFileName()));

            // Assert
            span.Text.Should().Be("1 of 626");
        }
    }
}
