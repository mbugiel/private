namespace ManagemateAPI.Management.M_Invoice.Table_Model
{
    public class Lease_Item_Calculation_Model
    {
        public long lease_item_id { get; set; }

        public decimal total_quantity { get; set; }

        public long days_on_construction_site { get; set; }

        public bool overwritten { get; set; }
    }
}
