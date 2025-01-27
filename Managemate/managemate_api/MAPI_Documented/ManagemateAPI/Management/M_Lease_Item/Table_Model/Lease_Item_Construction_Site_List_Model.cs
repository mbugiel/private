using ManagemateAPI.Management.M_Construction_Site.Table_Model;

namespace ManagemateAPI.Management.M_Lease_Item.Table_Model
{
    public class Lease_Item_Construction_Site_List_Model
    {
        public long order_id { get; set; }
        public string order_name { get; set; }
        public Construction_Site_Item_Quantity_Model construction_site_FK { get; set; }
        public decimal total_quantity { get; set; }
        public List<From_Storage_Model> from_storage_list { get; set; }
    }
}
