namespace CoffeeShop.Application.Configuration;

public class AdminSetupSettings
{
    public const string SectionName = "AdminSetup";

    public string DefaultAdminEmail { get; set; } = "admin@coffeeshop.com";
    public string DefaultAdminPassword { get; set; } = "Admin@123";
    public string SetupSecretKey { get; set; } = "dev-admin-setup-secret-change-me";
    public bool ResetDefaultAdminPasswordOnStartup { get; set; } = true;
}
