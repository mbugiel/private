using ManagemateAPI.Database.Context;
using ManagemateAPI.Database.Tables;
using ManagemateAPI.Encryption;
using ManagemateAPI.Encryption.Input_Objects;
using ManagemateAPI.Management.M_Company.Input_Objects;
using ManagemateAPI.Management.M_Company.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.Shared.Static;

/*
 * This is the Company_Info_Manager with methods dedicated to the company_info table.
 * 
 * It contains methods to:
 * add records,
 * edit records,
 * get record
 */
namespace ManagemateAPI.Management.M_Company.Manager
{
    public class Company_Info_Manager
    {

        private DB_Context _context;
        private readonly IConfiguration _configuration;


        public Company_Info_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /* 
         * Add_Company method
         * This method is used to add new records to the Company table.
         * 
         * It accepts Add_Company_Data object as input.
         * It then adds new record with values based on the data given in the input object.
         */
        public async Task<string> Add_Company_Info(Add_Company_Info_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var company_exist = _context.Company_Info.FirstOrDefault();

                if(company_exist != null)
                {
                    throw new Exception("23"); //_23_COMPANY_EXISTS_ERROR
                }

                List<Decrypted_Object> decrypted_fields =
                [
                    new Decrypted_Object { id = 1, decryptedValue = input_obj.name },
                    new Decrypted_Object { id = 2, decryptedValue = input_obj.surname },
                    new Decrypted_Object { id = 3, decryptedValue = input_obj.company_name },
                    new Decrypted_Object { id = 4, decryptedValue = input_obj.NIP },
                    new Decrypted_Object { id = 5, decryptedValue = input_obj.phone_number },
                    new Decrypted_Object { id = 6, decryptedValue = input_obj.email },
                    new Decrypted_Object { id = 7, decryptedValue = input_obj.address },
                    new Decrypted_Object { id = 8, decryptedValue = input_obj.bank_name },
                    new Decrypted_Object { id = 9, decryptedValue = input_obj.bank_number },
                    new Decrypted_Object { id = 10, decryptedValue = input_obj.web_page }
                ];

                List<Encrypted_Object> encrypted_fields = await Crypto.EncryptList(session, decrypted_fields);

                if(encrypted_fields == null)
                {
                    throw new Exception("2");//error while encrypting data 
                }

                Company_Info new_company = new Company_Info();

                foreach (var field in encrypted_fields)
                {

                    if (field == null)
                    {
                        throw new Exception("2");//error while encrypting data 
                    }
                    else
                    {
                        switch (field.id)
                        {

                            case 1:
                                new_company.name = field.encryptedValue; break;

                            case 2:
                                new_company.surname = field.encryptedValue; break;

                            case 3:
                                new_company.company_name = field.encryptedValue; break;

                            case 4:
                                new_company.nip = field.encryptedValue; break;

                            case 5:
                                new_company.phone_number = field.encryptedValue; break;

                            case 6:
                                new_company.email = field.encryptedValue; break;

                            case 7:
                                new_company.address = field.encryptedValue; break;

                            case 8:
                                new_company.bank_name = field.encryptedValue; break;

                            case 9:
                                new_company.bank_number = field.encryptedValue; break;

                            case 10:
                                new_company.web_page = field.encryptedValue; break;

                            default:
                                throw new Exception("2");//error while encrypting data 
                        }
                    }
                }

                _context.Company_Info.Add(new_company);
                _context.SaveChanges();

                return Info.SUCCESSFULLY_ADDED;

            }

        }

