﻿// Hello World is a good a place as any to start. So let's start a Hello World TestContainer!

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

// Don't forget to also check "Expose daemon on tcp://localhost:2375 without TLS" in Docker Desktop!
Environment.SetEnvironmentVariable("DOCKER_HOST", "tcp://localhost:2375");

var testcontainersBuilder =
    new TestcontainersBuilder<TestcontainersContainer>()
    .WithImage("hello-world")
    .WithName("hello-world")
    .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole());

await using (var testcontainers = testcontainersBuilder.Build())
{
    await testcontainers.StartAsync();
    Console.ReadKey();
}
