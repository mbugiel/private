using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("lease_to_sale_protocol_printed_data")]
    public class Lease_To_Sale_Protocol_Printed_Data
    {
        [Key]
        public long id { get; set; }
        public string lease_to_sale_protocol_file_name { get; set; }
        public DateTime print_timestamp { get; set; }

        public long lease_protocol_binary_data_FKid { get; set; }
        public Lease_To_Sale_Protocol_Binary_Data lease_to_sale_protocol_binary_data_FK { get; set; }

        public long lease_to_sale_protocol_FKid { get; set; }
        public Lease_To_Sale_Protocol lease_to_sale_protocol_FK { get; set; }
    }
}
