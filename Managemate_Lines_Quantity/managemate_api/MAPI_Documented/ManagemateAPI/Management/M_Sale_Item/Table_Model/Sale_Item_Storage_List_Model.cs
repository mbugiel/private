using ManagemateAPI.Management.M_Storage.Table_Model;

namespace ManagemateAPI.Management.M_Sale_Item.Table_Model
{
    public class Sale_Item_Storage_List_Model
    {
        public Item_Storage_List_Model storage_FK { get; set; }
        public decimal in_storage_quantity { get; set; }
        public decimal blocked_quantity { get; set; }
    }
}
