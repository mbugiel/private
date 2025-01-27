using ManagemateAPI.Database.Context;
using ManagemateAPI.Database.Tables;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Encryption;
using Microsoft.EntityFrameworkCore;
using ManagemateAPI.Encryption.Input_Objects;
using ManagemateAPI.Management.M_Counting_Unit.Table_Model;
using ManagemateAPI.Management.M_Sale_Item.Input_Objects;
using ManagemateAPI.Management.M_Sale_Item.Table_Model;
using ManagemateAPI.Management.M_Sale_Item_Group.Table_Model;
using ManagemateAPI.Management.M_Storage.Table_Model;
using ManagemateAPI.Management.Shared.Validator;
using ManagemateAPI.Management.Shared.Static;

namespace ManagemateAPI.Management.M_Sale_Item.Manager
{
    public class Sale_Item_Manager
    {
        private DB_Context _context;
        private readonly IConfiguration _configuration;

        public Sale_Item_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> Add_Sale_Item(Add_Sale_Item_Data input, Session_Data session)
        {
            if (input == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var catalog_number_exits = _context.Sale_Item.Where(i => i.catalog_number.Equals(input.catalog_number)).FirstOrDefault();

                if (catalog_number_exits != null)
                {
                    throw new Exception("18");//Catolog number already in use
                }

                var sale_item_group = _context.Sale_Item_Group.Where(i => i.id.Equals(input.sale_group_FK)).FirstOrDefault();
                var counting_unit = _context.Counting_Unit.Where(i => i.id.Equals(input.counting_unit_FK)).FirstOrDefault();

                if (sale_item_group == null || counting_unit == null)
                {
                    throw new Exception("19");// object not found
                }


                byte[] encrypted_field = await Crypto.Encrypt(session, input.comment);

                if(encrypted_field == null)
                {
                    throw new Exception("2");//encryption error
                }

                Sale_Item new_item = new Sale_Item
                {
                    catalog_number = input.catalog_number,
                    product_name = input.product_name,
                    sale_group_FK = sale_item_group,
                    price = input.price,
                    weight_kg = input.weight_kg,
                    size_cm_x = input.size_cm_x,
                    size_cm_y = input.size_cm_y,
                    area_m2 = (input.size_cm_x * input.size_cm_y) / 10000,
                    counting_unit_FK = counting_unit,
                    comment = encrypted_field,
                    deleted = false
                };

                _context.Sale_Item.Add(new_item);
                _context.SaveChanges();

                Sale_Item_Stock_History stock_update = new Sale_Item_Stock_History
                {
                    sale_item_FKid = new_item.id,
                    total_quantity = input.total_quantity,
                    in_storage_quantity = 0,
                    blocked_quantity = 0,
                    timestamp = input.timestamp
                };

                _context.Sale_Item_Stock_History.Add(stock_update);
                _context.SaveChanges();

                return Info.SUCCESSFULLY_ADDED;
            }
        }

