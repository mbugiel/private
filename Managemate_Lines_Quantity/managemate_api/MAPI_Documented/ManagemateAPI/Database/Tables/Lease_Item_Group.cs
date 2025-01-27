using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("lease_item_group")]
    public class Lease_Item_Group
    {
        [Key]
        public long id { get; set; }
        public string group_name { get; set; }
        public decimal tax_pct { get; set; }
        public decimal rate { get; set; }

        public List<Lease_Item> lease_item_in_group_list_FK { get; set; }

        public bool deleted { get; set; }
    }
}
