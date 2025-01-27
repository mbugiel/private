namespace ManagemateAPI.Management.M_User.AuthAPI_Input_Objects
{
    public class AuthAPI_Set_Passwd_Data
    {
        public string Email { get; set; }
        public string newPassword { get; set; }
        public string EmailToken { get; set; }
    }
}
