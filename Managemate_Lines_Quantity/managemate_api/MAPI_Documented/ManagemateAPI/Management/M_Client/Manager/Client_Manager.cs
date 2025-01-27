using ManagemateAPI.Database.Context;
using ManagemateAPI.Database.Tables;
using ManagemateAPI.Encryption;
using ManagemateAPI.Encryption.Input_Objects;
using ManagemateAPI.Helper.InputObjects.Client;
using ManagemateAPI.Management.M_Client.Input_Objects;
using ManagemateAPI.Management.M_Client.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.Shared.Static;
using Microsoft.EntityFrameworkCore;

/*
 * This is the Client_Manager with methods dedicated to the Client table.
 * 
 * It contains methods to:
 * add records,
 * edit records,
 * delete records,
 * get record by id,
 * get all the records.
 */

namespace ManagemateAPI.Management.M_Client.Manager
{
    public class Client_Manager
    {
        private DB_Context _context;
        private readonly IConfiguration _configuration;

        public Client_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /* 
         * Add_Client method
         * This method is used to add new records to the Client table.
         * 
         * It accepts Add_Client_Data object as input.
         * It then adds new record with values based on the data given in the input object.
         */
        public async Task<string> Add_Client(Add_Client_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var client_exists = _context.Client.Where(c => c.number.Equals(input_obj.number)).FirstOrDefault();
                if(client_exists != null)
                {
                    throw new Exception("18"); // Already in use
                }

                List<Decrypted_Object> decrypted_fields = 
                [
                    new Decrypted_Object { id=1, decryptedValue = input_obj.surname },
                    new Decrypted_Object { id=2, decryptedValue = input_obj.name },
                    new Decrypted_Object { id=5, decryptedValue = input_obj.phone_number },
                    new Decrypted_Object { id=6, decryptedValue = input_obj.email },
                    new Decrypted_Object { id=7, decryptedValue = input_obj.address },
                    new Decrypted_Object { id=8, decryptedValue = input_obj.comment }
                ];

                if (!input_obj.is_private_person)
                {
                    decrypted_fields.Add(new Decrypted_Object { id = 3, decryptedValue = input_obj.company_name });
                    decrypted_fields.Add(new Decrypted_Object { id = 4, decryptedValue = input_obj.nip });
                }

                var encrypted_fields = await Crypto.EncryptList(session, decrypted_fields);

                if(encrypted_fields == null)
                {
                    throw new Exception("2");//encryption error
                }


                Client new_record = new Client
                {
                    number = input_obj.number,
                    is_private_person = input_obj.is_private_person,
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
                            new_record.surname = field.encryptedValue;
                            break;

                        case 2:
                            new_record.name = field.encryptedValue;
                            break;

                        case 3:
                            new_record.company_name = field.encryptedValue;
                            break;

                        case 4:
                            new_record.nip = field.encryptedValue;
                            break;

                        case 5:
                            new_record.phone_number = field.encryptedValue;
                            break;

                        case 6:
                            new_record.email = field.encryptedValue;
                            break;

                        case 7:
                            new_record.address = field.encryptedValue;
                            break;

                        case 8:
                            new_record.comment = field.encryptedValue;
                            break;

                        default:
                            throw new Exception("2");

                    }

                }

                if (input_obj.is_private_person)
                {
                    new_record.company_name = Array.Empty<byte>();
                    new_record.nip = Array.Empty<byte>();
                }

                _context.Client.Add(new_record);
                _context.SaveChanges();

