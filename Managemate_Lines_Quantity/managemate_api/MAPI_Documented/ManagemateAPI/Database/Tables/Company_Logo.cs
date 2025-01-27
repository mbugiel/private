using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("company_logo")]
    public class Company_Logo
    {
        [Key]
        public long id { get; set; }
        public byte[]? company_logo { get; set; }
        public string? file_type { get; set; }
    }
}
