using Crosscutting;

namespace DiscordBot.Domain
{
    public class DiscordBotEntity
    {
        public DiscordBotEntity(DiscordModelDto model, IsValidCheck check)
        {
            try
            {
                switch (check)
                {
                    case IsValidCheck.StaffLicense:
                        IsValidStaffLicense(model);
                        break;
                }
            }
            catch
            {
                throw new Exception("Passed Model For Discord Bot Denied By Business Logic");
            }
        }
        protected bool IsValidStaffLicense(DiscordModelDto model)
        {
            return
                model.Roles!.Contains("860603777790771211")
                || model.Roles.Contains("860628656259203092");
        }



        public enum IsValidCheck
        {
            StaffLicense,
        }
    }

}
