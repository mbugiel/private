using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("counting_unit")]
    public class Counting_Unit
    {
        [Key]
        public long id { get; set; }
        public string unit { get; set; }
    }
}
