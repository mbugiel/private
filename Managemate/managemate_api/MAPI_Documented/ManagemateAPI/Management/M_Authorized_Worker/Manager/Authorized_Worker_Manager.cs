using ManagemateAPI.Database.Context;
using ManagemateAPI.Database.Tables;
using ManagemateAPI.Encryption;
using ManagemateAPI.Encryption.Input_Objects;
using ManagemateAPI.Management.M_Authorized_Worker.Input_Objects;
using ManagemateAPI.Management.M_Authorized_Worker.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.Shared.Static;
using Microsoft.EntityFrameworkCore;

/*
 * This is the Authorized_Worker_Manager with methods dedicated to the Authorized_Worker table.
 * 
 * It contains methods to:
 * add records,
 * edit records,
 * delete records,
 * get record by id,
 * get all the records.
 */
namespace ManagemateAPI.Management.M_Authorized_Worker.Manager
{
    public class Authorized_Worker_Manager
    {
        private DB_Context _context;
        private readonly IConfiguration _configuration;

        public Authorized_Worker_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        /* 
         * Add_Authorized_Worker method
         * This method is used to add new records to the Authorized_Worker table.
         * 
         * It accepts Add_Authorized_Worker_Data object as input.
         * It then adds new record with values based on the data given in the input object.
         */
        public async Task<string> Add_Authorized_Worker(Add_Authorized_Worker_Data input_obj, Session_Data session)
        {
            //Checking if object is empty
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var client = _context.Client.Where(c => 
                    c.id.Equals(input_obj.client_id_FK) && 
                    c.deleted.Equals(false)
                ).FirstOrDefault();

                if (client == null)
                {
                    //Throws an error if the client ID given doesn't correspond to any in the Client table.
                    throw new Exception("19");//_19_OBJECT_NOT_FOUND
                }
                else
                {
                    List<Decrypted_Object> decrypted_fields = [
                        new Decrypted_Object { id = 1, decryptedValue = input_obj.name },
                        new Decrypted_Object { id = 2, decryptedValue = input_obj.surname },
                        new Decrypted_Object { id = 3, decryptedValue = input_obj.phone_number },
                        new Decrypted_Object { id = 4, decryptedValue = input_obj.email },
                        new Decrypted_Object { id = 5, decryptedValue = input_obj.comment }
                    ];

                    var encrypted_fields = await Crypto.EncryptList(session, decrypted_fields);

                    if(encrypted_fields == null || encrypted_fields.Count != decrypted_fields.Count)
                    {
                        throw new Exception("2");//encryption error
                    }

                    //Creating new Authorized_Worker object with data given in the input object.
                    Authorized_Worker new_record = new Authorized_Worker
                    {
                        client_FK = client,                        
                        contact = input_obj.contact,
                        collection = input_obj.collection,
                        deleted = false
                    };

                    foreach(var field in encrypted_fields)
                    {
                        if(field == null)
                        {
                            throw new Exception("2");//encryption error
                        }

                        switch (field.id)
                        {
                            case 1:
                                new_record.name = field.encryptedValue;
                                break;
                            case 2:
                                new_record.surname = field.encryptedValue;
                                break;
                            case 3:
                                new_record.phone_number = field.encryptedValue;
                                break;
                            case 4:
                                new_record.email = field.encryptedValue;
                                break;
                            case 5:
                                new_record.comment = field.encryptedValue;
                                break;
                            default:
                                throw new Exception("2");
                        }

                    }

                    var auth_worker_exists = _context.Authorized_Worker.Where(a => 
                        a.client_FKid.Equals(client.id) &&
                        a.phone_number.SequenceEqual(new_record.phone_number) &&
                        a.email.SequenceEqual(new_record.email) &&
                        a.deleted.Equals(false)
                    ).FirstOrDefault();
                    if(auth_worker_exists != null)
                    {
                        throw new Exception("18");//duplicate
                    }

                    //New object is added to the Authorized_Worker table as a new record.
                    _context.Authorized_Worker.Add(new_record);
                    _context.SaveChanges();

                    return Info.SUCCESSFULLY_ADDED;
                }


            }
        }

