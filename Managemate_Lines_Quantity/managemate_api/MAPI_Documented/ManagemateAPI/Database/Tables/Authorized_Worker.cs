using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("authorized_worker")]
    public class Authorized_Worker
    {
        [Key]
        public long id { get; set; }

        public Client client_FK { get; set; }
        public long client_FKid { get; set; }

        public byte[] name { get; set; }
        public byte[] surname { get; set; }
        public byte[] phone_number { get; set; }
        public byte[] email { get; set; }

        public bool contact { get; set; }
        public bool collection { get; set; }

        public byte[] comment { get; set; }

        public bool deleted { get; set; }
    }
}
