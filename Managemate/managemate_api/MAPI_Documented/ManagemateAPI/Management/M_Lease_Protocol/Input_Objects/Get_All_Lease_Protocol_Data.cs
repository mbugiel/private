using ManagemateAPI.Management.Shared.Enum;

namespace ManagemateAPI.Management.M_Lease_Protocol.Input_Objects
{
    public class Get_All_Lease_Protocol_Data
    {
        public long order_id { get; set; }
        public Lease_Protocol_Type type { get; set; }
        public bool get_offer_list { get; set; }
    }
}
