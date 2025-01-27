using ManagemateAPI.Database.Context;
using ManagemateAPI.Encryption;
using ManagemateAPI.Database.Tables;
using Microsoft.EntityFrameworkCore;
using ManagemateAPI.Encryption.Input_Objects;
using ManagemateAPI.Management.M_Order.Input_Objects;
using ManagemateAPI.Management.M_Order.Table_Model;
using ManagemateAPI.Management.M_Client.Table_Model;
using ManagemateAPI.Management.M_Construction_Site.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.Shared.Enum;
using ManagemateAPI.Management.Shared.Static;


/*
 * This is the Order_Manager with methods dedicated to the Order table.
 * 
 * It contains methods to:
 * add records,
 * edit records,
 * delete records,
 * get record by id,
 * get all the records.
 */
namespace ManagemateAPI.Management.M_Order.Manager
{
    public class Order_Manager
    {

        private DB_Context _context;
        private readonly IConfiguration _configuration;


        public Order_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /* 
         * Add_Order method
         * This method is used to add new records to the Order table.
         * 
         * It accepts Add_Order_Data object as input.
         * It then adds new record with values based on the data given in the input object.
         */
        public async Task<string> Add_Order(Add_Order_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                if (input_obj.use_static_rate)
                {
                    if(input_obj.static_rate <= 0)
                    {
                        throw new Exception("50");//invalid rate
                    }
                }

                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var client = _context.Client.Where(c => 
                    c.id.Equals(input_obj.client_FK) && 
                    c.deleted.Equals(false)
                ).FirstOrDefault();

                var construction_site = _context.Construction_Site.Where(con => 
                    con.id.Equals(input_obj.construction_site_FK) &&
                    con.deleted.Equals(false)
                ).FirstOrDefault();

                if (client == null || construction_site == null)
                {
                    throw new Exception("19");// objects not found
                }

                var existing_order = _context.Order.Where(o => 
                    o.order_number.Equals(input_obj.order_number) &&
                    o.deleted.Equals(false)
                ).FirstOrDefault();

                if (existing_order != null)
                {
                    throw new Exception("18");//Order already exists
                }

                List<Decrypted_Object> decrypted_fields = new List<Decrypted_Object> 
                {
                    new Decrypted_Object { id = 1, decryptedValue = input_obj.order_name },
                    new Decrypted_Object { id = 2, decryptedValue =  input_obj.comment }
                };

                List<Encrypted_Object> encrypted_fields = await Crypto.EncryptList(session, decrypted_fields);

                if(encrypted_fields == null || encrypted_fields.Count != decrypted_fields.Count)
                {
                    throw new Exception("2");
                }

                Order newOrder = new Order
                {
                    order_number = input_obj.order_number,
                    client_FK = client,
                    construction_site_FK = construction_site,
                    state = Order_State.Active,
                    timestamp = input_obj.timestamp,
                    default_payment_date_offset = input_obj.default_payment_date_offset,
                    default_payment_method = input_obj.default_payment_method,
                    default_discount = input_obj.default_discount,
                    use_static_rate = input_obj.use_static_rate,
                    static_rate = input_obj.static_rate / 100
                };

                foreach (var field in encrypted_fields)
                {
                    if(field == null)
                    {
                        throw new Exception("2");
                    }

                    switch (field.id)
                    {
                        case 1:
                            newOrder.order_name = field.encryptedValue;
                            break;

                        case 2:
                            newOrder.comment = field.encryptedValue;
                            break;

                        default: 
                            throw new Exception("2");
                    }
                }

                _context.Order.Add(newOrder);
                _context.SaveChanges();

                return Info.SUCCESSFULLY_ADDED;
            }

        }

