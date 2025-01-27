namespace ManagemateAPI.Management.M_Service_On_Lease_Protocol.Table_Model
{
    public class Service_On_Lease_Protocol_Model
    {
        public long id { get; set; }
        public long service_id { get; set; }
        public string service_number { get; set; }
        public string service_name { get; set; }
        public decimal net_worth { get; set; }
        public string comment { get; set; }
    }
}
