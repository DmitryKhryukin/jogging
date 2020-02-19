namespace JoggingTracker.DataAccess
{
    public class UserRoles
    {
        public const string Admin = "Admin";
        public const string UserManager = "UserManager";
        public const string RegularUser = "RegularUser";

        public static string[] AllRoles
        {
            get { return new[] {Admin, UserManager, RegularUser}; }
        }
    }
}