using ManagemateAPI.Database.Context;
using ManagemateAPI.Database.Tables;
using ManagemateAPI.Encryption;
using ManagemateAPI.Encryption.Input_Objects;
using ManagemateAPI.Management.M_Storage.Input_Objects;
using ManagemateAPI.Management.M_Storage.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using Microsoft.EntityFrameworkCore;
using ManagemateAPI.Management.Shared.Static;

namespace ManagemateAPI.Management.M_Storage.Manager
{
    public class Storage_Manager
    {
        private DB_Context _context;
        private readonly IConfiguration _configuration;

        public Storage_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> Add_Storage(Add_Storage_Data input, Session_Data session)
        {
            if (input == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();


                var storage_exists = _context.Storage.Where(h => 
                    (h.number.Equals(input.number) || 
                    h.name.Equals(input.name)) &&
                    h.deleted.Equals(false)
                ).FirstOrDefault();

                if (storage_exists != null)
                {
                    throw new Exception("18");//Storage number/name already in use
                }
                else
                {
                    var description = await Crypto.Encrypt(session, input.description);
                    if(description == null)
                    {
                        throw new Exception("2");//encryption
                    }


                    Storage new_record = new Storage
                    {
                        number = input.number,
                        name = input.name,
                        description = description,
                        deleted = false
                    };

                    _context.Storage.Add(new_record);
                    _context.SaveChanges();

                    return Info.SUCCESSFULLY_ADDED;
                }
            }
        }

        public async Task<string> Edit_Storage(Edit_Storage_Data input, Session_Data session)
        {
            if (input == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var record_to_edit = _context.Storage.Where(k => 
                    k.id.Equals(input.id) &&
                    k.deleted.Equals(false)
                ).FirstOrDefault();

                if (record_to_edit != null)
                {
                    var storage_exists = _context.Storage.Where(o => 
                        (
                            o.number.Equals(input.number) || 
                            o.name.Equals(input.name)
                        ) && 
                        !o.id.Equals(record_to_edit.id) && 
                        o.deleted.Equals(false)
                    ).FirstOrDefault();

                    if (storage_exists != null)
                    {
                        throw new Exception("18");// number/name already in use
                    }

                    var description = await Crypto.Encrypt(session, input.description);
                    if (description == null)
                    {
                        throw new Exception("2");//encryption
                    }

                    record_to_edit.number = input.number;
                    record_to_edit.name = input.name;
                    record_to_edit.description = description;

                    _context.SaveChanges();

                    return Info.SUCCESSFULLY_CHANGED;
                    
                }
                else
                {
                    throw new Exception("19");
                }
            }
        }

        public string Delete_Storage(Delete_Storage_Data input, Session_Data session)
        {
            if (input == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var record_to_delete = _context.Storage.Where(i => 
                    i.id.Equals(input.storage_id) &&
                    i.deleted.Equals(false)
                )
                .Include(s => s.lease_item_in_storage_list_FK)
                .Include(s => s.sale_item_in_storage_list_FK)
                .FirstOrDefault();

                if (
                    record_to_delete == null || 
                    record_to_delete.lease_item_in_storage_list_FK == null || 
                    record_to_delete.sale_item_in_storage_list_FK == null
                )
                {
                    throw new Exception("19");
                }

                if(
                    record_to_delete.lease_item_in_storage_list_FK.Count > 0 ||
                    record_to_delete.sale_item_in_storage_list_FK.Count > 0
                )
                {
                    throw new Exception("38"); // in use
                }

                record_to_delete.deleted = true;

                _context.SaveChanges();

                return Info.SUCCESSFULLY_DELETED;
            }
        }

        public async Task<Storage_Model> Get_Storage_By_Id(Get_Storage_By_Id_Data input, Session_Data session)
        {
            if (input == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var selected_record = _context.Storage.Where(k => 
                    k.id.Equals(input.id_to_get) &&
                    k.deleted.Equals(false)
                ).FirstOrDefault();

                if (selected_record == null)
                {
                    throw new Exception("19");
                }

                var description = await Crypto.Decrypt(session, selected_record.description);
                if (description == null)
                {
                    throw new Exception("3");//decryption
                }

                Storage_Model return_object = new Storage_Model
                {
                    id = selected_record.id,
                    number = selected_record.number,
                    name = selected_record.name,
                    description = description
                };


                return return_object;
            }
        }

        public async Task<List<Storage_Model_List>> Get_All_Storage(Session_Data session)
        {
            if (session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                List<Storage> record_list = _context.Storage.Where(s => s.deleted.Equals(false)).ToList();

                List<Storage_Model_List> decrypted_records = new List<Storage_Model_List>();

                if (record_list.Count == 0)
                {
                    return decrypted_records;
                }

                List<Encrypted_Object> encrypted_description = new List<Encrypted_Object>();

                foreach (var storage in record_list)
                {
                    decrypted_records.Add(new Storage_Model_List {
                        id = storage.id,
                        number = storage.number,
                        name = storage.name,
                    });

                    encrypted_description.Add(new Encrypted_Object { id = storage.id, encryptedValue = storage.description });
                }

                List<Decrypted_Object> decrypted_description = await Crypto.DecryptList(session, encrypted_description);

                foreach (var storage in decrypted_records)
                {
                    var description = decrypted_description.Where(s => s.id == storage.id).FirstOrDefault();
                    if (description != null)
                    {
                        storage.description = description.decryptedValue;
                    }
                    else
                    {
                        throw new Exception("3");
                    }
                }


                return decrypted_records;
                
            }
        }

    }
}
