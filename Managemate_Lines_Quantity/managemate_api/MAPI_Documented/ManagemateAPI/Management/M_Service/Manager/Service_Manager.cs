using System.Globalization;
using ManagemateAPI.Database.Context;
using ManagemateAPI.Database.Tables;
using ManagemateAPI.Encryption;
using ManagemateAPI.Encryption.Input_Objects;
using ManagemateAPI.Management.M_Service.Input_Objects;
using ManagemateAPI.Management.M_Service.Table_Model;
using ManagemateAPI.Management.M_Service_Group.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.Shared.Static;
using Microsoft.EntityFrameworkCore;

namespace ManagemateAPI.Management.M_Service.Manager
{
    public class Service_Manager
    {
        private DB_Context _context;
        private readonly IConfiguration _configuration;

        public Service_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> Add_Service(Add_Service_Data input, Session_Data session)
        {
            if (input == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var service_number_exists = _context.Service.Where(s => 
                    s.service_number.Equals(input.service_number) &&
                    s.deleted.Equals(false)
                ).FirstOrDefault();

                if (service_number_exists != null)
                {
                    throw new Exception("18");//Service number already in use
                }
                
                var service_group = _context.Service_Group.Where(sg => 
                    sg.id.Equals(input.service_group_FK)
                ).FirstOrDefault();

                if (service_group == null)
                {
                    throw new Exception("19");// object not found
                }


                var encrypted_field = await Crypto.Encrypt(session, input.service_name);
                if(encrypted_field == null)
                {
                    throw new Exception("2");// encryption error
                }

                Service new_record = new Service
                {
                    service_number = input.service_number,
                    service_name = input.service_name,
                    price = input.price,
                    service_group_FK = service_group,
                    comment = encrypted_field
                };

                
                _context.Service.Add(new_record);
                _context.SaveChanges();

                return Info.SUCCESSFULLY_ADDED;
            }
        }

        public async Task<string> Edit_Service(Edit_Service_Data input, Session_Data session)
        {
            if (input == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var record_to_edit = _context.Service.Where(s => 
                    s.id.Equals(input.id) &&
                    s.deleted.Equals(false)
                ).FirstOrDefault();

                if (record_to_edit == null)
                {
                    throw new Exception("19");// not found
                }

                var service_number_check = _context.Service.Where(s => 
                    s.service_number.Equals(input.service_number) && 
                    !s.id.Equals(record_to_edit.id) &&
                    s.deleted.Equals(false)
                ).FirstOrDefault();

                if (service_number_check != null)
                {
                    throw new Exception("18");//Catolog number already in use
                }

                var service_group = _context.Service_Group.Where(sg => sg.id.Equals(input.service_group_FK)).FirstOrDefault();

                if (service_group == null)
                {
                    throw new Exception("19");// object not found
                }


                var encrypted_field = await Crypto.Encrypt(session, input.comment);
                if (encrypted_field == null)
                {
                    throw new Exception("2");// encryption error
                }

                record_to_edit.service_number = input.service_number;
                record_to_edit.service_name = input.service_name;
                record_to_edit.price = input.price;
                record_to_edit.service_group_FK = service_group;
                record_to_edit.comment = encrypted_field;


                _context.SaveChanges();

                return Info.SUCCESSFULLY_CHANGED;
            }
        }

        public string Delete_Service(Delete_Service_Data input, Session_Data session)
        {
            if (input == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var record_to_delete = _context.Service.Where(s => 
                    s.id.Equals(input.service_id) &&
                    s.deleted.Equals(false)
                )
                .Include(s => s.service_on_lease_protocol_list_FK)
                .Include(s => s.service_on_sale_protocol_list_FK)
                .FirstOrDefault();

                if (
                    record_to_delete == null || 
                    record_to_delete.service_on_sale_protocol_list_FK == null || 
                    record_to_delete.service_on_lease_protocol_list_FK == null
                )
                {
                    throw new Exception("19");//not found
                }

                if(
                    record_to_delete.service_on_sale_protocol_list_FK.Count > 0 ||
                    record_to_delete.service_on_lease_protocol_list_FK.Count > 0
                )
                {
                    throw new Exception("38");//in use
                }

                record_to_delete.deleted = true;

                _context.SaveChanges();

                return Info.SUCCESSFULLY_DELETED;
            }
        }

        public async Task<Service_Model> Get_Service_By_Id(Get_Service_By_Id_Data input, Session_Data session)
        {
            if (input == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var selected_record = _context.Service.Where(s => 
                    s.id.Equals(input.id_to_get) &&
                    s.deleted.Equals(false)
                )
                .Include(s => s.service_group_FK)
                .FirstOrDefault();

                if (selected_record == null || selected_record.service_group_FK == null)
                {
                    throw new Exception("19");// not found
                }


                var decrypted_field = await Crypto.Decrypt(session, selected_record.comment);

                if(decrypted_field == null)
                {
                    throw new Exception("3");//decryption error
                }

                Service_Model return_obj = new Service_Model
                {
                    id = selected_record.id,
                    price = selected_record.price,
                    service_number = selected_record.service_number,
                    service_name = selected_record.service_name,
                    service_group_FK = new Service_Group_Model
                    {
                        id = selected_record.service_group_FK.id,
                        group_name = selected_record.service_group_FK.group_name,
                        tax_pct = selected_record.service_group_FK.tax_pct
                    },
                    comment = decrypted_field
                };

                

                return return_obj;
            }
        }

        public async Task<List<Service_Model_List>> Get_All_Service(Session_Data session)
        {
            if (session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                List<Service> record_list = _context.Service.Where(s => 
                    s.deleted.Equals(false)
                )
                .Include(s => s.service_group_FK)
                .ToList();

                List<Service_Model_List> return_obj = new List<Service_Model_List>();

                if(record_list.Count == 0)
                {
                    return return_obj;
                }


                List<Encrypted_Object> encrypted_comment = new List<Encrypted_Object>();


                foreach (var service in record_list)
                {
                    if(service.service_group_FK == null)
                    {
                        throw new Exception("19");// not found
                    }

                    return_obj.Add(new Service_Model_List {
                        id = service.id,
                        service_number = service.service_number,
                        service_name = service.service_name,
                        service_group = service.service_group_FK.group_name,
                        price = service.price
                    });

                    encrypted_comment.Add(new Encrypted_Object { id = service.id, encryptedValue = service.comment });
                }

                var decrypted_comment = await Crypto.DecryptList(session, encrypted_comment);

                if(
                    decrypted_comment == null || decrypted_comment.Count != encrypted_comment.Count
                )
                {
                    throw new Exception("3");//decryption error
                }

                foreach (var service in return_obj)
                {
                    var comment = decrypted_comment.Where(s => s.id.Equals(service.id)).FirstOrDefault();
                    if (comment != null)
                    {
                        service.comment = comment.decryptedValue;
                    }
                    else
                    {
                        throw new Exception("3");
                    }                    
                    
                }

                return return_obj;
            }
        }


    }
}
