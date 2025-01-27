using ManagemateAPI.Database.Context;
using ManagemateAPI.Database.Tables;
using ManagemateAPI.Management.M_Sale_Item_In_Storage.Input_Objects;
using ManagemateAPI.Management.M_Session.Input_Objects;
using Microsoft.EntityFrameworkCore;
using ManagemateAPI.Management.M_Sale_Item_In_Storage.Table_Model;
using ManagemateAPI.Management.Shared.Validator;

namespace ManagemateAPI.Management.M_Sale_Item_In_Storage.Manager
{
    public class Sale_Item_In_Storage_Manager
    {
        private DB_Context _context;
        private readonly IConfiguration _configuration;

        public Sale_Item_In_Storage_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public Sale_Item_In_Storage_Error_Model Add_Sale_Item_In_Storage(Add_Sale_Item_In_Storage_Data input_obj, Session_Data session)
        {

            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();


                var sale_item = _context.Sale_Item.Where(s => 
                    s.id.Equals(input_obj.sale_item_FK) &&
                    s.deleted.Equals(false)
                ).FirstOrDefault();
                if (sale_item == null)
                {
                    throw new Exception("19");// not found
                }

                var storage = _context.Storage.Where(s => 
                    s.id.Equals(input_obj.storage_FK) &&
                    s.deleted.Equals(false)
                ).FirstOrDefault();
                if (storage == null)
                {
                    throw new Exception("19");// not found
                }

                var item_in_storage_exists = _context.Sale_Item_In_Storage.Where(s =>
                    s.storage_FKid.Equals(storage.id) &&
                    s.sale_item_FKid.Equals(sale_item.id)
                ).FirstOrDefault();

                var latest_stock_state = _context.Sale_Item_Stock_History.Where(ssh => ssh.sale_item_FKid.Equals(sale_item.id)).MaxBy(ssh => ssh.timestamp);
                if(latest_stock_state == null)
                {
                    throw new Exception("19");//not found
                }

                Stock_State_Validator.Validate_Stock_State(latest_stock_state);
                Timestamp_Validator.Validate_Input_Timestamp(latest_stock_state.timestamp, input_obj.timestamp);

                Sale_Item_In_Storage_Error_Model error_object = new Sale_Item_In_Storage_Error_Model();

                if (latest_stock_state.total_quantity - latest_stock_state.in_storage_quantity < input_obj.total_quantity)
                {
                    // not enough items that are not in any storage
                    error_object.code = "44";
                    error_object.timestamp = latest_stock_state.timestamp;
                    error_object.sale_item_id = latest_stock_state.sale_item_FKid;
                    error_object.sale_item_in_storage_id = null;
                    error_object.required_quantity = input_obj.total_quantity - (latest_stock_state.total_quantity - latest_stock_state.in_storage_quantity);

                    return error_object;
                }

                Sale_Item_Stock_History stock_update = new Sale_Item_Stock_History
                {
                    sale_item_FKid = sale_item.id,
                    total_quantity = latest_stock_state.total_quantity,
                    in_storage_quantity = latest_stock_state.in_storage_quantity + input_obj.total_quantity,
                    blocked_quantity = latest_stock_state.blocked_quantity,
                    timestamp = input_obj.timestamp
                };

                _context.Sale_Item_Stock_History.Add(stock_update);

                if (item_in_storage_exists != null)
                {
                    var in_storage_latest_stock_state = _context.Sale_Item_In_Storage_Stock_History.Where(ssh => 
                        ssh.sale_item_in_storage_FKid.Equals(item_in_storage_exists.id)
                    ).MaxBy(ssh => ssh.timestamp);
                    if(in_storage_latest_stock_state == null)
                    {
                        throw new Exception("19");//not found
                    }

                    Stock_State_Validator.Validate_Stock_State(in_storage_latest_stock_state);
                    Timestamp_Validator.Validate_Input_Timestamp(in_storage_latest_stock_state.timestamp, input_obj.timestamp);

                    Sale_Item_In_Storage_Stock_History in_storage_stock_update = new Sale_Item_In_Storage_Stock_History
                    {
                        sale_item_in_storage_FKid = item_in_storage_exists.id,
                        in_storage_quantity = in_storage_latest_stock_state.in_storage_quantity + input_obj.total_quantity,
                        blocked_quantity = in_storage_latest_stock_state.blocked_quantity,
                        timestamp = input_obj.timestamp
                    };

                    _context.Sale_Item_In_Storage_Stock_History.Add(in_storage_stock_update);

                    _context.SaveChanges();
                }
                else
                {
                    Sale_Item_In_Storage new_sale_item_in_storage = new Sale_Item_In_Storage
                    {
                        storage_FKid = storage.id,
                        sale_item_FKid = sale_item.id
                    };

                    _context.Sale_Item_In_Storage.Add(new_sale_item_in_storage);
                    _context.SaveChanges();

                    Sale_Item_In_Storage_Stock_History in_storage_stock_update = new Sale_Item_In_Storage_Stock_History
                    {
                        sale_item_in_storage_FKid = new_sale_item_in_storage.id,
                        in_storage_quantity = input_obj.total_quantity,
                        blocked_quantity = 0,
                        timestamp = input_obj.timestamp
                    };

                    _context.Sale_Item_In_Storage_Stock_History.Add(in_storage_stock_update);
                    _context.SaveChanges();
                }

                return error_object;
            }

        }


