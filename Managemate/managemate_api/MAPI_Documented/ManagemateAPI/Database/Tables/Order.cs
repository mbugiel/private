using ManagemateAPI.Management.Shared.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("order")]
    public class Order
    {
        [Key]
        public long id { get; set; }
        public byte[] order_name { get; set; }
        public string order_number { get; set; }

        public Client client_FK { get; set; }
        public long client_FKid { get; set; }

        public Construction_Site construction_site_FK { get; set; }
        public long construction_site_FKid { get; set; }

        public Order_State state { get; set; }
        public DateTime timestamp { get; set; }

        public byte[] comment { get; set; }

        public List<Lease_Protocol> lease_protocol_list_FK { get; set; }
        public List<Sale_Protocol> sale_protocol_list_FK { get; set; }
        public List<Lease_Item_Out_Of_Storage> lease_item_out_of_storage_list_FK { get; set; }
        public List<Lease_To_Sale_Protocol> lease_to_sale_protocol_list_FK { get; set; }


        public string default_payment_method { get; set; }
        public int default_payment_date_offset { get; set; }
        public decimal default_discount { get; set; }

        public List<Invoice> invoice_list_FK { get; set; }

        public bool use_static_rate { get; set; }
        public decimal static_rate { get; set; }

        public bool deleted { get; set; }
    }
}