        /* 
         * Edit_Authorized_Worker method
         * This method is used to edit a record in the Authorized_Worker table.
         * 
         * It accepts Edit_Authorized_Worker_Data object as input.
         * It then changes values of a record with those given in the input object only if its ID matches the one in the input object.
         */
        public async Task<string> Edit_Authorized_Worker(Edit_Authorized_Worker_Data input_obj, Session_Data session)
        {
            //Checking if input object is empty.
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                //Creating new DB_Context object and ensuring that the database if created.
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                //Checking if the edited record exists
                var edited_record = _context.Authorized_Worker.Where(a => 
                    a.id.Equals(input_obj.id) &&
                    a.deleted.Equals(false)
                ).FirstOrDefault();
                if(edited_record == null)
                {
                    throw new Exception("19");//_19_OBJECT_NOT_FOUND
                }

                List<Decrypted_Object> decrypted_fields = [
                    new Decrypted_Object { id = 1, decryptedValue = input_obj.name },
                    new Decrypted_Object { id = 2, decryptedValue = input_obj.surname },
                    new Decrypted_Object { id = 3, decryptedValue = input_obj.phone_number },
                    new Decrypted_Object { id = 4, decryptedValue = input_obj.email },
                    new Decrypted_Object { id = 5, decryptedValue = input_obj.comment }
                ];

                var encrypted_fields = await Crypto.EncryptList(session, decrypted_fields);
                if (encrypted_fields == null || encrypted_fields.Count != decrypted_fields.Count)
                {
                    throw new Exception("2");//encryption error
                }

                foreach (var field in encrypted_fields)
                {
                    if (field == null)
                    {
                        throw new Exception("2");//encryption error
                    }

                    switch (field.id)
                    {
                        case 1:
                            edited_record.name = field.encryptedValue;
                            break;
                        case 2:
                            edited_record.surname = field.encryptedValue;
                            break;
                        case 3:
                            edited_record.phone_number = field.encryptedValue;
                            break;
                        case 4:
                            edited_record.email = field.encryptedValue;
                            break;
                        case 5:
                            edited_record.comment = field.encryptedValue;
                            break;
                        default:
                            throw new Exception("2");
                    }

                }

                var auth_worker_exists = _context.Authorized_Worker.Where(a =>
                    a.client_FKid.Equals(edited_record.client_FKid) &&
                    a.phone_number.SequenceEqual(edited_record.phone_number) &&
                    a.email.SequenceEqual(edited_record.email) &&
                    !a.id.Equals(edited_record.id) &&
                    a.deleted.Equals(false)
                ).FirstOrDefault();
                if (auth_worker_exists != null)
                {
                    throw new Exception("18");//duplicate
                }

                //If there aren't any duplications it proceds further
                //Saving changes
                _context.SaveChanges();

                return Info.SUCCESSFULLY_CHANGED;
            }
        }

        /*
         * Delete_Authorized_Worker method
         * This method is used to a record from the Authorized_Worker table.
         *  
         * It accepts Delete_Authorized_Worker_Data object as input.
         * Then it deletes a record if its ID matches the one given in the input object.
         */
        public string Delete_Authorized_Worker(Delete_Authorized_Worker_Data input_obj, Session_Data session)
        {
            //Checking if input object is null
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                //Creating new DB_Context object and ensuring that the database if created.
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                //Checking if ID given in the input object corresponds to ID of any record in the Authorized_Worker table.
                var id_exits = _context.Authorized_Worker.Where(a => 
                    a.id.Equals(input_obj.id) &&
                    a.deleted.Equals(false)
                ).FirstOrDefault();
                if (id_exits == null)
                {
                    throw new Exception("19");//not found
                }

                //Deleting the record
                id_exits.deleted = true;
                //Saving changes
                _context.SaveChanges();

                return Info.SUCCESSFULLY_DELETED;
            }
        }

        /*
         * Get_Authorized_Worker_By_ID method
         * This method gets a record from the Authorized_Worker table by its ID and returns it.
         * 
         * It accepts Get_Authorized_Worker_By_ID_Data object as input.
         * Then it gets a records that has the same ID as the ID given in the input object
         */
        public async Task<Authorized_Worker_Model> Get_Authorized_Worker_By_Id(Get_Authorized_Worker_By_Id_Data input_obj, Session_Data session)
        {
            //Checking if input object is null
            if (input_obj == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                //Creating new DB_Context object and ensuring that the database if created.
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                //Getting the object from the database with matching ID
                var selected_record = _context.Authorized_Worker.Where(a => 
                    a.id.Equals(input_obj.id_to_get) &&
                    a.deleted.Equals(false)
                ).Include(a => a.client_FK).FirstOrDefault();

                //Checking if object is null
                if (selected_record == null || selected_record.client_FK == null)
                {
                    throw new Exception("19"); //_19_OBJECT_NOT_FOUND
                }

                //Creating a list with encrypted fields form Authorized_Worker and Client tables
                List<Encrypted_Object> encrypted_fields = [
                    //Encrypted fields from Authorized_Worker table
                    new Encrypted_Object { id = 1, encryptedValue = selected_record.name },
                    new Encrypted_Object { id = 2, encryptedValue = selected_record.surname },
                    new Encrypted_Object { id = 3, encryptedValue = selected_record.phone_number },
                    new Encrypted_Object { id = 4, encryptedValue = selected_record.email },
                    new Encrypted_Object { id = 5, encryptedValue = selected_record.comment }
                ];

                //Decrypting the list
                List<Decrypted_Object> decrypted_fields = await Crypto.DecryptList(session, encrypted_fields);

                if(decrypted_fields == null || decrypted_fields.Count != encrypted_fields.Count)
                {
                    throw new Exception("3");//_3_DECRYPTION_ERROR
                }

                //Creating an object to return
                Authorized_Worker_Model return_obj = new Authorized_Worker_Model
                {
                    //Adding values to unencrypted fields
                    id = selected_record.id,
                    contact = selected_record.contact,
                    collection = selected_record.collection
                };

                //Adding values to encrypted fields from the decrypted_items list.
                foreach (var field in decrypted_fields)
                {
                    if (field == null)
                    {
                        throw new Exception("3");//_3_DECRYPTION_ERROR
                    }

                    switch (field.id)
                    {
                        case 1:
                            return_obj.name = field.decryptedValue; break;
                        case 2:
                            return_obj.surname = field.decryptedValue; break;
                        case 3:
                            return_obj.phone_number = field.decryptedValue; break;
                        case 4:
                            return_obj.email = field.decryptedValue; break;
                        case 5:
                            return_obj.comment = field.decryptedValue; break;
                        default:
                            throw new Exception("3");
                    }

                }

                return return_obj;
            }
        }

        /*
         * Get_All_Authorized_Worker method
         * This method gets all of the records in the Authorized_Worker table assigned to given client_ID and returns them in a list.
         * 
         * It accepts Get_All_Authorized_Worker_Data object as input.
         */
        public async Task<List<Authorized_Worker_Model_List>> Get_All_Authorized_Worker(Get_All_Authorized_Worker_Data input_obj, Session_Data session)
        {
            //Checking if input object is null
            if (input_obj == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                //Creating new DB_Context object and ensuring that the database if created.
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                //Creating a list of Authorized_Worker objects assigned to given Client ID.
                List<Authorized_Worker> records_list = _context.Authorized_Worker.Where(a => 
                    a.client_FKid.Equals(input_obj.client_id) &&
                    a.deleted.Equals(false)
                ).ToList();

                //Creating a list for records with decrypted values
                List<Authorized_Worker_Model_List> return_obj = new List<Authorized_Worker_Model_List>();

                //Checking if list is empty
                if (records_list.Count == 0)
                {
                    return return_obj;
                }

                //Creating Encrypted_Object lists for encrypted fields in the Authorized_Worker table.
                List<Encrypted_Object> name_encrypted = new List<Encrypted_Object>();
                List<Encrypted_Object> surname_encrypted = new List<Encrypted_Object>();
                List<Encrypted_Object> phone_number_encrypted = new List<Encrypted_Object>();
                List<Encrypted_Object> email_encrypted = new List<Encrypted_Object>();
                List<Encrypted_Object> comment_encrypted = new List<Encrypted_Object>();

                //Filling the decrypted_records list with records
                foreach (var item in records_list)
                {
                    //Creating Authorized_Worker_Model_List object and adding values to unencrypted fields
                    return_obj.Add(new Authorized_Worker_Model_List
                    {
                        id = item.id,
                        contact = item.contact,
                        collection = item.collection
                    });

                    //Filling encrypted lists with encrypted values
                    name_encrypted.Add(new Encrypted_Object { id = item.id, encryptedValue = item.name });
                    surname_encrypted.Add(new Encrypted_Object { id = item.id, encryptedValue = item.surname });
                    phone_number_encrypted.Add(new Encrypted_Object { id = item.id, encryptedValue = item.phone_number });
                    email_encrypted.Add(new Encrypted_Object { id = item.id, encryptedValue = item.email });
                    comment_encrypted.Add(new Encrypted_Object { id = item.id, encryptedValue = item.comment });
                }

                //Decrypting encrypted field lists
                List<Decrypted_Object> name_decrypted = await Crypto.DecryptList(session, name_encrypted);
                List<Decrypted_Object> surname_decrypted = await Crypto.DecryptList(session, surname_encrypted);
                List<Decrypted_Object> phone_number_decrypted = await Crypto.DecryptList(session, phone_number_encrypted);
                List<Decrypted_Object> email_decrypted = await Crypto.DecryptList(session, email_encrypted);
                List<Decrypted_Object> comment_decrypted = await Crypto.DecryptList(session, comment_encrypted);

                if(
                    name_decrypted == null ||
                    surname_decrypted == null ||
                    phone_number_decrypted == null ||
                    email_decrypted == null ||
                    comment_decrypted == null
                )
                {
                    throw new Exception("3");
                }

                //Adding the rest of values to the Authorized_Worker_Model_List in the decrypted_records list
                foreach (var item in return_obj)
                {
                    var name = name_decrypted.Where(o => o.id.Equals(item.id)).FirstOrDefault();
                    if (name == null)
                    {
                        throw new Exception("3");
                    }
                    else
                    {
                        item.name = name.decryptedValue;
                    }

                    var surname = surname_decrypted.Where(o => o.id.Equals(item.id)).FirstOrDefault();
                    if (surname == null)
                    {
                        throw new Exception("3");
                    }
                    else
                    {
                        item.surname = surname.decryptedValue;
                    }

                    var phone_number = phone_number_decrypted.Where(o => o.id.Equals(item.id)).FirstOrDefault();
                    if (phone_number == null)
                    {
                        throw new Exception("3");
                    }
                    else
                    {
                        item.phone_number = phone_number.decryptedValue;
                    }

                    var email = email_decrypted.Where(o => o.id.Equals(item.id)).FirstOrDefault();
                    if (email == null)
                    {
                        throw new Exception("3");
                    }
                    else
                    {
                        item.email = email.decryptedValue;
                    }

                    var comment = comment_decrypted.Where(o => o.id.Equals(item.id)).FirstOrDefault();
                    if (comment == null)
                    {
                        throw new Exception("3");
                    }
                    else
                    {
                        item.comment = comment.decryptedValue;
                    }

                }

                return return_obj;
            }
        }


    }
}
