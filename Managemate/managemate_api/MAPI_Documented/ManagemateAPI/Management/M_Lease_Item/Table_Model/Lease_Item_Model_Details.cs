using ManagemateAPI.Management.M_Counting_Unit.Table_Model;
using ManagemateAPI.Management.M_Lease_Item_Group.Table_Model;

namespace ManagemateAPI.Management.M_Lease_Item.Table_Model
{
    public class Lease_Item_Model_Details
    {
        public long id { get; set; }
        public string catalog_number { get; set; }
        public string product_name { get; set; }
        public Lease_Item_Group_Model lease_group_FK { get; set; }

        public decimal total_quantity { get; set; }
        public decimal in_storage_quantity { get; set; }
        public decimal out_of_storage_quantity { get; set; }
        public decimal blocked_quantity { get; set; }
        public List<Lease_Item_Storage_List_Model> storage_list { get; set; }
        public List<Lease_Item_Construction_Site_List_Model> construction_site_list { get; set; }
        public List<Lease_Item_Movement_Model> movement_list { get; set; }

        public decimal weight_kg { get; set; }
        public decimal size_cm_x { get; set; }
        public decimal size_cm_y { get; set; }
        public decimal area_m2 { get; set; }

        public decimal price { get; set; }
        public Counting_Unit_Model counting_unit_FK { get; set; }
        public string comment { get; set; }
    }
}