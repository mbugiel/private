namespace ManagemateAPI.Database.Context
{
    public class Database_Creator
    {

        private DB_Context _context;
        private readonly IConfiguration _configuration;


        public Database_Creator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void CreateDataBase(long userid)
        {

            _context = new DB_Context(userid, _configuration);
            _context.Database.EnsureCreated();
            _context.Dispose();

        }

    }
}
