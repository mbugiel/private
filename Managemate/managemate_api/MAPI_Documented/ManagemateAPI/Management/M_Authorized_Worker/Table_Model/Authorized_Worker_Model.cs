using ManagemateAPI.Management.M_Client.Table_Model;

namespace ManagemateAPI.Management.M_Authorized_Worker.Table_Model
{
    public class Authorized_Worker_Model
    {
        public long id { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string phone_number { get; set; }
        public string email { get; set; }
        public bool contact { get; set; }
        public bool collection { get; set; }
        public string comment { get; set; }
    }
}
