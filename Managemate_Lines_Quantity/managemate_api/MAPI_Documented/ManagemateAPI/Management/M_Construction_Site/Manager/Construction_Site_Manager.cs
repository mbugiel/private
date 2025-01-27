using ManagemateAPI.Database.Context;
using ManagemateAPI.Database.Tables;
using ManagemateAPI.Encryption;
using ManagemateAPI.Encryption.Input_Objects;
using ManagemateAPI.Management.M_Construction_Site.Input_Objects;
using ManagemateAPI.Management.M_Construction_Site.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.Shared.Static;
using Microsoft.EntityFrameworkCore;

/*
 * This is the Construction_Site_Manager with methods dedicated to the Construction_Site table.
 * 
 * It contains methods to:
 * add records,
 * edit records,
 * delete records,
 * get record by id,
 * get all the records.
 */
namespace ManagemateAPI.Management.M_Construction_Site.Manager
{
    public class Construction_Site_Manager
    {
        private DB_Context _context;
        private readonly IConfiguration _configuration;

        public Construction_Site_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /* 
         * Add_Construction_Site method
         * This method is used to add new records to the Construction_Site table.
         * 
         * It accepts Add_Construction_Site_Data object as input.
         * It then adds new record with values based on the data given in the input object.
         */
        public async Task<string> Add_Construction_Site(Add_Construction_Site_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var con_site_exists = _context.Construction_Site.Where(c => c.number.Equals(input_obj.number) && c.deleted.Equals(false)).FirstOrDefault();

                if (con_site_exists != null)
                {
                    throw new Exception("18"); // Already exists
                }

                List<Decrypted_Object> decrypted_fields = [
                    new Decrypted_Object { id = 1, decryptedValue = input_obj.construction_site_name },
                    new Decrypted_Object { id = 2, decryptedValue = input_obj.address },
                    new Decrypted_Object { id = 3, decryptedValue = input_obj.comment }
                ];

                var encrypted_fields = await Crypto.EncryptList(session, decrypted_fields);

                if(encrypted_fields == null || encrypted_fields.Count != decrypted_fields.Count)
                {
                    throw new Exception("2");//error while encrypting data
                }

                Construction_Site new_record = new Construction_Site
                {
                    number = input_obj.number,
                    deleted = false
                };

                foreach (var field in encrypted_fields)
                {
                    if(field == null)
                    {
                        throw new Exception("2");//error while encrypting data
                    }

                    switch (field.id)
                    {
                        case 1:
                            new_record.construction_site_name = field.encryptedValue;
                            break;

                        case 2:
                            new_record.address = field.encryptedValue;
                            break;

                        case 3:
                            new_record.comment = field.encryptedValue;
                            break;

                        default:
                            throw new Exception("2");//error while encrypting data

                    }

                }

                _context.Construction_Site.Add(new_record);
                _context.SaveChanges();

                return Info.SUCCESSFULLY_ADDED;

            }

        }

