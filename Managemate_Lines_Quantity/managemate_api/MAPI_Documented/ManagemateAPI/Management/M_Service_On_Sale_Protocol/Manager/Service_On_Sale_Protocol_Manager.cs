using ManagemateAPI.Database.Context;
using ManagemateAPI.Database.Tables;
using ManagemateAPI.Encryption.Input_Objects;
using ManagemateAPI.Encryption;
using ManagemateAPI.Management.M_Service_On_Lease_Protocol.Input_Objects;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.Shared.Enum;
using ManagemateAPI.Management.M_Service_On_Sale_Protocol.Input_Objects;
using Microsoft.EntityFrameworkCore;
using ManagemateAPI.Management.M_Service_On_Lease_Protocol.Table_Model;
using System.Globalization;
using ManagemateAPI.Management.M_Service_On_Sale_Protocol.Table_Model;
using ManagemateAPI.Management.Shared.Static;

namespace ManagemateAPI.Management.M_Service_On_Sale_Protocol.Manager
{
    public class Service_On_Sale_Protocol_Manager
    {

        private DB_Context _context;
        private readonly IConfiguration _configuration;

        public Service_On_Sale_Protocol_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public async Task<string> Add_Service_On_Sale_Protocol(Add_Service_On_Sale_Protocol_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var protocol = _context.Sale_Protocol.Where(sp =>
                    sp.id.Equals(input_obj.sale_protocol_FK) &&
                    !sp.state.Equals(Protocol_State.Offer) &&
                    sp.deleted.Equals(false)
                ).FirstOrDefault();

                if (protocol == null)
                {
                    throw new Exception("19");// object not found
                }

                var service = _context.Service.Where(s =>
                    s.id.Equals(input_obj.service_FK) &&
                    s.deleted.Equals(false)
                )
                .FirstOrDefault();

                if (service == null)
                {
                    throw new Exception("19");// object not found
                }

                var encrypted_comment = await Crypto.Encrypt(session, input_obj.comment);
                if (encrypted_comment == null)
                {
                    throw new Exception("2");// encryption error
                }

                Service_On_Sale_Protocol new_record = new Service_On_Sale_Protocol
                {
                    sale_protocol_FKid = protocol.id,
                    service_FKid = service.id,
                    net_worth = input_obj.net_worth,
                    comment = encrypted_comment
                };


                _context.Service_On_Sale_Protocol.Add(new_record);
                _context.SaveChanges();

                return Info.SUCCESSFULLY_ADDED;
            }
        }


        public async Task<string> Edit_Service_On_Sale_Protocol(Edit_Service_On_Sale_Protocol_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var service_on_sale_protocol_exists = _context.Service_On_Sale_Protocol.Where(ss =>
                    ss.id.Equals(input_obj.id)
                )
                .FirstOrDefault();

                if (service_on_sale_protocol_exists == null)
                {
                    throw new Exception("19");// object not found
                }

                var encrypted_comment = await Crypto.Encrypt(session, input_obj.comment);
                if (encrypted_comment == null)
                {
                    throw new Exception("2");// encryption error
                }

                service_on_sale_protocol_exists.net_worth = input_obj.net_worth;
                service_on_sale_protocol_exists.comment = encrypted_comment;

                _context.SaveChanges();

                return Info.SUCCESSFULLY_CHANGED;
            }
        }


        public string Delete_Service_On_Sale_Protocol(Delete_Service_On_Sale_Protocol_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var service_on_sale_protocol_exists = _context.Service_On_Sale_Protocol.Where(ss =>
                    ss.id.Equals(input_obj.service_on_sale_protocol_id)
                )
                .FirstOrDefault();

                if (service_on_sale_protocol_exists == null)
                {
                    throw new Exception("19");// object not found
                }

                _context.Service_On_Sale_Protocol.Remove(service_on_sale_protocol_exists);

                _context.SaveChanges();

                return Info.SUCCESSFULLY_DELETED;
            }
        }


        public async Task<Service_On_Sale_Protocol_Model> Get_Service_On_Sale_Protocol_By_Id(Get_Service_On_Sale_Protocol_By_Id_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var service_on_sale_protocol_exists = _context.Service_On_Sale_Protocol.Where(ss =>
                    ss.id.Equals(input_obj.id_to_get)
                )
                .Include(sl => sl.service_FK)
                .FirstOrDefault();

                if (service_on_sale_protocol_exists == null || service_on_sale_protocol_exists.service_FK == null)
                {
                    throw new Exception("19");// object not found
                }

                var decrypted_field = await Crypto.Decrypt(session, service_on_sale_protocol_exists.comment);
                if (decrypted_field == null)
                {
                    throw new Exception("3");// decryption error
                }


                Service_On_Sale_Protocol_Model return_obj = new Service_On_Sale_Protocol_Model
                {
                    id = service_on_sale_protocol_exists.id,
                    service_id = service_on_sale_protocol_exists.service_FKid,
                    service_number = service_on_sale_protocol_exists.service_FK.service_number,
                    service_name = service_on_sale_protocol_exists.service_FK.service_name,
                    net_worth = service_on_sale_protocol_exists.net_worth,
                    comment = decrypted_field
                };



                return return_obj;
            }
        }


        public async Task<List<Service_On_Sale_Protocol_Model>> Get_All_Service_On_Sale_Protocol(Get_All_Service_On_Sale_Protocol_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var sale_protocol = _context.Sale_Protocol.Where(sp =>
                    sp.id.Equals(input_obj.sale_protocol_id) &&
                    !sp.state.Equals(Protocol_State.Offer) &&
                    sp.deleted.Equals(false)
                )
                .Include(lp => lp.service_on_sale_protocol_list_FK)
                .FirstOrDefault();

                if (sale_protocol == null || sale_protocol.service_on_sale_protocol_list_FK == null)
                {
                    throw new Exception("19");// object not found
                }

                List<Service_On_Sale_Protocol_Model> return_obj = new List<Service_On_Sale_Protocol_Model>();

                if (sale_protocol.service_on_sale_protocol_list_FK.Count.Equals(0))
                {
                    return return_obj;
                }

                List<Encrypted_Object> encrypted_comment = new List<Encrypted_Object>();

                foreach (var service_on_protocol in sale_protocol.service_on_sale_protocol_list_FK)
                {
                    if (service_on_protocol == null || service_on_protocol.service_FK == null)
                    {
                        throw new Exception("19");//not found
                    }

                    return_obj.Add(
                        new Service_On_Sale_Protocol_Model
                        {
                            id = service_on_protocol.id,
                            service_id = service_on_protocol.service_FKid,
                            service_number = service_on_protocol.service_FK.service_number,
                            service_name = service_on_protocol.service_FK.service_name,
                            net_worth = service_on_protocol.net_worth
                        }
                    );

                    encrypted_comment.Add(new Encrypted_Object { id = service_on_protocol.id, encryptedValue = service_on_protocol.comment });
                }

                var decrypted_comment = await Crypto.DecryptList(session, encrypted_comment);
                if (
                    decrypted_comment == null || decrypted_comment.Count != encrypted_comment.Count
                )
                {
                    throw new Exception("3");// decryption error
                }


                Decrypted_Object? comment;

                foreach (var service_on_protocol_model in return_obj)
                {
                    comment = decrypted_comment.Where(d => d.id.Equals(service_on_protocol_model.id)).FirstOrDefault();
                    if (comment == null)
                    {
                        throw new Exception("3");//decryption error
                    }

                    service_on_protocol_model.comment = comment.decryptedValue;

                }


                return return_obj;
            }
        }



    }
}
