namespace ManagemateAPI.Management.M_Lease_To_Sale_Protocol.Input_Objects
{
    public class Create_Lease_To_Sale_Protocol_Data
    {
        public long return_lease_protocol_id { get; set; }
        public bool discount_is_in_pct { get; set; }
        public decimal discount_value { get; set; }
    }
}