        public async Task<Sale_Item_Error_Model> Edit_Sale_Item(Edit_Sale_Item_Data input, Session_Data session)
        {
            if (input == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();
                
                var record_to_edit = _context.Sale_Item.Where(i => 
                    i.id.Equals(input.id) &&
                    i.deleted.Equals(false)
                )
                .Include(s => s.sale_item_in_storage_list_FK)
                .FirstOrDefault();

                if(record_to_edit == null || record_to_edit.sale_item_in_storage_list_FK == null)
                {
                    throw new Exception("19");//not found
                }

                var latest_stock_state = _context.Sale_Item_Stock_History.Where(ssh => ssh.sale_item_FKid.Equals(record_to_edit.id)).MaxBy(ssh => ssh.timestamp);
                if(latest_stock_state == null)
                {
                    throw new Exception("19");//not found
                }

                Stock_State_Validator.Validate_Stock_State(latest_stock_state);
                Timestamp_Validator.Validate_Input_Timestamp(latest_stock_state.timestamp, input.timestamp);


                Sale_Item_Error_Model error_object = new Sale_Item_Error_Model();

                decimal currently_used_quantity = Currently_Used_Quantity(record_to_edit.sale_item_in_storage_list_FK, latest_stock_state, ref error_object);

                if (error_object.code != null)
                {
                    return error_object;
                }

                if (input.total_quantity < currently_used_quantity)
                {
                    // new quantity is less than currently used
                    error_object.code = "35";
                    error_object.timestamp = latest_stock_state.timestamp;
                    error_object.sale_item_id = latest_stock_state.sale_item_FKid;
                    error_object.sale_item_in_storage_id = null;
                    error_object.required_quantity = currently_used_quantity - input.total_quantity;

                    return error_object;
                }

                var catalog_number_exits = _context.Sale_Item.Where(i =>
                    i.catalog_number.Equals(input.catalog_number) &&
                    !i.id.Equals(record_to_edit.id) &&
                    i.deleted.Equals(false)
                ).FirstOrDefault();

                if (catalog_number_exits != null)
                {
                    throw new Exception("18");//Catalog number already in use
                }

                var sale_item_group = _context.Sale_Item_Group.Where(i => i.id.Equals(input.sale_group_FK)).FirstOrDefault();
                var counting_unit = _context.Counting_Unit.Where(i => i.id.Equals(input.counting_unit_FK)).FirstOrDefault();

                if (sale_item_group == null || counting_unit == null)
                {
                    throw new Exception("19");// object not found
                }

                List<Decrypted_Object> decrypted_fields = new List<Decrypted_Object>
                {
                    new Decrypted_Object { id = 1, decryptedValue = input.product_name },
                    new Decrypted_Object { id = 2, decryptedValue = input.comment }
                };

                byte[] encrypted_field = await Crypto.Encrypt(session, input.comment);

                if (encrypted_field == null)
                {
                    throw new Exception("2");//encryption error
                }

                record_to_edit.catalog_number = input.catalog_number;
                record_to_edit.product_name = input.product_name;
                record_to_edit.sale_group_FK = sale_item_group;
                record_to_edit.price = input.price;
                record_to_edit.weight_kg = input.weight_kg;
                record_to_edit.size_cm_x = input.size_cm_x;
                record_to_edit.size_cm_y = input.size_cm_y;
                record_to_edit.area_m2 = (input.size_cm_x * input.size_cm_y) / 10000;
                record_to_edit.counting_unit_FK = counting_unit;
                record_to_edit.comment = encrypted_field;


                Sale_Item_Stock_History stock_update = new Sale_Item_Stock_History
                {
                    sale_item_FKid = record_to_edit.id,
                    total_quantity = input.total_quantity,
                    in_storage_quantity = latest_stock_state.in_storage_quantity,
                    blocked_quantity = latest_stock_state.blocked_quantity,
                    timestamp = input.timestamp
                };

                _context.Sale_Item_Stock_History.Add(stock_update);

                _context.SaveChanges();

                return error_object;
            }
        }

        public Sale_Item_Error_Model Delete_Sale_Item(Delete_Sale_Item_Data input, Session_Data session)
        {
            if (input == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var record_to_delete = _context.Sale_Item.Where(i => 
                    i.id.Equals(input.item_id) &&
                    i.deleted.Equals(false)
                )
                .Include(i => i.sale_item_in_storage_list_FK)
                .FirstOrDefault();

                if (record_to_delete == null || record_to_delete.sale_item_in_storage_list_FK == null)
                {
                    throw new Exception("19");// not found
                }

                var latest_stock_state = _context.Sale_Item_Stock_History.Where(ssh => ssh.sale_item_FKid.Equals(record_to_delete.id)).MaxBy(ssh => ssh.timestamp);
                if (latest_stock_state == null)
                {
                    throw new Exception("19");//not found
                }

                Stock_State_Validator.Validate_Stock_State(latest_stock_state);

                Sale_Item_Error_Model error_object = new Sale_Item_Error_Model();

                decimal currently_used_quantity = Currently_Used_Quantity(record_to_delete.sale_item_in_storage_list_FK, latest_stock_state, ref error_object);

                if (error_object.code != null)
                {
                    return error_object;
                }

                if (currently_used_quantity > 0)
                {
                    // cannot delete when there are items in use
                    error_object.code = "37";
                    error_object.timestamp = latest_stock_state.timestamp;
                    error_object.sale_item_id = latest_stock_state.sale_item_FKid;
                    error_object.sale_item_in_storage_id = null;
                    error_object.required_quantity = currently_used_quantity;

                    return error_object;
                }

                record_to_delete.deleted = true;

                _context.SaveChanges();

                return error_object;
            }
        }

