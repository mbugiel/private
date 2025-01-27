using ManagemateAPI.Database.Context;
using ManagemateAPI.Database.Tables;
using ManagemateAPI.Encryption;
using ManagemateAPI.Encryption.Input_Objects;
using ManagemateAPI.Management.M_Lease_Item_In_Storage.Input_Objects;
using ManagemateAPI.Management.M_Lease_Item_In_Storage.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.Shared.Validator;
using Microsoft.EntityFrameworkCore;

namespace ManagemateAPI.Management.M_Lease_Item_In_Storage.Manager
{
    public class Lease_Item_In_Storage_Manager
    {
        private DB_Context _context;
        private readonly IConfiguration _configuration;

        public Lease_Item_In_Storage_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public Lease_Item_In_Storage_Error_Model Add_Lease_Item_In_Storage(Add_Lease_Item_In_Storage_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var lease_item = _context.Lease_Item.Where(l => 
                    l.id.Equals(input_obj.lease_item_FK) &&
                    l.deleted.Equals(false)
                ).FirstOrDefault();
                if(lease_item == null)
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

                var item_in_storage_exists = _context.Lease_Item_In_Storage.Where(l => 
                    l.storage_FKid.Equals(storage.id) &&
                    l.lease_item_FKid.Equals(lease_item.id)
                ).FirstOrDefault();

                var latest_stock_state = _context.Lease_Item_Stock_History.Where(ish => ish.lease_item_FKid.Equals(lease_item.id)).MaxBy(ish => ish.timestamp);
                if (latest_stock_state == null)
                {
                    throw new Exception("19");// not found
                }

                Stock_State_Validator.Validate_Stock_State(latest_stock_state);
                Timestamp_Validator.Validate_Input_Timestamp(latest_stock_state.timestamp, input_obj.timestamp);

                Lease_Item_In_Storage_Error_Model error_object = new Lease_Item_In_Storage_Error_Model();

                if (latest_stock_state.total_quantity - latest_stock_state.in_storage_quantity - latest_stock_state.out_of_storage_quantity < input_obj.total_quantity)
                {
                    // not enough items that are not in any storage
                    error_object.code = "44";
                    error_object.timestamp = latest_stock_state.timestamp;
                    error_object.lease_item_id = latest_stock_state.lease_item_FKid;
                    error_object.lease_item_in_storage_id = null;
                    error_object.required_quantity = input_obj.total_quantity - (latest_stock_state.total_quantity - latest_stock_state.in_storage_quantity - latest_stock_state.out_of_storage_quantity);

                    return error_object;
                }


                Lease_Item_Stock_History stock_update = new Lease_Item_Stock_History
                {
                    lease_item_FKid = lease_item.id,

                    total_quantity = latest_stock_state.total_quantity,
                    in_storage_quantity = latest_stock_state.in_storage_quantity + input_obj.total_quantity,
                    out_of_storage_quantity = latest_stock_state.out_of_storage_quantity,
                    blocked_quantity = latest_stock_state.blocked_quantity,

                    timestamp = input_obj.timestamp
                };

                _context.Lease_Item_Stock_History.Add(stock_update);


                if (item_in_storage_exists != null)
                {
                    var in_storage_latest_stock_state = _context.Lease_Item_In_Storage_Stock_History.Where(lsh => lsh.lease_item_in_storage_FKid.Equals(item_in_storage_exists.id)).MaxBy(lsh => lsh.timestamp);
                    if(in_storage_latest_stock_state == null)
                    {
                        throw new Exception("19");// not found
                    }

                    Stock_State_Validator.Validate_Stock_State(in_storage_latest_stock_state);
                    Timestamp_Validator.Validate_Input_Timestamp(in_storage_latest_stock_state.timestamp, input_obj.timestamp);

                    Lease_Item_In_Storage_Stock_History in_storage_stock_update = new Lease_Item_In_Storage_Stock_History
                    {
                        lease_item_in_storage_FKid = item_in_storage_exists.id,

                        total_quantity = in_storage_latest_stock_state.total_quantity + input_obj.total_quantity,
                        in_storage_quantity = in_storage_latest_stock_state.in_storage_quantity + input_obj.total_quantity,
                        blocked_quantity = in_storage_latest_stock_state.blocked_quantity,
                        out_of_storage_quantity = in_storage_latest_stock_state.out_of_storage_quantity,

                        timestamp = input_obj.timestamp
                    };

                    _context.Lease_Item_In_Storage_Stock_History.Add(in_storage_stock_update);

                    _context.SaveChanges();
                }
                else
                {
                    Lease_Item_In_Storage new_lease_item_in_storage = new Lease_Item_In_Storage
                    {
                        storage_FKid = storage.id,
                        lease_item_FKid = lease_item.id
                    };

                    _context.Lease_Item_In_Storage.Add(new_lease_item_in_storage);
                    _context.SaveChanges();

                    Lease_Item_In_Storage_Stock_History in_storage_stock_update = new Lease_Item_In_Storage_Stock_History
                    {
                        lease_item_in_storage_FKid = new_lease_item_in_storage.id,

                        total_quantity = input_obj.total_quantity,
                        in_storage_quantity = input_obj.total_quantity,
                        blocked_quantity = 0,
                        out_of_storage_quantity = 0,

                        timestamp = input_obj.timestamp
                    };

                    _context.Lease_Item_In_Storage_Stock_History.Add(in_storage_stock_update);
                    _context.SaveChanges();
                }

                return error_object;
            }

        }


