using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("sale_protocol_printed_data")]
    public class Sale_Protocol_Printed_Data
    {
        [Key]
        public long id { get; set; }
        public string sale_protocol_file_name { get; set; }
        public DateTime print_timestamp { get; set; }

        public long sale_protocol_binary_data_FKid { get; set; }
        public Sale_Protocol_Binary_Data sale_protocol_binary_data_FK { get; set; }

        public long sale_protocol_FKid { get; set; }
        public Sale_Protocol sale_protocol_FK { get; set; }
    }
}
