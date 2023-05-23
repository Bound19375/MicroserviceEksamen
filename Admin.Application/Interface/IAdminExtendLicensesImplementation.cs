namespace Admin.Application.Interface;

public interface IAdminExtendLicensesImplementation
{
    Task<string> ExtendLicense(int minutesToExtend, string? discordId = null);
}