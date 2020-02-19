namespace JoggingTracker.Core
{
    public static class ErrorMessages
    {
        public const string InternalServerError = "Internal Server Error";
        public const string Forbidden = "You are not allowed to perform this action";
        
        public const string CouldntParseFilter = "Couldn't parse filter string";
        
        public const string UserNameIsRequired = "Username is required";
        public const string RolesAreRequired = "Roles are required";

        public const string PasswordIsRequired = "Password is required";
        public const string OldPasswordIsRequired = "Old password is required";
        public const string NewPasswordIsRequired = "New password is required";
        public const string PasswordShouldBe6CharLong = "Password must be at least 6 characters long";
        public static object CantUpdatePassword = "Can't update password";
        
        public const string DateIsRequired = "Date is requred";
        public const string DistanceIsRequired = "Distance is required";
        public const string TimeIsRequired = "Time is required";
        public const string LatitudeIsRequired = "Latitude is required";
        public const string LongitudeIsRequired = "Longitude is required";

        public const string WeatherProviderInternalError = "Weather provider internal error";
        public const string WeatherProviderTimeout = "Weather provider timeout";

        public const string RunSaveErrorMessage = "An error occured trying to add a run";
        public const string RunUpdateErrorMessage = "An error occured trying to update a run";
        public const string RunDeleteErrorMessage = "An error occured trying to delete a run";
        public const string RunNotFound = "Run is not found";
        
        public const string UserNameExists = "Username already exists";
        public const string UserNotFound = "User is not found";
        public const string UserCantBeCreated = "Can't create user";
        public const string UserCantBeUpdated = "Can't update user";
        public const string UserCantBeDeleted = "An error occured trying to delete user";

        public const string InvalidUserRoles = "Invalid roles";
        public const string LatitudeValue = "Latitude value should be between -90 and 90";
        public const string LongitudeValue = "Longitude value should be between -180 and 180";
    }
}