using ManagemateAPI.Management.Shared.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("invoice")]
    public class Invoice
    {
        [Key]
        public long id { get; set; }

        public Invoice_Type invoice_type {  get; set; }

        public string full_number { get; set; }
        public int year { get; set; }
        public int month { get; set; }
        public int number { get; set; }


        public long order_FKid { get; set; }
        public Order order_FK { get; set; }

        public long? invoice_printed_data_FKid { get; set; }
        public Invoice_Printed_Data? invoice_printed_data_FK { get; set; }


        public DateTime issue_date { get; set; }
        public DateTime sale_date { get; set; }
        public DateTime payment_date { get; set; }

        public string payment_method { get; set; }

        public decimal net_worth { get; set; }
        public decimal tax_worth { get; set; }
        public decimal gross_worth { get; set; }

        public string gross_worth_in_words { get; set; }
        public byte[] comment { get; set; }


        public List<Invoice_Row> invoice_row_list_FK { get; set; }
    }
}
