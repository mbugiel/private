using ManagemateAPI.Database.Context;
using ManagemateAPI.Database.Tables;
using ManagemateAPI.Encryption;
using ManagemateAPI.Encryption.Input_Objects;
using ManagemateAPI.Management.M_Client.Table_Model;
using ManagemateAPI.Management.M_Company.Table_Model;
using ManagemateAPI.Management.M_Construction_Site.Table_Model;
using ManagemateAPI.Management.M_Lease_Item_On_Protocol.Table_Model;
using ManagemateAPI.Management.M_Lease_Protocol.Input_Objects;
using ManagemateAPI.Management.M_Lease_Protocol.Table_Model;
using ManagemateAPI.Management.M_Service_On_Lease_Protocol.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.Shared.Enum;
using ManagemateAPI.Management.Shared.Json_Model;
using ManagemateAPI.Management.Shared.Static;
using ManagemateAPI.Management.Shared.Validator;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using PuppeteerSharp.Media;


/*
 * This is the Order_Protocol_Manager with methods dedicated to the Order_Protocol table.
 * 
 * It contains methods to:
 * add
 * edit
 * delete
 * get by id
 * 
 */
namespace ManagemateAPI.Management.M_Lease_Protocol.Manager
{
    public class Lease_Protocol_Manager
    {

        private DB_Context _context;
        private readonly IConfiguration _configuration;


        public Lease_Protocol_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public Lease_Protocol_Model_Id Add_Lease_Offer_Protocol(Add_Lease_Offer_Protocol_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var order = _context.Order.Where(o =>
                    o.id.Equals(input_obj.order_FK) &&
                    o.deleted.Equals(false)
                )
                .Include(o => o.lease_protocol_list_FK)
                .Include(o => o.lease_to_sale_protocol_list_FK)
                .Include(o => o.sale_protocol_list_FK)
                .FirstOrDefault();
                if (order == null || order.lease_protocol_list_FK == null || order.lease_to_sale_protocol_list_FK == null || order.sale_protocol_list_FK == null)
                {
                    throw new Exception("19");// object not found
                }

                Lease_Protocol_Model_Id return_obj = new Lease_Protocol_Model_Id { latest_protocol_timestamp = input_obj.user_current_timestamp.AddDays(-1) };

                var latest_lease_protocol = order.lease_protocol_list_FK.MaxBy(l => l.timestamp);
                var latest_lease_to_sale_protocol = order.lease_to_sale_protocol_list_FK.MaxBy(l => l.timestamp);
                var latest_sale_protocol = order.sale_protocol_list_FK.MaxBy(l => l.timestamp);
                if (latest_lease_protocol != null)
                {
                    if (input_obj.user_current_timestamp <= latest_lease_protocol.timestamp)
                    {
                        return_obj.latest_protocol_timestamp = latest_lease_protocol.timestamp;
                    }
                }
                if (latest_lease_to_sale_protocol != null)
                {
                    if (input_obj.user_current_timestamp <= latest_lease_to_sale_protocol.timestamp)
                    {
                        return_obj.latest_protocol_timestamp = latest_lease_to_sale_protocol.timestamp;
                    }
                }
                if (latest_sale_protocol != null)
                {
                    if (input_obj.user_current_timestamp <= latest_sale_protocol.timestamp)
                    {
                        return_obj.latest_protocol_timestamp = latest_sale_protocol.timestamp;
                    }
                }

                var settings = _context.Company_Invoice_Settings.FirstOrDefault();
                if (settings == null)
                {
                    throw new Exception("51");// settings not found
                }

                Lease_Protocol new_offer = new Lease_Protocol
                {
                    number = Get_Latest_Offer_Number(input_obj.user_current_timestamp.Year) + 1,
                    year = input_obj.user_current_timestamp.Year,
                    prefix = "O" + settings.lease_release_protocol_prefix,
                    type = Lease_Protocol_Type.Release,
                    timestamp = input_obj.user_current_timestamp,
                    order_FKid = order.id,
                    state = Protocol_State.Offer,
                    element = Array.Empty<byte>(),
                    transport = Array.Empty<byte>(),
                    comment = Array.Empty<byte>(),
                    total_weight_kg = 0,
                    total_area_m2 = 0,
                    total_worth = 0,
                    deleted = false
                };

                new_offer.full_number = new_offer.prefix + "-" + new_offer.number.ToString("D5") + "/" + new_offer.year.ToString();


                _context.Lease_Protocol.Add(new_offer);
                _context.SaveChanges();

                return_obj.protocol_id = new_offer.id;

                return return_obj;
            }

        }


