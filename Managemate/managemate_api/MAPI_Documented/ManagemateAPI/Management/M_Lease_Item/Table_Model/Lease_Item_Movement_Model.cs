using ManagemateAPI.Management.Shared.Enum;

namespace ManagemateAPI.Management.M_Lease_Item.Table_Model
{
    public class Lease_Item_Movement_Model
    {
        public long protocol_id { get; set; }
        public string protocol_number { get; set; }
        public Lease_Protocol_Type protocol_type { get; set; }

        public long storage_id { get; set; }
        public string storage_number { get; set; }
        public string storage_name { get; set; }

        public decimal total_quantity { get; set; }

        public long order_id { get; set; }
        public string order_name { get; set; }

        public DateTime date { get; set; }
    }
}
