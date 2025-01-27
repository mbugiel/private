namespace ManagemateAPI.Management.M_Service_On_Sale_Protocol.Input_Objects
{
    public class Add_Service_On_Sale_Protocol_Data
    {
        public long service_FK { get; set; }
        public long sale_protocol_FK { get; set; }
        public decimal net_worth { get; set; }
        public string comment { get; set; }
    }
}
