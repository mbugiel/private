using ManagemateAPI.Management.Shared.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("sale_protocol")]
    public class Sale_Protocol
    {
        [Key]
        public long id { get; set; }
        public string prefix { get; set; }
        public int number { get; set; }
        public int year { get; set; }
        public string full_number { get; set; }

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
        public List<Sale_Item_On_Protocol> sale_item_on_protocol_list_FK { get; set; }
        public List<Service_On_Sale_Protocol> service_on_sale_protocol_list_FK { get; set; }

        public long? sale_protocol_printed_data_FKid { get; set; }
        public Sale_Protocol_Printed_Data? sale_protocol_printed_data_FK { get; set; }

        public bool deleted { get; set; }
    }
}
