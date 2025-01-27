using ManagemateAPI.Management.Shared.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("lease_to_sale_protocol")]
    public class Lease_To_Sale_Protocol
    {
        [Key]
        public long id { get; set; }
        public string prefix { get; set; }
        public int number { get; set; }
        public int year { get; set; }
        public string full_number { get; set; }

        public DateTime timestamp { get; set; }

        public Order order_FK { get; set; }
        public long order_FKid { get; set; }

        public long return_lease_protocol_FKid { get; set; }
        public Lease_Protocol return_lease_protocol_FK { get; set; }

        public List<Lease_Item_On_Lease_To_Sale_Protocol> lease_item_on_lease_to_sale_protocol_list_FK { get; set; }

        public long? lease_to_sale_protocol_printed_data_FKid { get; set; }
        public Lease_To_Sale_Protocol_Printed_Data? lease_to_sale_protocol_printed_data_FK { get; set; }

    }
}
