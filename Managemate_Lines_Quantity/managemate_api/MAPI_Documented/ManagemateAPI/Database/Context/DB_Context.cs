using ManagemateAPI.Database.Tables;
using ManagemateAPI.Encryption;
using ManagemateAPI.Management.Shared.Static;
using Microsoft.EntityFrameworkCore;

namespace ManagemateAPI.Database.Context
{
    public class DB_Context : DbContext
    {
        private long user_id;
        private readonly IConfiguration _configuration;


        public DB_Context(long userid, IConfiguration configuration) 
        {
            user_id = userid;
            _configuration = configuration;
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(

                @"Server=" + _configuration.GetValue<string>("Database:Server") +
                ";Port=" + _configuration.GetValue<string>("Database:Port") + 
                ";Database=" + _configuration.GetValue<string>("Database:DB") + user_id + 
                ";User id=" + _configuration.GetValue<string>("Database:User") + 
                ";Password=" + Crypto.GetPasswd() + ";"
                
                );


        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);


/*            List<Item_Trading_Type> default_trading_types = new List<Item_Trading_Type>();
            int start_index = 1;
            foreach(var trading_type in System_Path.TRADING_TYPES)
            {
                default_trading_types.Add(new Item_Trading_Type { id = start_index++, trading_type_pl = trading_type[0], trading_type_en=trading_type[1] });
            }

            modelBuilder.Entity<Item_Trading_Type>().HasData(default_trading_types);*/


            List<Currency> default_currency_list = new List<Currency>();
            int start_index = 1;
            foreach (var currency in System_Path.CURRENCY_LIST)
            {
                default_currency_list.Add(new Currency { id = start_index++, currency_symbol = currency[0], currency_hundreth_symbol = currency[1] });
            }

            modelBuilder.Entity<Currency>().HasData(default_currency_list);

        }

        public DbSet<Company_Info> Company_Info { get; set; }
        public DbSet<Company_Logo> Company_Logo { get; set; }
        public DbSet<Company_Invoice_Settings> Company_Invoice_Settings { get; set; }

        public DbSet<Order> Order { get; set; }
        public DbSet<Client> Client { get; set; }
        public DbSet<Authorized_Worker> Authorized_Worker { get; set; }
        public DbSet<Construction_Site> Construction_Site { get; set; }

        public DbSet<Lease_Protocol> Lease_Protocol { get; set; }
        public DbSet<Lease_Item_On_Protocol> Lease_Item_On_Protocol { get; set; }
        public DbSet<Service_On_Lease_Protocol> Service_On_Lease_Protocol { get; set; }
        public DbSet<Lease_Protocol_Printed_Data> Lease_Protocol_Printed_Data { get; set; }
        public DbSet<Lease_Protocol_Binary_Data> Lease_Protocol_Binary_Data { get; set; }
        

        
        public DbSet<Sale_Protocol> Sale_Protocol { get; set; }
        public DbSet<Sale_Item_On_Protocol> Sale_Item_On_Protocol { get; set; }
        public DbSet<Service_On_Sale_Protocol> Service_On_Sale_Protocol { get; set; }
        public DbSet<Sale_Protocol_Printed_Data> Sale_Protocol_Printed_Data { get; set; }
        public DbSet<Sale_Protocol_Binary_Data> Sale_Protocol_Binary_Data { get; set; }



        public DbSet<Lease_To_Sale_Protocol> Lease_To_Sale_Protocol { get; set; }
        public DbSet<Lease_Item_On_Lease_To_Sale_Protocol> Lease_Item_On_Lease_To_Sale_Protocol { get; set; }
        public DbSet<Lease_To_Sale_Protocol_Printed_Data> Lease_To_Sale_Protocol_Printed_Data { get; set; }
        public DbSet<Lease_To_Sale_Protocol_Binary_Data> Lease_To_Sale_Protocol_Binary_Data { get; set; }



        public DbSet<Counting_Unit> Counting_Unit { get; set;}

        public DbSet<Lease_Item> Lease_Item { get; set; }
        public DbSet<Lease_Item_Group> Lease_Item_Group { get; set; }
        public DbSet<Lease_Item_Stock_History> Lease_Item_Stock_History { get; set; }

        public DbSet<Sale_Item> Sale_Item { get; set; }
        public DbSet<Sale_Item_Group> Sale_Item_Group { get; set; }
        public DbSet<Sale_Item_Stock_History> Sale_Item_Stock_History { get; set; }


        public DbSet<Storage> Storage { get; set; }
        public DbSet<Lease_Item_In_Storage> Lease_Item_In_Storage { get; set; }
        public DbSet<Lease_Item_In_Storage_Stock_History> Lease_Item_In_Storage_Stock_History { get; set; }

        public DbSet<Lease_Item_Out_Of_Storage> Lease_Item_Out_Of_Storage { get; set; }
        public DbSet<Lease_Item_Out_Of_Storage_History> Lease_Item_Out_Of_Storage_History { get; set; }

        public DbSet<Sale_Item_In_Storage> Sale_Item_In_Storage { get; set; }
        public DbSet<Sale_Item_In_Storage_Stock_History> Sale_Item_In_Storage_Stock_History { get; set; }



        public DbSet<Service> Service { get; set; }
        public DbSet<Service_Group> Service_Group { get; set; }
        
        public DbSet<Invoice> Invoice { get; set; }
        public DbSet<Invoice_Row> Invoice_Row { get; set; }
        public DbSet<Invoice_Printed_Data> Invoice_Printed_Data { get; set; }
        public DbSet<Invoice_Binary_Data> Invoice_Binary_Data { get; set; }
        public DbSet<Currency> Currency { get; set; }
    }
}
