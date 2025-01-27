using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("sale_item_group")]
    public class Sale_Item_Group
    {
        [Key]
        public long id { get; set; }
        public string group_name { get; set; }
        public decimal tax_pct { get; set; }

        public List<Sale_Item> sale_item_in_group_list_FK { get; set; }

        public bool deleted { get; set; }
    }
}
