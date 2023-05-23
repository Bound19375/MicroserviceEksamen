namespace Admin.Application.Interface;

public interface IAdminExtendLicensesRepository
{
    Task<string> ExtendLicense(int minutesToExtend, string? discordId = null);
}