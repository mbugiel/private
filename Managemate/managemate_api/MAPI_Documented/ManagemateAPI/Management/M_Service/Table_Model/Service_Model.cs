using ManagemateAPI.Management.M_Service_Group.Table_Model;

namespace ManagemateAPI.Management.M_Service.Table_Model
{
    public class Service_Model
    {
        public long id { get; set; }
        public string service_number { get; set; }
        public string service_name { get; set; }
        public Service_Group_Model service_group_FK { get; set; }
        public decimal price { get; set; }
        public string comment { get; set; }
    }
}
