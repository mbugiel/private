using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("sale_protocol_binary_data")]
    public class Sale_Protocol_Binary_Data
    {
        [Key]
        public long id { get; set; }
        public byte[] sale_protocol_bytes { get; set; }

        public long sale_protocol_printed_data_FKid { get; set; }
        public Sale_Protocol_Printed_Data sale_protocol_printed_data_FK { get; set; }
    }
}
