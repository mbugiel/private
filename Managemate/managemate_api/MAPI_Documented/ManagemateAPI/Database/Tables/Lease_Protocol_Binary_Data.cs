using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("lease_protocol_binary_data")]
    public class Lease_Protocol_Binary_Data
    {
        [Key]
        public long id { get; set; }
        public byte[] lease_protocol_bytes { get; set; }

        public long lease_protocol_printed_data_FKid { get; set; }
        public Lease_Protocol_Printed_Data lease_protocol_printed_data_FK { get; set; }
    }
}
