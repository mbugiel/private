using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("sale_item_in_storage")]
    public class Sale_Item_In_Storage
    {
        [Key]
        public long id { get; set; }

        public Storage storage_FK { get; set; }
        public long storage_FKid { get; set; }

        public Sale_Item sale_item_FK { get; set; }
        public long sale_item_FKid { get; set; }

        public List<Sale_Item_On_Protocol> sale_item_on_protocol_list_FK { get; set; }
        public List<Sale_Item_In_Storage_Stock_History> sale_item_in_storage_stock_history_FK { get; set; }
    }
}
