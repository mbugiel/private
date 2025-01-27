namespace ManagemateAPI.Management.M_Lease_Protocol.Input_Objects
{
    public class Create_Max_Lease_Offer_Data
    {
        public DateTime user_current_timestamp { get; set; }
        public List<long> lease_offer_protocol_list { get; set; }
    }
}
