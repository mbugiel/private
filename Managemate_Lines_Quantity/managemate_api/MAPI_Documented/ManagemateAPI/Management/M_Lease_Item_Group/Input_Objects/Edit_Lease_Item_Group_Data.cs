namespace ManagemateAPI.Management.M_Lease_Item_Group.Input_Objects
{
    public class Edit_Lease_Item_Group_Data
    {
        public long group_id { get; set; }
        public string group_name { get; set; }
        public decimal tax_pct { get; set; }
        public decimal rate { get; set; }
    }
}
