using ManagemateAPI.Database.Tables;

namespace ManagemateAPI.Management.M_Lease_Item_On_Protocol.Table_Model
{
    public class Calculation_Lease_Item_On_Protocol_Model
    {
        public long id { get; set; }

        public long lease_item_id { get; set; }

        public decimal count { get; set; }

        public bool overwritten { get; set; }

        public int days_on_construction_site { get; set; }

        public DateTime receipt_date { get; set; }
    }
}
