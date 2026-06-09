using Microsoft.EntityFrameworkCore;

namespace HospitalManagementApp.Data;

public static class AppDataSeed
{
    public static async Task SeedDemoDataAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await context.Patients.AnyAsync() || await context.Doctors.AnyAsync())
        {
            return;
        }

        context.AddRange(
            MockData.DeptCardiology,
            MockData.DeptNeurology,
            MockData.DeptGeneralMed,
            MockData.Doc1,
            MockData.Doc2,
            MockData.Doc3,
            MockData.Pat1,
            MockData.Pat2,
            MockData.Pat3,
            MockData.Mr1,
            MockData.Mr2,
            MockData.Mr3,
            MockData.Presc1,
            MockData.Presc2,
            MockData.Presc3,
            MockData.Med1,
            MockData.Med2,
            MockData.Med3,
            MockData.Appt1,
            MockData.Appt2,
            MockData.Appt3);

        await context.SaveChangesAsync();
    }
}