using System.Net.Http.Headers;
using Xunit;

namespace HospitalManagementApp.Tests.Infrastructure;

[Collection("IntegrationTests")]
public abstract class ApiTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected ApiTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.SchemeName);
    }

    protected Task ResetDatabaseAsync() => Factory.ResetDatabaseAsync();
}
