using ManagemateAPI.Management.Shared.Enum;

namespace ManagemateAPI.Management.M_Lease_Protocol.Input_Objects
{
    public class Edit_Lease_Protocol_Data
    {
        public long id { get; set; }
        public Protocol_State state { get; set; }
        public DateTime user_current_timestamp { get; set; }
        public DateTime timestamp { get; set; }
    }
}