        public Lease_Offer_Protocol_Model_Id Edit_Lease_Offer_Protocol(Edit_Lease_Protocol_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                Timestamp_Validator.Validate_New_Timestamp(input_obj.timestamp, input_obj.user_current_timestamp);

                if (!Enum.IsDefined(typeof(Protocol_State), input_obj.state))
                {
                    throw new Exception("19");// given state is not defined
                }

                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var editing_lease_protocol = _context.Lease_Protocol.Where(lp =>
                    lp.id.Equals(input_obj.id) &&
                    lp.state.Equals(Protocol_State.Offer) &&
                    lp.deleted.Equals(false)
                )
                .Include(lp => lp.lease_item_on_protocol_list_FK)
                    .ThenInclude(li => li.lease_item_in_storage_FK)
                .FirstOrDefault();

                if (
                    editing_lease_protocol == null ||
                    editing_lease_protocol.lease_item_on_protocol_list_FK == null
                )
                {
                    throw new Exception("19");//protocol not found
                }


                if (
                    editing_lease_protocol.type.Equals(Lease_Protocol_Type.Return)
                )
                {
                    throw new Exception("34");//return protocol can not be in reservation or offer state
                }


                Lease_Offer_Protocol_Model_Id return_obj;

                if (!input_obj.state.Equals(Protocol_State.Offer))
                {
                    return_obj = Create_Lease_Protocol_From_Offer(ref editing_lease_protocol, input_obj.timestamp);
                
                    if (return_obj.error_list.Count > 0)
                    {
                        return return_obj;
                    }
                }
                else
                {
                    return_obj = new Lease_Offer_Protocol_Model_Id
                    {
                        protocol_id = -1,
                        error_list = []
                    };
                }

                editing_lease_protocol.timestamp = input_obj.timestamp;

                _context.SaveChanges();

                return return_obj;
            }

        }

        private Lease_Offer_Protocol_Model_Id Create_Lease_Protocol_From_Offer(ref Lease_Protocol editing_lease_protocol, DateTime new_timestamp)
        {
            if (editing_lease_protocol.lease_item_on_protocol_list_FK.Count.Equals(0))
            {
                throw new Exception("13");//no items on offer protocol
            }

            Lease_Offer_Protocol_Model_Id return_obj = new Lease_Offer_Protocol_Model_Id
            {
                protocol_id = -1,
                error_list = []
            };

            List<Lease_Item_Stock_History> stock_history;
            List<Lease_Item_In_Storage_Stock_History> in_storage_stock_history;

            List<Lease_Item_In_Storage_Stock_History> in_storage_stock_history_operational;

            Lease_Item_Stock_History? reference_stock_state;
            Lease_Item_In_Storage_Stock_History? in_storage_reference_stock_state;

            Lease_Item_In_Storage_Stock_History? min_stock_state;



            List<Lease_Item_On_Protocol> lease_item_on_protocol_list = new List<Lease_Item_On_Protocol>();
            List<Lease_Item_Stock_History> lease_item_stock_history_list = new List<Lease_Item_Stock_History>();
            List<Lease_Item_In_Storage_Stock_History> lease_item_in_storage_stock_history_list = new List<Lease_Item_In_Storage_Stock_History>();

            foreach (var lease_item_on_offer in editing_lease_protocol.lease_item_on_protocol_list_FK)
            {

                stock_history = _context.Lease_Item_Stock_History.Where(lsh =>
                    lsh.lease_item_FKid.Equals(lease_item_on_offer.lease_item_in_storage_FK.lease_item_FKid)
                ).ToList();

                in_storage_stock_history = _context.Lease_Item_In_Storage_Stock_History.Where(ilsh =>
                    ilsh.lease_item_in_storage_FKid.Equals(lease_item_on_offer.lease_item_in_storage_FKid)
                ).ToList();

                Timestamp_Validator.Validate_Protocol_Timestamp(stock_history, new_timestamp);
                Timestamp_Validator.Validate_Protocol_Timestamp(in_storage_stock_history, new_timestamp);


                reference_stock_state = stock_history.Where(sh => sh.timestamp < new_timestamp).MaxBy(sh => sh.timestamp);
                if (reference_stock_state == null)
                {
                    throw new Exception("19");//not found
                }

                //latest before timestamp in storage stock state
                in_storage_reference_stock_state = in_storage_stock_history.Where(ish => ish.timestamp < new_timestamp).MaxBy(ish => ish.timestamp);
                if (in_storage_reference_stock_state == null)
                {
                    throw new Exception("19");//not found
                }

                Stock_State_Validator.Validate_Stock_State(reference_stock_state);
                Stock_State_Validator.Validate_Stock_State(in_storage_reference_stock_state);

                in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp).ToList();

                if (reference_stock_state.in_storage_quantity - reference_stock_state.blocked_quantity < lease_item_on_offer.total_quantity)
                {
                    //not enough in stock
                    return_obj.error_list.Add(
                        new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "22",
                            timestamp = reference_stock_state.timestamp,
                            lease_item_in_storage_id = lease_item_on_offer.lease_item_in_storage_FKid,
                            required_quantity = lease_item_on_offer.total_quantity - (reference_stock_state.in_storage_quantity - reference_stock_state.blocked_quantity)
                        }
                    );
                    continue;
                }

                if (in_storage_reference_stock_state.in_storage_quantity - in_storage_reference_stock_state.blocked_quantity < lease_item_on_offer.total_quantity)
                {
                    //not enough in stock
                    return_obj.error_list.Add(
                        new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "22",
                            timestamp = in_storage_reference_stock_state.timestamp,
                            lease_item_in_storage_id = lease_item_on_offer.lease_item_in_storage_FKid,
                            required_quantity = lease_item_on_offer.total_quantity - (in_storage_reference_stock_state.in_storage_quantity - in_storage_reference_stock_state.blocked_quantity)
                        }
                    );
                    continue;
                }

                min_stock_state = in_storage_stock_history_operational.MinBy(ishat => ishat.in_storage_quantity - ishat.blocked_quantity);

                if (min_stock_state != null)
                {                    
                    if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < lease_item_on_offer.total_quantity)
                    {
                        // not enough in storage
                        return_obj.error_list.Add(
                            new Lease_Item_On_Protocol_Error_Model
                            {
                                code = "22",
                                timestamp = min_stock_state.timestamp,
                                lease_item_in_storage_id = lease_item_on_offer.lease_item_in_storage_FKid,
                                required_quantity = lease_item_on_offer.total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                            }
                        );
                        continue;
                    }
                }

                if (return_obj.error_list.Count.Equals(0))
                {

                    lease_item_on_protocol_list.Add(
                        new Lease_Item_On_Protocol
                        {
                            lease_item_in_storage_FKid = lease_item_on_offer.lease_item_in_storage_FKid,
                            total_quantity = lease_item_on_offer.total_quantity,
                            total_weight_kg = lease_item_on_offer.total_weight_kg,
                            total_area_m2 = lease_item_on_offer.total_area_m2,
                            total_worth = lease_item_on_offer.total_worth,
                            comment = lease_item_on_offer.comment
                        }
                    );

                    lease_item_stock_history_list.Add(
                        new Lease_Item_Stock_History
                        {
                            lease_item_FKid = lease_item_on_offer.lease_item_in_storage_FK.lease_item_FKid,
                            total_quantity = reference_stock_state.total_quantity,
                            in_storage_quantity = reference_stock_state.in_storage_quantity,
                            blocked_quantity = reference_stock_state.blocked_quantity,
                            out_of_storage_quantity = reference_stock_state.out_of_storage_quantity,
                            timestamp = new_timestamp
                        }
                    );

                    lease_item_in_storage_stock_history_list.Add(
                        new Lease_Item_In_Storage_Stock_History
                        {
                            lease_item_in_storage_FKid = lease_item_on_offer.lease_item_in_storage_FKid,
                            total_quantity = in_storage_reference_stock_state.total_quantity,
                            in_storage_quantity = in_storage_reference_stock_state.in_storage_quantity,
                            blocked_quantity = in_storage_reference_stock_state.blocked_quantity,
                            out_of_storage_quantity = in_storage_reference_stock_state.out_of_storage_quantity,
                            timestamp = new_timestamp
                        }
                    );

                }

            }

            if(return_obj.error_list.Count > 0)
            {
                return return_obj;
            }


            var settings = _context.Company_Invoice_Settings.FirstOrDefault();
            if (settings == null)
            {
                throw new Exception("51");//settings not found
            }


            Lease_Protocol new_protocol = new Lease_Protocol
            {
                number = Get_Latest_Protocol_Number(Lease_Protocol_Type.Release, editing_lease_protocol.timestamp.Year) + 1,
                prefix = settings.lease_release_protocol_prefix,
                year = editing_lease_protocol.timestamp.Year,
                type = Lease_Protocol_Type.Release,
                timestamp = editing_lease_protocol.timestamp,
                order_FKid = editing_lease_protocol.order_FKid,
                state = Protocol_State.Draft,
                element = editing_lease_protocol.element,
                transport = editing_lease_protocol.transport,
                comment = editing_lease_protocol.comment,
                total_weight_kg = editing_lease_protocol.total_weight_kg,
                total_worth = editing_lease_protocol.total_worth,
                total_area_m2 = editing_lease_protocol.total_area_m2,
                deleted = false
            };
            new_protocol.full_number = new_protocol.prefix + "-" + new_protocol.number.ToString("D5") + "/" + new_protocol.year.ToString();

            _context.Lease_Protocol.Add(new_protocol);
            _context.SaveChanges();

            foreach(var lease_item_on_protocol in lease_item_on_protocol_list)
            {
                lease_item_on_protocol.lease_protocol_FKid = new_protocol.id;
            }

            return_obj.protocol_id = new_protocol.id;

            _context.Lease_Item_On_Protocol.AddRange(lease_item_on_protocol_list);
            _context.Lease_Item_Stock_History.AddRange(lease_item_stock_history_list);
            _context.Lease_Item_In_Storage_Stock_History.AddRange(lease_item_in_storage_stock_history_list);

            _context.SaveChanges();

            return return_obj;
        }


        public Lease_Offer_Protocol_Model_Id Create_Max_Offer_List(Create_Max_Lease_Offer_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                if (input_obj.lease_offer_protocol_list.Count.Equals(0))
                {
                    throw new Exception("19");//not found
                }

                long? order_id = null;
                List<Lease_Protocol> offer_list = new List<Lease_Protocol>();

                foreach(long offer_id in input_obj.lease_offer_protocol_list)
                {
                    if (offer_list.Where(ol => ol.id.Equals(offer_id)).FirstOrDefault() != null)
                    {
                        continue;
                    }

                    var offer = _context.Lease_Protocol.Where(lp =>
                        lp.id.Equals(offer_id) &&
                        lp.type.Equals(Lease_Protocol_Type.Release) &&
                        lp.state.Equals(Protocol_State.Offer)
                    )
                    .Include(lp => lp.lease_item_on_protocol_list_FK)
                    .FirstOrDefault();

                    if(offer == null || offer.lease_item_on_protocol_list_FK == null)
                    {
                        throw new Exception("19");//offer not found
                    }

                    if(order_id == null)
                    {
                        order_id = offer.order_FKid;
                    }
                    else
                    {
                        if (!order_id.Equals(offer.order_FKid))
                        {
                            throw new Exception("45");//not all offers are related with the same order
                        }
                    }

                    offer_list.Add(offer);

                }


                List<Lease_Item_On_Protocol> lease_item_on_offer_max_quantity_list = new List<Lease_Item_On_Protocol>();

                Lease_Item_On_Protocol? item_in_max_list_exists = null;

                foreach (var offer in offer_list)
                {

                    if(
                        offer == null || 
                        offer.lease_item_on_protocol_list_FK == null
                    )
                    {
                        throw new Exception("19");//not found
                    }

                    foreach(var item_on_offer in offer.lease_item_on_protocol_list_FK)
                    {
                        item_in_max_list_exists = lease_item_on_offer_max_quantity_list.Where(mq => mq.lease_item_in_storage_FKid.Equals(item_on_offer.lease_item_in_storage_FKid)).FirstOrDefault();

                        if(item_in_max_list_exists != null)
                        {
                            if(item_in_max_list_exists.total_quantity < item_on_offer.total_quantity)
                            {
                                item_in_max_list_exists.total_quantity = item_on_offer.total_quantity;

                            }

                            continue;
                        }

                        lease_item_on_offer_max_quantity_list.Add(new Lease_Item_On_Protocol
                        {
                            lease_item_in_storage_FKid = item_on_offer.lease_item_in_storage_FKid,
                            total_quantity = item_on_offer.total_quantity,
                            total_weight_kg = item_on_offer.total_weight_kg,
                            total_area_m2 = item_on_offer.total_area_m2,
                            total_worth = item_on_offer.total_worth,
                            comment = item_on_offer.comment
                        });                        

                    }

                }


                var order = _context.Order.Where(o =>
                    o.id.Equals(order_id) &&
                    o.deleted.Equals(false)
                )
                .Include(o => o.lease_protocol_list_FK)
                .Include(o => o.lease_to_sale_protocol_list_FK)
                .Include(o => o.sale_protocol_list_FK)
                .FirstOrDefault();
                if (order == null || order.lease_protocol_list_FK == null || order.lease_to_sale_protocol_list_FK == null || order.sale_protocol_list_FK == null)
                {
                    throw new Exception("19");// object not found
                }

                Lease_Offer_Protocol_Model_Id return_obj = new Lease_Offer_Protocol_Model_Id { latest_protocol_timestamp = input_obj.user_current_timestamp.AddDays(-1) };

                var latest_lease_protocol = order.lease_protocol_list_FK.MaxBy(l => l.timestamp);
                var latest_lease_to_sale_protocol = order.lease_to_sale_protocol_list_FK.MaxBy(l => l.timestamp);
                var latest_sale_protocol = order.sale_protocol_list_FK.MaxBy(l => l.timestamp);
                if (latest_lease_protocol != null)
                {
                    if (input_obj.user_current_timestamp <= latest_lease_protocol.timestamp)
                    {
                        return_obj.latest_protocol_timestamp = latest_lease_protocol.timestamp;
                    }
                }
                if (latest_lease_to_sale_protocol != null)
                {
                    if (input_obj.user_current_timestamp <= latest_lease_to_sale_protocol.timestamp)
                    {
                        return_obj.latest_protocol_timestamp = latest_lease_to_sale_protocol.timestamp;
                    }
                }
                if (latest_sale_protocol != null)
                {
                    if (input_obj.user_current_timestamp <= latest_sale_protocol.timestamp)
                    {
                        return_obj.latest_protocol_timestamp = latest_sale_protocol.timestamp;
                    }
                }

                var settings = _context.Company_Invoice_Settings.FirstOrDefault();
                if (settings == null)
                {
                    throw new Exception("51");//settings not found
                }

                var first_offer_data = offer_list.First();

                Lease_Protocol max_offer_protocol = new Lease_Protocol
                {
                    number = Get_Latest_Offer_Number(input_obj.user_current_timestamp.Year) + 1,
                    year = input_obj.user_current_timestamp.Year,
                    prefix = "O" + settings.lease_release_protocol_prefix,
                    type = Lease_Protocol_Type.Release,
                    timestamp = input_obj.user_current_timestamp,
                    order_FKid = order.id,
                    state = Protocol_State.Offer,
                    element = first_offer_data.element,
                    transport = first_offer_data.transport,
                    comment = Array.Empty<byte>(),
                    total_weight_kg = 0,
                    total_worth = 0,
                    total_area_m2 = 0,
                    deleted = false
                };

                max_offer_protocol.full_number = max_offer_protocol.prefix + "-" + max_offer_protocol.number.ToString("D5") + "/" + max_offer_protocol.year.ToString();


                _context.Lease_Protocol.Add(max_offer_protocol);
                _context.SaveChanges();


                foreach(var item_on_max_list in lease_item_on_offer_max_quantity_list)
                {
                    item_on_max_list.lease_protocol_FKid = max_offer_protocol.id;

                    max_offer_protocol.total_weight_kg += item_on_max_list.total_weight_kg;
                    max_offer_protocol.total_area_m2 += item_on_max_list.total_area_m2;
                    max_offer_protocol.total_worth += item_on_max_list.total_worth;
                }

                _context.Lease_Item_On_Protocol.AddRange(lease_item_on_offer_max_quantity_list);
                _context.SaveChanges();

                return_obj.protocol_id = max_offer_protocol.id;

                return return_obj;
            }
        }

        /* 
         * Add_Order_Protocol method
         * This method is used to add new records to the order_protocol table.
         * 
         * It accepts Add_Order_Protocol_Data object as input.
         * It then adds new record with values based on the data given in the input object.
         */
        public Lease_Protocol_Model_Id Add_Lease_Protocol(Add_Lease_Protocol_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                if (!Enum.IsDefined(typeof(Lease_Protocol_Type), input_obj.type))
                {
                    throw new Exception("19");// given type is not defined
                }

                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var order = _context.Order.Where(o => 
                    o.id.Equals(input_obj.order_FK) &&
                    o.deleted.Equals(false)
                )
                .Include(o => o.lease_protocol_list_FK)
                .Include(o => o.lease_to_sale_protocol_list_FK)
                .Include(o => o.sale_protocol_list_FK)
                .FirstOrDefault();
                if (order == null || order.lease_protocol_list_FK == null || order.lease_to_sale_protocol_list_FK == null || order.sale_protocol_list_FK == null)
                {
                    throw new Exception("19");// object not found
                }

                Lease_Protocol_Model_Id return_obj = new Lease_Protocol_Model_Id { latest_protocol_timestamp = input_obj.user_current_timestamp.AddDays(-1) };

                var latest_lease_protocol = order.lease_protocol_list_FK.MaxBy(l => l.timestamp);
                var latest_lease_to_sale_protocol = order.lease_to_sale_protocol_list_FK.MaxBy(l => l.timestamp);
                var latest_sale_protocol = order.sale_protocol_list_FK.MaxBy(l => l.timestamp);
                if (latest_lease_protocol != null)
                {
                    if (input_obj.user_current_timestamp <= latest_lease_protocol.timestamp)
                    {
                        return_obj.latest_protocol_timestamp = latest_lease_protocol.timestamp;
                    }
                }
                if (latest_lease_to_sale_protocol != null)
                {
                    if (input_obj.user_current_timestamp <= latest_lease_to_sale_protocol.timestamp)
                    {
                        return_obj.latest_protocol_timestamp = latest_lease_to_sale_protocol.timestamp;
                    }
                }
                if (latest_sale_protocol != null)
                {
                    if (input_obj.user_current_timestamp <= latest_sale_protocol.timestamp)
                    {
                        return_obj.latest_protocol_timestamp = latest_sale_protocol.timestamp;
                    }
                }

                var settings = _context.Company_Invoice_Settings.FirstOrDefault();
                if (settings == null)
                {
                    throw new Exception("51");//settings not found
                }

                Lease_Protocol new_protocol = new Lease_Protocol
                {
                    number = Get_Latest_Protocol_Number(input_obj.type, input_obj.user_current_timestamp.Year) + 1,
                    year = input_obj.user_current_timestamp.Year,
                    type = input_obj.type,
                    timestamp = input_obj.user_current_timestamp,
                    order_FKid = order.id,
                    state = Protocol_State.Draft,
                    element = Array.Empty<byte>(),
                    transport = Array.Empty<byte>(),
                    comment = Array.Empty<byte>(),
                    total_weight_kg = 0,
                    total_area_m2 = 0,
                    total_worth = 0,
                    deleted = false
                };

                if (input_obj.type.Equals(Lease_Protocol_Type.Release))
                {
                    new_protocol.prefix = settings.lease_release_protocol_prefix;
                }
                else if (input_obj.type.Equals(Lease_Protocol_Type.Return))
                {
                    new_protocol.prefix = settings.return_protocol_prefix;
                }

                new_protocol.full_number = new_protocol.prefix + "-" + new_protocol.number.ToString("D5") + "/" + new_protocol.year.ToString();


                _context.Lease_Protocol.Add(new_protocol);
                _context.SaveChanges();

                return_obj.protocol_id = new_protocol.id;

                return return_obj;
            }

        }


        private List<Lease_Item_On_Protocol_Error_Model> Change_Lease_Protocol_State(ref Lease_Protocol editing_lease_protocol, Protocol_State new_state, DateTime new_timestamp)
        {
            Protocol_State previous_state = editing_lease_protocol.state;
            DateTime previous_timestamp = editing_lease_protocol.timestamp;
            var order_id = editing_lease_protocol.order_FKid;

            List<Lease_Item_On_Protocol_Error_Model> error_list = new List<Lease_Item_On_Protocol_Error_Model>();

            if (                
                previous_state.Equals(Protocol_State.Draft) && 
                !new_state.Equals(Protocol_State.Draft) && 
                editing_lease_protocol.lease_item_on_protocol_list_FK.Count.Equals(0)
            )
            {
                throw new Exception("13");//no items on protocol
            }


            if (
                (previous_state.Equals(new_state) && previous_timestamp.Equals(new_timestamp)) || 
                editing_lease_protocol.lease_item_on_protocol_list_FK.Count.Equals(0)
            )
            {
                return error_list;
            }

            List<Lease_Item_On_Protocol> lease_item_on_protocol_list = editing_lease_protocol.lease_item_on_protocol_list_FK;

            List<Lease_Item_Stock_History>? stock_history = null;
            List<Lease_Item_In_Storage_Stock_History>? in_storage_stock_history = null;

                        
            List<Lease_Item_Stock_History> stock_history_operational;
            List<Lease_Item_In_Storage_Stock_History> in_storage_stock_history_operational;
            
            List<Lease_Item_Stock_History> stock_history_operational_second;
            List<Lease_Item_In_Storage_Stock_History> in_storage_stock_history_operational_second;
            

            Lease_Item_Stock_History? current_stock_state;
            Lease_Item_In_Storage_Stock_History? in_storage_current_stock_state;

            Lease_Item_Stock_History? latest_stock_state_before_new_timestamp = null;
            Lease_Item_In_Storage_Stock_History? in_storage_latest_stock_state_before_new_timestamp = null;

            Lease_Item_Stock_History? state_before_current = null;
            Lease_Item_In_Storage_Stock_History? in_storage_state_before_current = null;

            Lease_Item_In_Storage_Stock_History? min_stock_state = null;

            Lease_Item_Out_Of_Storage_History? min_out_of_storage = null;

            Lease_Item_Out_Of_Storage? out_of_storage_object = null;
            Lease_Item_Out_Of_Storage_History? out_of_storage_state = null;
            Lease_Item_Out_Of_Storage_History? out_of_storage_state_before_current = null;
            Lease_Item_Out_Of_Storage_History? out_of_storage_state_before_new_timestamp = null;

            List<Lease_Item_Out_Of_Storage_History> out_of_storage_history_operational;

            foreach (var lease_item_on_protocol in lease_item_on_protocol_list)
            {
                if(
                    lease_item_on_protocol == null || 
                    lease_item_on_protocol.lease_item_in_storage_FK == null
                )
                {
                    throw new Exception("19");//not found
                }

                if (previous_timestamp.Equals(new_timestamp))
                {
                    stock_history = _context.Lease_Item_Stock_History.Where(lsh => 
                        lsh.lease_item_FKid.Equals(lease_item_on_protocol.lease_item_in_storage_FK.lease_item_FKid) &&
                        lsh.timestamp >= previous_timestamp
                    ).ToList();

                    in_storage_stock_history = _context.Lease_Item_In_Storage_Stock_History.Where(ilsh => 
                        ilsh.lease_item_in_storage_FKid.Equals(lease_item_on_protocol.lease_item_in_storage_FKid) &&
                        ilsh.timestamp >= previous_timestamp
                    ).ToList();
                }
                else
                {
                    stock_history = _context.Lease_Item_Stock_History.Where(lsh =>
                        lsh.lease_item_FKid.Equals(lease_item_on_protocol.lease_item_in_storage_FK.lease_item_FKid)
                    ).ToList();

                    in_storage_stock_history = _context.Lease_Item_In_Storage_Stock_History.Where(ilsh =>
                        ilsh.lease_item_in_storage_FKid.Equals(lease_item_on_protocol.lease_item_in_storage_FKid)
                    ).ToList();
                }


                current_stock_state = stock_history.Where(lsh => lsh.timestamp.Equals(previous_timestamp)).FirstOrDefault();
                if(current_stock_state == null)
                {
                    throw new Exception("19");//not found
                }

                in_storage_current_stock_state = in_storage_stock_history.Where(ilsh => ilsh.timestamp.Equals(previous_timestamp)).FirstOrDefault();
                if (in_storage_current_stock_state == null)
                {
                    throw new Exception("19");//not found
                }

                Stock_State_Validator.Validate_Stock_State(current_stock_state);
                Stock_State_Validator.Validate_Stock_State(in_storage_current_stock_state);


                if(
                    previous_state.Equals(Protocol_State.Confirmed) || 
                    new_state.Equals(Protocol_State.Confirmed) || 
                    editing_lease_protocol.type.Equals(Lease_Protocol_Type.Return)
                )
                {
                    out_of_storage_object = _context.Lease_Item_Out_Of_Storage.Where(los =>
                        los.order_FKid.Equals(order_id) &&
                        los.lease_item_in_storage_FKid.Equals(lease_item_on_protocol.lease_item_in_storage_FKid)
                    )
                    .Include(los => los.lease_item_out_of_storage_history_FK)
                    .FirstOrDefault();
                }


                if (editing_lease_protocol.type.Equals(Lease_Protocol_Type.Release))
                {
                    if (previous_state.Equals(Protocol_State.Reserved))
                    {

                        if (current_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                        {
                            // miscallculation (current stock state should have at least item_on_protocol quantity in blocked state)
                            error_list.Clear();
                            error_list.Add(
                                new Lease_Item_On_Protocol_Error_Model
                                {
                                    code = "36",
                                    lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                    timestamp = current_stock_state.timestamp,
                                    required_quantity = 0
                                }
                            );

                            return error_list;
                        }

                        if (in_storage_current_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                        {
                            // miscallculation (current stock state should have at least item_on_protocol quantity in blocked state)
                            error_list.Clear();
                            error_list.Add(
                                new Lease_Item_On_Protocol_Error_Model
                                {
                                    code = "36",
                                    lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                    timestamp = in_storage_current_stock_state.timestamp,
                                    required_quantity = 0
                                }
                            );

                            return error_list;
                        }

                    }
                    else if (previous_state.Equals(Protocol_State.Confirmed))
                    {

                        if (current_stock_state.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                        {
                            // miscallculation (in current state out_of_storage quantity should be equal to or greater than released quantity (on_protocol quantity))
                            error_list.Clear();
                            error_list.Add(
                                new Lease_Item_On_Protocol_Error_Model
                                {
                                    code = "36",
                                    lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                    timestamp = current_stock_state.timestamp,
                                    required_quantity = 0
                                }
                            );

                            return error_list;
                        }

                        if (in_storage_current_stock_state.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                        {
                            // miscallculation (in current state out_of_storage quantity should be equal to or greater than released quantity (on_protocol quantity))
                            error_list.Clear();
                            error_list.Add(
                                new Lease_Item_On_Protocol_Error_Model
                                {
                                    code = "36",
                                    lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                    timestamp = in_storage_current_stock_state.timestamp,
                                    required_quantity = 0
                                }
                            );

                            return error_list;
                        }


                        if (out_of_storage_object == null)
                        {
                            //miscallculation (if previous state was confirmed then out_of_storage object related with item_on_protocol should exist)
                            error_list.Clear();
                            error_list.Add(
                                new Lease_Item_On_Protocol_Error_Model
                                {
                                    code = "36",
                                    lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                    timestamp = previous_timestamp,
                                    required_quantity = 0
                                }
                            );

                            return error_list;
                        }

                        if (
                            out_of_storage_object.lease_item_out_of_storage_history_FK == null ||
                            out_of_storage_object.lease_item_out_of_storage_history_FK.Count == 0
                        )
                        {
                            throw new Exception("19");//not found
                        }

                        out_of_storage_state = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp.Equals(previous_timestamp)).FirstOrDefault();

                        if (out_of_storage_state == null)
                        {
                            throw new Exception("19");//not found
                        }

                        Stock_State_Validator.Validate_Out_Of_Storage_State(out_of_storage_state, in_storage_current_stock_state.out_of_storage_quantity);

                        if (out_of_storage_state.total_quantity < lease_item_on_protocol.total_quantity)
                        {
                            // miscallculation (in current state out_of_storage quantity should be equal to or greater than released quantity (on_protocol quantity))
                            error_list.Clear();
                            error_list.Add(
                                new Lease_Item_On_Protocol_Error_Model
                                {
                                    code = "36",
                                    lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                    timestamp = out_of_storage_state.timestamp,
                                    required_quantity = 0
                                }
                            );

                            return error_list;
                        }


                    }

                }
                else if (editing_lease_protocol.type.Equals(Lease_Protocol_Type.Return))
                {

                    if (out_of_storage_object == null)
                    {
                        // miscalcullation (if return protocol is in draft state it means that out_of_storage object (with at least one history record) should exist)
                        error_list.Clear();
                        error_list.Add(
                            new Lease_Item_On_Protocol_Error_Model
                            {
                                code = "36",
                                lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                timestamp = previous_timestamp,
                                required_quantity = 0
                            }
                        );

                        return error_list;
                    }

                    if (
                        out_of_storage_object.lease_item_out_of_storage_history_FK == null ||
                        out_of_storage_object.lease_item_out_of_storage_history_FK.Count == 0
                    )
                    {
                        throw new Exception("19");//not found
                    }


                    out_of_storage_state = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp.Equals(previous_timestamp)).FirstOrDefault();

                    if (out_of_storage_state == null)
                    {
                        throw new Exception("19");//not found
                    }

                    Stock_State_Validator.Validate_Out_Of_Storage_State(out_of_storage_state, in_storage_current_stock_state.out_of_storage_quantity);



                    if (previous_state.Equals(Protocol_State.Confirmed))
                    {

                        if (current_stock_state.in_storage_quantity - current_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                        {
                            //miscalcullation (current state should have at least item_on_protocol free quantity (returned quantity))
                            error_list.Clear();
                            error_list.Add(
                                new Lease_Item_On_Protocol_Error_Model
                                {
                                    code = "36",
                                    lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                    timestamp = current_stock_state.timestamp,
                                    required_quantity = 0
                                }
                            );

                            return error_list;
                        }

                        if (in_storage_current_stock_state.in_storage_quantity - in_storage_current_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                        {
                            //miscalcullation (current state should have at least item_on_protocol free quantity (returned quantity))
                            error_list.Clear();
                            error_list.Add(
                                new Lease_Item_On_Protocol_Error_Model
                                {
                                    code = "36",
                                    lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                    timestamp = in_storage_current_stock_state.timestamp,
                                    required_quantity = 0
                                }
                            );

                            return error_list;
                        }

                    }

                }



                if (new_timestamp.Equals(previous_timestamp))
                {

                    if (previous_state.Equals(Protocol_State.Draft))
                    {
                        if (editing_lease_protocol.type.Equals(Lease_Protocol_Type.Release))
                        {

                            if (current_stock_state.in_storage_quantity - current_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                            {
                                // not enough in storage                                
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "22",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = current_stock_state.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - (current_stock_state.in_storage_quantity - current_stock_state.blocked_quantity)
                                    }
                                );
                                continue;
                            }

                            if (in_storage_current_stock_state.in_storage_quantity - in_storage_current_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                            {
                                // not enough in storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "22",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = in_storage_current_stock_state.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - (in_storage_current_stock_state.in_storage_quantity - in_storage_current_stock_state.blocked_quantity)
                                    }
                                );
                                continue;
                            }

                        }
                        else if (editing_lease_protocol.type.Equals(Lease_Protocol_Type.Return))
                        {

                            if (current_stock_state.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                            {
                                //not enough out of storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "43",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = current_stock_state.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - current_stock_state.out_of_storage_quantity
                                    }
                                );
                                continue;
                            }

                            if (in_storage_current_stock_state.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                            {
                                //not enough out of storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "43",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = in_storage_current_stock_state.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - in_storage_current_stock_state.out_of_storage_quantity
                                    }
                                );
                                continue;
                            }

                        }

                    }
                    else if (previous_state.Equals(Protocol_State.Confirmed))
                    {

                        if (editing_lease_protocol.type.Equals(Lease_Protocol_Type.Release))
                        {

                            out_of_storage_state_before_current = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(osh => osh.timestamp < previous_timestamp).MaxBy(osh => osh.timestamp);

                        }

                    }

                }
                else
                {
                    Timestamp_Validator.Validate_Protocol_Timestamp(stock_history, new_timestamp);
                    Timestamp_Validator.Validate_Protocol_Timestamp(in_storage_stock_history, new_timestamp);

                    latest_stock_state_before_new_timestamp = stock_history.Where(sh => sh.timestamp < new_timestamp).MaxBy(sh => sh.timestamp);
                    if (latest_stock_state_before_new_timestamp == null)
                    {
                        throw new Exception("19");//not found
                    }

                    in_storage_latest_stock_state_before_new_timestamp = in_storage_stock_history.Where(ish => ish.timestamp < new_timestamp).MaxBy(ish => ish.timestamp);
                    if (in_storage_latest_stock_state_before_new_timestamp == null)
                    {
                        throw new Exception("19");//not found
                    }

                    state_before_current = stock_history.Where(sh => sh.timestamp < previous_timestamp).MaxBy(sh => sh.timestamp);
                    in_storage_state_before_current = in_storage_stock_history.Where(ish => ish.timestamp < previous_timestamp).MaxBy(ish => ish.timestamp);

                    if (
                        state_before_current == null ||
                        in_storage_state_before_current == null
                    )
                    {
                        throw new Exception("19");//not found
                    }

                    if (previous_state.Equals(Protocol_State.Draft))
                    {
                        if (editing_lease_protocol.type.Equals(Lease_Protocol_Type.Release))
                        {

                            if (latest_stock_state_before_new_timestamp.in_storage_quantity - latest_stock_state_before_new_timestamp.blocked_quantity < lease_item_on_protocol.total_quantity)
                            {
                                // not enough in storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "22",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = latest_stock_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - (latest_stock_state_before_new_timestamp.in_storage_quantity - latest_stock_state_before_new_timestamp.blocked_quantity)
                                    }
                                );
                                continue;
                            }

                            if (in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity - in_storage_latest_stock_state_before_new_timestamp.blocked_quantity < lease_item_on_protocol.total_quantity)
                            {
                                // not enough in storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "22",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = in_storage_latest_stock_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - (in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity - in_storage_latest_stock_state_before_new_timestamp.blocked_quantity)
                                    }
                                );
                                continue;
                            }

                        }
                        else if (editing_lease_protocol.type.Equals(Lease_Protocol_Type.Return))
                        {

                            if (latest_stock_state_before_new_timestamp.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                            {
                                //not enough out of storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "43",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = latest_stock_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - latest_stock_state_before_new_timestamp.out_of_storage_quantity
                                    }
                                );
                                continue;
                            }

                            if (in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                            {
                                //not enough out of storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "43",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = in_storage_latest_stock_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity
                                    }
                                );
                                continue;
                            }

                            Timestamp_Validator.Validate_Protocol_Timestamp(out_of_storage_object.lease_item_out_of_storage_history_FK, new_timestamp);


                            out_of_storage_state_before_current = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp < previous_timestamp).MaxBy(losh => losh.timestamp);
                            if (out_of_storage_state_before_current != null)
                            {
                                if (!out_of_storage_state_before_current.total_quantity.Equals(out_of_storage_state.total_quantity))
                                {
                                    // miscallculation (state before draft protocol should be equal to the current one)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = out_of_storage_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }


                            out_of_storage_state_before_new_timestamp = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp < new_timestamp).MaxBy(losh => losh.timestamp);
                            if (out_of_storage_state_before_new_timestamp == null)
                            {
                                if (new_timestamp < previous_timestamp)
                                {
                                    //if new timestamp is earlier than current and there is no record before, it means that there is no amount of item to return
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "43",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = new_timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity
                                        }
                                    );
                                    continue;

                                }
                                else if(new_timestamp > previous_timestamp)
                                {
                                    throw new Exception("19");//not found (should be at least current state)
                                }
                            }


                        }


                        //checking if current state can be moved to the new timestamp safely
                        if (
                            !state_before_current.total_quantity.Equals(current_stock_state.total_quantity) ||
                            !state_before_current.in_storage_quantity.Equals(current_stock_state.in_storage_quantity) ||
                            !state_before_current.blocked_quantity.Equals(current_stock_state.blocked_quantity) ||
                            !state_before_current.out_of_storage_quantity.Equals(current_stock_state.out_of_storage_quantity)
                        )
                        {
                            // miscallculation (state before draft protocol should be equal to the current one)
                            error_list.Clear();
                            error_list.Add(
                                new Lease_Item_On_Protocol_Error_Model
                                {
                                    code = "36",
                                    lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                    timestamp = current_stock_state.timestamp,
                                    required_quantity = 0
                                }
                            );

                            return error_list;
                        }

                        if (
                            !in_storage_state_before_current.total_quantity.Equals(in_storage_current_stock_state.total_quantity) ||
                            !in_storage_state_before_current.in_storage_quantity.Equals(in_storage_current_stock_state.in_storage_quantity) ||
                            !in_storage_state_before_current.blocked_quantity.Equals(in_storage_current_stock_state.blocked_quantity) ||
                            !in_storage_state_before_current.out_of_storage_quantity.Equals(in_storage_current_stock_state.out_of_storage_quantity)
                        )
                        {
                            // miscallculation (state before draft protocol should be equal to the current one)
                            error_list.Clear();
                            error_list.Add(
                                new Lease_Item_On_Protocol_Error_Model
                                {
                                    code = "36",
                                    lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                    timestamp = in_storage_current_stock_state.timestamp,
                                    required_quantity = 0
                                }
                            );

                            return error_list;
                        }


                    }
                    else if (previous_state.Equals(Protocol_State.Reserved))
                    {
                        if (editing_lease_protocol.type.Equals(Lease_Protocol_Type.Release))
                        {
                            //checking if current state can be moved to the new timestamp safely
                            if (
                                !state_before_current.total_quantity.Equals(current_stock_state.total_quantity) ||
                                !state_before_current.in_storage_quantity.Equals(current_stock_state.in_storage_quantity) ||
                                !state_before_current.blocked_quantity.Equals(current_stock_state.blocked_quantity - lease_item_on_protocol.total_quantity) ||
                                !state_before_current.out_of_storage_quantity.Equals(current_stock_state.out_of_storage_quantity)
                            )
                            {
                                // miscallculation (state before reservation protocol should be different only in blocked_quantity (item on protocol quantity))
                                error_list.Clear();
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "36",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = current_stock_state.timestamp,
                                        required_quantity = 0
                                    }
                                );

                                return error_list;
                            }

                            if (
                                !in_storage_state_before_current.total_quantity.Equals(in_storage_current_stock_state.total_quantity) ||
                                !in_storage_state_before_current.in_storage_quantity.Equals(in_storage_current_stock_state.in_storage_quantity) ||
                                !in_storage_state_before_current.blocked_quantity.Equals(in_storage_current_stock_state.blocked_quantity - lease_item_on_protocol.total_quantity) ||
                                !in_storage_state_before_current.out_of_storage_quantity.Equals(in_storage_current_stock_state.out_of_storage_quantity)
                            )
                            {
                                // miscallculation (state before reservation protocol should be different only in blocked_quantity (item on protocol quantity))
                                error_list.Clear();
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "36",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = in_storage_current_stock_state.timestamp,
                                        required_quantity = 0
                                    }
                                );

                                return error_list;
                            }

                        }
                    }
                    else if (previous_state.Equals(Protocol_State.Confirmed))
                    {
                        if (editing_lease_protocol.type.Equals(Lease_Protocol_Type.Release))
                        {

                            out_of_storage_state_before_current = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp < previous_timestamp).MaxBy(losh => losh.timestamp);
                            if (out_of_storage_state_before_current != null)
                            {
                                if (!out_of_storage_state_before_current.total_quantity.Equals(out_of_storage_state.total_quantity - lease_item_on_protocol.total_quantity))
                                {
                                    // miscallculation (state before confirmed protocol should be different only in total_quantity (item on protocol quantity))
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = out_of_storage_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }


                            //checking if current state can be moved to the new timestamp safely
                            if (
                                !state_before_current.total_quantity.Equals(current_stock_state.total_quantity) ||
                                !state_before_current.in_storage_quantity.Equals(current_stock_state.in_storage_quantity + lease_item_on_protocol.total_quantity) ||
                                !state_before_current.blocked_quantity.Equals(current_stock_state.blocked_quantity) ||
                                !state_before_current.out_of_storage_quantity.Equals(current_stock_state.out_of_storage_quantity - lease_item_on_protocol.total_quantity)
                            )
                            {
                                // miscallculation (state before confirmed protocol should be different only in in_storage_quantity and out_of_storage_quantity (item on protocol quantity))
                                error_list.Clear();
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "36",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = current_stock_state.timestamp,
                                        required_quantity = 0
                                    }
                                );

                                return error_list;
                            }

                            if (
                                !in_storage_state_before_current.total_quantity.Equals(in_storage_current_stock_state.total_quantity) ||
                                !in_storage_state_before_current.in_storage_quantity.Equals(in_storage_current_stock_state.in_storage_quantity + lease_item_on_protocol.total_quantity) ||
                                !in_storage_state_before_current.blocked_quantity.Equals(in_storage_current_stock_state.blocked_quantity) ||
                                !in_storage_state_before_current.out_of_storage_quantity.Equals(in_storage_current_stock_state.out_of_storage_quantity - lease_item_on_protocol.total_quantity)
                            )
                            {
                                // miscallculation (state before confirmed protocol should be different only in in_storage_quantity and out_of_storage_quantity (item on protocol quantity))
                                error_list.Clear();
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "36",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = in_storage_current_stock_state.timestamp,
                                        required_quantity = 0
                                    }
                                );

                                return error_list;
                            }

                        }
                        else if (editing_lease_protocol.type.Equals(Lease_Protocol_Type.Return))
                        {

                            Timestamp_Validator.Validate_Protocol_Timestamp(out_of_storage_object.lease_item_out_of_storage_history_FK, new_timestamp);


                            out_of_storage_state_before_current = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp < previous_timestamp).MaxBy(losh => losh.timestamp);
                            if (out_of_storage_state_before_current != null)
                            {
                                if (!out_of_storage_state_before_current.total_quantity.Equals(out_of_storage_state.total_quantity + lease_item_on_protocol.total_quantity))
                                {
                                    // miscallculation (state before confirmed protocol should be different only in total_quantity (returned quantity (on_protocol quantity)))
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = out_of_storage_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }
                            else
                            {
                                // miscallculation (if return protocol is in confirmed state, it means that there have to be at least one record before (released quantity that has been returned in this protocol))
                                error_list.Clear();
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "36",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = out_of_storage_state.timestamp,
                                        required_quantity = 0
                                    }
                                );

                                return error_list;
                            }


                            out_of_storage_state_before_new_timestamp = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp < new_timestamp).MaxBy(losh => losh.timestamp);
                            if (out_of_storage_state_before_new_timestamp == null)
                            {
                                if (new_timestamp < previous_timestamp)
                                {
                                    //if new timestamp is earlier than current and there is no record before, it means that there is no amount of item to return
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "43",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = new_timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity
                                        }
                                    );
                                    continue;
                                }
                                else
                                {
                                    throw new Exception("19");//not found (should be at least current state)
                                }
                            }


                            //checking if current state can be moved to the new timestamp safely
                            if (
                                !state_before_current.total_quantity.Equals(current_stock_state.total_quantity) ||
                                !state_before_current.in_storage_quantity.Equals(current_stock_state.in_storage_quantity - lease_item_on_protocol.total_quantity) ||
                                !state_before_current.blocked_quantity.Equals(current_stock_state.blocked_quantity) ||
                                !state_before_current.out_of_storage_quantity.Equals(current_stock_state.out_of_storage_quantity + lease_item_on_protocol.total_quantity)
                            )
                            {
                                // miscallculation (state before draft protocol should be equal to current state)
                                error_list.Clear();
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "36",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = current_stock_state.timestamp,
                                        required_quantity = 0
                                    }
                                );

                                return error_list;
                            }

                            if (
                                !in_storage_state_before_current.total_quantity.Equals(in_storage_current_stock_state.total_quantity) ||
                                !in_storage_state_before_current.in_storage_quantity.Equals(in_storage_current_stock_state.in_storage_quantity - lease_item_on_protocol.total_quantity) ||
                                !in_storage_state_before_current.blocked_quantity.Equals(in_storage_current_stock_state.blocked_quantity) ||
                                !in_storage_state_before_current.out_of_storage_quantity.Equals(in_storage_current_stock_state.out_of_storage_quantity + lease_item_on_protocol.total_quantity)
                            )
                            {
                                // miscallculation (state before draft protocol should be equal to current state)
                                error_list.Clear();
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "36",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = in_storage_current_stock_state.timestamp,
                                        required_quantity = 0
                                    }
                                );

                                return error_list;
                            }

                        }
                    }


                }



                if (editing_lease_protocol.type.Equals(Lease_Protocol_Type.Release))
                {

                    if (previous_state.Equals(Protocol_State.Draft) && new_state.Equals(Protocol_State.Draft))
                    {
                        if (new_timestamp > previous_timestamp)
                        {

                            min_stock_state = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp).MinBy(ishat => ishat.in_storage_quantity - ishat.blocked_quantity);

                            if (min_stock_state != null)
                            {
                                if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // not enough in storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "22",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                                        }
                                    );
                                    continue;
                                }
                            }

                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                        }
                        else if (new_timestamp < previous_timestamp)
                        {

                            min_stock_state = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp).MinBy(ishat => ishat.in_storage_quantity - ishat.blocked_quantity);

                            if (min_stock_state != null)
                            {
                                if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // not enough in storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "22",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                                        }
                                    );
                                    continue;
                                }
                            }
                            else
                            {
                                throw new Exception("19");//not found (if new timestamp is earlier than current, it means that there have to be at least one state after new timestamp (current state))
                            }

                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity;
                        }
                    }

                    if (previous_state.Equals(Protocol_State.Draft) && new_state.Equals(Protocol_State.Reserved))
                    {
                        if (previous_timestamp.Equals(new_timestamp))
                        {
                            //stock history after previous_timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > previous_timestamp).ToList();

                            //in storage stock history after previous_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp).ToList();


                            min_stock_state = in_storage_stock_history_operational.MinBy(ishat => ishat.in_storage_quantity - ishat.blocked_quantity);

                            if (min_stock_state != null)
                            {
                                if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // not enough in storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "22",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                                        }
                                    );
                                    continue;
                                }
                            }

                            current_stock_state.blocked_quantity += lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.blocked_quantity += lease_item_on_protocol.total_quantity;

                            foreach (var next_state in stock_history_operational)
                            {
                                next_state.blocked_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.blocked_quantity += lease_item_on_protocol.total_quantity;
                            }

                        }
                        else if (new_timestamp > previous_timestamp)
                        {

                            min_stock_state = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp).MinBy(ishat => ishat.in_storage_quantity - ishat.blocked_quantity);

                            if (min_stock_state != null)
                            {
                                if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // not enough in storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "22",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                                        }
                                    );
                                    continue;
                                }
                            }
                            
                            //stock history after new_timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > new_timestamp).ToList();
                                                       
                            //in storage stock history after new_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp).ToList();

                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity + lease_item_on_protocol.total_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity + lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            foreach (var next_state in stock_history_operational)
                            {
                                next_state.blocked_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.blocked_quantity += lease_item_on_protocol.total_quantity;
                            }

                        }
                        else if (new_timestamp < previous_timestamp)
                        {

                            min_stock_state = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp).MinBy(ishat => ishat.in_storage_quantity - ishat.blocked_quantity);

                            if (min_stock_state != null)
                            {
                                if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // not enough in storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "22",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                                        }
                                    );
                                    continue;
                                }
                            }
                            else
                            {
                                throw new Exception("19");//not found (if new timestamp is earlier than current, it means that there have to be at least one state after new timestamp (current state))
                            }


                            //stock history after new_timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > new_timestamp).ToList();

                            //in storage stock history after new_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp).ToList();


                            foreach (var next_state in stock_history_operational)
                            {
                                next_state.blocked_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.blocked_quantity += lease_item_on_protocol.total_quantity;
                            }
                            
                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity + lease_item_on_protocol.total_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity + lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                        }

                    }

                    if (previous_state.Equals(Protocol_State.Draft) && new_state.Equals(Protocol_State.Confirmed))
                    {
                        if (previous_timestamp.Equals(new_timestamp))
                        {
                            //stock history after previous_timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > previous_timestamp).ToList();

                            //in storage stock history after previous_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp).ToList();


                            min_stock_state = in_storage_stock_history_operational.MinBy(ishat => ishat.in_storage_quantity - ishat.blocked_quantity);

                            if (min_stock_state != null)
                            {
                                if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // not enough in storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "22",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                                        }
                                    );
                                    continue;
                                }
                            }


                            current_stock_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                            current_stock_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;

                            in_storage_current_stock_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;


                            foreach (var next_state in stock_history_operational)
                            {
                                next_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                                next_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                                in_storage_next_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                            }

                        }
                        else if (new_timestamp > previous_timestamp)
                        {

                            //stock history after new_timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > new_timestamp).ToList();

                            //in storage stock history after new_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp).ToList();


                            min_stock_state = in_storage_stock_history_operational.MinBy(ishat => ishat.in_storage_quantity - ishat.blocked_quantity);

                            if (min_stock_state != null)
                            {
                                if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // not enough in storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "22",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                                        }
                                    );
                                    continue;
                                }
                            }
                                                        
                            
                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity - lease_item_on_protocol.total_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity + lease_item_on_protocol.total_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity - lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity + lease_item_on_protocol.total_quantity;

                            foreach (var next_state in stock_history_operational)
                            {
                                next_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                                next_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                                in_storage_next_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                            }

                        }
                        else if (new_timestamp < previous_timestamp)
                        {

                            //stock history after new_timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > new_timestamp).ToList();

                            //in storage stock history after new_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp).ToList();


                            min_stock_state = in_storage_stock_history_operational.MinBy(ishat => ishat.in_storage_quantity - ishat.blocked_quantity);

                            if (min_stock_state != null)
                            {
                                if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // not enough in storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "22",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                                        }
                                    );
                                    continue;
                                }
                            }
                            else
                            {
                                throw new Exception("19");//not found (if new timestamp is earlier than current, it means that there have to be at least one state after new timestamp (current state))
                            }
                            

                            foreach (var next_state in stock_history_operational)
                            {
                                next_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                                next_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                                in_storage_next_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                            }

                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity - lease_item_on_protocol.total_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity + lease_item_on_protocol.total_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity - lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity + lease_item_on_protocol.total_quantity;
                        }

                        //
                        // new_timestamp is used for both: when previous_timestamp == new_timestamp and previous_timestamp != new_timestamp cases, because
                        // if timestamps are equal it doesnt matter which is used, but new_timestamp is required to "not equal" cases
                        //
                        if (out_of_storage_object == null)
                        {
                            //if out_of_storage object doesnt exist - it means that it is first release of this item in current order
                            Lease_Item_Out_Of_Storage new_out_of_storage_object = new Lease_Item_Out_Of_Storage
                            {
                                lease_item_in_storage_FKid = lease_item_on_protocol.lease_item_in_storage_FKid,
                                order_FKid = order_id,
                                lease_item_out_of_storage_history_FK =
                                [
                                    new Lease_Item_Out_Of_Storage_History { total_quantity = lease_item_on_protocol.total_quantity, timestamp = new_timestamp }
                                ]
                            };

                            _context.Lease_Item_Out_Of_Storage.Add(new_out_of_storage_object);
                        }
                        else
                        {
                            if (
                                out_of_storage_object.lease_item_out_of_storage_history_FK == null ||
                                out_of_storage_object.lease_item_out_of_storage_history_FK.Count == 0
                            )
                            {
                                throw new Exception("19");//not found
                            }

                            Timestamp_Validator.Validate_Protocol_Timestamp(out_of_storage_object.lease_item_out_of_storage_history_FK, new_timestamp);

                            out_of_storage_state = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp < new_timestamp).MaxBy(losh => losh.timestamp);

                            Lease_Item_Out_Of_Storage_History out_of_storage_update = new Lease_Item_Out_Of_Storage_History
                            {
                                timestamp = new_timestamp,
                                lease_item_out_of_storage_FKid = out_of_storage_object.id
                            };

                            if (out_of_storage_state == null)
                            {
                                //current timestamp is before all of out_of_storage states

                                out_of_storage_update.total_quantity = lease_item_on_protocol.total_quantity;
                            }
                            else
                            {
                                //current timestamp is after latest state

                                if (previous_timestamp.Equals(new_timestamp))
                                {
                                    Stock_State_Validator.Validate_Out_Of_Storage_State(out_of_storage_state, in_storage_current_stock_state.out_of_storage_quantity);
                                }
                                else
                                {
                                    Stock_State_Validator.Validate_Out_Of_Storage_State(out_of_storage_state, in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity);
                                }


                                out_of_storage_update.total_quantity = out_of_storage_state.total_quantity + lease_item_on_protocol.total_quantity;
                            }

                            _context.Lease_Item_Out_Of_Storage_History.Add(out_of_storage_update);

                            //out_of_storage history after new_timestamp (or current if they are equal)
                            out_of_storage_history_operational = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp > new_timestamp).ToList();

                            if (out_of_storage_state == null && out_of_storage_history_operational.Count == 0)
                            {
                                //if timestamp is before other states, after_timestamp list should contain at least one record
                                error_list.Clear();
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "36",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = previous_timestamp,
                                        required_quantity = 0
                                    }
                                );

                                return error_list;
                            }

                            foreach (var out_of_storage_next_state in out_of_storage_history_operational)
                            {
                                out_of_storage_next_state.total_quantity += lease_item_on_protocol.total_quantity;
                            }

                        }


                    }



                    if (previous_state.Equals(Protocol_State.Reserved) && new_state.Equals(Protocol_State.Reserved))
                    {
                        if (new_timestamp > previous_timestamp)
                        {

                            //in storage stock history after previous_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp).ToList();

                            min_stock_state = in_storage_stock_history_operational.MinBy(ishat => ishat.blocked_quantity);

                            if (min_stock_state != null)
                            {
                                if (min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // miscallculation (all of stock_states after current should contain at least current amount of blocked items, until blockade is removed)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }

                            //stock history between old and new timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > previous_timestamp && sh.timestamp < new_timestamp).ToList();

                            //in storage stock history between old and new timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp && ish.timestamp < new_timestamp).ToList();


                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            foreach(var between_state in stock_history_operational)
                            {
                                between_state.blocked_quantity -= lease_item_on_protocol.total_quantity;
                            }
                            
                            foreach(var in_storage_between_state in in_storage_stock_history_operational)
                            {
                                in_storage_between_state.blocked_quantity -= lease_item_on_protocol.total_quantity;
                            }

                        }
                        else if (new_timestamp < previous_timestamp)
                        {

                            if (latest_stock_state_before_new_timestamp.in_storage_quantity - latest_stock_state_before_new_timestamp.blocked_quantity < lease_item_on_protocol.total_quantity)
                            {
                                // not enough in storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "22",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = latest_stock_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - (latest_stock_state_before_new_timestamp.in_storage_quantity - latest_stock_state_before_new_timestamp.blocked_quantity)
                                    }
                                );
                                continue;
                            }

                            if (in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity - in_storage_latest_stock_state_before_new_timestamp.blocked_quantity < lease_item_on_protocol.total_quantity)
                            {
                                // not enough in storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "22",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = in_storage_latest_stock_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - (in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity - in_storage_latest_stock_state_before_new_timestamp.blocked_quantity)
                                    }
                                );
                                continue;
                            }
                            
                            
                            //checking if it is possible to block item_on_protocol quantity in stock_states between new and current
                            min_stock_state = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp && ish.timestamp < previous_timestamp).MinBy(ishat => ishat.in_storage_quantity - ishat.blocked_quantity);

                            if (min_stock_state != null)
                            {
                                if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // not enough in storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "22",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                                        }
                                    );
                                    continue;
                                }
                            }


                            //in storage stock history after previous_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp).ToList();


                            //checking if all of stock_states after current have at least item_on_protocol quantity in blocked state
                            min_stock_state = in_storage_stock_history_operational.MinBy(ishat => ishat.blocked_quantity);

                            if (min_stock_state != null)
                            {
                                if (min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // miscallculation (all of stock_states after current should contain at least current amount of blocked items, until blockade is removed)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }


                            //stock history between new and old timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > new_timestamp && sh.timestamp < previous_timestamp).ToList();
                            
                            //in storage stock history between new and old timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp && ish.timestamp < previous_timestamp).ToList();

                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity + lease_item_on_protocol.total_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity + lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            foreach(var between_state in stock_history_operational)
                            {
                                between_state.blocked_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach(var in_storage_between_state in in_storage_stock_history_operational)
                            {
                                in_storage_between_state.blocked_quantity += lease_item_on_protocol.total_quantity;
                            }

                        }
                    }

                    if (previous_state.Equals(Protocol_State.Reserved) && new_state.Equals(Protocol_State.Draft))
                    {
                        if (previous_timestamp.Equals(new_timestamp))
                        {
                            //stock history after previous_timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > previous_timestamp).ToList();

                            //in storage stock history after previous_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp).ToList();


                            min_stock_state = in_storage_stock_history_operational.MinBy(ishat => ishat.blocked_quantity);

                            if (min_stock_state != null)
                            {
                                if (min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // miscallculation (all of stock_states after current should contain at least current amount of blocked items, until blockade is removed)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }

                            current_stock_state.blocked_quantity -= lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.blocked_quantity -= lease_item_on_protocol.total_quantity;

                            foreach(var next_state in stock_history_operational)
                            {
                                next_state.blocked_quantity -= lease_item_on_protocol.total_quantity;
                            }

                            foreach(var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.blocked_quantity -= lease_item_on_protocol.total_quantity;
                            }

                        }
                        else if (new_timestamp > previous_timestamp)
                        {

                            //stock history after previous_timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > previous_timestamp).ToList();

                            //in storage stock history after previous_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp).ToList();


                            min_stock_state = in_storage_stock_history_operational.MinBy(ishat => ishat.blocked_quantity);

                            if (min_stock_state != null)
                            {
                                if (min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // miscallculation (all of stock_states after current should contain at least current amount of blocked items, until blockade is removed)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }

                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity - lease_item_on_protocol.total_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity - lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            foreach(var next_state in stock_history_operational)
                            {
                                next_state.blocked_quantity -= lease_item_on_protocol.total_quantity;
                            }

                            foreach(var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.blocked_quantity -= lease_item_on_protocol.total_quantity;
                            }

                        }
                        else if (new_timestamp < previous_timestamp)
                        {

                            if (latest_stock_state_before_new_timestamp.in_storage_quantity - latest_stock_state_before_new_timestamp.blocked_quantity < lease_item_on_protocol.total_quantity)
                            {
                                // not enough in storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "22",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = latest_stock_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - (latest_stock_state_before_new_timestamp.in_storage_quantity - latest_stock_state_before_new_timestamp.blocked_quantity)
                                    }
                                );
                                continue;
                            }

                            if (in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity - in_storage_latest_stock_state_before_new_timestamp.blocked_quantity < lease_item_on_protocol.total_quantity)
                            {
                                // not enough in storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "22",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = in_storage_latest_stock_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - (in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity - in_storage_latest_stock_state_before_new_timestamp.blocked_quantity)
                                    }
                                );
                                continue;
                            }
                            
                            
                            //checking if it is possible to release (but it is draft) item_on_protocol quantity in stock_states between new and current
                            min_stock_state = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp && ish.timestamp < previous_timestamp).MinBy(ishat => ishat.in_storage_quantity - ishat.blocked_quantity);

                            if (min_stock_state != null)
                            {
                                if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // not enough in storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "22",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                                        }
                                    );
                                    continue;
                                }
                            }


                            //stock history after previous_timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > previous_timestamp).ToList();

                            //in storage stock history after previous_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp).ToList();


                            //checking if all of stock_states after current have at least item_on_protocol quantity in blocked state
                            min_stock_state = in_storage_stock_history_operational.MinBy(ishat => ishat.blocked_quantity);

                            if (min_stock_state != null)
                            {
                                if (min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // miscallculation (all of stock_states after current should contain at least current amount of blocked items, until blockade is removed)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }
                            
                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            foreach (var next_state in stock_history_operational)
                            {
                                next_state.blocked_quantity -= lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.blocked_quantity -= lease_item_on_protocol.total_quantity;
                            }

                        }
                    }

                    if (previous_state.Equals(Protocol_State.Reserved) && new_state.Equals(Protocol_State.Confirmed))
                    {
                        if (previous_timestamp.Equals(new_timestamp))
                        {
                            //stock history after previous_timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > previous_timestamp).ToList();

                            //in storage stock history after previous_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp).ToList();


                            min_stock_state = in_storage_stock_history_operational.MinBy(ishat => ishat.blocked_quantity);

                            if (min_stock_state != null)
                            {
                                if (min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // miscallculation (all of stock_states after current should contain at least current amount of blocked items, until blockade is removed)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }

                            current_stock_state.blocked_quantity -= lease_item_on_protocol.total_quantity;
                            current_stock_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                            current_stock_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;

                            in_storage_current_stock_state.blocked_quantity -= lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;

                            foreach (var next_state in stock_history_operational)
                            {
                                next_state.blocked_quantity -= lease_item_on_protocol.total_quantity;
                                next_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                                next_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.blocked_quantity -= lease_item_on_protocol.total_quantity;
                                in_storage_next_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                                in_storage_next_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                        }
                        else if (new_timestamp > previous_timestamp)
                        {

                            //in storage stock history after previous_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp).ToList();


                            min_stock_state = in_storage_stock_history_operational.MinBy(ishat => ishat.blocked_quantity);

                            if (min_stock_state != null)
                            {
                                if (min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // miscallculation (all of stock_states after current should contain at least current amount of blocked items, until blockade is removed)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }
                            
                            //stock history between old and new timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > previous_timestamp && sh.timestamp < new_timestamp).ToList();
                            
                            //in storage stock history between old and new timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp && ish.timestamp < new_timestamp).ToList();

                            //stock history after new timestamp
                            stock_history_operational_second = stock_history.Where(sh => sh.timestamp > new_timestamp).ToList();
                            
                            //in storage stock history after new timestamp
                            in_storage_stock_history_operational_second = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp).ToList();

                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity - lease_item_on_protocol.total_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity - lease_item_on_protocol.total_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity + lease_item_on_protocol.total_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity - lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity - lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity + lease_item_on_protocol.total_quantity;

                            foreach (var between_state in stock_history_operational)
                            {
                                between_state.blocked_quantity -= lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_between_state in in_storage_stock_history_operational)
                            {
                                in_storage_between_state.blocked_quantity -= lease_item_on_protocol.total_quantity;
                            }

                            foreach (var next_state in stock_history_operational_second)
                            {
                                next_state.blocked_quantity -= lease_item_on_protocol.total_quantity;
                                next_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                                next_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_next_state in in_storage_stock_history_operational_second)
                            {
                                in_storage_next_state.blocked_quantity -= lease_item_on_protocol.total_quantity;
                                in_storage_next_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                                in_storage_next_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                        }
                        else if (new_timestamp < previous_timestamp)
                        {

                            if (latest_stock_state_before_new_timestamp.in_storage_quantity - latest_stock_state_before_new_timestamp.blocked_quantity < lease_item_on_protocol.total_quantity)
                            {
                                // not enough in storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "22",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = latest_stock_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - (latest_stock_state_before_new_timestamp.in_storage_quantity - latest_stock_state_before_new_timestamp.blocked_quantity)
                                    }
                                );
                                continue;
                            }

                            if (in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity - in_storage_latest_stock_state_before_new_timestamp.blocked_quantity < lease_item_on_protocol.total_quantity)
                            {
                                // not enough in storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "22",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = in_storage_latest_stock_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - (in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity - in_storage_latest_stock_state_before_new_timestamp.blocked_quantity)
                                    }
                                );
                                continue;
                            }
                            
                            
                            //checking if it is possible to release item_on_protocol quantity in stock_states between new and current
                            min_stock_state = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp && ish.timestamp < previous_timestamp).MinBy(ishat => ishat.in_storage_quantity - ishat.blocked_quantity);

                            if (min_stock_state != null)
                            {
                                if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // not enough in storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "22",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                                        }
                                    );
                                    continue;
                                }
                            }

                            //stock history after previous_timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > previous_timestamp).ToList();

                            //in storage stock history after previous_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp).ToList();


                            //checking if all of stock_states after current have at least item_on_protocol quantity in blocked state
                            min_stock_state = in_storage_stock_history_operational.MinBy(ishat => ishat.blocked_quantity);

                            if (min_stock_state != null)
                            {
                                if (min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // miscallculation (all of stock_states after current should contain at least current amount of blocked items, until blockade is removed)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }

                            //stock history between new and old timestamp
                            stock_history_operational_second = stock_history.Where(sh => sh.timestamp > new_timestamp && sh.timestamp < previous_timestamp).ToList();
                            
                            //in storage stock history between new and old timestamp
                            in_storage_stock_history_operational_second = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp && ish.timestamp < previous_timestamp).ToList();

                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity - lease_item_on_protocol.total_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity + lease_item_on_protocol.total_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity - lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity + lease_item_on_protocol.total_quantity;

                            foreach (var between_state in stock_history_operational_second)
                            {
                                between_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                                between_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_between_state in in_storage_stock_history_operational_second)
                            {
                                in_storage_between_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                                in_storage_between_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var next_state in stock_history_operational)
                            {
                                next_state.blocked_quantity -= lease_item_on_protocol.total_quantity;
                                next_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                                next_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.blocked_quantity -= lease_item_on_protocol.total_quantity;
                                in_storage_next_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                                in_storage_next_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                        }

                        //
                        // new_timestamp is used for both: when previous_timestamp == new_timestamp and previous_timestamp != new_timestamp cases, because
                        // if timestamps are equal it doesnt matter which is used, but new_timestamp is required to "not equal" cases
                        //
                        if (out_of_storage_object == null)
                        {
                            //if out_of_storage object doesnt exist - it means that it is first release of this item in current order
                            Lease_Item_Out_Of_Storage new_out_of_storage_object = new Lease_Item_Out_Of_Storage
                            {
                                lease_item_in_storage_FKid = lease_item_on_protocol.lease_item_in_storage_FKid,
                                order_FKid = order_id,
                                lease_item_out_of_storage_history_FK =
                                [
                                    new Lease_Item_Out_Of_Storage_History { total_quantity = lease_item_on_protocol.total_quantity, timestamp = new_timestamp }
                                ]
                            };

                            _context.Lease_Item_Out_Of_Storage.Add(new_out_of_storage_object);
                        }
                        else
                        {
                            if (
                                out_of_storage_object.lease_item_out_of_storage_history_FK == null ||
                                out_of_storage_object.lease_item_out_of_storage_history_FK.Count == 0
                            )
                            {
                                throw new Exception("19");//not found
                            }

                            Timestamp_Validator.Validate_Protocol_Timestamp(out_of_storage_object.lease_item_out_of_storage_history_FK, new_timestamp);

                            out_of_storage_state = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp < new_timestamp).MaxBy(losh => losh.timestamp);

                            Lease_Item_Out_Of_Storage_History out_of_storage_update = new Lease_Item_Out_Of_Storage_History
                            {
                                timestamp = new_timestamp,
                                lease_item_out_of_storage_FKid = out_of_storage_object.id
                            };

                            if (out_of_storage_state == null)
                            {
                                //current timestamp is before all of out_of_storage states

                                out_of_storage_update.total_quantity = lease_item_on_protocol.total_quantity;
                            }
                            else
                            {
                                //current timestamp is after latest state

                                if (previous_timestamp.Equals(new_timestamp))
                                {
                                    Stock_State_Validator.Validate_Out_Of_Storage_State(out_of_storage_state, in_storage_current_stock_state.out_of_storage_quantity);
                                }
                                else
                                {
                                    Stock_State_Validator.Validate_Out_Of_Storage_State(out_of_storage_state, in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity);
                                }

                                out_of_storage_update.total_quantity = out_of_storage_state.total_quantity + lease_item_on_protocol.total_quantity;
                            }

                            _context.Lease_Item_Out_Of_Storage_History.Add(out_of_storage_update);

                            out_of_storage_history_operational = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp > new_timestamp).ToList();

                            if (out_of_storage_state == null && out_of_storage_history_operational.Count == 0)
                            {
                                //if timestamp is before other states, after_timestamp list should contain at least one record
                                error_list.Clear();
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "36",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = previous_timestamp,
                                        required_quantity = 0
                                    }
                                );

                                return error_list;
                            }

                            foreach (var out_of_storage_next_state in out_of_storage_history_operational)
                            {
                                out_of_storage_next_state.total_quantity += lease_item_on_protocol.total_quantity;
                            }

                        }


                    }



                    if (previous_state.Equals(Protocol_State.Confirmed) && new_state.Equals(Protocol_State.Confirmed))
                    {

                        Timestamp_Validator.Validate_Protocol_Timestamp(out_of_storage_object.lease_item_out_of_storage_history_FK, new_timestamp);

                        out_of_storage_state_before_new_timestamp = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp < new_timestamp).MaxBy(losh => losh.timestamp);


                        if (new_timestamp > previous_timestamp)
                        {
                                                                                    
                            if(out_of_storage_state_before_new_timestamp == null)
                            {
                                throw new Exception("19");//not found
                            }

                            //out of storage between old and new timestamp
                            out_of_storage_history_operational = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp > previous_timestamp && losh.timestamp < new_timestamp).ToList();
                            
                            min_out_of_storage = out_of_storage_history_operational.MinBy(ous => ous.total_quantity);

                            if(min_out_of_storage != null)
                            {
                                if(min_out_of_storage.total_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    //not enough out of storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "43",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_out_of_storage.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - min_out_of_storage.total_quantity
                                        }
                                    );
                                    continue;
                                }
                            }

                            //in storage stock history between old and new timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp && ish.timestamp < new_timestamp).ToList();
                            

                            min_stock_state = in_storage_stock_history_operational.MinBy(sh => sh.out_of_storage_quantity);
                            if(min_stock_state != null)
                            {
                                if(min_stock_state.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // miscallculation (if out_of_storage_quantity is less than on_protocol quantity, then min_out_of_storage quantity (checked above) should also be less)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }

                            //stock history between old and new timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > previous_timestamp && sh.timestamp < new_timestamp).ToList();
                            

                            out_of_storage_state.timestamp = new_timestamp;
                            out_of_storage_state.total_quantity = out_of_storage_state_before_new_timestamp.total_quantity;
                            
                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            foreach(var out_of_storage_between_state in out_of_storage_history_operational)
                            {
                                out_of_storage_between_state.total_quantity -= lease_item_on_protocol.total_quantity;
                            }

                            foreach (var between_state in stock_history_operational)
                            {
                                between_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                                between_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_between_state in in_storage_stock_history_operational)
                            {
                                in_storage_between_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                                in_storage_between_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                        }
                        else if (new_timestamp < previous_timestamp)
                        {

                            if (latest_stock_state_before_new_timestamp.in_storage_quantity - latest_stock_state_before_new_timestamp.blocked_quantity < lease_item_on_protocol.total_quantity)
                            {
                                // not enough in storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "22",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = latest_stock_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - (latest_stock_state_before_new_timestamp.in_storage_quantity - latest_stock_state_before_new_timestamp.blocked_quantity)
                                    }
                                );
                                continue;
                            }

                            if (in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity - in_storage_latest_stock_state_before_new_timestamp.blocked_quantity < lease_item_on_protocol.total_quantity)
                            {
                                // not enough in storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "22",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = in_storage_latest_stock_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - (in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity - in_storage_latest_stock_state_before_new_timestamp.blocked_quantity)
                                    }
                                );
                                continue;
                            }

                            //in storage stock history between new and old timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp && ish.timestamp < previous_timestamp).ToList();


                            //checking if it is possible to release item_on_protocol quantity in stock_states between new and current
                            min_stock_state = in_storage_stock_history_operational.MinBy(ishat => ishat.in_storage_quantity - ishat.blocked_quantity);

                            if (min_stock_state != null)
                            {
                                if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // not enough in storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "22",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                                        }
                                    );
                                    continue;
                                }
                            }
                            
                            //out of storage between new and old timestamp
                            out_of_storage_history_operational = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp > new_timestamp && losh.timestamp < previous_timestamp).ToList();

                            //stock history between new and old timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > new_timestamp && sh.timestamp < previous_timestamp).ToList();
                            

                            if(out_of_storage_state_before_new_timestamp != null)
                            {
                                out_of_storage_state.timestamp = new_timestamp;
                                out_of_storage_state.total_quantity = out_of_storage_state_before_new_timestamp.total_quantity + lease_item_on_protocol.total_quantity;
                            }
                            else
                            {
                                out_of_storage_state.timestamp = new_timestamp;
                                out_of_storage_state.total_quantity = lease_item_on_protocol.total_quantity;
                            }

                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity - lease_item_on_protocol.total_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity + lease_item_on_protocol.total_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity - lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity + lease_item_on_protocol.total_quantity;

                            foreach(var out_of_storage_between_state in out_of_storage_history_operational)
                            {
                                out_of_storage_between_state.total_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var between_state in stock_history_operational)
                            {
                                between_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                                between_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_between_state in in_storage_stock_history_operational)
                            {
                                in_storage_between_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                                in_storage_between_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                            }

                        }
                    }

                    if (previous_state.Equals(Protocol_State.Confirmed) && new_state.Equals(Protocol_State.Draft))
                    {
                        if (previous_timestamp.Equals(new_timestamp))
                        {
                            //out of storage states after previous_timestamp
                            out_of_storage_history_operational = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp > previous_timestamp).ToList();

                            min_out_of_storage = out_of_storage_history_operational.MinBy(losh => losh.total_quantity);
                            if(min_out_of_storage != null)
                            {
                                if(min_out_of_storage.total_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    //not enough out of storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "43",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_out_of_storage.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - min_out_of_storage.total_quantity
                                        }
                                    );
                                    continue;
                                }
                            }

                            //in storage stock history after previous_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp).ToList();


                            min_stock_state = in_storage_stock_history_operational.MinBy(ish => ish.out_of_storage_quantity);
                            if(min_stock_state != null)
                            {
                                if(min_stock_state.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // miscallculation (if out_of_storage_quantity is less than on_protocol quantity, then min_out_of_storage quantity (checked above) should also be less)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }
                            
                            //stock history after previous_timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > previous_timestamp).ToList();
                            

                            out_of_storage_state.total_quantity -= lease_item_on_protocol.total_quantity;

                            current_stock_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                            current_stock_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;

                            in_storage_current_stock_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;

                            foreach(var next_state in stock_history_operational)
                            {
                                next_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                                next_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                                in_storage_next_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var out_of_storage_next_state in out_of_storage_history_operational)
                            {
                                out_of_storage_next_state.total_quantity -= lease_item_on_protocol.total_quantity;
                            }


                            if (out_of_storage_state_before_current != null)
                            {
                                if (out_of_storage_state.total_quantity.Equals(out_of_storage_state_before_current.total_quantity))
                                {
                                    _context.Lease_Item_Out_Of_Storage_History.Remove(out_of_storage_state);
                                }
                                else
                                {
                                    //miscallculation (previous history record should differ from current only in item_on_protocol quantity, so if quantity is removed there should be no difference)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = out_of_storage_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }
                            else
                            {
                                //if current record is the only one in out_of_storage history
                                if (out_of_storage_object.lease_item_out_of_storage_history_FK.Count.Equals(1))
                                {
                                    _context.Lease_Item_Out_Of_Storage_History.Remove(out_of_storage_state);
                                    _context.Lease_Item_Out_Of_Storage.Remove(out_of_storage_object);
                                }
                                else //if current record is before the rest of history records
                                {
                                    _context.Lease_Item_Out_Of_Storage_History.Remove(out_of_storage_state);
                                }
                            }

                        }
                        else if (new_timestamp > previous_timestamp)
                        {                           
                            
                            //out of storage after previous_timestamp
                            out_of_storage_history_operational = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp > previous_timestamp).ToList();
                            

                            min_out_of_storage = out_of_storage_history_operational.MinBy(ous => ous.total_quantity);

                            if (min_out_of_storage != null)
                            {
                                if (min_out_of_storage.total_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    //not enough out of storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "43",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_out_of_storage.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - min_out_of_storage.total_quantity
                                        }
                                    );
                                    continue;
                                }
                            }

                            //in storage stock history after previous_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp).ToList();


                            min_stock_state = in_storage_stock_history_operational.MinBy(sh => sh.out_of_storage_quantity);
                            if (min_stock_state != null)
                            {
                                if (min_stock_state.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // miscallculation (if out_of_storage_quantity is less than on_protocol quantity, then min_out_of_storage quantity (checked above) should also be less)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }

                            //stock history after previous_timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > previous_timestamp).ToList();

                            
                            out_of_storage_state.total_quantity -= lease_item_on_protocol.total_quantity;

                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity + lease_item_on_protocol.total_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity - lease_item_on_protocol.total_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity + lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity - lease_item_on_protocol.total_quantity;

                            
                            foreach (var out_of_storage_next_state in out_of_storage_history_operational)
                            {
                                out_of_storage_next_state.total_quantity -= lease_item_on_protocol.total_quantity;
                            }

                            foreach (var next_state in stock_history_operational)
                            {
                                next_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                                next_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                                in_storage_next_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                            }                                                        

                            //if current out_of_storage record is the only one in out_of_storage history
                            if (out_of_storage_object.lease_item_out_of_storage_history_FK.Count.Equals(1))
                            {
                                _context.Lease_Item_Out_Of_Storage_History.Remove(out_of_storage_state);
                                _context.Lease_Item_Out_Of_Storage.Remove(out_of_storage_object);
                            }
                            else
                            {
                                _context.Lease_Item_Out_Of_Storage_History.Remove(out_of_storage_state);
                            }

                        }
                        else if (new_timestamp < previous_timestamp)
                        {

                            if (latest_stock_state_before_new_timestamp.in_storage_quantity - latest_stock_state_before_new_timestamp.blocked_quantity < lease_item_on_protocol.total_quantity)
                            {
                                // not enough in storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "22",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = latest_stock_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - (latest_stock_state_before_new_timestamp.in_storage_quantity - latest_stock_state_before_new_timestamp.blocked_quantity)
                                    }
                                );
                                continue;
                            }

                            if (in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity - in_storage_latest_stock_state_before_new_timestamp.blocked_quantity < lease_item_on_protocol.total_quantity)
                            {
                                // not enough in storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "22",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = in_storage_latest_stock_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - (in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity - in_storage_latest_stock_state_before_new_timestamp.blocked_quantity)
                                    }
                                );
                                continue;
                            }
                            

                            //checking if it is possible to release (but it is draft) item_on_protocol quantity in stock_states between new and current
                            min_stock_state = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp && ish.timestamp < previous_timestamp).MinBy(ishat => ishat.in_storage_quantity - ishat.blocked_quantity);

                            if (min_stock_state != null)
                            {
                                if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // not enough in storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "22",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                                        }
                                    );
                                    continue;
                                }
                            }

                            //out of storage after previous_timestamp
                            out_of_storage_history_operational = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp > previous_timestamp).ToList();


                            //checking if it is possible to cancel release after current timestamp
                            min_out_of_storage = out_of_storage_history_operational.MinBy(losh => losh.total_quantity);
                            if(min_out_of_storage != null)
                            {
                                if(min_out_of_storage.total_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    //not enough out of storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "43",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_out_of_storage.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - min_out_of_storage.total_quantity
                                        }
                                    );
                                    continue;
                                }
                            }
                            
                            //in storage stock history after previous_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp).ToList();


                            min_stock_state = in_storage_stock_history_operational.MinBy(ish => ish.out_of_storage_quantity);
                            if(min_stock_state != null)
                            {
                                if(min_stock_state.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // miscallculation (if out_of_storage_quantity is less than on_protocol quantity, then min_out_of_storage quantity (checked above) should also be less)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }

                            //stock history after previous_timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > previous_timestamp).ToList();

                            out_of_storage_state.total_quantity -= lease_item_on_protocol.total_quantity;

                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            foreach (var out_of_storage_next_state in out_of_storage_history_operational)
                            {
                                out_of_storage_next_state.total_quantity -= lease_item_on_protocol.total_quantity;
                            }

                            foreach (var next_state in stock_history_operational)
                            {
                                next_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                                next_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                                in_storage_next_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                            //if current out_of_storage record is the only one in out_of_storage history
                            if (out_of_storage_object.lease_item_out_of_storage_history_FK.Count.Equals(1))
                            {
                                _context.Lease_Item_Out_Of_Storage_History.Remove(out_of_storage_state);
                                _context.Lease_Item_Out_Of_Storage.Remove(out_of_storage_object);
                            }
                            else
                            {
                                _context.Lease_Item_Out_Of_Storage_History.Remove(out_of_storage_state);
                            }

                        }
                    }

                    if (previous_state.Equals(Protocol_State.Confirmed) && new_state.Equals(Protocol_State.Reserved))
                    {
                        if (previous_timestamp.Equals(new_timestamp))
                        {

                            //out of storage states after previous_timestamp
                            out_of_storage_history_operational = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp > previous_timestamp).ToList();

                            min_out_of_storage = out_of_storage_history_operational.MinBy(losh => losh.total_quantity);
                            if (min_out_of_storage != null)
                            {
                                if (min_out_of_storage.total_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    //not enough out of storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "43",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_out_of_storage.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - min_out_of_storage.total_quantity
                                        }
                                    );
                                    continue;
                                }
                            }

                            //in storage stock history after previous_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp).ToList();


                            min_stock_state = in_storage_stock_history_operational.MinBy(ish => ish.out_of_storage_quantity);
                            if (min_stock_state != null)
                            {
                                if (min_stock_state.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // miscallculation (if out_of_storage_quantity is less than on_protocol quantity, then min_out_of_storage quantity (checked above) should also be less)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }

                            //stock history after previous_timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > previous_timestamp).ToList();


                            out_of_storage_state.total_quantity -= lease_item_on_protocol.total_quantity;

                            current_stock_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                            current_stock_state.blocked_quantity += lease_item_on_protocol.total_quantity;
                            current_stock_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;

                            in_storage_current_stock_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.blocked_quantity += lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;

                            foreach (var next_state in stock_history_operational)
                            {
                                next_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                                next_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                                next_state.blocked_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                                in_storage_next_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                                in_storage_next_state.blocked_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var out_of_storage_next_state in out_of_storage_history_operational)
                            {
                                out_of_storage_next_state.total_quantity -= lease_item_on_protocol.total_quantity;
                            }


                            if (out_of_storage_state_before_current != null)
                            {
                                if (out_of_storage_state.total_quantity.Equals(out_of_storage_state_before_current.total_quantity))
                                {
                                    _context.Lease_Item_Out_Of_Storage_History.Remove(out_of_storage_state);
                                }
                                else
                                {
                                    //miscallculation (previous history record should differ from current only in item_on_protocol quantity, so if quantity is removed there should be no difference)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = out_of_storage_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }
                            else
                            {
                                //if current record is the only one in out_of_storage history
                                if (out_of_storage_object.lease_item_out_of_storage_history_FK.Count.Equals(1))
                                {
                                    _context.Lease_Item_Out_Of_Storage_History.Remove(out_of_storage_state);
                                    _context.Lease_Item_Out_Of_Storage.Remove(out_of_storage_object);
                                }
                                else //if current record is before the rest of history records
                                {
                                    _context.Lease_Item_Out_Of_Storage_History.Remove(out_of_storage_state);
                                }
                            }

                        }
                        else if (new_timestamp > previous_timestamp)
                        {

                            //out of storage states after previous_timestamp
                            out_of_storage_history_operational = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp > previous_timestamp).ToList();

                            min_out_of_storage = out_of_storage_history_operational.MinBy(ous => ous.total_quantity);

                            if (min_out_of_storage != null)
                            {
                                if (min_out_of_storage.total_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    //not enough out of storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "43",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_out_of_storage.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - min_out_of_storage.total_quantity
                                        }
                                    );
                                    continue;
                                }
                            }

                            min_stock_state = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp).MinBy(sh => sh.out_of_storage_quantity);
                            if (min_stock_state != null)
                            {
                                if (min_stock_state.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // miscallculation (if out_of_storage_quantity is less than on_protocol quantity, then min_out_of_storage quantity (checked above) should also be less)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }
                            
                            //stock history between old and new timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > previous_timestamp && sh.timestamp < new_timestamp).ToList();
                            
                            //in storage stock history between old and new timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp && ish.timestamp < new_timestamp).ToList();

                            //stock history after new_timestamp
                            stock_history_operational_second = stock_history.Where(sh => sh.timestamp > new_timestamp).ToList();
                            
                            //in storage stock history after new_timestamp
                            in_storage_stock_history_operational_second = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp).ToList();


                            out_of_storage_state.total_quantity -= lease_item_on_protocol.total_quantity;

                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity + lease_item_on_protocol.total_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity + lease_item_on_protocol.total_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity - lease_item_on_protocol.total_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity + lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity + lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity - lease_item_on_protocol.total_quantity;

                            foreach (var out_of_storage_next_state in out_of_storage_history_operational)
                            {
                                out_of_storage_next_state.total_quantity -= lease_item_on_protocol.total_quantity;
                            }

                            foreach (var between_state in stock_history_operational)
                            {
                                between_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                                between_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_between_state in in_storage_stock_history_operational)
                            {
                                in_storage_between_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                                in_storage_between_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                            }



                            foreach (var next_state in stock_history_operational_second)
                            {
                                next_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                                next_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                                next_state.blocked_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_next_state in in_storage_stock_history_operational_second)
                            {
                                in_storage_next_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                                in_storage_next_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                                in_storage_next_state.blocked_quantity += lease_item_on_protocol.total_quantity;
                            }

                            //if current out_of_storage record is the only one in out_of_storage history
                            if (out_of_storage_object.lease_item_out_of_storage_history_FK.Count.Equals(1))
                            {
                                _context.Lease_Item_Out_Of_Storage_History.Remove(out_of_storage_state);
                                _context.Lease_Item_Out_Of_Storage.Remove(out_of_storage_object);
                            }
                            else
                            {
                                _context.Lease_Item_Out_Of_Storage_History.Remove(out_of_storage_state);
                            }

                        }
                        else if (new_timestamp < previous_timestamp)
                        {

                            if (latest_stock_state_before_new_timestamp.in_storage_quantity - latest_stock_state_before_new_timestamp.blocked_quantity < lease_item_on_protocol.total_quantity)
                            {
                                // not enough in storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "22",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = latest_stock_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - (latest_stock_state_before_new_timestamp.in_storage_quantity - latest_stock_state_before_new_timestamp.blocked_quantity)
                                    }
                                );
                                continue;
                            }

                            if (in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity - in_storage_latest_stock_state_before_new_timestamp.blocked_quantity < lease_item_on_protocol.total_quantity)
                            {
                                // not enough in storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "22",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = in_storage_latest_stock_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - (in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity - in_storage_latest_stock_state_before_new_timestamp.blocked_quantity)
                                    }
                                );
                                continue;
                            }
                            
                            
                            //in storage stock history between new and old timestamp
                            in_storage_stock_history_operational_second = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp && ish.timestamp < previous_timestamp).ToList();


                            //checking if it is possible to block (reserve) item_on_protocol quantity in stock_states between new and current
                            min_stock_state = in_storage_stock_history_operational_second.MinBy(ishat => ishat.in_storage_quantity - ishat.blocked_quantity);

                            if (min_stock_state != null)
                            {
                                if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // not enough in storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "22",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                                        }
                                    );
                                    continue;
                                }
                            }

                            //out of storage states after previous_timestamp
                            out_of_storage_history_operational = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp > previous_timestamp).ToList();


                            //checking if it is possible to cancel release after current timestamp
                            min_out_of_storage = out_of_storage_history_operational.MinBy(losh => losh.total_quantity);
                            if (min_out_of_storage != null)
                            {
                                if (min_out_of_storage.total_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    //not enough out of storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "43",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_out_of_storage.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - min_out_of_storage.total_quantity
                                        }
                                    );
                                    continue;
                                }
                            }

                            //in storage stock history after previous_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp).ToList();


                            min_stock_state = in_storage_stock_history_operational.MinBy(ish => ish.out_of_storage_quantity);
                            if (min_stock_state != null)
                            {
                                if (min_stock_state.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // miscallculation (if out_of_storage_quantity is less than on_protocol quantity, then min_out_of_storage quantity (checked above) should also be less)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }

                            //stock history after previous_timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > previous_timestamp).ToList();

                            //stock history between new and old timestamp
                            stock_history_operational_second = stock_history.Where(sh => sh.timestamp > new_timestamp && sh.timestamp < previous_timestamp).ToList();

                            out_of_storage_state.total_quantity -= lease_item_on_protocol.total_quantity;

                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity + lease_item_on_protocol.total_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity + lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            foreach (var between_state in stock_history_operational_second)
                            {
                                between_state.blocked_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_between_state in in_storage_stock_history_operational_second)
                            {
                                in_storage_between_state.blocked_quantity += lease_item_on_protocol.total_quantity;
                            }


                            foreach (var out_of_storage_next_state in out_of_storage_history_operational)
                            {
                                out_of_storage_next_state.total_quantity -= lease_item_on_protocol.total_quantity;
                            }

                            foreach (var next_state in stock_history_operational)
                            {
                                next_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                                next_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                                next_state.blocked_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                                in_storage_next_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                                in_storage_next_state.blocked_quantity += lease_item_on_protocol.total_quantity;
                            }

                            //if current out_of_storage record is the only one in out_of_storage history
                            if (out_of_storage_object.lease_item_out_of_storage_history_FK.Count.Equals(1))
                            {
                                _context.Lease_Item_Out_Of_Storage_History.Remove(out_of_storage_state);
                                _context.Lease_Item_Out_Of_Storage.Remove(out_of_storage_object);
                            }
                            else
                            {
                                _context.Lease_Item_Out_Of_Storage_History.Remove(out_of_storage_state);
                            }

                        }
                    }

                }

                if (editing_lease_protocol.type.Equals(Lease_Protocol_Type.Return))
                {

                    if (previous_state.Equals(Protocol_State.Draft) && new_state.Equals(Protocol_State.Draft))
                    {
                        
                        if (new_timestamp > previous_timestamp)
                        {

                            if (out_of_storage_state_before_new_timestamp.total_quantity < lease_item_on_protocol.total_quantity)
                            {
                                //not enough out of storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "43",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = out_of_storage_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - out_of_storage_state_before_new_timestamp.total_quantity
                                    }
                                );
                                continue;
                            }


                            min_out_of_storage = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp > new_timestamp).MinBy(ous => ous.total_quantity);

                            if (min_out_of_storage != null)
                            {
                                if (min_out_of_storage.total_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    //not enough out of storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "43",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_out_of_storage.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - min_out_of_storage.total_quantity
                                        }
                                    );
                                    continue;
                                }
                            }

                            min_stock_state = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp).MinBy(sh => sh.out_of_storage_quantity);
                            if (min_stock_state != null)
                            {
                                if (min_stock_state.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // miscallculation (if out_of_storage_quantity is less than on_protocol quantity, then min_out_of_storage quantity (checked above) should also be less)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }

                            out_of_storage_state.timestamp = new_timestamp;
                            out_of_storage_state.total_quantity = out_of_storage_state_before_new_timestamp.total_quantity;

                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                        }
                        else if (new_timestamp < previous_timestamp)
                        {

                            if (out_of_storage_state_before_new_timestamp.total_quantity < lease_item_on_protocol.total_quantity)
                            {
                                //not enough out of storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "43",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = out_of_storage_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - out_of_storage_state_before_new_timestamp.total_quantity
                                    }
                                );
                                continue;
                            }

                            min_out_of_storage = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp > new_timestamp).MinBy(ous => ous.total_quantity);

                            if (min_out_of_storage != null)
                            {
                                if (min_out_of_storage.total_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    //not enough out of storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "43",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_out_of_storage.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - min_out_of_storage.total_quantity
                                        }
                                    );
                                    continue;
                                }
                            }
                            else
                            {
                                throw new Exception("19");//not found (if new timestamp is earlier than current, it means that there have to be at least one state after new timestamp (current state))
                            }

                            min_stock_state = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp).MinBy(sh => sh.out_of_storage_quantity);
                            if (min_stock_state != null)
                            {
                                if (min_stock_state.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // miscallculation (if out_of_storage_quantity is less than on_protocol quantity, then min_out_of_storage quantity (checked above) should also be less)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }
                            else
                            {
                                throw new Exception("19");//not found (if new timestamp is earlier than current, it means that there have to be at least one state after new timestamp (current state))
                            }

                            out_of_storage_state.timestamp = new_timestamp;
                            out_of_storage_state.total_quantity = out_of_storage_state_before_new_timestamp.total_quantity;

                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                        }

                    }
                    
                    if (previous_state.Equals(Protocol_State.Draft) && new_state.Equals(Protocol_State.Confirmed))
                    {
                        if (previous_timestamp.Equals(new_timestamp))
                        {

                            if(out_of_storage_state.total_quantity < lease_item_on_protocol.total_quantity)
                            {
                                //not enough out of storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "43",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = out_of_storage_state.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - out_of_storage_state.total_quantity
                                    }
                                );
                                continue;
                            }

                            //out of storage states after previous_timestamp
                            out_of_storage_history_operational = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp > previous_timestamp).ToList();


                            min_out_of_storage = out_of_storage_history_operational.MinBy(ous => ous.total_quantity);

                            if (min_out_of_storage != null)
                            {
                                if (min_out_of_storage.total_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    //not enough out of storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "43",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_out_of_storage.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - min_out_of_storage.total_quantity
                                        }
                                    );
                                    continue;
                                }
                            }

                            //in storage stock history after previous_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp).ToList();


                            min_stock_state = in_storage_stock_history_operational.MinBy(sh => sh.out_of_storage_quantity);
                            if (min_stock_state != null)
                            {
                                if (min_stock_state.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // miscallculation (if out_of_storage_quantity is less than on_protocol quantity, then min_out_of_storage quantity (checked above) should also be less)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }

                            //stock history after previous_timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > previous_timestamp).ToList();


                            out_of_storage_state.total_quantity -= lease_item_on_protocol.total_quantity;

                            current_stock_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                            current_stock_state.in_storage_quantity += lease_item_on_protocol.total_quantity;

                            in_storage_current_stock_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity += lease_item_on_protocol.total_quantity;

                            foreach(var out_of_storage_next_state in out_of_storage_history_operational)
                            {
                                out_of_storage_next_state.total_quantity -= lease_item_on_protocol.total_quantity;
                            }

                            foreach(var next_state in stock_history_operational)
                            {
                                next_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                                next_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach(var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                                in_storage_next_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                            }                            

                        }
                        else if (new_timestamp > previous_timestamp)
                        {

                            if (out_of_storage_state_before_new_timestamp.total_quantity < lease_item_on_protocol.total_quantity)
                            {
                                //not enough out of storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "43",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = out_of_storage_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - out_of_storage_state_before_new_timestamp.total_quantity
                                    }
                                );
                                continue;
                            }


                            //out of storage states after new timestamp
                            out_of_storage_history_operational = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp > new_timestamp).ToList();


                            min_out_of_storage = out_of_storage_history_operational.MinBy(ous => ous.total_quantity);

                            if (min_out_of_storage != null)
                            {
                                if (min_out_of_storage.total_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    //not enough out of storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "43",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_out_of_storage.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - min_out_of_storage.total_quantity
                                        }
                                    );
                                    continue;
                                }
                            }
                            
                            //in storage stock history after new timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp).ToList();


                            min_stock_state = in_storage_stock_history_operational.MinBy(sh => sh.out_of_storage_quantity);
                            if (min_stock_state != null)
                            {
                                if (min_stock_state.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // miscallculation (if out_of_storage_quantity is less than on_protocol quantity, then min_out_of_storage quantity (checked above) should also be less)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }

                            //stock history after new timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > new_timestamp).ToList();


                            out_of_storage_state.timestamp = new_timestamp;
                            out_of_storage_state.total_quantity = out_of_storage_state_before_new_timestamp.total_quantity - lease_item_on_protocol.total_quantity;

                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity + lease_item_on_protocol.total_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity - lease_item_on_protocol.total_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity + lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity - lease_item_on_protocol.total_quantity;

                            foreach(var out_of_storage_next_state in out_of_storage_history_operational)
                            {
                                out_of_storage_next_state.total_quantity -= lease_item_on_protocol.total_quantity;
                            }

                            foreach(var next_state in stock_history_operational)
                            {
                                next_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                                next_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach(var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                                in_storage_next_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                        }
                        else if (new_timestamp < previous_timestamp)
                        {

                            if (out_of_storage_state_before_new_timestamp.total_quantity < lease_item_on_protocol.total_quantity)
                            {
                                //not enough out of storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "43",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = out_of_storage_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - out_of_storage_state_before_new_timestamp.total_quantity
                                    }
                                );
                                continue;
                            }

                            //out of storage states after new timestamp
                            out_of_storage_history_operational = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp > new_timestamp).ToList();


                            min_out_of_storage = out_of_storage_history_operational.MinBy(ous => ous.total_quantity);

                            if (min_out_of_storage != null)
                            {
                                if (min_out_of_storage.total_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    //not enough out of storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "43",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_out_of_storage.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - min_out_of_storage.total_quantity
                                        }
                                    );
                                    continue;
                                }
                            }
                            else
                            {
                                throw new Exception("19");//not found (if new timestamp is earlier than current, it means that there have to be at least one state after new timestamp (current state))
                            }

                            //in storage stock history after new timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp).ToList();


                            min_stock_state = in_storage_stock_history_operational.MinBy(sh => sh.out_of_storage_quantity);
                            if (min_stock_state != null)
                            {
                                if (min_stock_state.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // miscallculation (if out_of_storage_quantity is less than on_protocol quantity, then min_out_of_storage quantity (checked above) should also be less)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }
                            else
                            {
                                throw new Exception("19");//not found (if new timestamp is earlier than current, it means that there have to be at least one state after new timestamp (current state))
                            }


                            //stock history after new timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > new_timestamp).ToList();



                            foreach (var out_of_storage_next_state in out_of_storage_history_operational)
                            {
                                out_of_storage_next_state.total_quantity -= lease_item_on_protocol.total_quantity;
                            }

                            foreach (var next_state in stock_history_operational)
                            {
                                next_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                                next_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                                in_storage_next_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                            }
                            
                            out_of_storage_state.timestamp = new_timestamp;
                            out_of_storage_state.total_quantity = out_of_storage_state_before_new_timestamp.total_quantity - lease_item_on_protocol.total_quantity;

                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity + lease_item_on_protocol.total_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity - lease_item_on_protocol.total_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity + lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity - lease_item_on_protocol.total_quantity;

                        }
                    }



                    if (previous_state.Equals(Protocol_State.Confirmed) && new_state.Equals(Protocol_State.Confirmed))
                    {
                        if (new_timestamp > previous_timestamp)
                        {                                                        
                            
                            //in storage stock history between old and new timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp && ish.timestamp < new_timestamp).ToList();


                            min_stock_state = in_storage_stock_history_operational.MinBy(ish => ish.in_storage_quantity - ish.blocked_quantity);
                            if(min_stock_state != null)
                            {
                                if(min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    //not enough in storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "22",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                                        }
                                    );
                                    continue;
                                }
                            }                           

                            //out of storage states between old and new timestamp
                            out_of_storage_history_operational = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp > previous_timestamp && losh.timestamp < new_timestamp).ToList();

                            //stock history between old and new timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > previous_timestamp && sh.timestamp < new_timestamp).ToList();


                            out_of_storage_state.timestamp = new_timestamp;
                            out_of_storage_state.total_quantity = out_of_storage_state_before_new_timestamp.total_quantity;

                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            foreach(var out_of_storage_between_state in out_of_storage_history_operational)
                            {
                                out_of_storage_between_state.total_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach(var between_state in stock_history_operational)
                            {
                                between_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                                between_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach(var in_storage_between_state in in_storage_stock_history_operational)
                            {
                                in_storage_between_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                                in_storage_between_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                        }
                        else if (new_timestamp < previous_timestamp)
                        {

                            if (latest_stock_state_before_new_timestamp.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                            {
                                // not enough out of storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "43",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = latest_stock_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - latest_stock_state_before_new_timestamp.out_of_storage_quantity
                                    }
                                );
                                continue;
                            }

                            if (in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                            {
                                // not enough out of storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "43",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = in_storage_latest_stock_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity
                                    }
                                );
                                continue;
                            }


                            if(out_of_storage_state_before_new_timestamp.total_quantity < lease_item_on_protocol.total_quantity)
                            {
                                // not enough out of storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "43",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = out_of_storage_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - out_of_storage_state_before_new_timestamp.total_quantity
                                    }
                                );
                                continue;
                            }

                            //out of storage states between new and old timestamp
                            out_of_storage_history_operational = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(ous => ous.timestamp > new_timestamp && ous.timestamp < previous_timestamp).ToList();


                            min_out_of_storage = out_of_storage_history_operational.MinBy(ous => ous.total_quantity);
                            if(min_out_of_storage != null)
                            {
                                if(min_out_of_storage.total_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // not enough out of storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "43",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_out_of_storage.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - min_out_of_storage.total_quantity
                                        }
                                    );
                                    continue;
                                }
                            }
                            
                            //in storage stock history between new and old timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp && ish.timestamp < previous_timestamp).ToList();


                            min_stock_state = in_storage_stock_history_operational.MinBy(ish => ish.out_of_storage_quantity);
                            if (min_stock_state != null)
                            {
                                if (min_stock_state.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // miscallculation (if out_of_storage_quantity is less than on_protocol quantity, then min_out_of_storage quantity (checked above) should also be less)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }

                            //stock history between new and old timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > new_timestamp && sh.timestamp < previous_timestamp).ToList();


                            out_of_storage_state.timestamp = new_timestamp;
                            out_of_storage_state.total_quantity = out_of_storage_state_before_new_timestamp.total_quantity - lease_item_on_protocol.total_quantity;

                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity + lease_item_on_protocol.total_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity - lease_item_on_protocol.total_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity + lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity - lease_item_on_protocol.total_quantity;

                            foreach (var out_of_storage_between_state in out_of_storage_history_operational)
                            {
                                out_of_storage_between_state.total_quantity -= lease_item_on_protocol.total_quantity;
                            }

                            foreach (var next_state in stock_history_operational)
                            {
                                next_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                                next_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                                in_storage_next_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                            }
                        }
                    }

                    if (previous_state.Equals(Protocol_State.Confirmed) && new_state.Equals(Protocol_State.Draft))
                    {
                        if (previous_timestamp.Equals(new_timestamp))
                        {

                            //in storage stock history after previous_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp).ToList();

                            min_stock_state = in_storage_stock_history_operational.MinBy(ish => ish.in_storage_quantity - ish.blocked_quantity);
                            if (min_stock_state != null)
                            {
                                if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    //not enough in storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "22",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                                        }
                                    );
                                    continue;
                                }
                            }

                            //stock history after previous_timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > previous_timestamp).ToList();

                            //out of storage after previous_timestamp
                            out_of_storage_history_operational = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp > previous_timestamp).ToList();


                            out_of_storage_state.total_quantity += lease_item_on_protocol.total_quantity;

                            current_stock_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                            current_stock_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;

                            in_storage_current_stock_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;

                            foreach (var out_of_storage_next_state in out_of_storage_history_operational)
                            {
                                out_of_storage_next_state.total_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var next_state in stock_history_operational)
                            {
                                next_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                                next_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                                in_storage_next_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                        }
                        else if (new_timestamp > previous_timestamp)
                        {

                            //in storage stock history after previous_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp).ToList();


                            min_stock_state = in_storage_stock_history_operational.MinBy(ish => ish.in_storage_quantity - ish.blocked_quantity);
                            if (min_stock_state != null)
                            {
                                if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    //not enough in storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "22",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                                        }
                                    );
                                    continue;
                                }
                            }

                            //stock history after previous_timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > previous_timestamp).ToList();

                            //out of storage after previous_timestamp
                            out_of_storage_history_operational = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp > previous_timestamp).ToList();


                            out_of_storage_state.timestamp = new_timestamp;
                            out_of_storage_state.total_quantity = out_of_storage_state_before_new_timestamp.total_quantity + lease_item_on_protocol.total_quantity;

                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity - lease_item_on_protocol.total_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity + lease_item_on_protocol.total_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity - lease_item_on_protocol.total_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity + lease_item_on_protocol.total_quantity;


                            foreach (var out_of_storage_next_state in out_of_storage_history_operational)
                            {
                                out_of_storage_next_state.total_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var next_state in stock_history_operational)
                            {
                                next_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                                next_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                                in_storage_next_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                        }
                        else if (new_timestamp < previous_timestamp)
                        {

                            if (latest_stock_state_before_new_timestamp.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                            {
                                // not enough out of storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "43",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = latest_stock_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - latest_stock_state_before_new_timestamp.out_of_storage_quantity
                                    }
                                );
                                continue;
                            }

                            if (in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                            {
                                // not enough out of storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "43",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = in_storage_latest_stock_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity
                                    }
                                );
                                continue;
                            }


                            if (out_of_storage_state_before_new_timestamp.total_quantity < lease_item_on_protocol.total_quantity)
                            {
                                // not enough out of storage
                                error_list.Add(
                                    new Lease_Item_On_Protocol_Error_Model
                                    {
                                        code = "43",
                                        lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                        timestamp = out_of_storage_state_before_new_timestamp.timestamp,
                                        required_quantity = lease_item_on_protocol.total_quantity - out_of_storage_state_before_new_timestamp.total_quantity
                                    }
                                );
                                continue;
                            }


                            min_out_of_storage = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(ous => ous.timestamp > new_timestamp && ous.timestamp < previous_timestamp).MinBy(ous => ous.total_quantity);
                            if (min_out_of_storage != null)
                            {
                                if (min_out_of_storage.total_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // not enough out of storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "43",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_out_of_storage.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - min_out_of_storage.total_quantity
                                        }
                                    );
                                    continue;
                                }
                            }

                            min_stock_state = in_storage_stock_history.Where(ish => ish.timestamp > new_timestamp && ish.timestamp < previous_timestamp).MinBy(ish => ish.out_of_storage_quantity);
                            if (min_stock_state != null)
                            {
                                if (min_stock_state.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // miscallculation (if out_of_storage_quantity is less than on_protocol quantity, then min_out_of_storage quantity (checked above) should also be less)
                                    error_list.Clear();
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "36",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = 0
                                        }
                                    );

                                    return error_list;
                                }
                            }

                            //in storage stock history after previous_timestamp
                            in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > previous_timestamp).ToList();


                            min_stock_state = in_storage_stock_history_operational.MinBy(ish => ish.in_storage_quantity - ish.blocked_quantity);
                            if(min_stock_state != null)
                            {
                                if(min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < lease_item_on_protocol.total_quantity)
                                {
                                    // not enough in storage
                                    error_list.Add(
                                        new Lease_Item_On_Protocol_Error_Model
                                        {
                                            code = "22",
                                            lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                            timestamp = min_stock_state.timestamp,
                                            required_quantity = lease_item_on_protocol.total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                                        }
                                    );
                                    continue;
                                }
                            }


                            //stock history after previous_timestamp
                            stock_history_operational = stock_history.Where(sh => sh.timestamp > previous_timestamp).ToList();

                            //out of storage after previous_timestamp
                            out_of_storage_history_operational = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp > previous_timestamp).ToList();


                            out_of_storage_state.timestamp = new_timestamp;
                            out_of_storage_state.total_quantity = out_of_storage_state_before_new_timestamp.total_quantity;

                            current_stock_state.timestamp = new_timestamp;
                            current_stock_state.total_quantity = latest_stock_state_before_new_timestamp.total_quantity;
                            current_stock_state.in_storage_quantity = latest_stock_state_before_new_timestamp.in_storage_quantity;
                            current_stock_state.blocked_quantity = latest_stock_state_before_new_timestamp.blocked_quantity;
                            current_stock_state.out_of_storage_quantity = latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            in_storage_current_stock_state.timestamp = new_timestamp;
                            in_storage_current_stock_state.total_quantity = in_storage_latest_stock_state_before_new_timestamp.total_quantity;
                            in_storage_current_stock_state.in_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.in_storage_quantity;
                            in_storage_current_stock_state.blocked_quantity = in_storage_latest_stock_state_before_new_timestamp.blocked_quantity;
                            in_storage_current_stock_state.out_of_storage_quantity = in_storage_latest_stock_state_before_new_timestamp.out_of_storage_quantity;

                            foreach (var out_of_storage_next_state in out_of_storage_history_operational)
                            {
                                out_of_storage_next_state.total_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var next_state in stock_history_operational)
                            {
                                next_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                                next_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                            foreach (var in_storage_next_state in in_storage_stock_history_operational)
                            {
                                in_storage_next_state.in_storage_quantity -= lease_item_on_protocol.total_quantity;
                                in_storage_next_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                            }

                        }
                    }

                }


            }

            return error_list;

        }


        /* 
         * Edit_Order_Protocol method
         * This method is used to edit a record in the order_protocol table.
         * 
         * It accepts Edit_Order_Protocol_Data object as input.
         * It then changes values of a record with those given in the input object only if its ID matches the one in the input object.
         * It also modifies lease_item, sale_item objects if state of the protocol changes.
         */

        public async Task<string> Edit_Lease_Protocol_Base(Edit_Lease_Protocol_Base_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var editing_lease_protocol = _context.Lease_Protocol.Where(lp =>
                    lp.id.Equals(input_obj.id) &&
                    lp.deleted.Equals(false)
                )
                .FirstOrDefault();

                if (
                    editing_lease_protocol == null
                )
                {
                    throw new Exception("19");//protocol not found
                }

                List<Decrypted_Object> decrypted_fields =
                [
                    new Decrypted_Object { id = 1, decryptedValue = input_obj.element },
                    new Decrypted_Object { id = 2, decryptedValue = input_obj.transport },
                    new Decrypted_Object { id = 3, decryptedValue = input_obj.comment }
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
                        throw new Exception("2");//Encryption Error
                    }

                    switch (field.id)
                    {
                        case 1:
                            editing_lease_protocol.element = field.encryptedValue;
                            break;

                        case 2:
                            editing_lease_protocol.transport = field.encryptedValue;
                            break;

                        case 3:
                            editing_lease_protocol.comment = field.encryptedValue;
                            break;

                        default:
                            throw new Exception("2");//Encryption Error
                    }

                }

                _context.SaveChanges();

                return Info.SUCCESSFULLY_CHANGED;
            }
        }

        public List<Lease_Item_On_Protocol_Error_Model> Edit_Lease_Protocol(Edit_Lease_Protocol_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                Timestamp_Validator.Validate_New_Timestamp(input_obj.timestamp, input_obj.user_current_timestamp);

                if (!Enum.IsDefined(typeof(Protocol_State), input_obj.state))
                {
                    throw new Exception("19");// given state is not defined
                }

                if (input_obj.state.Equals(Protocol_State.Offer))
                {
                    throw new Exception("19");//it is not possible to change protocol state to offer
                }

                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var editing_lease_protocol = _context.Lease_Protocol.Where(lp => 
                    lp.id.Equals(input_obj.id) &&
                    !lp.state.Equals(Protocol_State.Offer) &&
                    lp.deleted.Equals(false) &&
                    lp.lease_to_sale_protocol_FKid == null
                )
                .Include(lp => lp.lease_item_on_protocol_list_FK)
                    .ThenInclude(li => li.lease_item_in_storage_FK)
                .FirstOrDefault();

                if (
                    editing_lease_protocol == null || 
                    editing_lease_protocol.lease_item_on_protocol_list_FK == null
                )
                {
                    throw new Exception("19");//protocol not found
                }


                if (
                    editing_lease_protocol.type.Equals(Lease_Protocol_Type.Return) && 
                    (
                        editing_lease_protocol.state.Equals(Protocol_State.Reserved) ||
                        input_obj.state.Equals(Protocol_State.Reserved)
                    )
                )
                {
                    throw new Exception("34");//return protocol can not be in reservation or offer state
                }


                List<Lease_Item_On_Protocol_Error_Model> error_list = Change_Lease_Protocol_State(ref editing_lease_protocol, input_obj.state, input_obj.timestamp);

                if(error_list.Count > 0)
                {
                    return error_list;
                }

                editing_lease_protocol.timestamp = input_obj.timestamp;
                editing_lease_protocol.state = input_obj.state;


                _context.SaveChanges();

                return error_list;
            }

        }
        


        /*
         * Delete_Order_Protocol method
         * This method is used to delete a record from the order_protocol table.
         *  
         * It accepts Delete_Order_Protocol_Data object as input.
         * Then it deletes a record if its ID matches the one given in the input object.
         */
        public string Delete_Lease_Protocol(Delete_Lease_Protocol_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var existing_lease_protocol = _context.Lease_Protocol.Where(lp => 
                    lp.id.Equals(input_obj.protocol_id) &&
                    lp.deleted.Equals(false)
                )
                .FirstOrDefault();

                if (existing_lease_protocol == null)
                {
                    throw new Exception("19");//protocol not found
                }

                if (
                    (
                        !existing_lease_protocol.state.Equals(Protocol_State.Draft) &&
                        !existing_lease_protocol.state.Equals(Protocol_State.Offer)
                    ) ||
                    existing_lease_protocol.lease_to_sale_protocol_FKid != null
                )
                {
                    throw new Exception("38");//currently in use
                }

                existing_lease_protocol.deleted = true;
                _context.SaveChanges();

                return Info.SUCCESSFULLY_DELETED;
            }
        }


        public async Task<Lease_Protocol_Base_Model> Get_Lease_Protocol_Base_By_Id(Get_Lease_Protocol_By_Id_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var selected_protocol = _context.Lease_Protocol.Where(lp =>
                    lp.id.Equals(input_obj.id_to_get) &&
                    lp.deleted.Equals(false)
                )
                .FirstOrDefault();

                if (
                    selected_protocol == null
                )
                {
                    throw new Exception("19");//protocol not found
                }

                List<Encrypted_Object> encrypted_fields =
                    [
                        new Encrypted_Object { id = 1, encryptedValue = selected_protocol.element },
                        new Encrypted_Object { id = 2, encryptedValue = selected_protocol.transport },
                        new Encrypted_Object { id = 3, encryptedValue = selected_protocol.comment }
                    ];

                var decrypted_fields = await Crypto.DecryptList(session, encrypted_fields);

                if (decrypted_fields == null || decrypted_fields.Count != encrypted_fields.Count)
                {
                    throw new Exception("3");//decryption error
                }

                Lease_Protocol_Base_Model return_obj = new Lease_Protocol_Base_Model
                {
                    id = selected_protocol.id,
                    state = selected_protocol.state,
                    type = selected_protocol.type,
                    full_number = selected_protocol.full_number,
                    timestamp = selected_protocol.timestamp,
                    total_weight_kg = selected_protocol.total_weight_kg,
                    total_area_m2 = selected_protocol.total_area_m2,
                    total_worth = selected_protocol.total_worth,
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
                            return_obj.element = field.decryptedValue; break;

                        case 2:
                            return_obj.transport = field.decryptedValue; break;

                        case 3:
                            return_obj.comment = field.decryptedValue; break;

                        default:
                            throw new Exception("3");//error while decrypting data 
                    }
                }

                return return_obj;
            }
        }

        /*
         * Get_Order_Protocol_By_Id method
         * This method gets a record from the order_protocol table by its ID and returns it.
         * 
         * It accepts Get_Order_Protocol_By_Id_Data object as input.
         * Then it gets a records that has the same ID as the ID given in the input object
         */
        public async Task<Lease_Protocol_Model> Get_Lease_Protocol_By_Id(Get_Lease_Protocol_By_Id_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var selected_protocol = _context.Lease_Protocol.Where(lp => 
                    lp.id.Equals(input_obj.id_to_get) &&
                    lp.deleted.Equals(false)
                )
                .Include(lp => lp.lease_item_on_protocol_list_FK)
                    .ThenInclude(i => i.lease_item_in_storage_FK)
                        .ThenInclude(li => li.lease_item_FK)
                            .ThenInclude(li => li.lease_group_FK)
                .Include(lp => lp.lease_item_on_protocol_list_FK)
                    .ThenInclude(i => i.lease_item_in_storage_FK)
                        .ThenInclude(li => li.lease_item_FK)
                            .ThenInclude(li => li.counting_unit_FK)
                .Include(lp => lp.service_on_lease_protocol_list_FK)
                    .ThenInclude(i => i.service_FK)
                .FirstOrDefault();

                if (
                    selected_protocol == null || 
                    selected_protocol.lease_item_on_protocol_list_FK == null || 
                    selected_protocol.service_on_lease_protocol_list_FK == null
                )
                {
                    throw new Exception("19");//protocol not found
                }

                Lease_Protocol_Model return_obj = new Lease_Protocol_Model
                {
                    id = selected_protocol.id,
                    state = selected_protocol.state,
                    type = selected_protocol.type,
                    full_number = selected_protocol.full_number,
                    order_id = selected_protocol.order_FKid,
                    timestamp = selected_protocol.timestamp,
                    total_weight_kg = selected_protocol.total_weight_kg,
                    total_area_m2 = selected_protocol.total_area_m2,
                    total_worth = selected_protocol.total_worth,
                    lease_to_sale_protocol_id = selected_protocol.lease_to_sale_protocol_FKid
                };


                Task get_protocol_info = Task.Run(async () =>
                {
                    List<Encrypted_Object> encrypted_fields =
                    [
                        new Encrypted_Object { id = 1, encryptedValue = selected_protocol.element },
                        new Encrypted_Object { id = 2, encryptedValue = selected_protocol.transport },
                        new Encrypted_Object { id = 3, encryptedValue = selected_protocol.comment }
                    ];

                    var decrypted_fields = await Crypto.DecryptList(session, encrypted_fields);

                    if(decrypted_fields == null || decrypted_fields.Count != encrypted_fields.Count)
                    {
                        throw new Exception("3");//decryption error
                    }

                    foreach (var field in decrypted_fields)
                    {
                        if (field == null)
                        {
                            throw new Exception("3");//error while decrypting data 
                        }

                        switch (field.id)
                        {
                            case 1:
                                return_obj.element = field.decryptedValue; break;

                            case 2:
                                return_obj.transport = field.decryptedValue; break;

                            case 3:
                                return_obj.comment = field.decryptedValue; break;

                            default:
                                throw new Exception("3");//error while decrypting data 
                        }
                    }

                    return_obj.lease_value_for_30_days = Calculate_30_Days_Lease_Value(ref selected_protocol);

                });


                Task get_items_on_protocol_info = Task.Run(async () =>
                {
                    List<Lease_Item_On_Protocol_Model> items_on_protocol_model = new List<Lease_Item_On_Protocol_Model>();

                    if(selected_protocol.lease_item_on_protocol_list_FK.Count == 0)
                    {
                        return_obj.lease_item_on_protocol_list_FK = items_on_protocol_model;
                        return;
                    }

                    List<Encrypted_Object> encrypted_comment_list = new List<Encrypted_Object>();

                    foreach (var item in selected_protocol.lease_item_on_protocol_list_FK)
                    {
                        if(
                            item == null ||
                            item.lease_item_in_storage_FK == null || 
                            item.lease_item_in_storage_FK.lease_item_FK == null || 
                            item.lease_item_in_storage_FK.lease_item_FK.counting_unit_FK == null
                        )
                        {
                            throw new Exception("19");//not found
                        }

                        items_on_protocol_model.Add(new Lease_Item_On_Protocol_Model
                        {
                            id = item.id,
                            lease_item_id = item.lease_item_in_storage_FK.lease_item_FKid,
                            lease_item_in_storage_id = item.lease_item_in_storage_FKid,
                            catalog_number = item.lease_item_in_storage_FK.lease_item_FK.catalog_number,
                            product_name = item.lease_item_in_storage_FK.lease_item_FK.product_name,
                            counting_unit = item.lease_item_in_storage_FK.lease_item_FK.counting_unit_FK.unit,
                            total_quantity = item.total_quantity,
                            weight_kg = item.lease_item_in_storage_FK.lease_item_FK.weight_kg,
                            total_weight_kg = item.total_weight_kg,
                            total_worth = item.total_worth,
                            total_area_m2 = item.total_area_m2
                        });

                        encrypted_comment_list.Add(new Encrypted_Object { id = item.id, encryptedValue = item.comment });
                    }


                    var decrypted_comment_list = await Crypto.DecryptList(session, encrypted_comment_list);

                    if(
                        decrypted_comment_list == null || 
                        decrypted_comment_list.Count != encrypted_comment_list.Count
                    )
                    {
                        throw new Exception("3");//decryption error
                    }

                    foreach (var item in items_on_protocol_model)
                    {
                        var comment = decrypted_comment_list.Where(c => c.id.Equals(item.id)).FirstOrDefault();
                        if (comment != null)
                        {
                            item.comment = comment.decryptedValue;
                        }
                        else
                        {
                            throw new Exception("3");//error while decrypting data 
                        }
                    }

                    return_obj.lease_item_on_protocol_list_FK = items_on_protocol_model;

                });


                Task get_services_on_protocol_info = Task.Run(async () =>
                {
                    List<Service_On_Lease_Protocol_Model> services_on_protocol_model = new List<Service_On_Lease_Protocol_Model>();

                    if (selected_protocol.service_on_lease_protocol_list_FK.Count == 0)
                    {
                        return_obj.service_on_protocol_list_FK = services_on_protocol_model;
                        return;
                    }

                    List<Encrypted_Object> encrypted_comment = new List<Encrypted_Object>();

                    foreach(var service in selected_protocol.service_on_lease_protocol_list_FK)
                    {
                        if(
                            service == null ||
                            service.service_FK == null
                        )
                        {
                            throw new Exception("19");//not found
                        }

                        services_on_protocol_model.Add(new Service_On_Lease_Protocol_Model
                        {
                            id = service.id,
                            service_id = service.service_FKid,
                            service_number = service.service_FK.service_number,
                            service_name = service.service_FK.service_name,
                            net_worth = service.net_worth
                        });

                        encrypted_comment.Add(new Encrypted_Object { id = service.id, encryptedValue = service.comment });
                    }

                    var decrypted_comment = await Crypto.DecryptList(session, encrypted_comment);

                    if(
                        decrypted_comment == null || 
                        decrypted_comment.Count != encrypted_comment.Count
                    )
                    {
                        throw new Exception("3");//decryption error
                    }

                    foreach(var service in services_on_protocol_model)
                    {
                        var comment = decrypted_comment.Where(c => c.id.Equals(service.id)).FirstOrDefault();
                        if (comment != null)
                        {
                            service.comment = comment.decryptedValue;
                        }
                        else
                        {
                            throw new Exception("3");//error while decrypting data 
                        }
                    }

                    return_obj.service_on_protocol_list_FK = services_on_protocol_model;

                });


                await get_protocol_info;
                await get_items_on_protocol_info;
                await get_services_on_protocol_info;

                return return_obj;
            }

        }


        public async Task<List<Lease_Protocol_Model_List>> Get_All_Lease_Protocol(Get_All_Lease_Protocol_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var order = _context.Order.Where(o => 
                    o.id.Equals(input_obj.order_id) &&
                    o.deleted.Equals(false)
                ).FirstOrDefault();
                if (order == null)
                {
                    throw new Exception("19"); // order not found
                }

                List<Lease_Protocol> lease_protocol_list;
                if (input_obj.get_offer_list)
                {
                    lease_protocol_list = _context.Lease_Protocol.Where(lp =>
                        lp.order_FKid.Equals(order.id) &&
                        lp.type.Equals(Lease_Protocol_Type.Release) &&
                        lp.state.Equals(Protocol_State.Offer) &&
                        lp.deleted.Equals(false)
                    ).ToList();
                }
                else
                {
                    lease_protocol_list = _context.Lease_Protocol.Where(lp =>
                        lp.order_FKid.Equals(order.id) &&
                        lp.type.Equals(input_obj.type) &&
                        !lp.state.Equals(Protocol_State.Offer) &&
                        lp.deleted.Equals(false)
                    ).ToList();
                }

                List<Lease_Protocol_Model_List> lease_protocol_list_model = new List<Lease_Protocol_Model_List>();

                if (lease_protocol_list.Count == 0)
                {
                    return lease_protocol_list_model;
                }

                List<Encrypted_Object> encrypted_protocol_element = new List<Encrypted_Object>();
                List<Encrypted_Object> encrypted_protocol_transport = new List<Encrypted_Object>();
                List<Encrypted_Object> encrypted_comment = new List<Encrypted_Object>();


                foreach (var protocol in lease_protocol_list)
                {
                    lease_protocol_list_model.Add(new Lease_Protocol_Model_List
                    {
                        id = protocol.id,
                        timestamp = protocol.timestamp,
                        state = protocol.state,
                        full_number = protocol.full_number,
                        type = protocol.type,
                        total_weight_kg = protocol.total_weight_kg,
                        total_area_m2 = protocol.total_area_m2,
                        total_worth = protocol.total_worth,
                        lease_to_sale_protocol_id = protocol.lease_to_sale_protocol_FKid
                    });

                    encrypted_protocol_element.Add(new Encrypted_Object { id = protocol.id, encryptedValue = protocol.element });
                    encrypted_protocol_transport.Add(new Encrypted_Object { id = protocol.id, encryptedValue = protocol.transport });
                    encrypted_comment.Add(new Encrypted_Object { id = protocol.id, encryptedValue = protocol.comment });                                        
                }

                var decrypted_protocol_element = await Crypto.DecryptList(session, encrypted_protocol_element);
                var decrypted_protocol_transport = await Crypto.DecryptList(session, encrypted_protocol_transport);
                var decrypted_comment = await Crypto.DecryptList(session, encrypted_comment);

                if(
                    decrypted_protocol_element == null || 
                    decrypted_protocol_element.Count != encrypted_protocol_element.Count || 
                    decrypted_protocol_transport == null || 
                    decrypted_protocol_transport.Count != encrypted_protocol_transport.Count || 
                    decrypted_comment == null || 
                    decrypted_comment.Count != encrypted_comment.Count
                )
                {
                    throw new Exception("3");//decryption error
                }

                foreach (var protocol in lease_protocol_list_model)
                {
                    var element = decrypted_protocol_element.Where(pe => pe.id.Equals(protocol.id)).FirstOrDefault();
                    if (element == null)
                    {
                        throw new Exception("3");//error while decrypting data 
                    }
                    else
                    {
                        protocol.element = element.decryptedValue;
                    }


                    var transport = decrypted_protocol_transport.Where(pt => pt.id.Equals(protocol.id)).FirstOrDefault();
                    if (transport == null)
                    {
                        throw new Exception("3");//error while decrypting data 
                    }
                    else
                    {
                        protocol.transport = transport.decryptedValue;
                    }

                    var comment = decrypted_comment.Where(c => c.id.Equals(protocol.id)).FirstOrDefault();
                    if (comment == null)
                    {
                        throw new Exception("3");//error while decrypting data 
                    }
                    else
                    {
                        protocol.comment = comment.decryptedValue;
                    }
                }

                return lease_protocol_list_model;
            }
        }



        public async Task<Lease_Protocol_Print_Model> Print_Lease_Protocol(Print_Lease_Protocol_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                if (!input_obj.print_new)
                {
                    var protocol = _context.Lease_Protocol.Where(lp =>
                        lp.id.Equals(input_obj.lease_protocol_id) &&
                        lp.deleted.Equals(false)
                    )
                    .Include(lp => lp.lease_protocol_printed_data_FK)
                    .FirstOrDefault();

                    if(protocol == null)
                    {
                        throw new Exception("19");
                    }

                    if(protocol.lease_protocol_printed_data_FK != null)
                    {

                        var binary_data = _context.Lease_Protocol_Binary_Data.Where(lb => lb.lease_protocol_printed_data_FKid.Equals(protocol.lease_protocol_printed_data_FKid)).FirstOrDefault();

                        if(binary_data == null)
                        {
                            throw new Exception("19"); // binary data should exist if printed data exists
                        }

                        byte[] file = await Crypto.DecryptByte(session, binary_data.lease_protocol_bytes);
                        if (file == null) 
                        {
                            throw new Exception("3");//decryption
                        }

                        return new Lease_Protocol_Print_Model { protocol_bytes = file, protocol_file_name = protocol.lease_protocol_printed_data_FK.lease_protocol_file_name };

                    }
                }

                var selected_protocol = _context.Lease_Protocol.Where(lp =>
                    lp.id.Equals(input_obj.lease_protocol_id) &&
                    lp.deleted.Equals(false)
                )
                .Include(lp => lp.order_FK)
                    .ThenInclude(o => o.client_FK)
                .Include(lp => lp.order_FK)
                    .ThenInclude(o => o.construction_site_FK)
                .Include(lp => lp.lease_item_on_protocol_list_FK)
                    .ThenInclude(i => i.lease_item_in_storage_FK)
                        .ThenInclude(li => li.lease_item_FK)
                            .ThenInclude(li => li.counting_unit_FK)
                .FirstOrDefault();

                if (
                    selected_protocol == null ||
                    selected_protocol.order_FK == null ||
                    selected_protocol.order_FK.client_FK == null ||
                    selected_protocol.order_FK.construction_site_FK == null ||
                    selected_protocol.lease_item_on_protocol_list_FK == null
                )
                {
                    throw new Exception("19");//protocol not found
                }

                Lease_Protocol_Base_Model protocol_info = new Lease_Protocol_Base_Model
                {
                    id = selected_protocol.id,
                    state = selected_protocol.state,
                    type = selected_protocol.type,
                    full_number = selected_protocol.full_number,
                    timestamp = selected_protocol.timestamp,
                    total_weight_kg = selected_protocol.total_weight_kg,
                    total_area_m2 = selected_protocol.total_area_m2,
                    total_worth = selected_protocol.total_worth
                };

                Client_Model_List client_info = new Client_Model_List
                {
                    id = selected_protocol.order_FK.client_FKid,
                    number = selected_protocol.order_FK.client_FK.number,
                    is_private_person = selected_protocol.order_FK.client_FK.is_private_person,
                    address = "",
                    comment = "",
                    email = "",
                    nip = "",
                    phone_number = ""
                };

                Construction_Site_Model_List construction_site_info = new Construction_Site_Model_List
                {
                    id = selected_protocol.order_FK.construction_site_FKid,
                    number = selected_protocol.order_FK.construction_site_FK.number,
                    address = "",
                    comment = ""
                };

                List<Lease_Item_On_Protocol_Model> items_on_protocol_model = new List<Lease_Item_On_Protocol_Model>();

                var company_data = _context.Company_Info.FirstOrDefault();
                if(company_data == null)
                {
                    throw new Exception("19");//not found
                }

                Company_Info_Model company_info = new Company_Info_Model
                {
                    address = "",
                    bank_name = "",
                    bank_number = "",
                    name = "",
                    surname = "",
                    nip = ""
                };

                Task get_company_info = Task.Run(async () =>
                {
                    List<Encrypted_Object> encrypted_fields =
                    [
                        new Encrypted_Object { id = 1, encryptedValue = company_data.company_name },
                        new Encrypted_Object { id = 2, encryptedValue = company_data.phone_number },
                        new Encrypted_Object { id = 3, encryptedValue = company_data.email },
                        new Encrypted_Object { id = 4, encryptedValue = company_data.web_page }
                    ];

                    List<Decrypted_Object> decrypted_fields = await Crypto.DecryptList(session, encrypted_fields);

                    if (decrypted_fields == null || decrypted_fields.Count != encrypted_fields.Count)
                    {
                        throw new Exception("3");//error while decrypting data 
                    }

                    foreach (var field in decrypted_fields)
                    {
                        if (field == null)
                        {
                            throw new Exception("3");//error while decrypting data 
                        }
                        else
                        {
                            switch (field.id)
                            {

                                case 1:
                                    company_info.company_name = field.decryptedValue; break;

                                case 2:
                                    company_info.phone_number = field.decryptedValue; break;

                                case 3:
                                    company_info.email = field.decryptedValue; break;

                                case 4:
                                    company_info.web_page = field.decryptedValue; break;

                                default:
                                    throw new Exception("2");//error while encrypting data 
                            }
                        }
                    }

                });

                Task get_client_info = Task.Run(async () =>
                {

                    List<Encrypted_Object> encrypted_items = [
                        new Encrypted_Object { id = 1, encryptedValue = selected_protocol.order_FK.client_FK.surname },
                        new Encrypted_Object { id = 2, encryptedValue = selected_protocol.order_FK.client_FK.name }
                    ];

                    if (!selected_protocol.order_FK.client_FK.is_private_person)
                    {
                        encrypted_items.Add(new Encrypted_Object { id = 3, encryptedValue = selected_protocol.order_FK.client_FK.company_name });
                    }

                    List<Decrypted_Object> decrypted_items = await Crypto.DecryptList(session, encrypted_items);
                    if(decrypted_items == null || decrypted_items.Count != encrypted_items.Count)
                    {
                        throw new Exception("3");
                    }

                    foreach (var field in decrypted_items)
                    {
                        if (field == null)
                        {
                            throw new Exception("3");
                        }

                        switch (field.id)
                        {

                            case 1:
                                client_info.surname = field.decryptedValue;
                                break;

                            case 2:
                                client_info.name = field.decryptedValue;
                                break;

                            case 3:
                                client_info.company_name = field.decryptedValue;
                                break;

                            default:
                                throw new Exception("3");

                        }

                    }

                    if (selected_protocol.order_FK.client_FK.is_private_person)
                    {
                        client_info.company_name = "";
                    }

                });

                Task get_construction_site_info = Task.Run(async () =>
                {

                    var decrypted_name = await Crypto.Decrypt(session, selected_protocol.order_FK.construction_site_FK.construction_site_name);

                    if (decrypted_name == null)
                    {
                        throw new Exception("3");//decryption error
                    }

                    construction_site_info.construction_site_name = decrypted_name;

                });

                Task get_protocol_info = Task.Run(async () =>
                {
                    List<Encrypted_Object> encrypted_fields =
                    [
                        new Encrypted_Object { id = 1, encryptedValue = selected_protocol.element },
                        new Encrypted_Object { id = 2, encryptedValue = selected_protocol.transport },
                        new Encrypted_Object { id = 3, encryptedValue = selected_protocol.comment }
                    ];

                    var decrypted_fields = await Crypto.DecryptList(session, encrypted_fields);

                    if (decrypted_fields == null || decrypted_fields.Count != encrypted_fields.Count)
                    {
                        throw new Exception("3");//decryption error
                    }

                    foreach (var field in decrypted_fields)
                    {
                        if (field == null)
                        {
                            throw new Exception("3");//error while decrypting data 
                        }

                        switch (field.id)
                        {
                            case 1:
                                protocol_info.element = field.decryptedValue; break;

                            case 2:
                                protocol_info.transport = field.decryptedValue; break;

                            case 3:
                                protocol_info.comment = field.decryptedValue; break;

                            default:
                                throw new Exception("3");//error while decrypting data 
                        }
                    }
                });


                Task get_items_on_protocol_info = Task.Run(async () =>
                {

                    if (selected_protocol.lease_item_on_protocol_list_FK.Count.Equals(0))
                    {
                        return;
                    }

                    List<Encrypted_Object> encrypted_comment_list = new List<Encrypted_Object>();

                    foreach (var item in selected_protocol.lease_item_on_protocol_list_FK)
                    {
                        if (
                            item == null ||
                            item.lease_item_in_storage_FK == null ||
                            item.lease_item_in_storage_FK.lease_item_FK == null ||
                            item.lease_item_in_storage_FK.lease_item_FK.counting_unit_FK == null
                        )
                        {
                            throw new Exception("19");//not found
                        }

                        items_on_protocol_model.Add(new Lease_Item_On_Protocol_Model
                        {
                            id = item.id,
                            lease_item_id = item.lease_item_in_storage_FK.lease_item_FKid,
                            lease_item_in_storage_id = item.lease_item_in_storage_FKid,
                            catalog_number = item.lease_item_in_storage_FK.lease_item_FK.catalog_number,
                            product_name = item.lease_item_in_storage_FK.lease_item_FK.product_name,
                            counting_unit = item.lease_item_in_storage_FK.lease_item_FK.counting_unit_FK.unit,
                            total_quantity = item.total_quantity,
                            weight_kg = item.lease_item_in_storage_FK.lease_item_FK.weight_kg,
                            total_weight_kg = item.total_weight_kg,
                            total_worth = item.total_worth,
                            total_area_m2 = item.total_area_m2
                        });

                        encrypted_comment_list.Add(new Encrypted_Object { id = item.id, encryptedValue = item.comment });
                    }


                    var decrypted_comment_list = await Crypto.DecryptList(session, encrypted_comment_list);

                    if (
                        decrypted_comment_list == null ||
                        decrypted_comment_list.Count != encrypted_comment_list.Count
                    )
                    {
                        throw new Exception("3");//decryption error
                    }

                    foreach (var item in items_on_protocol_model)
                    {                     

                        var comment = decrypted_comment_list.Where(c => c.id.Equals(item.id)).FirstOrDefault();
                        if (comment != null)
                        {
                            item.comment = comment.decryptedValue;
                        }
                        else
                        {
                            throw new Exception("3");//error while decrypting data 
                        }
                    }

                });

                await get_company_info;
                await get_protocol_info;
                await get_items_on_protocol_info;
                await get_client_info;
                await get_construction_site_info;

                string protocol_html = File_Provider.Get_Protocol_HTML();

                string protocol_table = "";

                int i = 1;

                foreach(var protocol_row in items_on_protocol_model)
                {

                    protocol_table += 
                    @"<tr>
                        <td style=""padding: 0.1rem 0.2rem;
                    border: 1px solid black;width: 5%;"">
                            " + i.ToString() + @"
                        </td>

                        <td style=""padding: 0.1rem 0.2rem;
                    border: 1px solid black; width: 10%;"">
                            " + protocol_row.catalog_number + @"
                        </td>

                        <td style=""padding: 0.1rem 0.2rem;
                    border: 1px solid black;width: 35%;"">
                            " + protocol_row.product_name + @"
                        </td>

                        <td style=""padding: 0.1rem 0.2rem;
                    border: 1px solid black;width: 5%;"">
                            " + protocol_row.counting_unit + @"
                        </td>

                        <td style=""padding: 0.1rem 0.2rem;
                    border: 1px solid black;width: 10%;"">
                            " + protocol_row.total_quantity.ToString() + @"
                        </td>

                        <td style=""padding: 0.1rem 0.2rem;
                    border: 1px solid black;width: 10%;"">
                            " + protocol_row.weight_kg.ToString() + @"
                        </td>

                        <td style=""padding: 0.1rem 0.2rem;
                    border: 1px solid black;width: 10%;"">
                            " + protocol_row.total_weight_kg.ToString() + @"
                        </td>

                        <td style=""padding: 0.1rem 0.2rem;
                    border: 1px solid black;width: 15%;"">
                            " + protocol_row.comment + @"
                        </td>
                    </tr>";

                    i++;

                }

                //HARDCODED pl
                Protocol_Language_Model language_model = File_Provider.Get_Protocol_Language_Data("pl");

                var company_logo_data = _context.Company_Logo.FirstOrDefault();
                if(company_logo_data == null || company_logo_data.company_logo == null || company_logo_data.file_type == null)
                {
                    throw new Exception("19");//not found
                }

                byte[] decrypted_logo = await Crypto.DecryptByte(session, company_logo_data.company_logo);
                if(decrypted_logo == null)
                {
                    throw new Exception("3");//decryption
                }

                string client_name = client_info.company_name.Equals("") ? client_info.name + " " + client_info.surname : client_info.company_name;

                protocol_info.comment = protocol_info.comment == "" ? "" :
                    @"<h5>Uwagi:</h5>
                    <div>"+ protocol_info.comment + "</div>";



                string only_for_offer = "";

                if (protocol_info.state.Equals(Protocol_State.Offer))
                {
                    selected_protocol.lease_item_on_protocol_list_FK = _context.Lease_Item_On_Protocol.Where(li => 
                        li.lease_protocol_FK.Equals(selected_protocol)
                    )
                    .Include(li => li.lease_item_in_storage_FK)
                        .ThenInclude(lis => lis.lease_item_FK)
                            .ThenInclude(l => l.lease_group_FK)
                    .ToList();

                    decimal lease_value_for_30_days = Calculate_30_Days_Lease_Value(ref selected_protocol);

                    only_for_offer = @"
                        <tr style=""border: 1px solid transparent;"">
                            <th style=""padding: 0.5rem 0;text-align: right;"" colspan=""6"">
                                Wartość sprzętu łącznie :
                            </th>

                            <th style=""padding: 0.5rem 0;"">
                                "+ protocol_info.total_worth.ToString() +@"
                            </th>
                        </tr>
                        <tr style=""border: 1px solid transparent;"">
                            <th style=""padding: 0.5rem 0;text-align: right;"" colspan=""6"">
                                Dzierżawa sprzętu łącznie za 30 dni :
                            </th>

                            <th style=""padding: 0.5rem 0;"">
                                "+ lease_value_for_30_days + @"
                            </th>
                        </tr>
                        <tr style=""border: 1px solid transparent;"">
                            <th style=""padding: 0.5rem 0;text-align: right;"" colspan=""6"">
                                Dzierżawa sprzętu łącznie za 1 dzień :
                            </th>

                            <th style=""padding: 0.5rem 0;"">
                                "+ lease_value_for_30_days / 30 + @"
                            </th>
                        </tr>";
                }




                protocol_html = string.Format(protocol_html,
                [
                    protocol_info.full_number,
                    selected_protocol.lease_to_sale_protocol_FKid == null ? File_Provider.Get_Protocol_State_Name(ref language_model, protocol_info.state) : language_model.ReturnToSale,
                    protocol_info.timestamp.ToString("dd.MM.yyyy HH:mm"),
                    company_info.company_name,
                    company_info.phone_number,
                    company_info.email,
                    company_info.web_page,
                    Get_Protocol_Type_Name(ref language_model, protocol_info.type) + language_model.FromDay,
                    protocol_info.timestamp.ToString("dd.MM.yyyy"),
                    File_Provider.Get_Base64_Tag(decrypted_logo, company_logo_data.file_type),
                    client_name,
                    construction_site_info.construction_site_name,
                    protocol_info.element,
                    protocol_table,
                    protocol_info.total_weight_kg,
                    protocol_info.total_area_m2,
                    protocol_info.comment,
                    protocol_info.transport,
                    only_for_offer
                ]);


                BrowserFetcher browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
                using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true }))
                {
                    using (var page = await browser.NewPageAsync())
                    {
                        await page.SetContentAsync(protocol_html);
                        await page.EvaluateExpressionAsync("document.title = '"+protocol_info.full_number+"';");

                        byte[] pdf = await page.PdfDataAsync(new PdfOptions
                        {
                            Format = PaperFormat.A4,
                            DisplayHeaderFooter = false,
                            MarginOptions = new MarginOptions
                            {
                                Top = "70px",
                                Right = "70px",
                                Bottom = "70px",
                                Left = "70px"
                            }
                        });

                        byte[] encrypted_pdf = await Crypto.EncryptByte(session, pdf);
                        if(encrypted_pdf == null)
                        {
                            throw new Exception("2");//encryption
                        }

                        Lease_Protocol_Printed_Data printed_data = new Lease_Protocol_Printed_Data
                        {
                            lease_protocol_file_name = protocol_info.full_number.Replace("/", "_"),
                            lease_protocol_FKid = protocol_info.id,
                            print_timestamp = input_obj.user_current_timestamp,
                            lease_protocol_binary_data_FK = new Lease_Protocol_Binary_Data
                            {
                                lease_protocol_bytes = encrypted_pdf
                            }
                        };

                        _context.Lease_Protocol_Printed_Data.Add(printed_data);
                        _context.SaveChanges();

                        return new Lease_Protocol_Print_Model { protocol_file_name = printed_data.lease_protocol_file_name, protocol_bytes = pdf };

                    }
                }
                
                


            }

        }


        public static string Get_Protocol_Type_Name(ref Protocol_Language_Model language_model, Lease_Protocol_Type type)
        {
            if (type.Equals(Lease_Protocol_Type.Release)) 
            {
                return language_model.Release;
            }
            else if (type.Equals(Lease_Protocol_Type.Return))
            {
                return language_model.Return;
            }
            else
            {
                throw new Exception("19");//not found
            }
        }





        private decimal Calculate_30_Days_Lease_Value(ref Lease_Protocol lease_protocol)
        {

            if (lease_protocol.lease_item_on_protocol_list_FK.Count.Equals(0))
            {
                return 0;
            }

            decimal lease_value = 0;

            long order_id = lease_protocol.order_FKid;

            var order = _context.Order.Where(o => o.id.Equals(order_id) && o.deleted.Equals(false)).FirstOrDefault();
            if(order == null)
            {
                throw new Exception("19");//not found
            }

            foreach(var lease_item_on_protocol in lease_protocol.lease_item_on_protocol_list_FK)
            {
                if(
                    lease_item_on_protocol == null ||
                    lease_item_on_protocol.lease_item_in_storage_FK == null || 
                    lease_item_on_protocol.lease_item_in_storage_FK.lease_item_FK == null || 
                    lease_item_on_protocol.lease_item_in_storage_FK.lease_item_FK.lease_group_FK == null
                )
                {
                    throw new Exception("19");//not found
                }

                if (order.use_static_rate)
                {
                    lease_value +=
                        lease_item_on_protocol.total_quantity *
                        lease_item_on_protocol.lease_item_in_storage_FK.lease_item_FK.price *
                        order.static_rate;
                }
                else
                {
                    lease_value += 
                        lease_item_on_protocol.total_quantity * 
                        lease_item_on_protocol.lease_item_in_storage_FK.lease_item_FK.price * 
                        lease_item_on_protocol.lease_item_in_storage_FK.lease_item_FK.lease_group_FK.rate;
                }
            }

            if (lease_value <= 0)
            {
                throw new Exception("36");//checksum error (if item on protocol list is greater than 0 then lease_value should be greater than 0)
            }

            return lease_value;

        }


        

        private int Get_Latest_Offer_Number(int year)
        {
            Lease_Protocol? offer = _context.Lease_Protocol.Where(lp => 
                lp.type.Equals(Lease_Protocol_Type.Release) && 
                lp.state.Equals(Protocol_State.Offer) &&
                lp.year.Equals(year)
            ).MaxBy(lp => lp.number);

            if(offer == null)
            {
                return 0;
            }

            return offer.number;
        }
        
        private int Get_Latest_Protocol_Number(Lease_Protocol_Type protocol_type, int year)
        {
            Lease_Protocol? protocol = _context.Lease_Protocol.Where(lp => 
                lp.type.Equals(protocol_type) && 
                !lp.state.Equals(Protocol_State.Offer) &&
                lp.year.Equals(year)
            ).MaxBy(lp => lp.number);

            if(protocol == null)
            {
                return 0;
            }

            return protocol.number;
        }

    }
}
