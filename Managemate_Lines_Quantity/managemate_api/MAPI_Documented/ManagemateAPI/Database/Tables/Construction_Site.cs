using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("construction_site")]
    public class Construction_Site
    {
        [Key]
        public long id { get; set; }

        public string number { get; set; }
        public byte[] construction_site_name { get; set; }
        public byte[] address { get; set; }

        public byte[] comment { get; set; }

        public List<Order> order_list_FK { get; set; }

        public bool deleted { get; set; }
    }
}
