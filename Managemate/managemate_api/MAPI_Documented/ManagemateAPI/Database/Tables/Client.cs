using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("client")]
    public class Client
    {
        [Key]
        public long id { get; set; }

        public string number { get; set; }
        public byte[] surname { get; set; }
        public byte[] name { get; set; }

        public bool is_private_person { get; set; }
        public byte[] company_name { get; set; }
        public byte[] nip { get; set; }

        public byte[] phone_number { get; set; }
        public byte[] email { get; set; }
        public byte[] address { get; set; }
        public byte[] comment { get; set; }

        public List<Order> order_list_FK { get; set; }

        public bool deleted { get; set; }
    }
}
