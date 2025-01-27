using ManagemateAPI.Database.Context;
using ManagemateAPI.Database.Tables;
using ManagemateAPI.Encryption;
using ManagemateAPI.Management.M_Company_Logo.Input_Objects;
using ManagemateAPI.Management.M_Company_Logo.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.Shared.Static;

namespace ManagemateAPI.Management.M_Company_Logo.Manager
{

    /*
     * This is the Company_Logo_Manager with methods dedicated to the company_logo table.
     * 
     * It contains methods to:
     * add records,
     * edit records,
     * get record
     */
    public class Company_Logo_Manager
    {

        private DB_Context _context;
        private readonly IConfiguration _configuration;


        public Company_Logo_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool Valid_File_Extension(string extension)
        {
            if(extension != null)
            {
                if (extension.Contains("jpg") || extension.Contains("jpeg") || extension.Contains("png"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                throw new Exception("14");
            }
        }



        /* 
         * Add_Company_Logo method
         * This method is used to add new records to the company_logo table.
         * 
         * It accepts Add_Company_Logo_Data object as input.
         * It then adds new record with values based on the data given in the input object.
         */
        public async Task<string> Add_Company_Logo(Add_Company_Logo_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var company_logo = _context.Company_Logo.FirstOrDefault();

                if (company_logo == null)
                {

                    if (input_obj.company_logo != null)
                    {

                        if (!Valid_File_Extension(input_obj.company_logo.ContentType))
                        {
                            throw new Exception("31");// _31_INVALID_FILE_FORMAT_ERROR
                        }

                        if (input_obj.company_logo.Length > 6 * 1024 * 1024)
                        {
                            throw new Exception("29"); // File is to large (currently max is 6MB )
                        }


                        byte[] uploaded_logo;

                        using (var stream = new MemoryStream())
                        {
                            await input_obj.company_logo.CopyToAsync(stream);
                            uploaded_logo = stream.ToArray();
                        }


                        Company_Logo new_company_logo = new Company_Logo
                        {
                            company_logo = await Crypto.EncryptByte(session, uploaded_logo),
                            file_type = input_obj.company_logo.ContentType
                        };

                        _context.Company_Logo.Add(new_company_logo);
                        _context.SaveChanges();

                        return Info.SUCCESSFULLY_ADDED;

                    }
                    else
                    {
                        throw new Exception("19");// objects not found
                    }

                }
                else
                {
                    throw new Exception("23"); //_23_COMPANY_EXISTS_ERROR
                }


            }

        }

        /* 
         * Edit_Company_Logo method
         * This method is used to edit a record in the company_logo table.
         * 
         * It accepts Edit_Company_Logo_Data object as input.
         * It then changes values of a record with those given in the input object only if its ID matches the one in the input object.
         */
        public async Task<string> Edit_Company_Logo(Edit_Company_Logo_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var editing_data = _context.Company_Logo.FirstOrDefault();


                if (editing_data != null)
                {

                    if (input_obj.company_logo != null)
                    {
                        if (!Valid_File_Extension(input_obj.company_logo.ContentType))
                        {
                            throw new Exception("31");// _31_INVALID_FILE_FORMAT_ERROR
                        }

                        if (input_obj.company_logo.Length > 6 * 1024 * 1024)
                        {
                            throw new Exception("29"); // File is to large (currently max is 6MB )
                        }


                        byte[] uploaded_logo;

                        using (var stream = new MemoryStream())
                        {
                            await input_obj.company_logo.CopyToAsync(stream);
                            uploaded_logo = stream.ToArray();
                        }


                        editing_data.company_logo = await Crypto.EncryptByte(session, uploaded_logo);
                        editing_data.file_type = input_obj.company_logo.ContentType;

                    }
                    else
                    {
                        editing_data.company_logo = null;
                    }

                    _context.SaveChanges();

                    return Info.SUCCESSFULLY_CHANGED;

                }
                else
                {
                    throw new Exception("19");// objects not found
                }


            }
        }



        /*
         * Get_Company_Logo method
         * This method gets a record from the company_logo table by its ID and returns it.
         * 
         * It accepts Get_Company_Logo_Data object as input.
         * Then it gets a records that has the same ID as the ID given in the input object
         */
        public async Task<Company_Logo_Model> Get_Company_Logo(Session_Data session)
        {
            if (session == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var company_logo = _context.Company_Logo.FirstOrDefault();

                if (company_logo == null)
                {
                    throw new Exception("19");// company data not found
                }
                else
                {
                    if (company_logo.company_logo != null && company_logo.file_type != null)
                    {
                        var logo = await Crypto.DecryptByte(session, company_logo.company_logo);
                        if(logo == null)
                        {
                            throw new Exception("3");//decryption error
                        }

                        return new Company_Logo_Model { company_logo = logo, file_type = company_logo.file_type };
                    }
                    else
                    {
                        throw new Exception("30"); //_30_EMPTY_LOGO_ERROR
                    }

                }


            }
        }



    }
}