        public Lease_Item_In_Storage_Error_Model Remove_Lease_Item_In_Storage(Remove_Lease_Item_In_Storage_Data input_obj, Session_Data session)
        {

            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var item_in_storage_exists = _context.Lease_Item_In_Storage.Where(l =>
                    l.id.Equals(input_obj.id)
                ).Include(l => l.storage_FK)
                .Include(l => l.lease_item_FK)
                .FirstOrDefault();
                if (
                    item_in_storage_exists == null ||
                    item_in_storage_exists.storage_FK == null ||
                    item_in_storage_exists.lease_item_FK == null
                )
                {
                    throw new Exception("19");// not found
                }

                //get latest stock data about lease item
                var latest_stock_state = _context.Lease_Item_Stock_History.Where(ish => ish.lease_item_FKid.Equals(item_in_storage_exists.lease_item_FK.id)).MaxBy(ish => ish.timestamp);
                if (latest_stock_state == null)
                {
                    throw new Exception("19");// not found
                }

                Stock_State_Validator.Validate_Stock_State(latest_stock_state);
                Timestamp_Validator.Validate_Input_Timestamp(latest_stock_state.timestamp, input_obj.timestamp);

                Lease_Item_In_Storage_Error_Model error_object = new Lease_Item_In_Storage_Error_Model();

                if (latest_stock_state.in_storage_quantity - latest_stock_state.blocked_quantity < input_obj.total_quantity)
                {
                    // not enough items in storages
                    error_object.code = "22";
                    error_object.timestamp = latest_stock_state.timestamp;
                    error_object.lease_item_id = latest_stock_state.lease_item_FKid;
                    error_object.lease_item_in_storage_id = null;
                    error_object.required_quantity = input_obj.total_quantity - (latest_stock_state.in_storage_quantity - latest_stock_state.blocked_quantity);

                    return error_object;
                }

                //get latest stock data about lease item in storage
                var in_storage_latest_stock_state = _context.Lease_Item_In_Storage_Stock_History.Where(lsh => lsh.lease_item_in_storage_FKid.Equals(item_in_storage_exists.id)).MaxBy(lsh => lsh.timestamp);
                if (in_storage_latest_stock_state == null)
                {
                    throw new Exception("19");// not found
                }

                Stock_State_Validator.Validate_Stock_State(in_storage_latest_stock_state);
                Timestamp_Validator.Validate_Input_Timestamp(in_storage_latest_stock_state.timestamp, input_obj.timestamp);

                if (in_storage_latest_stock_state.in_storage_quantity - in_storage_latest_stock_state.blocked_quantity < input_obj.total_quantity)
                {
                    // not enough items in storage
                    error_object.code = "22";
                    error_object.timestamp = in_storage_latest_stock_state.timestamp;
                    error_object.lease_item_id = latest_stock_state.lease_item_FKid;
                    error_object.lease_item_in_storage_id = in_storage_latest_stock_state.lease_item_in_storage_FKid;
                    error_object.required_quantity = input_obj.total_quantity - (in_storage_latest_stock_state.in_storage_quantity - in_storage_latest_stock_state.blocked_quantity);

                    return error_object;
                }

                Lease_Item_Stock_History stock_update = new Lease_Item_Stock_History
                {
                    lease_item_FKid = item_in_storage_exists.lease_item_FK.id,

                    total_quantity = latest_stock_state.total_quantity,
                    in_storage_quantity = latest_stock_state.in_storage_quantity - input_obj.total_quantity,
                    blocked_quantity = latest_stock_state.blocked_quantity,
                    out_of_storage_quantity = latest_stock_state.out_of_storage_quantity,

                    timestamp = input_obj.timestamp
                };

                Lease_Item_In_Storage_Stock_History in_storage_stock_update = new Lease_Item_In_Storage_Stock_History
                {
                    lease_item_in_storage_FKid = item_in_storage_exists.id,

                    total_quantity = in_storage_latest_stock_state.total_quantity - input_obj.total_quantity,
                    in_storage_quantity = in_storage_latest_stock_state.in_storage_quantity - input_obj.total_quantity,
                    blocked_quantity = in_storage_latest_stock_state.blocked_quantity,
                    out_of_storage_quantity = in_storage_latest_stock_state.out_of_storage_quantity,

                    timestamp = input_obj.timestamp
                };

                _context.Lease_Item_Stock_History.Add(stock_update);
                _context.Lease_Item_In_Storage_Stock_History.Add(in_storage_stock_update);

                _context.SaveChanges();


                return error_object;
            }

        }


        public Lease_Item_In_Storage_Model Get_Lease_Item_In_Storage_By_Id(Get_Lease_Item_In_Storage_By_Id_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var item_in_storage_exists = _context.Lease_Item_In_Storage.Where(l =>
                    l.id.Equals(input_obj.id_to_get)
                )
                .Include(l => l.lease_item_FK).ThenInclude(li => li.counting_unit_FK)
                .Include(l => l.storage_FK)
                .FirstOrDefault();
                if (
                    item_in_storage_exists == null ||
                    item_in_storage_exists.storage_FK == null ||
                    item_in_storage_exists.lease_item_FK == null ||
                    item_in_storage_exists.lease_item_FK.counting_unit_FK == null
                )
                {
                    throw new Exception("19");// not found
                }

                //get latest stock data about lease item
                var latest_stock_state = _context.Lease_Item_Stock_History.Where(ish => ish.lease_item_FKid.Equals(item_in_storage_exists.lease_item_FK.id)).MaxBy(ish => ish.timestamp);
                if (latest_stock_state == null)
                {
                    throw new Exception("19");// not found
                }

                Stock_State_Validator.Validate_Stock_State(latest_stock_state);

                //get latest stock data about lease item in storage
                var in_storage_latest_stock_state = _context.Lease_Item_In_Storage_Stock_History.Where(lsh => lsh.lease_item_in_storage_FKid.Equals(item_in_storage_exists.id)).MaxBy(lsh => lsh.timestamp);
                if (in_storage_latest_stock_state == null)
                {
                    throw new Exception("19");// not found
                }

                Stock_State_Validator.Validate_Stock_State(in_storage_latest_stock_state);


                Lease_Item_In_Storage_Model return_obj = new Lease_Item_In_Storage_Model
                {
                    id = item_in_storage_exists.id,
                    lease_item_number = item_in_storage_exists.lease_item_FK.catalog_number,
                    lease_item_name = item_in_storage_exists.lease_item_FK.product_name,
                    storage_number = item_in_storage_exists.storage_FK.number,
                    counting_unit = item_in_storage_exists.lease_item_FK.counting_unit_FK.unit,

                    total_quantity = in_storage_latest_stock_state.total_quantity,
                    in_storage_quantity = in_storage_latest_stock_state.in_storage_quantity,
                    out_of_storage_quantity = in_storage_latest_stock_state.out_of_storage_quantity,
                    blocked_quantity = in_storage_latest_stock_state.blocked_quantity,
                    storage_name = item_in_storage_exists.storage_FK.name
                };
                

                return return_obj;
            }
        }


        public List<Lease_Item_In_Storage_Model_List> Get_All_Lease_Item_In_Storage(Get_All_Lease_Item_In_Storage_Data input_obj, Session_Data session)
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

                var item_in_storage_list = _context.Lease_Item_In_Storage.Where(l =>
                    l.storage_FKid.Equals(input_obj.storage_id)
                )
                .Include(l => l.lease_item_FK).ThenInclude(li => li.counting_unit_FK)
                .ToList();

                List<Lease_Item_In_Storage_Model_List> return_obj = new List<Lease_Item_In_Storage_Model_List>();

                if(item_in_storage_list.Count == 0)
                {
                    return return_obj;
                }

                foreach (var item_in_storage in item_in_storage_list) 
                {
                    if(
                        item_in_storage == null || 
                        item_in_storage.lease_item_FK == null || 
                        item_in_storage.lease_item_FK.counting_unit_FK == null
                    )
                    {
                        throw new Exception("19");// not Found
                    }

                    //get latest stock data about lease item
                    var latest_stock_state = _context.Lease_Item_Stock_History.Where(ish => ish.lease_item_FKid.Equals(item_in_storage.lease_item_FK.id)).MaxBy(ish => ish.timestamp);
                    if (latest_stock_state == null)
                    {
                        throw new Exception("19");// not found
                    }

                    Stock_State_Validator.Validate_Stock_State(latest_stock_state);

                    //get latest stock data about lease item in storage
                    var in_storage_latest_stock_state = _context.Lease_Item_In_Storage_Stock_History.Where(lsh => lsh.lease_item_in_storage_FKid.Equals(item_in_storage.id)).MaxBy(lsh => lsh.timestamp);
                    if (in_storage_latest_stock_state == null)
                    {
                        throw new Exception("19");// not found
                    }

                    Stock_State_Validator.Validate_Stock_State(in_storage_latest_stock_state);

                    if (in_storage_latest_stock_state.total_quantity.Equals(0))
                    {
                        continue;
                    }

                    return_obj.Add(new Lease_Item_In_Storage_Model_List
                    {
                        id = item_in_storage.id,
                        lease_item_number = item_in_storage.lease_item_FK.catalog_number,
                        lease_item_name = item_in_storage.lease_item_FK.product_name,

                        total_quantity = in_storage_latest_stock_state.total_quantity,
                        in_storage_quantity = in_storage_latest_stock_state.in_storage_quantity,
                        out_of_storage_quantity = in_storage_latest_stock_state.out_of_storage_quantity,
                        blocked_quantity = in_storage_latest_stock_state.blocked_quantity,

                        counting_unit = item_in_storage.lease_item_FK.counting_unit_FK.unit
                    });

                }               


                return return_obj;
            }

        }
        

    }
}
