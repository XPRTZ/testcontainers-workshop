# About

This solution was part of the workshop 'Destructive testing with Testcontainers' given at the [Blipz on the Radar](https://www.blipz.io/) conference of 2022.

Part of this code is based on examples in the [Testcontainers for .NET](https://github.com/testcontainers/testcontainers-dotnet) repository.

# Talk description

## Destructive testing with Testcontainers

We've all been there: needing to test something with external dependencies. Usually, it's a database or an API.
And it always starts hopeful, a small mock here, a little setup there. But before you know it: it's become a hot mess of hard-to-read setup code and mocked data because we can’t do destructive tests on live environments.

In this talk we'll walk through why this is a problem, what testcontainers are and how they can help solve this problem.

# Prerequisites

To run the solution please make sure to have installed:
- Docker Desktop
- .NET 6 SDK
- Visual Studio 2022 (or later) or Visual Studio Code