        /* 
         * Edit_Company method
         * This method is used to edit a record in the Company table.
         * 
         * It accepts Edit_Company_Data object as input.
         * It then changes values of a record with those given in the input object only if its ID matches the one in the input object.
         */
        public async Task<string> Edit_Company_Info(Edit_Company_Info_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var editing_data = _context.Company_Info.FirstOrDefault();

                if(editing_data == null)
                {
                    throw new Exception("19");// objects not found
                }


                List<Decrypted_Object> decrypted_fields =
                [

                    new Decrypted_Object { id = 1, decryptedValue = input_obj.name },
                    new Decrypted_Object { id = 2, decryptedValue = input_obj.surname },
                    new Decrypted_Object { id = 3, decryptedValue = input_obj.company_name },
                    new Decrypted_Object { id = 4, decryptedValue = input_obj.NIP },
                    new Decrypted_Object { id = 5, decryptedValue = input_obj.phone_number },
                    new Decrypted_Object { id = 6, decryptedValue = input_obj.email },
                    new Decrypted_Object { id = 7, decryptedValue = input_obj.address },
                    new Decrypted_Object { id = 8, decryptedValue = input_obj.bank_name },
                    new Decrypted_Object { id = 9, decryptedValue = input_obj.bank_number },
                    new Decrypted_Object { id = 10, decryptedValue = input_obj.web_page }

                ];

                List<Encrypted_Object> encrypted_fields = await Crypto.EncryptList(session, decrypted_fields);

                if (encrypted_fields == null)
                {
                    throw new Exception("2");//error while encrypting data 
                }

                foreach (var field in encrypted_fields)
                {

                    if (field == null)
                    {
                        throw new Exception("2");//error while encrypting data 
                    }
                    else
                    {
                        switch (field.id)
                        {

                            case 1:
                                editing_data.name = field.encryptedValue; break;

                            case 2:
                                editing_data.surname = field.encryptedValue; break;

                            case 3:
                                editing_data.company_name = field.encryptedValue; break;

                            case 4:
                                editing_data.nip = field.encryptedValue; break;

                            case 5:
                                editing_data.phone_number = field.encryptedValue; break;

                            case 6:
                                editing_data.email = field.encryptedValue; break;

                            case 7:
                                editing_data.address = field.encryptedValue; break;

                            case 8:
                                editing_data.bank_name = field.encryptedValue; break;

                            case 9:
                                editing_data.bank_number = field.encryptedValue; break;

                            case 10:
                                editing_data.web_page = field.encryptedValue; break;

                            default:
                                throw new Exception("2");//error while encrypting data 

                        }
                    }
                }


                _context.SaveChanges();

                return Info.SUCCESSFULLY_CHANGED;


            }
        }


        
        /*
         * Get_Company method
         * This method gets a record from the Company table by its ID and returns it.
         * 
         * It accepts Get_Company_By_ID_Data object as input.
         * Then it gets a records that has the same ID as the ID given in the input object
         */
        public async Task<Company_Info_Model> Get_Company_Info(Session_Data session)
        {
            if (session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var company = _context.Company_Info.FirstOrDefault();

                if (company == null)
                {
                    throw new Exception("19");// company data not found
                }                

                List<Encrypted_Object> encrypted_fields =
                [
                    new Encrypted_Object { id = 1, encryptedValue = company.name },
                    new Encrypted_Object { id = 2, encryptedValue = company.surname },
                    new Encrypted_Object { id = 3, encryptedValue = company.company_name },
                    new Encrypted_Object { id = 4, encryptedValue = company.nip },
                    new Encrypted_Object { id = 5, encryptedValue = company.phone_number },
                    new Encrypted_Object { id = 6, encryptedValue = company.email },
                    new Encrypted_Object { id = 7, encryptedValue = company.address },
                    new Encrypted_Object { id = 8, encryptedValue = company.bank_name },
                    new Encrypted_Object { id = 9, encryptedValue = company.bank_number },
                    new Encrypted_Object { id = 10, encryptedValue = company.web_page }
                ];

                List<Decrypted_Object> decrypted_fields = await Crypto.DecryptList(session, encrypted_fields);

                if(decrypted_fields == null)
                {
                    throw new Exception("3");//error while decrypting data 
                }

                Company_Info_Model company_model = new Company_Info_Model();

                foreach (var field in decrypted_fields)
                {
                    if (field == null)
                    {
                        throw new Exception("3");//error while decrypting data 
                    }
                    else
                    {

                        switch (field.id)
                        {
                            case 1:
                                company_model.name = field.decryptedValue; break;

                            case 2:
                                company_model.surname = field.decryptedValue; break;

                            case 3:
                                company_model.company_name = field.decryptedValue; break;

                            case 4:
                                company_model.nip = field.decryptedValue; break;

                            case 5:
                                company_model.phone_number = field.decryptedValue; break;

                            case 6:
                                company_model.email = field.decryptedValue; break;

                            case 7:
                                company_model.address = field.decryptedValue; break;

                            case 8:
                                company_model.bank_name = field.decryptedValue; break;

                            case 9:
                                company_model.bank_number = field.decryptedValue; break;

                            case 10:
                                company_model.web_page = field.decryptedValue; break;

                            default:
                                throw new Exception("3");//error while decrypting data 
                        }
                    }
                }

                return company_model;

            }
        }

        
    }
}
