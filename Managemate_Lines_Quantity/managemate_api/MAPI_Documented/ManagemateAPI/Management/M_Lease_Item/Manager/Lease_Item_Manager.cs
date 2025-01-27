using ManagemateAPI.Database.Context;
using ManagemateAPI.Encryption;
using ManagemateAPI.Database.Tables;
using Microsoft.EntityFrameworkCore;
using ManagemateAPI.Encryption.Input_Objects;
using ManagemateAPI.Management.M_Counting_Unit.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Lease_Item_Group.Table_Model;
using ManagemateAPI.Management.M_Lease_Item.Input_Objects;
using ManagemateAPI.Management.M_Lease_Item.Table_Model;
using ManagemateAPI.Management.M_Storage.Table_Model;
using ManagemateAPI.Management.M_Construction_Site.Table_Model;
using ManagemateAPI.Management.Shared.Validator;
using ManagemateAPI.Management.Shared.Static;

/*
 * This is the Item_Manager with methods dedicated to the Item table.
 * 
 * It contains methods to:
 * add records,
 * edit records,
 * delete records,
 * get record by id,
 * get all the records.
 */
namespace ManagemateAPI.Management.M_Lease_Item.Manager
{
    public class Lease_Item_Manager
    {

        private DB_Context _context;
        private readonly IConfiguration _configuration;


        public Lease_Item_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /* 
         * Add_Lease_Item method
         * This method is used to add new records to the Lease_Item table.
         * 
         * It accepts Add_Lease_Item_Data object as input.
         * It then adds new record with values based on the data given in the input object.
         */
        public async Task<string> Add_Lease_Item(Add_Lease_Item_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var cat_num_exits = _context.Lease_Item.Where(i => i.catalog_number.Equals(input_obj.catalog_number)).FirstOrDefault();

                if (cat_num_exits != null)
                {
                    throw new Exception("18");//Catolog number already in use
                }

                var lease_item_group = _context.Lease_Item_Group.Where(i => i.id.Equals(input_obj.lease_group_FK)).FirstOrDefault();
                var counting_unit = _context.Counting_Unit.Where(i => i.id.Equals(input_obj.counting_unit_FK)).FirstOrDefault();

                if (lease_item_group == null || counting_unit == null)
                {
                    throw new Exception("19");// object not found
                }

                var encrypted_field = await Crypto.Encrypt(session, input_obj.comment);

                if(encrypted_field == null)
                {
                    throw new Exception("2");//encryption error
                }

                Lease_Item new_item = new Lease_Item
                {
                    catalog_number = input_obj.catalog_number,
                    product_name = input_obj.product_name,
                    lease_group_FK = lease_item_group,
                    weight_kg = input_obj.weight_kg,
                    price = input_obj.price,
                    counting_unit_FK = counting_unit,
                    size_cm_x = input_obj.size_cm_x,
                    size_cm_y = input_obj.size_cm_y,
                    area_m2 = (input_obj.size_cm_x * input_obj.size_cm_y) / 10000,
                    comment = encrypted_field,
                    deleted = false
                };

                

                _context.Lease_Item.Add(new_item);
                _context.SaveChanges();

                Lease_Item_Stock_History stock_update = new Lease_Item_Stock_History
                {
                    lease_item_FKid = new_item.id,
                    total_quantity = input_obj.total_quantity,
                    in_storage_quantity = 0,
                    out_of_storage_quantity = 0,
                    blocked_quantity = 0,
                    timestamp = input_obj.timestamp
                };
                _context.Lease_Item_Stock_History.Add(stock_update);
                _context.SaveChanges();

                return Info.SUCCESSFULLY_ADDED;
            }

        }

