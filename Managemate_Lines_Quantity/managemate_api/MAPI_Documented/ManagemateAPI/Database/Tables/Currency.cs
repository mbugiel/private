using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("currency")]
    public class Currency
    {
        [Key]
        public long id {  get; set; }
        public string currency_symbol { get; set; }
        public string currency_hundreth_symbol { get; set; }
    }
}
