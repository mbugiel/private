using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("invoice_printed_data")]
    public class Invoice_Printed_Data
    {
        [Key]
        public long id { get; set; }

        public DateTime print_timestamp { get; set; }
        public string invoice_file_name { get; set; }

        public long invoice_binary_data_FKid { get; set; }
        public Invoice_Binary_Data invoice_binary_data_FK { get; set; }

        public long invoice_FKid { get; set; }
        public Invoice invoice_FK { get; set; }
    }
}