                return Info.SUCCESSFULLY_ADDED;

            }

        }

        /* 
         * Edit_Client method
         * This method is used to edit a record in the Client table.
         * 
         * It accepts Edit_Client_Data object as input.
         * It then changes values of a record with those given in the input object only if its ID matches the one in the input object.
         */
        public async Task<string> Edit_Client(Edit_Client_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var edited_record = _context.Client.Where(c => 
                    c.id.Equals(input_obj.id) &&
                    c.deleted.Equals(false)
                ).FirstOrDefault();
                if (edited_record == null)
                {
                    throw new Exception("19");
                }

                var client_exists = _context.Client.Where(c => c.number.Equals(input_obj.number)).FirstOrDefault();
                if (client_exists != null)
                {
                    throw new Exception("18"); // Already in use
                }

                List<Decrypted_Object> decrypted_fields =
                [
                    new Decrypted_Object { id=1, decryptedValue = input_obj.surname },
                    new Decrypted_Object { id=2, decryptedValue = input_obj.name },
                    new Decrypted_Object { id=5, decryptedValue = input_obj.phone_number },
                    new Decrypted_Object { id=6, decryptedValue = input_obj.email },
                    new Decrypted_Object { id=7, decryptedValue = input_obj.address },
                    new Decrypted_Object { id=8, decryptedValue = input_obj.comment }
                ];

                if (!edited_record.is_private_person)
                {
                    decrypted_fields.Add(new Decrypted_Object { id = 3, decryptedValue = input_obj.company_name });
                    decrypted_fields.Add(new Decrypted_Object { id = 4, decryptedValue = input_obj.nip });
                }

                var encrypted_fields = await Crypto.EncryptList(session, decrypted_fields);

                if (encrypted_fields == null)
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
                            edited_record.surname = field.encryptedValue;
                            break;

                        case 2:
                            edited_record.name = field.encryptedValue;
                            break;

                        case 3:
                            edited_record.company_name = field.encryptedValue;
                            break;

                        case 4:
                            edited_record.nip = field.encryptedValue;
                            break;

                        case 5:
                            edited_record.phone_number = field.encryptedValue;
                            break;

                        case 6:
                            edited_record.email = field.encryptedValue;
                            break;

                        case 7:
                            edited_record.address = field.encryptedValue;
                            break;

                        case 8:
                            edited_record.comment = field.encryptedValue;
                            break;

                        default:
                            throw new Exception("2");

                    }

                }


                edited_record.number = input_obj.number;

                _context.SaveChanges();

                return Info.SUCCESSFULLY_CHANGED;
            }
        }

        /*
         * Delete_Client method
         * This method is used to a record from the Client table.
         *  
         * It accepts Delete_Client_Data object as input.
         * Then it deletes a record if its ID matches the one given in the input object.
         */
        public string Delete_Client(Delete_Client_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var id_exits = _context.Client.Where(c => 
                    c.id.Equals(input_obj.id) &&
                    c.deleted.Equals(false)
                )
                .Include(c => c.order_list_FK)
                .FirstOrDefault();

                if (id_exits == null || id_exits.order_list_FK == null)
                {
                    throw new Exception("19");//not found
                }

                if(id_exits.order_list_FK.Count > 0)
                {
                    throw new Exception("38");// client is currently in use
                }

                id_exits.deleted = true;

                _context.SaveChanges();

                return Info.SUCCESSFULLY_DELETED;
            }
        }

        /*
         * Get_Client_By_ID method
         * This method gets a record from the Client table by its ID and returns it.
         * 
         * It accepts Get_Client_By_ID_Data object as input.
         * Then it gets a records that has the same ID as the ID given in the input object
         */
        public async Task<Client_Model> Get_Client_by_Id(Get_Client_By_Id_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var selected_record = _context.Client.Where(c => 
                    c.id.Equals(input_obj.id_to_get) && 
                    c.deleted.Equals(false)
                ).FirstOrDefault();

                if (selected_record == null)
                {
                    throw new Exception("19"); //not found
                }

                List<Encrypted_Object> encrypted_items = [
                    new Encrypted_Object { id = 1, encryptedValue = selected_record.surname },
                    new Encrypted_Object { id = 2, encryptedValue = selected_record.name },
                    new Encrypted_Object { id = 5, encryptedValue = selected_record.phone_number },
                    new Encrypted_Object { id = 6, encryptedValue = selected_record.email },
                    new Encrypted_Object { id = 7, encryptedValue = selected_record.address },
                    new Encrypted_Object { id = 8, encryptedValue = selected_record.comment }
                ];

                if (!selected_record.is_private_person)
                {
                    encrypted_items.Add(new Encrypted_Object { id = 3, encryptedValue = selected_record.company_name });
                    encrypted_items.Add(new Encrypted_Object { id = 4, encryptedValue = selected_record.nip });
                }

                List<Decrypted_Object> decrypted_items = await Crypto.DecryptList(session, encrypted_items);

                Client_Model return_obj = new Client_Model
                {
                    id = selected_record.id,
                    number = selected_record.number,
                    is_private_person = selected_record.is_private_person
                };

                foreach (var field in decrypted_items)
                {
                    if (field == null)
                    {
                        throw new Exception("3");
                    }

                    switch (field.id)
                    {

                        case 1:
                            return_obj.surname = field.decryptedValue;
                            break;

                        case 2:
                            return_obj.name = field.decryptedValue;
                            break;

                        case 3:
                            return_obj.company_name = field.decryptedValue;
                            break;

                        case 4:
                            return_obj.nip = field.decryptedValue;
                            break;

                        case 5:
                            return_obj.phone_number = field.decryptedValue;
                            break;

                        case 6:
                            return_obj.email = field.decryptedValue;
                            break;

                        case 7:
                            return_obj.address = field.decryptedValue;
                            break;

                        case 8:
                            return_obj.comment = field.decryptedValue;
                            break;

                        default:
                            throw new Exception("3");

                    }

                }

                if (selected_record.is_private_person)
                {
                    return_obj.company_name = "";
                    return_obj.nip = "";
                }

                return return_obj;
                

            }
        }

        /*
         * Get_All_Client method
         * This method gets all of the records in the Client table and returns them in a list.
         * 
         * It accepts Get_All_Client_Data object as input.
         */
        public async Task<List<Client_Model_List>> Get_All_Client(Session_Data session)
        {
            if (session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                List<Client> records_list = _context.Client.Where(c => c.deleted.Equals(false)).ToList();

                List<Client_Model_List> return_obj = new List<Client_Model_List>();

                if (records_list.Count == 0)
                {
                    return return_obj;
                }

                List<Encrypted_Object> surname_list = new List<Encrypted_Object>();
                List<Encrypted_Object> name_list = new List<Encrypted_Object>();
                List<Encrypted_Object> company_name_list = new List<Encrypted_Object>();
                List<Encrypted_Object> nip_list = new List<Encrypted_Object>();
                List<Encrypted_Object> phone_number_list = new List<Encrypted_Object>();
                List<Encrypted_Object> email_list = new List<Encrypted_Object>();
                List<Encrypted_Object> address_list = new List<Encrypted_Object>();
                List<Encrypted_Object> comment_list = new List<Encrypted_Object>();


                foreach (var client in records_list)
                {
                    return_obj.Add(new Client_Model_List
                    {
                        id = client.id,
                        number = client.number,
                        is_private_person = client.is_private_person
                    });

                    surname_list.Add(new Encrypted_Object { id = client.id, encryptedValue = client.surname });
                    name_list.Add(new Encrypted_Object { id = client.id, encryptedValue = client.name });                    
                    phone_number_list.Add(new Encrypted_Object { id = client.id, encryptedValue = client.phone_number });
                    email_list.Add(new Encrypted_Object { id = client.id, encryptedValue = client.email });
                    address_list.Add(new Encrypted_Object { id = client.id, encryptedValue = client.address });
                    comment_list.Add(new Encrypted_Object { id = client.id, encryptedValue = client.comment });

                    if (!client.is_private_person)
                    {
                        company_name_list.Add(new Encrypted_Object { id = client.id, encryptedValue = client.company_name });
                        nip_list.Add(new Encrypted_Object { id = client.id, encryptedValue = client.nip });
                    }
                }

                List<Decrypted_Object> surname_list_decrypted = await Crypto.DecryptList(session, surname_list);
                List<Decrypted_Object> name_list_decrypted = await Crypto.DecryptList(session, name_list);
                List<Decrypted_Object> company_name_list_decrypted = await Crypto.DecryptList(session, company_name_list);
                List<Decrypted_Object> nip_list_decrypted = await Crypto.DecryptList(session, nip_list);
                List<Decrypted_Object> phone_number_list_decrypted = await Crypto.DecryptList(session, phone_number_list);
                List<Decrypted_Object> email_list_decrypted = await Crypto.DecryptList(session, email_list);
                List<Decrypted_Object> address_list_decrypted = await Crypto.DecryptList(session, address_list);
                List<Decrypted_Object> comment_list_decrypted = await Crypto.DecryptList(session, comment_list);

                foreach (var client in return_obj)
                {
                    var surname = surname_list_decrypted.Where(k => k.id.Equals(client.id)).FirstOrDefault();
                    if (surname == null)
                    {
                        throw new Exception("3");
                    }
                    else
                    {
                        client.surname = surname.decryptedValue;
                    }

                    var name = name_list_decrypted.Where(k => k.id.Equals(client.id)).FirstOrDefault();
                    if (name == null)
                    {
                        throw new Exception("3");
                    }
                    else
                    {
                        client.name = name.decryptedValue;
                    }

                    if (client.is_private_person)
                    {
                        client.company_name = "";
                        client.nip = "";
                    }
                    else
                    {
                        var company_name = company_name_list_decrypted.Where(k => k.id.Equals(client.id)).FirstOrDefault();
                        if (company_name == null)
                        {
                            throw new Exception("3");
                        }
                        else
                        {
                            client.company_name = company_name.decryptedValue;
                        }

                        var nip = nip_list_decrypted.Where(k => k.id.Equals(client.id)).FirstOrDefault();
                        if (nip == null)
                        {
                            throw new Exception("3");
                        }
                        else
                        {
                            client.nip = nip.decryptedValue;
                        }
                    }

                    var phone_number = phone_number_list_decrypted.Where(k => k.id.Equals(client.id)).FirstOrDefault();
                    if (phone_number == null)
                    {
                        throw new Exception("3");
                    }
                    else
                    {
                        client.phone_number = phone_number.decryptedValue;
                    }

                    var email = email_list_decrypted.Where(k => k.id.Equals(client.id)).FirstOrDefault();
                    if (email == null)
                    {
                        throw new Exception("3");
                    }
                    else
                    {
                        client.email = email.decryptedValue;
                    }

                    var address = address_list_decrypted.Where(k => k.id.Equals(client.id)).FirstOrDefault();
                    if (address == null)
                    {
                        throw new Exception("3");
                    }
                    else
                    {
                        client.address = address.decryptedValue;
                    }                    

                    var comment = comment_list_decrypted.Where(k => k.id.Equals(client.id)).FirstOrDefault();
                    if (comment == null)
                    {
                        throw new Exception("3");
                    }
                    else
                    {
                        client.comment = comment.decryptedValue;
                    }
                }

                return return_obj;

            }
        }


    }
}
