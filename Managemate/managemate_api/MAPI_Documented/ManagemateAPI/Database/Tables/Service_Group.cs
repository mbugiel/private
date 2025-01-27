using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("service_group")]
    public class Service_Group
    {
        [Key]
        public long id { get; set; }
        public string group_name { get; set; }
        public decimal tax_pct { get; set; }
        public List<Service> service_list_FK { get; set; }
    }
}
