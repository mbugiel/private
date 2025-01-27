using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("service_on_sale_protocol")]
    public class Service_On_Sale_Protocol
    {
        [Key]
        public long id { get; set; }

        public Sale_Protocol sale_protocol_FK { get; set; }
        public long sale_protocol_FKid { get; set; }

        public Service service_FK { get; set; }
        public long service_FKid { get; set; }

        public decimal net_worth { get; set; }
        public byte[] comment { get; set; }
    }
}
