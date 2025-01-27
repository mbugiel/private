using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("company_info")]
    public class Company_Info
    {
        [Key]
        public long id { get; set; }

        public byte[] name { get; set; }

        public byte[] surname { get; set; }

        public byte[] company_name { get; set; }

        public byte[] nip { get; set; }

        public byte[] phone_number { get; set; }

        public byte[] email { get; set; }

        public byte[] address { get; set; }

        public byte[] bank_name { get; set; }

        public byte[] bank_number { get; set; }

        public byte[] web_page { get; set; }
    }
}
