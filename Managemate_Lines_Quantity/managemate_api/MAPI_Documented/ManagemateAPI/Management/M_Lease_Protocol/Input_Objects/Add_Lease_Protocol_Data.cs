using ManagemateAPI.Management.Shared.Enum;

namespace ManagemateAPI.Management.M_Lease_Protocol.Input_Objects
{
    public class Add_Lease_Protocol_Data
    {
        public DateTime user_current_timestamp { get; set; }
        public Lease_Protocol_Type type { get; set; }
        public long order_FK { get; set; }
    }
}
