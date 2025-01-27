using ManagemateAPI.Management.Shared.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("lease_protocol")]
    public class Lease_Protocol
    {
        [Key]
        public long id { get; set; }
        public string prefix { get; set; }
        public int number { get; set; }
        public int year { get; set; }
        public string full_number { get; set; }

        public Lease_Protocol_Type type { get; set; }
        public Protocol_State state { get; set; }
        public DateTime timestamp { get; set; }

        public Order order_FK { get; set; }
        public long order_FKid { get; set; }

        public byte[] element { get; set; }
        public byte[] transport { get; set; }
        public byte[] comment { get; set; }

        public decimal total_weight_kg { get; set; }
        public decimal total_area_m2 { get; set; }
        public decimal total_worth { get; set; }
        public List<Lease_Item_On_Protocol> lease_item_on_protocol_list_FK { get; set; }
        public List<Service_On_Lease_Protocol> service_on_lease_protocol_list_FK { get; set; }

        public long? lease_protocol_printed_data_FKid { get; set; }
        public Lease_Protocol_Printed_Data? lease_protocol_printed_data_FK { get; set; }

        public long? lease_to_sale_protocol_FKid { get; set; }
        public Lease_To_Sale_Protocol? lease_to_sale_protocol_FK { get; set; }

        public bool deleted { get; set; }
    }
}