        /* 
         * Edit_Order method
         * This method is used to edit a record in the Order table.
         * 
         * It accepts Edit_Order_Data object as input.
         * It then changes values of a record with those given in the input object only if its ID matches the one in the input object.
         */
        public async Task<List<Order_Error_Model>> Edit_Order(Edit_Order_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                if (input_obj.use_static_rate)
                {
                    if (input_obj.static_rate <= 0)
                    {
                        throw new Exception("50");//invalid rate
                    }
                }

                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var editing_order = _context.Order.Where(o => 
                    o.id.Equals(input_obj.id) &&
                    o.deleted.Equals(false)
                )
                .Include(o => o.lease_protocol_list_FK)
                .Include(o => o.sale_protocol_list_FK)
                .Include(o => o.lease_to_sale_protocol_list_FK)
                .Include(o => o.lease_item_out_of_storage_list_FK)
                .FirstOrDefault();
                if (
                    editing_order == null || 
                    editing_order.lease_protocol_list_FK == null || 
                    editing_order.sale_protocol_list_FK == null || 
                    editing_order.lease_to_sale_protocol_list_FK == null ||
                    editing_order.lease_item_out_of_storage_list_FK == null
                )
                {
                    throw new Exception("19");//Order not found
                }

                var min_lease_date = editing_order.lease_protocol_list_FK.MinBy(p => p.timestamp);
                var min_sale_date = editing_order.sale_protocol_list_FK.MinBy(p => p.timestamp);
                var min_lease_to_sale_date = editing_order.lease_to_sale_protocol_list_FK.MinBy(p => p.timestamp);

                List<Order_Error_Model> return_obj = new List<Order_Error_Model>();


                if(min_lease_date != null)
                {
                    if(input_obj.timestamp > min_lease_date.timestamp)
                    {
                        return_obj.Add(new Order_Error_Model
                        {
                            code = "40",
                            timestamp = min_lease_date.timestamp,
                            object_number = min_lease_date.full_number,
                            total_quantity = 0
                        });

                        return return_obj;
                    }
                }
                if (min_sale_date != null)
                {
                    if (input_obj.timestamp > min_sale_date.timestamp)
                    {
                        return_obj.Add(new Order_Error_Model
                        {
                            code = "40",
                            timestamp = min_sale_date.timestamp,
                            object_number = min_sale_date.full_number,
                            total_quantity = 0
                        });

                        return return_obj;
                    }
                }
                if (min_lease_to_sale_date != null)
                {
                    if (input_obj.timestamp > min_lease_to_sale_date.timestamp)
                    {
                        return_obj.Add(new Order_Error_Model
                        {
                            code = "40",
                            timestamp = min_lease_to_sale_date.timestamp,
                            object_number = min_lease_to_sale_date.full_number,
                            total_quantity = 0
                        });

                        return return_obj;
                    }
                }

                if(!input_obj.client_FK.Equals(editing_order.client_FKid) || !input_obj.construction_site_FK.Equals(editing_order.construction_site_FKid))
                {
                    throw new Exception("38");//in use
                }


                if (input_obj.state.Equals(Order_State.Closed))
                {

                    Lease_Item_Out_Of_Storage_History? out_of_storage_state;

                    foreach(var item_out_of_storage in editing_order.lease_item_out_of_storage_list_FK)
                    {
                        out_of_storage_state = _context.Lease_Item_Out_Of_Storage_History.Where(o => 
                            o.lease_item_out_of_storage_FKid.Equals(item_out_of_storage.id)
                        ).Include( o => o.lease_item_out_of_storage_FK).ThenInclude(li => li.lease_item_in_storage_FK).ThenInclude(li => li.lease_item_FK).MaxBy(i => i.timestamp);

                        if(
                            out_of_storage_state == null || 
                            out_of_storage_state.lease_item_out_of_storage_FK == null || 
                            out_of_storage_state.lease_item_out_of_storage_FK.lease_item_in_storage_FK == null || 
                            out_of_storage_state.lease_item_out_of_storage_FK.lease_item_in_storage_FK.lease_item_FK == null
                        )
                        {
                            throw new Exception("19");//not found - at least one record in history should exist if out_of_storage object exists
                        }

                        if(out_of_storage_state.total_quantity > 0)
                        {
                            return_obj.Add(new Order_Error_Model
                            {
                                code = "49",
                                timestamp = out_of_storage_state.timestamp,
                                object_number = out_of_storage_state.lease_item_out_of_storage_FK.lease_item_in_storage_FK.lease_item_FK.catalog_number,
                                total_quantity = out_of_storage_state.total_quantity
                            });
                        }

                    }

                    if (return_obj.Count > 0)
                    {
                        return return_obj;
                    }

                }



                var client = _context.Client.Where(c => 
                    c.id.Equals(input_obj.client_FK) && 
                    c.deleted.Equals(false)
                ).FirstOrDefault();

                var construction_site = _context.Construction_Site.Where(con => 
                    con.id.Equals(input_obj.construction_site_FK) && 
                    con.deleted.Equals(false)
                ).FirstOrDefault();

                if (client == null || construction_site == null)
                {
                    throw new Exception("19");//dependancies not found
                }

                var existing_order = _context.Order.Where(o =>
                    o.order_number.Equals(input_obj.order_number) &&
                    o.deleted.Equals(false)
                ).FirstOrDefault();

                if (existing_order != null)
                {
                    throw new Exception("18");//Order already exists
                }


                List<Decrypted_Object> decrypted_fields = new List<Decrypted_Object>
                {
                    new Decrypted_Object { id = 1, decryptedValue = input_obj.order_name },
                    new Decrypted_Object { id = 2, decryptedValue =  input_obj.comment }
                };

                List<Encrypted_Object> encrypted_fields = await Crypto.EncryptList(session, decrypted_fields);

                if (encrypted_fields == null || encrypted_fields.Count != decrypted_fields.Count)
                {
                    throw new Exception("2");
                }

                editing_order.order_number = input_obj.order_number;
                editing_order.use_static_rate = input_obj.use_static_rate;
                editing_order.static_rate = input_obj.static_rate / 100;
                editing_order.client_FK = client;
                editing_order.construction_site_FK = construction_site;
                editing_order.state = input_obj.state;
                editing_order.timestamp = input_obj.timestamp;
                editing_order.default_payment_date_offset = input_obj.default_payment_date_offset;
                editing_order.default_payment_method = input_obj.default_payment_method;
                editing_order.default_discount = input_obj.default_discount;

                foreach (var field in encrypted_fields)
                {
                    if (field == null)
                    {
                        throw new Exception("2");
                    }

                    switch (field.id)
                    {
                        case 1:
                            editing_order.order_name = field.encryptedValue;
                            break;

                        case 2:
                            editing_order.comment = field.encryptedValue;
                            break;

                        default:
                            throw new Exception("2");
                    }
                }

                _context.SaveChanges();

                return return_obj;

            }

        }

        /*
         * Delete_Order method
         * This method is used to a record from the Order table.
         *  
         * It accepts Delete_Order_Data object as input.
         * Then it deletes a record if its ID matches the one given in the input object.
         */
        public string Delete_Order(Delete_Order_Data input_obj, Session_Data session)
        {

            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var existing_order = _context.Order.Where(o => 
                    o.id.Equals(input_obj.id) &&
                    o.deleted.Equals(false)
                )
                .Include(o => o.lease_item_out_of_storage_list_FK)
                .Include(o => o.lease_protocol_list_FK)
                .Include(o => o.sale_protocol_list_FK)
                .Include(o => o.lease_to_sale_protocol_list_FK)
                .FirstOrDefault();

                if (
                    existing_order == null || 
                    existing_order.lease_item_out_of_storage_list_FK == null || 
                    existing_order.lease_protocol_list_FK == null || 
                    existing_order.sale_protocol_list_FK == null ||
                    existing_order.lease_to_sale_protocol_list_FK == null
                )
                {
                    throw new Exception("19");//not found
                }

                if(
                    existing_order.lease_item_out_of_storage_list_FK.Count > 0 ||
                    existing_order.lease_protocol_list_FK.Where(lp => lp.deleted.Equals(false)).Count() > 0 ||
                    existing_order.sale_protocol_list_FK.Where(sp => sp.deleted.Equals(false)).Count() > 0 ||
                    existing_order.lease_to_sale_protocol_list_FK.Count > 0
                )
                {
                    throw new Exception("38");//currently in use
                }

               existing_order.deleted = true;

                _context.SaveChanges();

                return Info.SUCCESSFULLY_DELETED;
            }

        }

        /*
         * Get_Order_By_ID method
         * This method gets a record from the Order table by its ID and returns it.
         * 
         * It accepts Get_Order_By_ID_Data object as input.
         * Then it gets a records that has the same ID as the ID given in the input object
         */
        public async Task<Order_Model> Get_Order_By_Id(Get_Order_By_Id_Data input_obj, Session_Data session)
        {

            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var selected_order = _context.Order.Where(o => 
                    o.id.Equals(input_obj.id_to_get) &&
                    o.deleted.Equals(false)
                )
                .Include(o => o.construction_site_FK)
                .Include(o => o.client_FK)
                .FirstOrDefault();

                if (selected_order == null || selected_order.construction_site_FK == null || selected_order.client_FK == null)
                {
                    throw new Exception("19");//not found
                }
                else
                {
                    List<Encrypted_Object> encrypted_fields =
                    [
                        //encrypted fields in Order table
                        new Encrypted_Object { id = 1, encryptedValue = selected_order.order_name },
                        new Encrypted_Object { id = 2, encryptedValue = selected_order.comment },
                        //encrypted fields in Client table
                        new Encrypted_Object { id = 3, encryptedValue = selected_order.client_FK.surname },
                        new Encrypted_Object { id = 4, encryptedValue = selected_order.client_FK.name },
                        new Encrypted_Object { id = 7, encryptedValue = selected_order.client_FK.phone_number },
                        new Encrypted_Object { id = 8, encryptedValue = selected_order.client_FK.email },
                        new Encrypted_Object { id = 9, encryptedValue = selected_order.client_FK.address },
                        new Encrypted_Object { id = 10, encryptedValue = selected_order.client_FK.comment },
                        //encrypted fields in Construction_Site table
                        new Encrypted_Object { id = 11, encryptedValue = selected_order.construction_site_FK.construction_site_name },
                        new Encrypted_Object { id = 12, encryptedValue = selected_order.construction_site_FK.address },
                        new Encrypted_Object { id = 13, encryptedValue = selected_order.construction_site_FK.comment }
                    ];

                    if (!selected_order.client_FK.is_private_person)
                    {
                        encrypted_fields.Add(new Encrypted_Object { id = 5, encryptedValue = selected_order.client_FK.company_name });
                        encrypted_fields.Add(new Encrypted_Object { id = 6, encryptedValue = selected_order.client_FK.nip });
                    }

                    List<Decrypted_Object> decrypted_fields = await Crypto.DecryptList(session, encrypted_fields);

                    if(decrypted_fields == null || decrypted_fields.Count != encrypted_fields.Count)
                    {
                        throw new Exception("3");//decryption error
                    }

                    Order_Model order_model = new Order_Model
                    {
                        order_number = selected_order.order_number,
                        id = selected_order.id,
                        state = selected_order.state,
                        timestamp = selected_order.timestamp,
                        default_payment_date_offset = selected_order.default_payment_date_offset,
                        default_payment_method = selected_order.default_payment_method,
                        default_discount = selected_order.default_discount,
                        use_static_rate = selected_order.use_static_rate,
                        static_rate = selected_order.static_rate * 100,
                        client_FK = new Client_Model { id = selected_order.client_FK.id, company_name = "", nip = "" },
                        construction_site_FK = new Construction_Site_Model { id = selected_order.construction_site_FK.id }
                    };

                    foreach (var field in decrypted_fields)
                    {
                        if (field == null)
                        {
                            throw new Exception("3");//error while decrypting data 
                        }

                        switch (field.id)
                        {
                            case 1:
                                order_model.order_name = field.decryptedValue; break;

                            case 2:
                                order_model.comment = field.decryptedValue; break;

                            //client

                            case 3:
                                order_model.client_FK.surname = field.decryptedValue; break;

                            case 4:
                                order_model.client_FK.name = field.decryptedValue; break;


                            case 5:
                                order_model.client_FK.company_name = field.decryptedValue; break;

                            case 6:
                                order_model.client_FK.nip = field.decryptedValue; break;


                            case 7:
                                order_model.client_FK.phone_number = field.decryptedValue; break;

                            case 8:
                                order_model.client_FK.email = field.decryptedValue; break;

                            case 9:
                                order_model.client_FK.address = field.decryptedValue; break;

                            case 10:
                                order_model.client_FK.comment = field.decryptedValue; break;

                            //construction_site:
                            case 11:
                                order_model.construction_site_FK.construction_site_name = field.decryptedValue; break;

                            case 12:
                                order_model.construction_site_FK.address = field.decryptedValue; break;

                            case 13:
                                order_model.construction_site_FK.comment = field.decryptedValue; break;

                            default:
                                throw new Exception("3");//error while decrypting data 
                        }

                    }

                    return order_model;

                }

            }

        }

        /*
         * Get_All_Order method
         * This method gets all of the records in the Order table and returns them in a list.
         * 
         * It accepts Get_All_Order_Data object as input.
         */
        public async Task<List<Order_Model_List>> Get_All_Order(Session_Data session)
        {

            if (session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                List<Order> order_list = _context.Order.Where(o => 
                    o.deleted.Equals(false)
                )
                .Include(o => o.client_FK)
                .Include(o => o.construction_site_FK)
                .ToList(); // all of orders

                List<Order_Model_List> return_obj = new List<Order_Model_List>();

                if (order_list.Count == 0)
                {
                    return return_obj;
                }

                List<Encrypted_Object> order_names = new List<Encrypted_Object>();
                List<Encrypted_Object> order_comments = new List<Encrypted_Object>();
                List<Encrypted_Object> order_client_names = new List<Encrypted_Object>();
                List<Encrypted_Object> order_client_surnames = new List<Encrypted_Object>();
                List<Encrypted_Object> order_client_company_names = new List<Encrypted_Object>();
                List<Encrypted_Object> order_construction_sites_names = new List<Encrypted_Object>();


                foreach (var order in order_list)
                {
                    if(order.client_FK == null || order.construction_site_FK == null)
                    {
                        throw new Exception("19");//no order found
                    }

                    return_obj.Add(new Order_Model_List
                    {
                        id = order.id,
                        order_number = order.order_number,
                        client_id = order.client_FKid,
                        client_number = order.client_FK.number,
                        client_is_private_person = order.client_FK.is_private_person,
                        construction_site_id = order.construction_site_FKid,
                        construction_site_number = order.construction_site_FK.number,
                        state = order.state,
                        timestamp = order.timestamp,
                        default_payment_date_offset = order.default_payment_date_offset,
                        default_payment_method = order.default_payment_method,
                        default_discount = order.default_discount,
                        use_static_rate = order.use_static_rate,
                        static_rate = order.static_rate * 100
                    });

                    order_names.Add(new Encrypted_Object { id = order.id, encryptedValue = order.order_name });
                    order_comments.Add(new Encrypted_Object { id = order.id, encryptedValue = order.comment });

                    order_client_names.Add(new Encrypted_Object { id = order.id, encryptedValue = order.client_FK.name });
                    order_client_surnames.Add(new Encrypted_Object { id = order.id, encryptedValue = order.client_FK.surname });
                    if (!order.client_FK.is_private_person)
                    {
                        order_client_company_names.Add(new Encrypted_Object { id = order.id, encryptedValue = order.client_FK.company_name });
                    }

                    order_construction_sites_names.Add(new Encrypted_Object { id = order.id, encryptedValue = order.construction_site_FK.construction_site_name });
                }

                List<Decrypted_Object> order_names_decrypted = await Crypto.DecryptList(session, order_names);
                List<Decrypted_Object> order_comments_decrypted = await Crypto.DecryptList(session, order_comments);

                List<Decrypted_Object> order_client_names_decrypted = await Crypto.DecryptList(session, order_client_names);
                List<Decrypted_Object> order_client_surnames_decrypted = await Crypto.DecryptList(session, order_client_surnames);
                List<Decrypted_Object> order_client_company_names_decrypted = await Crypto.DecryptList(session, order_client_company_names);

                List<Decrypted_Object> order_construction_sites_names_decrypted = await Crypto.DecryptList(session, order_construction_sites_names);

                foreach (var order in return_obj)
                {
                    var name = order_names_decrypted.Where(o => o.id.Equals(order.id)).FirstOrDefault();

                    if (name == null)
                    {
                        throw new Exception("3");//error while decrypting data 
                    }
                    else
                    {
                        order.order_name = name.decryptedValue;
                    }


                    var comment = order_comments_decrypted.Where(o => o.id.Equals(order.id)).FirstOrDefault();

                    if (comment == null)
                    {
                        throw new Exception("3");//error while decrypting data 
                    }
                    else
                    {
                        order.comment = comment.decryptedValue;
                    }


                    var client_name = order_client_names_decrypted.Where(o => o.id.Equals(order.id)).FirstOrDefault();

                    if (client_name == null)
                    {
                        throw new Exception("3");//error while decrypting data 
                    }
                    else
                    {
                        order.client_name = client_name.decryptedValue;
                    }


                    var client_surname = order_client_surnames_decrypted.Where(o => o.id.Equals(order.id)).FirstOrDefault();

                    if (client_surname == null)
                    {
                        throw new Exception("3");//error while decrypting data 
                    }
                    else
                    {
                        order.client_surname = client_surname.decryptedValue;
                    }

                    if (order.client_is_private_person)
                    {
                        order.client_company_name = "";
                    }
                    else
                    {
                        var client_company_name = order_client_company_names_decrypted.Where(o => o.id.Equals(order.id)).FirstOrDefault();

                        if (client_company_name == null)
                        {
                            throw new Exception("3");//error while decrypting data 
                        }
                        else
                        {
                            order.client_company_name = client_company_name.decryptedValue;
                        }
                    }

                    var construction_site_name = order_construction_sites_names_decrypted.Where(o => o.id.Equals(order.id)).FirstOrDefault();

                    if (construction_site_name == null)
                    {
                        throw new Exception("3");//error while decrypting data 
                    }
                    else
                    {
                        order.construction_site_name = construction_site_name.decryptedValue;
                    }

                }

                return return_obj;

            }

        }



    }
}
