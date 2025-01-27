using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("storage")]
    public class Storage
    {
        [Key]
        public long id { get; set; }
        public string number { get; set; }
        public string name { get; set; }
        public byte[] description { get; set; }

        public List<Lease_Item_In_Storage> lease_item_in_storage_list_FK { get; set; }
        public List<Sale_Item_In_Storage> sale_item_in_storage_list_FK { get; set; }

        public bool deleted { get; set; }
    }
}
