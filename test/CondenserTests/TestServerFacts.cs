﻿using CondenserDotNet.Client;
using CondenserTests.Fakes;
using Microsoft.Extensions.Configuration;
using CondenserDotNet.Client.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CondenserTests
{
    public class TestServerFacts
    {
        [Fact]
        public async void CanReloadOptionDetails()
        {
            var registry = new FakeConfigurationRegistry();
            await registry.SetKeyAsync("FakeConfig:Setting1", "abc");
            await registry.SetKeyAsync("FakeConfig:Setting2", "def");

            var root = new ConfigurationBuilder()
                .AddJsonConsul(registry)
                .Build();

            var builder = new WebHostBuilder()
                .Configure(x => x.UseMvcWithDefaultRoute())
                .ConfigureServices(x =>
                {
                    x.AddMvcCore();
                    x.AddOptions();
                    x.ConfigureReloadable<FakeConfig>(root, registry);
                });

            using (var server = new TestServer(builder))
            {
                using (var client = server.CreateClient())
                {
                    var response = await client.GetAsync("Fake");
                    var setting = await response.Content.ReadAsStringAsync();

                    Assert.Equal("Config: abc", setting);

                    await registry.SetKeyAsync("FakeConfig:Setting1", "this is the new setting");
                    registry.FakeReload();

                    response = await client.GetAsync("Fake");
                    setting = await response.Content.ReadAsStringAsync();

                    Assert.Equal("Config: this is the new setting", setting);
                }
            }
        }
    }
}