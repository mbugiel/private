using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("service")]
    public class Service
    {
        [Key]
        public long id { get; set; }
        public string service_number { get; set; }
        public string service_name { get; set; }

        public Service_Group service_group_FK { get; set; }
        public long service_group_FKid { get; set; }

        public decimal price { get; set; }
        public byte[] comment { get; set; }

        public List<Service_On_Sale_Protocol> service_on_sale_protocol_list_FK { get; set; }
        public List<Service_On_Lease_Protocol> service_on_lease_protocol_list_FK { get; set; }

        public bool deleted { get; set; }
    }
}
