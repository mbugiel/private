using ManagemateAPI.Management.M_Session.Input_Objects;

/*
 * Add_Authorized_Worker_Data class
 * 
 * This class is used in
 */

namespace ManagemateAPI.Management.M_Authorized_Worker.Input_Objects
{
    public class Add_Authorized_Worker_Data
    {
        public long client_id_FK { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string phone_number { get; set; }
        public string email { get; set; }
        public bool contact { get; set; }
        public bool collection { get; set; }
        public string comment { get; set; }
    }
}