        public async Task<Sale_Item_Model> Get_Sale_Item_By_Id(Get_Sale_Item_By_Id_Data input, Session_Data session)
        {
            if (input == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var selected_record = _context.Sale_Item.Where(i =>
                    i.id.Equals(input.id_to_get) &&
                    i.deleted.Equals(false)
                )
                .Include(e => e.sale_group_FK)
                .Include(d => d.counting_unit_FK)
                .FirstOrDefault();

                if (
                    selected_record == null || 
                    selected_record.sale_group_FK == null || 
                    selected_record.counting_unit_FK == null
                )
                {
                    throw new Exception("19"); // not found
                }


                string decrypted_field = await Crypto.Decrypt(session, selected_record.comment);

                if (decrypted_field == null)
                {
                    throw new Exception("3");//decryption error
                }

                var latest_stock_state = _context.Sale_Item_Stock_History.Where(ssh => ssh.sale_item_FKid.Equals(selected_record.id)).MaxBy(ssh => ssh.timestamp);
                if (latest_stock_state == null)
                {
                    throw new Exception("19");//not found
                }

                Stock_State_Validator.Validate_Stock_State(latest_stock_state);

                Sale_Item_Model return_obj = new Sale_Item_Model
                {
                    id = selected_record.id,
                    catalog_number = selected_record.catalog_number,
                    product_name = selected_record.product_name,
                    sale_group_FK = new Sale_Item_Group_Model
                    {
                        id = selected_record.sale_group_FK.id,
                        group_name = selected_record.sale_group_FK.group_name,
                        tax_pct = selected_record.sale_group_FK.tax_pct
                    },
                    price = selected_record.price,
                    weight_kg = selected_record.weight_kg,
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

        public async Task<Sale_Item_Model_Details> Get_Sale_Item_Details(Get_Sale_Item_By_Id_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var selected_record = _context.Sale_Item.Where(i => i.id.Equals(input_obj.id_to_get))
                    .Include(i => i.sale_group_FK)
                    .Include(i => i.counting_unit_FK)
                    .Include(i => i.sale_item_in_storage_list_FK)
                        .ThenInclude(s => s.storage_FK)
                    .Include(i => i.sale_item_in_storage_list_FK)
                        .ThenInclude(s => s.sale_item_on_protocol_list_FK)
                            .ThenInclude(p => p.sale_protocol_FK)
                                .ThenInclude(sp => sp.order_FK)
                    .FirstOrDefault();

                if (
                    selected_record == null ||
                    selected_record.sale_group_FK == null ||
                    selected_record.counting_unit_FK == null ||
                    selected_record.sale_item_in_storage_list_FK == null
                )
                {
                    throw new Exception("19"); // object not found
                }

                var latest_stock_state = _context.Sale_Item_Stock_History.Where(ssh => ssh.sale_item_FKid.Equals(selected_record.id)).MaxBy(ssh => ssh.timestamp);
                if (latest_stock_state == null)
                {
                    throw new Exception("19");//not found
                }

                Stock_State_Validator.Validate_Stock_State(latest_stock_state);

                Sale_Item_Model_Details return_obj = new Sale_Item_Model_Details();

                var get_base_info = Task.Run(async () =>
                {

                    string decrypted_field = await Crypto.Decrypt(session, selected_record.comment);

                    if (decrypted_field == null)
                    {
                        throw new Exception("3");//decryption error
                    }

                    return_obj.id = selected_record.id;
                    return_obj.catalog_number = selected_record.catalog_number;
                    return_obj.product_name = selected_record.product_name;
                    return_obj.sale_group_FK = new Sale_Item_Group_Model
                    {
                        id = selected_record.sale_group_FK.id,
                        group_name = selected_record.sale_group_FK.group_name,
                        tax_pct = selected_record.sale_group_FK.tax_pct
                    };
                    return_obj.price = selected_record.price;
                    return_obj.weight_kg = selected_record.weight_kg;
                    return_obj.size_cm_x = selected_record.size_cm_x;
                    return_obj.size_cm_y = selected_record.size_cm_y;
                    return_obj.area_m2 = selected_record.area_m2;

                    return_obj.total_quantity = latest_stock_state.total_quantity;
                    return_obj.in_storage_quantity = latest_stock_state.in_storage_quantity;
                    return_obj.blocked_quantity = latest_stock_state.blocked_quantity;

                    return_obj.counting_unit_FK = new Counting_Unit_Model
                    {
                        id = selected_record.counting_unit_FK.id,
                        unit = selected_record.counting_unit_FK.unit
                    };

                    return_obj.comment = decrypted_field;

                });

                List<Sale_Item_Storage_List_Model> storage_list = new List<Sale_Item_Storage_List_Model>();
                List<Sale_Item_Movement_Model> movement_list = new List<Sale_Item_Movement_Model>();

                if (selected_record.sale_item_in_storage_list_FK.Count == 0)
                {
                    await get_base_info;

                    return_obj.storage_list = storage_list;
                    return_obj.movement_list = movement_list;

                    return return_obj;
                }


                List<Encrypted_Object> encrypted_order_name = new List<Encrypted_Object>();

                foreach (var sale_item_in_storage in selected_record.sale_item_in_storage_list_FK)
                {
                    if (
                        sale_item_in_storage == null ||
                        sale_item_in_storage.storage_FK == null ||
                        sale_item_in_storage.sale_item_on_protocol_list_FK == null
                    )
                    {
                        throw new Exception("19"); // object not found
                    }

                    var in_storage_latest_stock_state = _context.Sale_Item_In_Storage_Stock_History.Where(ssh => ssh.sale_item_in_storage_FKid.Equals(sale_item_in_storage.id)).MaxBy(ssh => ssh.timestamp);
                    if(in_storage_latest_stock_state == null)
                    {
                        throw new Exception("19"); // object not found
                    }

                    Stock_State_Validator.Validate_Stock_State(in_storage_latest_stock_state);

                    if (
                        in_storage_latest_stock_state.in_storage_quantity.Equals(0) &&
                        sale_item_in_storage.sale_item_on_protocol_list_FK.Count.Equals(0)
                    )
                    {
                        continue;
                    }

                    var storage = sale_item_in_storage.storage_FK;
                                        
                    if (in_storage_latest_stock_state.in_storage_quantity > 0)
                    {
                        storage_list.Add(
                            new Sale_Item_Storage_List_Model
                            {
                                storage_FK = new Item_Storage_List_Model { id = storage.id, number = storage.number, name = storage.name },
                                in_storage_quantity = in_storage_latest_stock_state.in_storage_quantity,
                                blocked_quantity = in_storage_latest_stock_state.blocked_quantity
                            }
                        );
                    }


                    var item_on_protocol_list = sale_item_in_storage.sale_item_on_protocol_list_FK;

                    foreach (var item_on_protocol in item_on_protocol_list)
                    {
                        var protocol = item_on_protocol.sale_protocol_FK;
                        if (protocol == null || protocol.order_FK == null)
                        {
                            throw new Exception("19"); // object not found
                        }

                        movement_list.Add(new Sale_Item_Movement_Model
                        {
                            date = protocol.timestamp,
                            protocol_id = protocol.id,
                            protocol_number = protocol.full_number,
                            storage_id = storage.id,
                            storage_number = storage.number,
                            storage_name = storage.name,
                            order_id = protocol.order_FK.id,
                            total_quantity = item_on_protocol.total_quantity
                        });

                        var order_name_exists = encrypted_order_name.Where(c => c.id.Equals(protocol.order_FK.id)).FirstOrDefault();
                        if (order_name_exists == null)
                        {
                            encrypted_order_name.Add(new Encrypted_Object { id = protocol.order_FK.id, encryptedValue = protocol.order_FK.order_name });
                        }
                    }

                }

                List<Decrypted_Object> decrypted_order_name = await Crypto.DecryptList(session, encrypted_order_name);

                if (
                    decrypted_order_name == null || decrypted_order_name.Count != encrypted_order_name.Count
                )
                {
                    throw new Exception("3"); // decryption error
                }                

                
                foreach (var movement_record in movement_list)
                {
                    var order_name = decrypted_order_name.Where(o => o.id.Equals(movement_record.order_id)).FirstOrDefault();

                    if (order_name == null)
                    {
                        throw new Exception("3"); // decryption error
                    }

                    movement_record.order_name = order_name.decryptedValue;
                }
                
                await get_base_info;

                return_obj.storage_list = storage_list;
                return_obj.movement_list = movement_list;

                return return_obj;

            }

        }


        public async Task<List<Sale_Item_Model_List>> Get_All_Sale_Item(Session_Data session)
        {
            if(session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                List<Sale_Item> record_list = _context.Sale_Item.Where(si => si.deleted.Equals(false)).Include(si => si.sale_group_FK).Include(sg => sg.counting_unit_FK).ToList();

                List<Sale_Item_Model_List> return_list = new List<Sale_Item_Model_List>();

                if (record_list.Count == 0)
                {
                    return return_list;
                }

                List<Encrypted_Object> encrypted_comment = new List<Encrypted_Object>();

                foreach (var item in record_list)
                {
                    if (
                        item == null || 
                        item.sale_group_FK == null || 
                        item.counting_unit_FK == null
                    )
                    {
                        throw new Exception("19");// not found
                    }

                    var latest_stock_state = _context.Sale_Item_Stock_History.Where(ssh => ssh.sale_item_FKid.Equals(item.id)).MaxBy(ssh => ssh.timestamp);
                    if (latest_stock_state == null)
                    {
                        throw new Exception("19");//not found
                    }

                    Stock_State_Validator.Validate_Stock_State(latest_stock_state);

                    return_list.Add(
                        new Sale_Item_Model_List
                        {
                            id = item.id,
                            catalog_number = item.catalog_number,
                            product_name = item.product_name,
                            sale_group = item.sale_group_FK.group_name,
                            price = item.price,
                            weight_kg = item.weight_kg,
                            size_cm_x = item.size_cm_x,
                            size_cm_y = item.size_cm_y,
                            area_m2 = item.area_m2,

                            total_quantity = latest_stock_state.total_quantity,
                            in_storage_quantity = latest_stock_state.in_storage_quantity,
                            blocked_quantity = latest_stock_state.blocked_quantity,

                            counting_unit = item.counting_unit_FK.unit
                        }
                    );

                    encrypted_comment.Add(new Encrypted_Object { id = item.id, encryptedValue = item.comment });
                }

                List<Decrypted_Object> decrypted_comment = await Crypto.DecryptList(session, encrypted_comment);

                if (
                    decrypted_comment == null || decrypted_comment.Count != encrypted_comment.Count
                )
                {
                    throw new Exception("3");// decryption error
                }

                foreach (var item in return_list)
                {

                    var comment = decrypted_comment.Where(s => s.id.Equals(item.id)).FirstOrDefault();
                    if (comment != null)
                    {
                        item.comment = comment.decryptedValue;
                    }
                    else
                    {
                        throw new Exception("3");
                    }

                }

                return return_list;
            }
        }


        private decimal Currently_Used_Quantity(List<Sale_Item_In_Storage> sale_item_in_storage_list, Sale_Item_Stock_History latest_stock_state, ref Sale_Item_Error_Model error_object)
        {
            decimal currently_used_quantity = latest_stock_state.in_storage_quantity;

            if (sale_item_in_storage_list.Count > 0)
            {
                decimal currently_used_quantity_check = 0;

                foreach (var item_in_storage in sale_item_in_storage_list)
                {
                    if (item_in_storage == null)
                    {
                        throw new Exception("19");//not found
                    }

                    var current_stock_state = _context.Sale_Item_In_Storage_Stock_History.Where(ssh => ssh.sale_item_in_storage_FKid.Equals(item_in_storage.id)).MaxBy(ssh => ssh.timestamp);
                    if(current_stock_state == null)
                    {
                        throw new Exception("19");//not found
                    }

                    if(current_stock_state.blocked_quantity > current_stock_state.in_storage_quantity)
                    {
                        // checksum error
                        error_object.code = "36";
                        error_object.timestamp = current_stock_state.timestamp;
                        error_object.sale_item_id = latest_stock_state.sale_item_FKid;
                        error_object.sale_item_in_storage_id = item_in_storage.id;
                        error_object.required_quantity = current_stock_state.blocked_quantity - current_stock_state.in_storage_quantity;
                    }

                    currently_used_quantity_check += current_stock_state.in_storage_quantity;
                }

                if (currently_used_quantity != currently_used_quantity_check)
                {
                    // checksum error
                    error_object.code = "36";
                    error_object.timestamp = latest_stock_state.timestamp;
                    error_object.sale_item_id = latest_stock_state.sale_item_FKid;
                    error_object.sale_item_in_storage_id = null;
                    error_object.required_quantity = Math.Abs(currently_used_quantity - currently_used_quantity_check);
                }

            }

            return currently_used_quantity;
        }


    }
}
