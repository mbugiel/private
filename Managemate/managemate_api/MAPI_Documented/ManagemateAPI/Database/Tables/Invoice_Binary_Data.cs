using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("invoice_binary_data")]
    public class Invoice_Binary_Data
    {
        [Key]
        public long id { get; set; }
        public byte[] invoice_bytes { get; set; }

        public long invoice_printed_data_FKid { get; set; }
        public Invoice_Printed_Data invoice_printed_data_FK { get; set; }
    }
}