        public Sale_Item_In_Storage_Error_Model Remove_Sale_Item_In_Storage(Remove_Sale_Item_In_Storage_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var item_in_storage_exists = _context.Sale_Item_In_Storage.Where(s =>
                    s.id.Equals(input_obj.id)
                )
                .Include(s => s.sale_item_FK)
                .Include(s => s.storage_FK)
                .FirstOrDefault();
                if (
                    item_in_storage_exists == null ||
                    item_in_storage_exists.sale_item_FK == null ||
                    item_in_storage_exists.storage_FK == null
                )
                {
                    throw new Exception("19");// not found
                }

                var latest_stock_state = _context.Sale_Item_Stock_History.Where(ssh => 
                    ssh.sale_item_FKid.Equals(item_in_storage_exists.sale_item_FK.id)
                ).MaxBy(ssh => ssh.timestamp);
                if (latest_stock_state == null)
                {
                    throw new Exception("19");//not found
                }

                Stock_State_Validator.Validate_Stock_State(latest_stock_state);
                Timestamp_Validator.Validate_Input_Timestamp(latest_stock_state.timestamp, input_obj.timestamp);

                Sale_Item_In_Storage_Error_Model error_object = new Sale_Item_In_Storage_Error_Model();

                if (latest_stock_state.in_storage_quantity - latest_stock_state.blocked_quantity < input_obj.total_quantity)
                {
                    // not enough items in storages
                    error_object.code = "22";
                    error_object.timestamp = latest_stock_state.timestamp;
                    error_object.sale_item_id = latest_stock_state.sale_item_FKid;
                    error_object.sale_item_in_storage_id = null;
                    error_object.required_quantity = input_obj.total_quantity - (latest_stock_state.in_storage_quantity - latest_stock_state.blocked_quantity);

                    return error_object;
                }

                var in_storage_latest_stock_state = _context.Sale_Item_In_Storage_Stock_History.Where(ssh =>
                        ssh.sale_item_in_storage_FKid.Equals(item_in_storage_exists.id)
                    ).MaxBy(ssh => ssh.timestamp);
                if (in_storage_latest_stock_state == null)
                {
                    throw new Exception("19");//not found
                }

                Stock_State_Validator.Validate_Stock_State(in_storage_latest_stock_state);
                Timestamp_Validator.Validate_Input_Timestamp(in_storage_latest_stock_state.timestamp, input_obj.timestamp);

                if (in_storage_latest_stock_state.in_storage_quantity - in_storage_latest_stock_state.blocked_quantity < input_obj.total_quantity)
                {
                    // not enough items in storage
                    error_object.code = "22";
                    error_object.timestamp = in_storage_latest_stock_state.timestamp;
                    error_object.sale_item_id = latest_stock_state.sale_item_FKid;
                    error_object.sale_item_in_storage_id = in_storage_latest_stock_state.sale_item_in_storage_FKid;
                    error_object.required_quantity = input_obj.total_quantity - (in_storage_latest_stock_state.in_storage_quantity - in_storage_latest_stock_state.blocked_quantity);

                    return error_object;
                }

                Sale_Item_Stock_History stock_update = new Sale_Item_Stock_History
                {
                    sale_item_FKid = item_in_storage_exists.sale_item_FK.id,
                    total_quantity = latest_stock_state.total_quantity,
                    in_storage_quantity = latest_stock_state.in_storage_quantity - input_obj.total_quantity,
                    blocked_quantity = latest_stock_state.blocked_quantity,
                    timestamp = input_obj.timestamp
                };

                Sale_Item_In_Storage_Stock_History in_storage_stock_update = new Sale_Item_In_Storage_Stock_History
                {
                    sale_item_in_storage_FKid = item_in_storage_exists.id,
                    in_storage_quantity = in_storage_latest_stock_state.in_storage_quantity - input_obj.total_quantity,
                    blocked_quantity = in_storage_latest_stock_state.blocked_quantity,
                    timestamp = input_obj.timestamp
                };

                _context.Sale_Item_Stock_History.Add(stock_update);
                _context.Sale_Item_In_Storage_Stock_History.Add(in_storage_stock_update);

                _context.SaveChanges();

                return error_object;
            }

        }


        public Sale_Item_In_Storage_Model Get_Sale_Item_In_Storage_By_Id(Get_Sale_Item_In_Storage_By_Id_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var item_in_storage_exists = _context.Sale_Item_In_Storage.Where(s =>
                    s.id.Equals(input_obj.id_to_get)
                )
                .Include(s => s.sale_item_FK).ThenInclude(li => li.counting_unit_FK)
                .Include(s => s.storage_FK)
                .FirstOrDefault();
                if (
                    item_in_storage_exists == null ||
                    item_in_storage_exists.storage_FK == null ||
                    item_in_storage_exists.sale_item_FK == null ||
                    item_in_storage_exists.sale_item_FK.counting_unit_FK == null
                )
                {
                    throw new Exception("19");// not found
                }

                var latest_stock_state = _context.Sale_Item_Stock_History.Where(ssh =>
                    ssh.sale_item_FKid.Equals(item_in_storage_exists.sale_item_FK.id)
                ).MaxBy(ssh => ssh.timestamp);
                if (latest_stock_state == null)
                {
                    throw new Exception("19");//not found
                }

                Stock_State_Validator.Validate_Stock_State(latest_stock_state);

                var in_storage_latest_stock_state = _context.Sale_Item_In_Storage_Stock_History.Where(ssh =>
                       ssh.sale_item_in_storage_FKid.Equals(item_in_storage_exists.id)
                   ).MaxBy(ssh => ssh.timestamp);
                if (in_storage_latest_stock_state == null)
                {
                    throw new Exception("19");//not found
                }

                Stock_State_Validator.Validate_Stock_State(in_storage_latest_stock_state);


                Sale_Item_In_Storage_Model return_obj = new Sale_Item_In_Storage_Model
                {
                    id = item_in_storage_exists.id,
                    sale_item_number = item_in_storage_exists.sale_item_FK.catalog_number,
                    sale_item_name = item_in_storage_exists.sale_item_FK.product_name,
                    storage_number = item_in_storage_exists.storage_FK.number,
                    counting_unit = item_in_storage_exists.sale_item_FK.counting_unit_FK.unit,

                    storage_name = item_in_storage_exists.storage_FK.name,
                    in_storage_quantity = in_storage_latest_stock_state.in_storage_quantity,
                    blocked_quantity = in_storage_latest_stock_state.blocked_quantity
                };
                

                return return_obj;
            }
        }


        public List<Sale_Item_In_Storage_Model_List> Get_All_Sale_Item_In_Storage(Get_All_Sale_Item_In_Storage_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var storage = _context.Storage.Where(s => s.id.Equals(input_obj.storage_id) && s.deleted.Equals(false)).FirstOrDefault();
                if (storage == null)
                {
                    throw new Exception("19");// not Found
                }

                var item_in_storage_list = _context.Sale_Item_In_Storage.Where(s =>
                    s.storage_FKid.Equals(input_obj.storage_id)
                )
                .Include(s => s.sale_item_FK).ThenInclude(si => si.counting_unit_FK)
                .ToList();

                List<Sale_Item_In_Storage_Model_List> return_obj = new List<Sale_Item_In_Storage_Model_List>();

                if (item_in_storage_list.Count == 0)
                {
                    return return_obj;
                }


                foreach (var item_in_storage in item_in_storage_list)
                {
                    if (
                        item_in_storage == null ||
                        item_in_storage.sale_item_FK == null || 
                        item_in_storage.sale_item_FK.counting_unit_FK == null
                    )
                    {
                        throw new Exception("19");// not Found
                    }

                    var latest_stock_state = _context.Sale_Item_Stock_History.Where(ssh =>
                        ssh.sale_item_FKid.Equals(item_in_storage.sale_item_FK.id)
                    ).MaxBy(ssh => ssh.timestamp);
                    if (latest_stock_state == null)
                    {
                        throw new Exception("19");//not found
                    }

                    Stock_State_Validator.Validate_Stock_State(latest_stock_state);

                    var in_storage_latest_stock_state = _context.Sale_Item_In_Storage_Stock_History.Where(ssh =>
                        ssh.sale_item_in_storage_FKid.Equals(item_in_storage.id)
                    ).MaxBy(ssh => ssh.timestamp);
                    if (in_storage_latest_stock_state == null)
                    {
                        throw new Exception("19");//not found
                    }

                    Stock_State_Validator.Validate_Stock_State(in_storage_latest_stock_state);


                    return_obj.Add(new Sale_Item_In_Storage_Model_List
                    {
                        id = item_in_storage.id,
                        sale_item_number = item_in_storage.sale_item_FK.catalog_number,
                        sale_item_name = item_in_storage.sale_item_FK.product_name,

                        in_storage_quantity = in_storage_latest_stock_state.in_storage_quantity,
                        blocked_quantity = in_storage_latest_stock_state.blocked_quantity,

                        counting_unit = item_in_storage.sale_item_FK.counting_unit_FK.unit
                    });

                }
                

                return return_obj;
            }

        }


    }
}
