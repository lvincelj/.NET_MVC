using System.Text.Json;

namespace HospitalManagementApp.Tests.Infrastructure;

public static class ApiResponseHelpers
{
    public static async Task<int> ReadIdAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("id").GetInt32();
    }
}