        /* 
         * Edit_Lease_Item method
         * This method is used to edit a record in the item_lease table.
         * 
         * It accepts Edit_Lease_Item_Data object as input.
         * It then changes values of a record with those given in the input object only if its ID matches the one in the input object.
         */
        public async Task<Lease_Item_Error_Model> Edit_Lease_Item(Edit_Lease_Item_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var edited_record = _context.Lease_Item.Where(l => 
                    l.id.Equals(input_obj.id) &&
                    l.deleted.Equals(false)
                )
                .Include(e => e.lease_item_in_storage_list_FK)
                .FirstOrDefault();

                if(edited_record == null || edited_record.lease_item_in_storage_list_FK == null)
                {
                    throw new Exception("19");// not found
                }

                var latest_stock_state = _context.Lease_Item_Stock_History.Where(lsh => lsh.lease_item_FKid.Equals(edited_record.id)).MaxBy(lsh => lsh.timestamp);
                if(latest_stock_state == null)
                {
                    throw new Exception("19");// not found
                }

                Stock_State_Validator.Validate_Stock_State(latest_stock_state);
                Timestamp_Validator.Validate_Input_Timestamp(latest_stock_state.timestamp, input_obj.timestamp);

                Lease_Item_Error_Model error_object = new Lease_Item_Error_Model();

                decimal currently_used_quantity = Currently_Used_Quantity(edited_record.lease_item_in_storage_list_FK, latest_stock_state, ref error_object);

                if(error_object.code != null)
                {
                    return error_object;
                }

                if (input_obj.total_quantity < currently_used_quantity)
                {
                    // new quantity is less than currently used
                    error_object.code = "35";
                    error_object.timestamp = latest_stock_state.timestamp;
                    error_object.lease_item_id = latest_stock_state.lease_item_FKid;
                    error_object.lease_item_in_storage_id = null;
                    error_object.required_quantity = currently_used_quantity - input_obj.total_quantity;

                    return error_object;
                }

                var cat_num_exists = _context.Lease_Item.Where(i => 
                    i.catalog_number.Equals(input_obj.catalog_number) && 
                    !i.id.Equals(input_obj.id) &&
                    i.deleted.Equals(false)
                ).FirstOrDefault();

                if (cat_num_exists != null)
                {
                    throw new Exception("18"); // Duplicate
                }

                var lease_item_group = _context.Lease_Item_Group.Where(i => i.id.Equals(input_obj.lease_group_FK)).FirstOrDefault();
                var counting_unit = _context.Counting_Unit.Where(i => i.id.Equals(input_obj.counting_unit_FK)).FirstOrDefault();

                if (lease_item_group == null || counting_unit == null)
                {
                    throw new Exception("19");// object not found
                }


                var encrypted_field = await Crypto.Encrypt(session, input_obj.comment);

                if (encrypted_field == null)
                {
                    throw new Exception("2");//encryption error
                }

                edited_record.catalog_number = input_obj.catalog_number;
                edited_record.product_name = input_obj.product_name;
                edited_record.lease_group_FK = lease_item_group;
                edited_record.weight_kg = input_obj.weight_kg;
                edited_record.price = input_obj.price;
                edited_record.size_cm_x = input_obj.size_cm_x;
                edited_record.size_cm_y = input_obj.size_cm_y;
                edited_record.area_m2 = (input_obj.size_cm_x * input_obj.size_cm_y) / 10000;
                edited_record.counting_unit_FK = counting_unit;
                edited_record.comment = encrypted_field;


                Lease_Item_Stock_History stock_update = new Lease_Item_Stock_History
                {
                    lease_item_FKid = edited_record.id,
                    total_quantity = input_obj.total_quantity,
                    in_storage_quantity = latest_stock_state.in_storage_quantity,
                    blocked_quantity = latest_stock_state.blocked_quantity,
                    out_of_storage_quantity = latest_stock_state.out_of_storage_quantity,
                    timestamp = input_obj.timestamp
                };

                _context.Lease_Item_Stock_History.Add(stock_update);

                _context.SaveChanges();

                return error_object;
            }
        }

        /*
         * Delete_Lease_Item method
         * This method is used to a record from the item_lease table.
         *  
         * It accepts Delete_Lease_Item_Data object as input.
         * Then it deletes a record if its ID matches the one given in the input object.
         */
        public Lease_Item_Error_Model Delete_Lease_Item(Delete_Lease_Item_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var id_exits = _context.Lease_Item.Where(i => 
                    i.id.Equals(input_obj.item_id) &&
                    i.deleted.Equals(false)
                )
                .Include(i => i.lease_item_in_storage_list_FK)
                .FirstOrDefault();

                if (id_exits == null || id_exits.lease_item_in_storage_list_FK == null)
                {
                    throw new Exception("19");// object not found
                }

                var latest_stock_state = _context.Lease_Item_Stock_History.Where(ish => ish.lease_item_FKid.Equals(id_exits.id)).MaxBy(ish => ish.timestamp);
                if (latest_stock_state == null)
                {
                    throw new Exception("19");// not found
                }

                Stock_State_Validator.Validate_Stock_State(latest_stock_state);

                Lease_Item_Error_Model error_object = new Lease_Item_Error_Model();

                decimal currently_used_quantity = Currently_Used_Quantity(id_exits.lease_item_in_storage_list_FK, latest_stock_state, ref error_object);

                if(error_object.code != null)
                {
                    return error_object;
                }

                if(currently_used_quantity > 0)
                {
                    // cannot delete when there are items in use
                    error_object.code = "37";
                    error_object.timestamp = latest_stock_state.timestamp;
                    error_object.lease_item_id = latest_stock_state.lease_item_FKid;
                    error_object.lease_item_in_storage_id = null;
                    error_object.required_quantity = currently_used_quantity;
                    
                    return error_object;
                }

                id_exits.deleted = true;

                _context.SaveChanges();
                    
                return error_object;
            }
        }

        /*
         * Get_Lease_Item_By_Id method
         * This method gets a record from the item_lease table by its ID and returns it.
         * 
         * It accepts Get_Lease_Item_By_Id_Data object as input.
         * Then it gets a record that has the same ID as the ID given in the input object
         */
        public async Task<Lease_Item_Model> Get_Lease_Item_By_Id(Get_Lease_Item_By_Id_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var selected_record = _context.Lease_Item.Where(i => 
                    i.id.Equals(input_obj.id_to_get) &&
                    i.deleted.Equals(false)
                )
                .Include(i => i.lease_group_FK)
                .Include(l => l.counting_unit_FK)
                .FirstOrDefault();

                if (
                    selected_record == null || 
                    selected_record.lease_group_FK == null || 
                    selected_record.counting_unit_FK == null
                )
                {
                    throw new Exception("19"); // object not found
                }


                string decrypted_field = await Crypto.Decrypt(session, selected_record.comment);

                if(decrypted_field == null)
                {
                    throw new Exception("3");
                }

                var latest_stock_state = _context.Lease_Item_Stock_History.Where(ish => ish.lease_item_FKid.Equals(selected_record.id)).MaxBy(ish => ish.timestamp);
                if (latest_stock_state == null)
                {
                    throw new Exception("19");// not found
                }

                Stock_State_Validator.Validate_Stock_State(latest_stock_state);

                Lease_Item_Model return_obj = new Lease_Item_Model
                {
                    id = selected_record.id,
                    catalog_number = selected_record.catalog_number,
                    product_name = selected_record.product_name,
                    lease_group_FK = new Lease_Item_Group_Model
                    {
                        id = selected_record.lease_group_FK.id,
                        group_name = selected_record.lease_group_FK.group_name,
                        rate = selected_record.lease_group_FK.rate,
                        tax_pct = selected_record.lease_group_FK.tax_pct
                    },
                    weight_kg = selected_record.weight_kg,
                    price = selected_record.price,
                    size_cm_x = selected_record.size_cm_x,
                    size_cm_y = selected_record.size_cm_y,
                    area_m2 = selected_record.area_m2,

                    total_quantity = latest_stock_state.total_quantity,

                    counting_unit_FK = new Counting_Unit_Model { id = selected_record.counting_unit_FK.id, unit = selected_record.counting_unit_FK.unit },

                    comment = decrypted_field
                };

                return return_obj;
            }
        }

        /*
         * Get_Lease_Item_Details method
         * This method gets a record from the item_lease table by its ID and returns it with additional info about places where it can be found.
         * 
         * It accepts Get_Lease_Item_By_Id_Data object as input.
         * Then it gets a record that has the same ID as the ID given in the input object
         */
        public async Task<Lease_Item_Model_Details> Get_Lease_Item_Details(Get_Lease_Item_By_Id_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var selected_record = _context.Lease_Item.Where(i => i.id.Equals(input_obj.id_to_get))
                    .Include(i => i.lease_group_FK)
                    .Include(i => i.counting_unit_FK)
                    .Include(i => i.lease_item_in_storage_list_FK)
                        .ThenInclude(s => s.storage_FK)
                    .Include(i => i.lease_item_in_storage_list_FK)
                        .ThenInclude(s => s.lease_item_on_protocol_list_FK)
                            .ThenInclude(p => p.lease_protocol_FK)
                                .ThenInclude(lp => lp.order_FK)
                    .Include(i => i.lease_item_in_storage_list_FK)
                        .ThenInclude(s => s.lease_item_out_of_storage_list_FK)
                            .ThenInclude(o => o.order_FK)
                                    .ThenInclude(c => c.construction_site_FK)
                    .FirstOrDefault();

                if (
                    selected_record == null ||
                    selected_record.lease_group_FK == null ||
                    selected_record.counting_unit_FK == null ||
                    selected_record.lease_item_in_storage_list_FK == null
                )
                {
                    throw new Exception("19"); // object not found
                }

                var latest_stock_state = _context.Lease_Item_Stock_History.Where(ish => ish.lease_item_FKid.Equals(selected_record.id)).MaxBy(ish => ish.timestamp);
                if (latest_stock_state == null)
                {
                    throw new Exception("19");// not found
                }

                Stock_State_Validator.Validate_Stock_State(latest_stock_state);

                Lease_Item_Model_Details return_obj = new Lease_Item_Model_Details();

                var get_base_info = Task.Run(async () => 
                {

                    string decrypted_field = await Crypto.Decrypt(session, selected_record.comment);

                    if (decrypted_field == null)
                    {
                        throw new Exception("3");
                    }

                    return_obj.id = selected_record.id;
                    return_obj.catalog_number = selected_record.catalog_number;
                    return_obj.product_name = selected_record.product_name;
                    return_obj.lease_group_FK = new Lease_Item_Group_Model
                    {
                        id = selected_record.lease_group_FK.id,
                        group_name = selected_record.lease_group_FK.group_name,
                        rate = selected_record.lease_group_FK.rate,
                        tax_pct = selected_record.lease_group_FK.tax_pct
                    };
                    return_obj.weight_kg = selected_record.weight_kg;
                    return_obj.price = selected_record.price;
                    return_obj.size_cm_x = selected_record.size_cm_x;
                    return_obj.size_cm_y = selected_record.size_cm_y;
                    return_obj.area_m2 = selected_record.area_m2;

                    return_obj.total_quantity = latest_stock_state.total_quantity;
                    return_obj.in_storage_quantity = latest_stock_state.in_storage_quantity;
                    return_obj.out_of_storage_quantity = latest_stock_state.out_of_storage_quantity;
                    return_obj.blocked_quantity = latest_stock_state.blocked_quantity;

                    return_obj.counting_unit_FK = new Counting_Unit_Model 
                    { 
                        id = selected_record.counting_unit_FK.id, 
                        unit = selected_record.counting_unit_FK.unit
                    };

                    return_obj.comment = decrypted_field;

                });                    

                List<Lease_Item_Storage_List_Model> storage_list = new List<Lease_Item_Storage_List_Model>();
                List<Lease_Item_Construction_Site_List_Model> construction_site_list = new List<Lease_Item_Construction_Site_List_Model>();
                List<Lease_Item_Movement_Model> movement_list = new List<Lease_Item_Movement_Model>();

                if(selected_record.lease_item_in_storage_list_FK.Count == 0)
                {
                    await get_base_info;

                    return_obj.storage_list = storage_list;
                    return_obj.construction_site_list = construction_site_list;
                    return_obj.movement_list = movement_list;
                        
                    return return_obj;
                }


                List<Encrypted_Object> encrypted_order_name = new List<Encrypted_Object>();
                List<Encrypted_Object> encrypted_con_site_name = new List<Encrypted_Object>();
                List<Encrypted_Object> encrypted_con_site_address = new List<Encrypted_Object>();

                foreach (var lease_item_in_storage in selected_record.lease_item_in_storage_list_FK)
                {
                    if (
                        lease_item_in_storage == null ||
                        lease_item_in_storage.storage_FK == null ||
                        lease_item_in_storage.lease_item_on_protocol_list_FK == null ||
                        lease_item_in_storage.lease_item_out_of_storage_list_FK == null
                    )
                    {
                        throw new Exception("19"); // object not found
                    }

                    var in_storage_latest_stock_state = _context.Lease_Item_In_Storage_Stock_History.Where(ish => ish.lease_item_in_storage_FKid.Equals(lease_item_in_storage.id)).MaxBy(ish => ish.timestamp);
                    if (in_storage_latest_stock_state == null)
                    {
                        throw new Exception("19");// not found
                    }

                    Stock_State_Validator.Validate_Stock_State(in_storage_latest_stock_state);

                    if (
                        in_storage_latest_stock_state.total_quantity.Equals(0) &&
                        lease_item_in_storage.lease_item_on_protocol_list_FK.Count.Equals(0)
                    )
                    {
                        continue;
                    }

                    var storage = lease_item_in_storage.storage_FK;

                    if (in_storage_latest_stock_state.total_quantity > 0)
                    {
                        storage_list.Add(
                            new Lease_Item_Storage_List_Model
                            {
                                storage_FK = new Item_Storage_List_Model { id = storage.id, number = storage.number, name = storage.name },
                                total_quantity = in_storage_latest_stock_state.total_quantity,
                                in_storage_quantity = in_storage_latest_stock_state.in_storage_quantity,
                                out_of_storage_quantity = in_storage_latest_stock_state.out_of_storage_quantity,
                                blocked_quantity = in_storage_latest_stock_state.blocked_quantity
                            }
                        );
                    }


                    var protocol_list = lease_item_in_storage.lease_item_on_protocol_list_FK;

                    foreach (var protocol_link in protocol_list)
                    {
                        var protocol = protocol_link.lease_protocol_FK;
                        if (protocol == null || protocol.order_FK == null)
                        {
                            throw new Exception("19"); // object not found
                        }

                        movement_list.Add(new Lease_Item_Movement_Model
                        {
                            date = protocol.timestamp,
                            protocol_id = protocol.id,
                            protocol_number = protocol.full_number,
                            protocol_type = protocol.type,
                            storage_id = storage.id,
                            storage_number = storage.number,
                            order_id = protocol.order_FK.id,
                            total_quantity = protocol_link.total_quantity
                        });

                        var order_name_exists = encrypted_order_name.Where(c => c.id.Equals(protocol.order_FK.id)).FirstOrDefault();
                        if (order_name_exists == null)
                        {
                            encrypted_order_name.Add(new Encrypted_Object { id = protocol.order_FK.id, encryptedValue = protocol.order_FK.order_name });
                        }
                    }


                    var out_of_storage_list = lease_item_in_storage.lease_item_out_of_storage_list_FK;

                    foreach(var out_of_storage in out_of_storage_list) 
                    {
                        if(
                            out_of_storage == null || 
                            out_of_storage.order_FK == null ||
                            out_of_storage.order_FK.construction_site_FK == null
                        )
                        {
                            throw new Exception("19"); // object not found
                        }

                        var out_of_storage_latest_state = _context.Lease_Item_Out_Of_Storage_History.Where(ish => ish.lease_item_out_of_storage_FKid.Equals(out_of_storage.id)).MaxBy(ish => ish.timestamp);
                        if (out_of_storage_latest_state == null)
                        {
                            throw new Exception("19");// not found
                        }

                        Stock_State_Validator.Validate_Out_Of_Storage_State(out_of_storage_latest_state, in_storage_latest_stock_state.out_of_storage_quantity);

                        if (out_of_storage_latest_state.total_quantity.Equals(0))
                        {
                            continue;
                        }

                        var existing_construction_site_name = encrypted_con_site_name.Where(c => c.id.Equals(out_of_storage.order_FK.construction_site_FK.id)).FirstOrDefault();
                        if(existing_construction_site_name == null)
                        {
                            encrypted_con_site_name.Add(new Encrypted_Object { id = out_of_storage.order_FK.construction_site_FK.id, encryptedValue = out_of_storage.order_FK.construction_site_FK.construction_site_name });
                            encrypted_con_site_address.Add(new Encrypted_Object { id = out_of_storage.order_FK.construction_site_FK.id, encryptedValue = out_of_storage.order_FK.construction_site_FK.address });
                        }

                        var order_name_exists = encrypted_order_name.Where(o => o.id.Equals(out_of_storage.order_FK.id)).FirstOrDefault();
                        if (order_name_exists == null)
                        {
                            encrypted_order_name.Add(new Encrypted_Object { id = out_of_storage.order_FK.id, encryptedValue = out_of_storage.order_FK.order_name });
                        }

                        var order_exist_in_output_list = construction_site_list.Where(c => c.order_id.Equals(out_of_storage.order_FK.id)).FirstOrDefault();
                        if(order_exist_in_output_list == null)
                        {
                            construction_site_list.Add(
                                new Lease_Item_Construction_Site_List_Model
                                {
                                    order_id = out_of_storage.order_FK.id,
                                    total_quantity = out_of_storage_latest_state.total_quantity,
                                    construction_site_FK = new Construction_Site_Item_Quantity_Model
                                    {
                                        id = out_of_storage.order_FK.construction_site_FK.id
                                    },
                                    from_storage_list = new List<From_Storage_Model>()
                                    {
                                        new From_Storage_Model { id = storage.id, number = storage.number, from_storage_quantity = out_of_storage_latest_state.total_quantity }
                                    }
                                }
                            );

                            continue;
                        }

                        order_exist_in_output_list.total_quantity += out_of_storage_latest_state.total_quantity;
                        order_exist_in_output_list.from_storage_list.Add(new From_Storage_Model
                        {
                            id = storage.id,
                            number = storage.number,
                            from_storage_quantity = out_of_storage_latest_state.total_quantity
                        });

                    }
                                     
                }

                List<Decrypted_Object> decrypted_storage_name = await Crypto.DecryptList(session, encrypted_storage_name);
                List<Decrypted_Object> decrypted_order_name = await Crypto.DecryptList(session, encrypted_order_name);
                List<Decrypted_Object> decrypted_con_site_name = await Crypto.DecryptList(session, encrypted_con_site_name);
                List<Decrypted_Object> decrypted_con_site_address = await Crypto.DecryptList(session, encrypted_con_site_address);

                if(
                    decrypted_storage_name == null || decrypted_storage_name.Count != encrypted_storage_name.Count ||
                    decrypted_order_name == null || decrypted_order_name.Count != encrypted_order_name.Count ||
                    decrypted_con_site_name == null || decrypted_con_site_name.Count != encrypted_con_site_name.Count ||
                    decrypted_con_site_address == null || decrypted_con_site_address.Count != encrypted_con_site_address.Count
                )
                {
                    throw new Exception("3"); // decryption error
                }

                Task assignDecryptedFields = Task.Run(() =>
                {
                    foreach(var con_site in construction_site_list)
                    {
                        if(con_site == null)
                        {
                            throw new Exception("19");
                        }

                        var order_name = decrypted_order_name.Where(o => o.id.Equals(con_site.order_id)).FirstOrDefault();
                        var con_site_name = decrypted_con_site_name.Where(c => c.id.Equals(con_site.construction_site_FK.id)).FirstOrDefault();
                        var con_site_address = decrypted_con_site_address.Where(c => c.id.Equals(con_site.construction_site_FK.id)).FirstOrDefault();
                        
                        if (order_name == null || con_site_name == null || con_site_address == null)
                        {
                            throw new Exception("3"); // decryption error
                        }

                        con_site.order_name = order_name.decryptedValue;
                        con_site.construction_site_FK.construction_site_name = con_site_name.decryptedValue;
                        con_site.construction_site_FK.address = con_site_address.decryptedValue;

                        foreach(var from_storage in con_site.from_storage_list)
                        {
                            var storage_name = decrypted_storage_name.Where(s => s.id.Equals(from_storage.id)).FirstOrDefault();
                            if (storage_name == null)
                            {
                                throw new Exception("3"); // decryption error
                            }
                            from_storage.name = storage_name.decryptedValue;
                        }

                    }

                });

                Task assignDecryptedFields_movement_list = Task.Run(() =>
                {
                    foreach(var movement_record in movement_list)
                    {
                        if(movement_record == null)
                        {
                            throw new Exception("19");
                        }

                        var order_name = decrypted_order_name.Where(o => o.id.Equals(movement_record.order_id)).FirstOrDefault();
                        var storage_name = decrypted_storage_name.Where(s => s.id.Equals(movement_record.storage_id)).FirstOrDefault();

                        if(order_name == null || storage_name == null)
                        {
                            throw new Exception("3"); // decryption error
                        }

                        movement_record.order_name = order_name.decryptedValue;
                        movement_record.storage_name = storage_name.decryptedValue;
                    }
                });

                foreach (var storage_info in storage_list)
                {
                    if(storage_info == null)
                    {
                        throw new Exception("19");
                    }

                    var name = decrypted_storage_name.Where(s => s.id.Equals(storage_info.storage_FK.id)).FirstOrDefault();
                    if(name == null)
                    {
                        throw new Exception("3"); // decryption error
                    }

                    storage_info.storage_FK.name = name.decryptedValue;
                }


                await get_base_info;
                await assignDecryptedFields;
                await assignDecryptedFields_movement_list;

                return_obj.storage_list = storage_list;
                return_obj.construction_site_list = construction_site_list;
                return_obj.movement_list = movement_list;

                return return_obj;

            }

        }

        /*
         * Get_All_Lease_Item method
         * This method gets all of the records in the Item table that aren't services and returns them in a list.
         * 
         * It accepts Session_Data object as input.
         */
        public async Task<List<Lease_Item_Model_List>> Get_All_Lease_Item(Session_Data session)
        {
            if (session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                List<Lease_Item> records_list = _context.Lease_Item.Where(li => li.deleted.Equals(false)).Include(s => s.lease_group_FK).Include(r => r.counting_unit_FK).ToList();

                List<Lease_Item_Model_List> return_list = new List<Lease_Item_Model_List>();

                if (records_list.Count == 0)
                {
                    return return_list;
                }

                List<Encrypted_Object> comment_list = new List<Encrypted_Object>();

                foreach (var item in records_list)
                {
                    if(
                        item == null ||
                        item.lease_group_FK == null || 
                        item.counting_unit_FK == null
                    )
                    {
                        throw new Exception("19");// not found
                    }

                    var latest_stock_state = _context.Lease_Item_Stock_History.Where(ish => ish.lease_item_FKid.Equals(item.id)).MaxBy(ish => ish.timestamp);
                    if (latest_stock_state == null)
                    {
                        throw new Exception("19");// not found
                    }

                    Stock_State_Validator.Validate_Stock_State(latest_stock_state);

                    return_list.Add(
                        new Lease_Item_Model_List
                        {
                            id = item.id,
                            catalog_number = item.catalog_number,
                            product_name = item.product_name,
                            lease_group = item.lease_group_FK.group_name,
                            weight_kg = item.weight_kg,
                            price = item.price,
                            size_cm_x = item.size_cm_x,
                            size_cm_y = item.size_cm_y,
                            area_m2 = item.area_m2,

                            total_quantity = latest_stock_state.total_quantity,
                            in_storage_quantity = latest_stock_state.in_storage_quantity,
                            out_of_storage_quantity = latest_stock_state.out_of_storage_quantity,
                            blocked_quantity = latest_stock_state.blocked_quantity,

                            counting_unit = item.counting_unit_FK.unit
                        }
                    );

                    comment_list.Add(new Encrypted_Object { id = item.id, encryptedValue = item.comment });
                }

                List<Decrypted_Object> comment_list_decrypted = await Crypto.DecryptList(session, comment_list);

                if(
                    comment_list_decrypted == null || comment_list_decrypted.Count != comment_list.Count
                )
                {
                    throw new Exception("3");// decryption error
                }

                foreach (var item in return_list)
                {

                    var comment = comment_list_decrypted.Where(s => s.id.Equals(item.id)).FirstOrDefault();
                    if (comment == null)
                    {
                        throw new Exception("3");// decryption error
                    }
                    else
                    {
                        item.comment = comment.decryptedValue;
                    }

                }

                return return_list;
            }
        }


        private decimal Currently_Used_Quantity(List<Lease_Item_In_Storage> lease_item_in_storage_list, Lease_Item_Stock_History latest_stock_state, ref Lease_Item_Error_Model error_object)
        {
            decimal currently_used_quantity = latest_stock_state.in_storage_quantity + latest_stock_state.out_of_storage_quantity;

            if (lease_item_in_storage_list.Count > 0)
            {
                decimal currently_used_quantity_check = 0;

                foreach (var item_in_storage in lease_item_in_storage_list)
                {
                    if(item_in_storage == null)
                    {
                        throw new Exception("19");//not found
                    }

                    var current_stock_state = _context.Lease_Item_In_Storage_Stock_History.Where(lsh => lsh.lease_item_in_storage_FKid.Equals(item_in_storage.id)).MaxBy(lsh => lsh.timestamp);
                    if(current_stock_state == null)
                    {
                        throw new Exception("19");//not found
                    }

                    if (current_stock_state.total_quantity.Equals(current_stock_state.in_storage_quantity + current_stock_state.out_of_storage_quantity))
                    {
                        currently_used_quantity_check += current_stock_state.total_quantity;
                    }
                    else
                    {
                        // checksum error
                        error_object.code = "36";
                        error_object.timestamp = current_stock_state.timestamp;
                        error_object.lease_item_id = latest_stock_state.lease_item_FKid;
                        error_object.lease_item_in_storage_id = item_in_storage.id;
                        error_object.required_quantity = Math.Abs(current_stock_state.total_quantity - (current_stock_state.in_storage_quantity + current_stock_state.out_of_storage_quantity));
                    }
                }

                if (!currently_used_quantity.Equals(currently_used_quantity_check))
                {
                    // checksum error
                    error_object.code = "36";
                    error_object.timestamp = latest_stock_state.timestamp;
                    error_object.lease_item_id = latest_stock_state.lease_item_FKid;
                    error_object.lease_item_in_storage_id = null;
                    error_object.required_quantity = Math.Abs(currently_used_quantity - currently_used_quantity_check);
                }

            }

            return currently_used_quantity;
        }


    }
}