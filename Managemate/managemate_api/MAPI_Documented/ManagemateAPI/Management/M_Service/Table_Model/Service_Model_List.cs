using ManagemateAPI.Database.Tables;

namespace ManagemateAPI.Management.M_Service.Table_Model
{
    public class Service_Model_List
    {
        public long id { get; set; }
        public string service_number { get; set; }
        public string service_name { get; set; }
        public string service_group { get; set; }
        public decimal price { get; set; }
        public string comment { get; set; }
    }
}
