namespace ManagemateAPI.Management.M_Service.Input_Objects
{
    public class Add_Service_Data
    {
        public string service_number { get; set; }
        public string service_name { get; set; }
        public long service_group_FK { get; set; }
        public decimal price { get; set; }
        public string comment { get; set; }
    }
}