        /* 
         * Edit_Construction_Site method
         * This method is used to edit a record in the Construction_Site table.
         * 
         * It accepts Edit_Construction_Site_Data object as input.
         * It then changes values of a record with those given in the input object only if its ID matches the one in the input object.
         */
        public async Task<string> Edit_Construction_Site(Edit_Construction_Site_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var edited_record = _context.Construction_Site.Where(h => h.id.Equals(input_obj.id) && h.deleted.Equals(false)).FirstOrDefault();
                if(edited_record == null)
                {
                    throw new Exception("19"); // not found
                }                

                var con_site_exists = _context.Construction_Site.Where(c => c.number.Equals(input_obj.number) && c.id != edited_record.id).FirstOrDefault();
                if (con_site_exists != null)
                {
                    throw new Exception("18"); // Already exists
                }
                
                edited_record.number = input_obj.number;

                List<Decrypted_Object> decrypted_fields = [
                    new Decrypted_Object { id = 1, decryptedValue = input_obj.construction_site_name },
                    new Decrypted_Object { id = 2, decryptedValue = input_obj.address },
                    new Decrypted_Object { id = 3, decryptedValue = input_obj.comment }
                ];

                var encrypted_fields = await Crypto.EncryptList(session, decrypted_fields);

                if (encrypted_fields == null || encrypted_fields.Count != decrypted_fields.Count)
                {
                    throw new Exception("2");//error while encrypting data
                }

                foreach (var field in encrypted_fields)
                {
                    if (field == null)
                    {
                        throw new Exception("2");//error while encrypting data
                    }

                    switch (field.id)
                    {
                        case 1:
                            edited_record.construction_site_name = field.encryptedValue;
                            break;

                        case 2:
                            edited_record.address = field.encryptedValue;
                            break;

                        case 3:
                            edited_record.comment = field.encryptedValue;
                            break;

                        default:
                            throw new Exception("2");//error while encrypting data

                    }

                }

                _context.SaveChanges();

                return Info.SUCCESSFULLY_CHANGED;

            }
        }

        /*
         * Delete_Construction_Site method
         * This method is used to a record from the Construction_Site table.
         *  
         * It accepts Delete_Construction_Site_Data object as input.
         * Then it deletes a record if its ID matches the one given in the input object.
         */
        public string Delete_Construction_Site(Delete_Construction_Site_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var id_exits = _context.Construction_Site.Where(i => 
                    i.id.Equals(input_obj.id) && 
                    i.deleted.Equals(false)
                )
                .Include(i => i.order_list_FK)
                .FirstOrDefault();

                if (id_exits == null || id_exits.order_list_FK == null)
                {
                    throw new Exception("19");// not found
                }

                if(id_exits.order_list_FK.Count > 0)
                {
                    throw new Exception("38");// construction site is in use
                }
                
                id_exits.deleted = true;

                _context.SaveChanges();

                return Info.SUCCESSFULLY_DELETED;
            }
        }

        /*
         * Get_Construction_Site_By_ID method
         * This method gets a record from the Construction_Site table by its ID and returns it.
         * 
         * It accepts Get_Construction_Site_By_ID_Data object as input.
         * Then it gets a records that has the same ID as the ID given in the input object
         */
        public async Task<Construction_Site_Model> Get_Construction_Site_By_Id(Get_Construction_Site_By_Id input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var selected_record = _context.Construction_Site.Where(r => 
                    r.id.Equals(input_obj.id_to_get) && 
                    r.deleted.Equals(false)
                ).FirstOrDefault();

                if (selected_record == null)
                {
                    throw new Exception("19"); //not found
                }

                List<Encrypted_Object> encrypted_fields = [
                    new Encrypted_Object { id = 1, encryptedValue = selected_record.construction_site_name },
                    new Encrypted_Object { id = 2, encryptedValue = selected_record.address },
                    new Encrypted_Object { id = 3, encryptedValue = selected_record.comment }
                ];

                var decrypted_fields = await Crypto.DecryptList(session, encrypted_fields);

                if(decrypted_fields == null || decrypted_fields.Count != encrypted_fields.Count)
                {
                    throw new Exception("3");//decryption error
                }

                Construction_Site_Model return_obj = new Construction_Site_Model
                {
                    id = selected_record.id,
                    number = selected_record.number
                };

                foreach (var field in decrypted_fields)
                {
                    if (field == null)
                    {
                        throw new Exception("3");//decryption error
                    }

                    switch (field.id)
                    {
                        case 1:
                            return_obj.construction_site_name = field.decryptedValue; break;
                        case 2:
                            return_obj.address = field.decryptedValue; break;
                        case 3:
                            return_obj.comment = field.decryptedValue; break;
                        default:
                            throw new Exception("3");
                    }
                }

                return return_obj;
            }
        }

        /*
         * Get_All_Construction_Site method
         * This method gets all of the records in the Construction_Site table and returns them in a list.
         * 
         * It accepts Get_All_Construction_Site_Data object as input.
         */
        public async Task<List<Construction_Site_Model_List>> Get_All_Construction_Site(Session_Data session)
        {
            if (session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var record_list = _context.Construction_Site.Where(c => c.deleted.Equals(false)).ToList();

                List<Construction_Site_Model_List> return_obj = new List<Construction_Site_Model_List>();

                if(record_list.Count == 0)
                {
                    return return_obj;
                }
                
                List<Encrypted_Object> encrypted_construction_site_name = new List<Encrypted_Object>();
                List<Encrypted_Object> encrypted_address = new List<Encrypted_Object>();
                List<Encrypted_Object> encrypted_comment = new List<Encrypted_Object>();

                foreach (var record in record_list)
                {
                    return_obj.Add(
                        new Construction_Site_Model_List
                        {
                            id = record.id,
                            number = record.number
                        }
                    );

                    encrypted_construction_site_name.Add(new Encrypted_Object { id = record.id, encryptedValue = record.construction_site_name });
                    encrypted_address.Add(new Encrypted_Object { id = record.id, encryptedValue = record.address });
                    encrypted_comment.Add(new Encrypted_Object { id = record.id, encryptedValue = record.comment });
                }

                List<Decrypted_Object> decrypted_construction_site_name = await Crypto.DecryptList(session, encrypted_construction_site_name);
                List<Decrypted_Object> decrypted_address = await Crypto.DecryptList(session, encrypted_address);
                List<Decrypted_Object> decrypted_comment = await Crypto.DecryptList(session, encrypted_comment);

                if(decrypted_construction_site_name == null || decrypted_address == null || decrypted_comment == null)
                {
                    throw new Exception("3");//decryption error
                }

                foreach (var record in return_obj)
                {
                    var construction_site_name = decrypted_construction_site_name.Where(s => s.id.Equals(record.id)).FirstOrDefault();
                    if (construction_site_name == null)
                    {
                        throw new Exception("3");
                    }
                    else
                    {
                        record.construction_site_name = construction_site_name.decryptedValue;
                    }

                    var address = decrypted_address.Where(s => s.id.Equals(record.id)).FirstOrDefault();
                    if (address == null)
                    {
                        throw new Exception("3");
                    }
                    else
                    {
                        record.address = address.decryptedValue;
                    }

                    var comment = decrypted_comment.Where(s => s.id.Equals(record.id)).FirstOrDefault();
                    if (comment == null)
                    {
                        throw new Exception("3");
                    }
                    else
                    {
                        record.comment = comment.decryptedValue;
                    }
                }

                return return_obj;
            }
        }


    }
}
