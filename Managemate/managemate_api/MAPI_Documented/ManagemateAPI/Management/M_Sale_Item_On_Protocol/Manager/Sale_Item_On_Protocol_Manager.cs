using ManagemateAPI.Database.Context;
using ManagemateAPI.Database.Tables;
using ManagemateAPI.Encryption;
using ManagemateAPI.Encryption.Input_Objects;
using ManagemateAPI.Management.M_Sale_Item_On_Protocol.Input_Objects;
using ManagemateAPI.Management.M_Sale_Item_On_Protocol.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.Shared.Enum;
using ManagemateAPI.Management.Shared.Static;
using ManagemateAPI.Management.Shared.Validator;
using Microsoft.EntityFrameworkCore;

namespace ManagemateAPI.Management.M_Sale_Item_On_Protocol.Manager
{
    public class Sale_Item_On_Protocol_Manager
    {
        private DB_Context _context;
        private readonly IConfiguration _configuration;


        public Sale_Item_On_Protocol_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public string Add_Sale_Item_On_Offer_Protocol(Add_Sale_Item_On_Protocol_Data input_obj, Session_Data session)
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
                    sp.id.Equals(input_obj.protocol_FK) &&
                    sp.state.Equals(Protocol_State.Offer) &&
                    sp.deleted.Equals(false)
                )
                .FirstOrDefault();


                var sale_item_in_storage = _context.Sale_Item_In_Storage.Where(si =>
                    si.id.Equals(input_obj.sale_item_in_storage_FK)
                )
                .Include(si => si.sale_item_FK)
                .FirstOrDefault();

                if (
                    protocol == null ||
                    sale_item_in_storage == null ||
                    sale_item_in_storage.sale_item_FK == null
                )
                {
                    throw new Exception("19");//not found
                }


                var sale_item_on_protocol_exists = _context.Sale_Item_On_Protocol.Where(iop =>
                    iop.sale_item_in_storage_FKid.Equals(sale_item_in_storage.id) &&
                    iop.sale_protocol_FKid.Equals(protocol.id)
                ).FirstOrDefault();

                if (sale_item_on_protocol_exists == null)
                {
                    var new_item_on_protocol = new Sale_Item_On_Protocol
                    {
                        sale_item_in_storage_FKid = sale_item_in_storage.id,
                        sale_protocol_FKid = protocol.id,
                        total_quantity = input_obj.total_quantity,
                        total_weight_kg = input_obj.total_quantity * sale_item_in_storage.sale_item_FK.weight_kg,
                        total_area_m2 = input_obj.total_quantity * sale_item_in_storage.sale_item_FK.area_m2,
                        total_worth = input_obj.total_quantity * sale_item_in_storage.sale_item_FK.price,
                        comment = Array.Empty<byte>()
                    };

                    _context.Sale_Item_On_Protocol.Add(new_item_on_protocol);
                }
                else
                {
                    sale_item_on_protocol_exists.total_quantity += input_obj.total_quantity;
                }

                _context.SaveChanges();

                return Info.SUCCESSFULLY_ADDED;
            }
        }


        public async Task<string> Edit_Sale_Item_On_Offer_Protocol(Edit_Sale_Item_On_Protocol_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var sale_item_on_protocol_exists = _context.Sale_Item_On_Protocol.Where(sip =>
                    sip.id.Equals(input_obj.id)
                )
                .Include(sp => sp.sale_protocol_FK)
                .FirstOrDefault();

                if (
                    sale_item_on_protocol_exists == null ||
                    sale_item_on_protocol_exists.sale_protocol_FK == null
                )
                {
                    throw new Exception("19");//not found
                }

                if (!sale_item_on_protocol_exists.sale_protocol_FK.state.Equals(Protocol_State.Offer))
                {
                    throw new Exception("19");//not found
                }


                var comment = await Crypto.Encrypt(session, input_obj.comment);
                if (comment == null)
                {
                    throw new Exception("2");//encryption error
                }

                sale_item_on_protocol_exists.comment = comment;

                if (input_obj.total_quantity > sale_item_on_protocol_exists.total_quantity)
                {
                    decimal quantity_to_add = input_obj.total_quantity - sale_item_on_protocol_exists.total_quantity;

                    sale_item_on_protocol_exists.total_quantity += quantity_to_add;
                }

                if (input_obj.total_quantity < sale_item_on_protocol_exists.total_quantity)
                {
                    decimal quantity_to_remove = sale_item_on_protocol_exists.total_quantity - input_obj.total_quantity;

                    if (quantity_to_remove > sale_item_on_protocol_exists.total_quantity || quantity_to_remove < 0)
                    {
                        throw new Exception("39");// negative quantity
                    }

                    sale_item_on_protocol_exists.total_quantity -= quantity_to_remove;

                    if (sale_item_on_protocol_exists.total_quantity.Equals(0))
                    {
                        _context.Sale_Item_On_Protocol.Remove(sale_item_on_protocol_exists);
                    }
                }

                _context.SaveChanges();

                return Info.SUCCESSFULLY_CHANGED;
            }
        }


        public Sale_Item_On_Protocol_Id_Model Add_Sale_Item_On_Protocol(Add_Sale_Item_On_Protocol_Data input_obj, Session_Data session)
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
                    sp.id.Equals(input_obj.protocol_FK) &&
                    sp.deleted.Equals(false)
                )
                .FirstOrDefault();


                var sale_item_in_storage = _context.Sale_Item_In_Storage.Where(si =>
                    si.id.Equals(input_obj.sale_item_in_storage_FK)
                )
                .Include(li => li.sale_item_FK)
                .FirstOrDefault();

                if (
                    protocol == null ||
                    sale_item_in_storage == null ||
                    sale_item_in_storage.sale_item_FK == null
                )
                {
                    throw new Exception("19");//not found
                }


                if (protocol.state.Equals(Protocol_State.Offer))
                {
                    throw new Exception("19");//not found protocol which isn't in offer state
                }

                var sale_item_on_protocol_exists = _context.Sale_Item_On_Protocol.Where(iop =>
                    iop.sale_item_in_storage_FKid.Equals(sale_item_in_storage.id) &&
                    iop.sale_protocol_FKid.Equals(protocol.id)
                ).FirstOrDefault();


                return Add_Sale_Item_On_Protocol(session, ref protocol, ref sale_item_in_storage, ref sale_item_on_protocol_exists, input_obj.total_quantity);
            }

        }


        private Sale_Item_On_Protocol_Id_Model Add_Sale_Item_On_Protocol(Session_Data session, ref Sale_Protocol protocol, ref Sale_Item_In_Storage sale_item_in_storage, ref Sale_Item_On_Protocol? sale_item_on_protocol_exists, decimal input_total_quantity)
        {
            DateTime protocolTimestamp = protocol.timestamp;
            Sale_Item_In_Storage sale_item_in_storage_for_lambda = sale_item_in_storage;
            Sale_Protocol protocol_for_lambda = protocol;

            var stock_history = _context.Sale_Item_Stock_History.Where(ssh =>
                ssh.sale_item_FKid.Equals(sale_item_in_storage_for_lambda.sale_item_FKid)
            ).ToList();

            var in_storage_stock_history = _context.Sale_Item_In_Storage_Stock_History.Where(ssh =>
                ssh.sale_item_in_storage_FKid.Equals(sale_item_in_storage_for_lambda.id)
            ).ToList();

            Sale_Item_Stock_History? reference_stock_state;
            Sale_Item_In_Storage_Stock_History? in_storage_reference_stock_state;
            

            if (sale_item_on_protocol_exists == null)
            {
                Timestamp_Validator.Validate_Protocol_Timestamp(stock_history, protocol.timestamp);
                Timestamp_Validator.Validate_Protocol_Timestamp(in_storage_stock_history, protocol.timestamp);

                //latest before timestamp stock state
                reference_stock_state = stock_history.Where(sh => sh.timestamp < protocol_for_lambda.timestamp).MaxBy(sh => sh.timestamp);
                if (reference_stock_state == null)
                {
                    throw new Exception("19");//not found
                }

                //latest before timestamp in storage stock state
                in_storage_reference_stock_state = in_storage_stock_history.Where(ish => ish.timestamp < protocol_for_lambda.timestamp).MaxBy(ish => ish.timestamp);
                if (in_storage_reference_stock_state == null)
                {
                    throw new Exception("19");//not found
                }
            }
            else
            {
                //current stock state
                reference_stock_state = stock_history.Where(sh => sh.timestamp.Equals(protocol_for_lambda.timestamp)).FirstOrDefault();
                if (reference_stock_state == null)
                {
                    throw new Exception("19");//not found
                }

                //current in storage stock state
                in_storage_reference_stock_state = in_storage_stock_history.Where(ish => ish.timestamp.Equals(protocol_for_lambda.timestamp)).FirstOrDefault();
                if (in_storage_reference_stock_state == null)
                {
                    throw new Exception("19");//not found
                }
            }

            Stock_State_Validator.Validate_Stock_State(reference_stock_state);
            Stock_State_Validator.Validate_Stock_State(in_storage_reference_stock_state);


            var stock_history_after_timestamp = stock_history.Where(sh => sh.timestamp > protocol_for_lambda.timestamp).ToList();
            var in_storage_stock_history_after_timestamp = in_storage_stock_history.Where(ish => ish.timestamp > protocol_for_lambda.timestamp).ToList();


            Sale_Item_On_Protocol_Id_Model return_obj = new Sale_Item_On_Protocol_Id_Model();


            if (sale_item_on_protocol_exists != null && protocol.state.Equals(Protocol_State.Draft))
            {
                if (reference_stock_state.in_storage_quantity - reference_stock_state.blocked_quantity - sale_item_on_protocol_exists.total_quantity < input_total_quantity)
                {
                    //not enough in stock
                    return_obj.error_object = new Sale_Item_On_Protocol_Error_Model
                    {
                        code = "22",
                        timestamp = reference_stock_state.timestamp,
                        sale_item_in_storage_id = sale_item_in_storage.id,
                        required_quantity = input_total_quantity - (reference_stock_state.in_storage_quantity - reference_stock_state.blocked_quantity - sale_item_on_protocol_exists.total_quantity)
                    };
                    return return_obj;
                }

                if (in_storage_reference_stock_state.in_storage_quantity - in_storage_reference_stock_state.blocked_quantity - sale_item_on_protocol_exists.total_quantity < input_total_quantity)
                {
                    //not enough in stock
                    return_obj.error_object = new Sale_Item_On_Protocol_Error_Model
                    {
                        code = "22",
                        timestamp = in_storage_reference_stock_state.timestamp,
                        sale_item_in_storage_id = sale_item_in_storage.id,
                        required_quantity = input_total_quantity - (in_storage_reference_stock_state.in_storage_quantity - in_storage_reference_stock_state.blocked_quantity - sale_item_on_protocol_exists.total_quantity)
                    };
                    return return_obj;
                }
            }
            else
            {
                if (reference_stock_state.in_storage_quantity - reference_stock_state.blocked_quantity < input_total_quantity)
                {
                    //not enough in stock
                    return_obj.error_object = new Sale_Item_On_Protocol_Error_Model
                    {
                        code = "22",
                        timestamp = reference_stock_state.timestamp,
                        sale_item_in_storage_id = sale_item_in_storage.id,
                        required_quantity = input_total_quantity - (reference_stock_state.in_storage_quantity - reference_stock_state.blocked_quantity)
                    };
                    return return_obj;
                }

                if (in_storage_reference_stock_state.in_storage_quantity - in_storage_reference_stock_state.blocked_quantity < input_total_quantity)
                {
                    //not enough in stock
                    return_obj.error_object = new Sale_Item_On_Protocol_Error_Model
                    {
                        code = "22",
                        timestamp = in_storage_reference_stock_state.timestamp,
                        sale_item_in_storage_id = sale_item_in_storage.id,
                        required_quantity = input_total_quantity - (in_storage_reference_stock_state.in_storage_quantity - in_storage_reference_stock_state.blocked_quantity)
                    };
                    return return_obj;
                }
            }

            var min_stock_state = in_storage_stock_history_after_timestamp.MinBy(ishat => ishat.in_storage_quantity - ishat.blocked_quantity);

            if (min_stock_state != null)
            {
                if (sale_item_on_protocol_exists != null && protocol.state.Equals(Protocol_State.Draft))
                {
                    if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity - sale_item_on_protocol_exists.total_quantity < input_total_quantity)
                    {
                        // not enough in storage
                        return_obj.error_object = new Sale_Item_On_Protocol_Error_Model
                        {
                            code = "22",
                            timestamp = min_stock_state.timestamp,
                            sale_item_in_storage_id = sale_item_in_storage.id,
                            required_quantity = input_total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity - sale_item_on_protocol_exists.total_quantity)
                        };
                        return return_obj;
                    }
                }
                else
                {
                    if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < input_total_quantity)
                    {
                        // not enough in storage
                        return_obj.error_object = new Sale_Item_On_Protocol_Error_Model
                        {
                            code = "22",
                            timestamp = min_stock_state.timestamp,
                            sale_item_in_storage_id = sale_item_in_storage.id,
                            required_quantity = input_total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                        };
                        return return_obj;
                    }
                }
            }



            Sale_Item_On_Protocol new_item_on_protocol = new Sale_Item_On_Protocol();

            if (protocol.state.Equals(Protocol_State.Draft))
            {

                if (sale_item_on_protocol_exists == null)
                {
                    new_item_on_protocol = new Sale_Item_On_Protocol
                    {
                        sale_item_in_storage_FKid = sale_item_in_storage.id,
                        sale_protocol_FKid = protocol.id,
                        total_quantity = input_total_quantity,
                        total_weight_kg = input_total_quantity * sale_item_in_storage.sale_item_FK.weight_kg,
                        total_area_m2 = input_total_quantity * sale_item_in_storage.sale_item_FK.area_m2,
                        total_worth = input_total_quantity * sale_item_in_storage.sale_item_FK.price,
                        comment = Array.Empty<byte>()
                    };

                    Sale_Item_Stock_History stock_update = new Sale_Item_Stock_History
                    {
                        sale_item_FKid = sale_item_in_storage.sale_item_FKid,
                        total_quantity = reference_stock_state.total_quantity,
                        in_storage_quantity = reference_stock_state.in_storage_quantity,
                        blocked_quantity = reference_stock_state.blocked_quantity,
                        timestamp = protocol.timestamp
                    };

                    Sale_Item_In_Storage_Stock_History in_storage_stock_update = new Sale_Item_In_Storage_Stock_History
                    {
                        sale_item_in_storage_FKid = sale_item_in_storage.id,
                        in_storage_quantity = in_storage_reference_stock_state.in_storage_quantity,
                        blocked_quantity = in_storage_reference_stock_state.blocked_quantity,
                        timestamp = protocol.timestamp
                    };

                    _context.Sale_Item_Stock_History.Add(stock_update);
                    _context.Sale_Item_In_Storage_Stock_History.Add(in_storage_stock_update);
                }

            }
            else if (protocol.state.Equals(Protocol_State.Reserved))
            {

                if (sale_item_on_protocol_exists != null)
                {
                    reference_stock_state.blocked_quantity += input_total_quantity;
                    in_storage_reference_stock_state.blocked_quantity += input_total_quantity;
                }
                else
                {
                    new_item_on_protocol = new Sale_Item_On_Protocol
                    {
                        sale_item_in_storage_FKid = sale_item_in_storage.id,
                        sale_protocol_FKid = protocol.id,
                        total_quantity = input_total_quantity,
                        total_weight_kg = input_total_quantity * sale_item_in_storage.sale_item_FK.weight_kg,
                        total_area_m2 = input_total_quantity * sale_item_in_storage.sale_item_FK.area_m2,
                        total_worth = input_total_quantity * sale_item_in_storage.sale_item_FK.price,
                        comment = Array.Empty<byte>()
                    };

                    Sale_Item_Stock_History stock_update = new Sale_Item_Stock_History
                    {
                        sale_item_FKid = sale_item_in_storage.sale_item_FKid,
                        total_quantity = reference_stock_state.total_quantity,
                        in_storage_quantity = reference_stock_state.in_storage_quantity,
                        blocked_quantity = reference_stock_state.blocked_quantity + input_total_quantity,
                        timestamp = protocol.timestamp
                    };

                    Sale_Item_In_Storage_Stock_History in_storage_stock_update = new Sale_Item_In_Storage_Stock_History
                    {
                        sale_item_in_storage_FKid = sale_item_in_storage.id,
                        in_storage_quantity = in_storage_reference_stock_state.in_storage_quantity,
                        blocked_quantity = in_storage_reference_stock_state.blocked_quantity + input_total_quantity,
                        timestamp = protocol.timestamp
                    };

                    _context.Sale_Item_Stock_History.Add(stock_update);
                    _context.Sale_Item_In_Storage_Stock_History.Add(in_storage_stock_update);
                }

                foreach (var next_state in stock_history_after_timestamp)
                {
                    next_state.blocked_quantity += input_total_quantity;
                }

                foreach (var in_storage_next_state in in_storage_stock_history_after_timestamp)
                {
                    in_storage_next_state.blocked_quantity += input_total_quantity;
                }

            }
            else if (protocol.state.Equals(Protocol_State.Confirmed))
            {

                if (sale_item_on_protocol_exists != null)
                {

                    reference_stock_state.in_storage_quantity -= input_total_quantity;

                    in_storage_reference_stock_state.in_storage_quantity -= input_total_quantity;

                }
                else
                {
                    new_item_on_protocol = new Sale_Item_On_Protocol
                    {
                        sale_item_in_storage_FKid = sale_item_in_storage.id,
                        sale_protocol_FKid = protocol.id,
                        total_quantity = input_total_quantity,
                        total_weight_kg = input_total_quantity * sale_item_in_storage.sale_item_FK.weight_kg,
                        total_area_m2 = input_total_quantity * sale_item_in_storage.sale_item_FK.area_m2,
                        total_worth = input_total_quantity * sale_item_in_storage.sale_item_FK.price,
                        comment = Array.Empty<byte>()
                    };

                    Sale_Item_Stock_History stock_update = new Sale_Item_Stock_History
                    {
                        sale_item_FKid = sale_item_in_storage.sale_item_FKid,
                        total_quantity = reference_stock_state.total_quantity,
                        in_storage_quantity = reference_stock_state.in_storage_quantity - input_total_quantity,
                        blocked_quantity = reference_stock_state.blocked_quantity,
                        timestamp = protocol.timestamp
                    };

                    Sale_Item_In_Storage_Stock_History in_storage_stock_update = new Sale_Item_In_Storage_Stock_History
                    {
                        sale_item_in_storage_FKid = sale_item_in_storage.id,
                        in_storage_quantity = in_storage_reference_stock_state.in_storage_quantity - input_total_quantity,
                        blocked_quantity = in_storage_reference_stock_state.blocked_quantity,
                        timestamp = protocol.timestamp
                    };

                    _context.Sale_Item_Stock_History.Add(stock_update);
                    _context.Sale_Item_In_Storage_Stock_History.Add(in_storage_stock_update);
                                        

                }


                foreach (var next_state in stock_history_after_timestamp)
                {
                    next_state.in_storage_quantity -= input_total_quantity;
                }

                foreach (var in_storage_next_state in in_storage_stock_history_after_timestamp)
                {
                    in_storage_next_state.in_storage_quantity -= input_total_quantity;
                }

            }


            protocol.total_weight_kg += input_total_quantity * sale_item_in_storage.sale_item_FK.weight_kg;
            protocol.total_area_m2 += input_total_quantity * sale_item_in_storage.sale_item_FK.area_m2;
            protocol.total_worth += input_total_quantity * sale_item_in_storage.sale_item_FK.price;

            if (sale_item_on_protocol_exists != null)
            {
                sale_item_on_protocol_exists.total_quantity += input_total_quantity;
                sale_item_on_protocol_exists.total_weight_kg += input_total_quantity * sale_item_in_storage.sale_item_FK.weight_kg;
                sale_item_on_protocol_exists.total_area_m2 += input_total_quantity * sale_item_in_storage.sale_item_FK.area_m2;
                sale_item_on_protocol_exists.total_worth += input_total_quantity * sale_item_in_storage.sale_item_FK.price;

                return_obj.id = sale_item_on_protocol_exists.id;

                _context.SaveChanges();
            }
            else
            {
                if (new_item_on_protocol.sale_protocol_FK == null)
                {
                    throw new Exception("19"); // not found (if item on protocol doesn't exist it should be created in one of if statement above)
                }

                _context.Sale_Item_On_Protocol.Add(new_item_on_protocol);
                _context.SaveChanges();

                return_obj.id = new_item_on_protocol.id;
            }

            return return_obj;

        }



        private Sale_Item_On_Protocol_Id_Model Remove_Sale_Item_On_Protocol(Session_Data session, ref Sale_Protocol protocol, ref Sale_Item_In_Storage sale_item_in_storage, ref Sale_Item_On_Protocol sale_item_on_protocol_exists, decimal input_total_quantity)
        {
            if (input_total_quantity > sale_item_on_protocol_exists.total_quantity || input_total_quantity < 0)
            {
                throw new Exception("39");// negative quantity
            }

            Sale_Item_On_Protocol_Id_Model return_obj = new Sale_Item_On_Protocol_Id_Model
            {
                id = sale_item_on_protocol_exists.id
            };


            DateTime protocolTimestamp = protocol.timestamp;
            Sale_Item_In_Storage sale_item_in_storage_for_lambda = sale_item_in_storage;
            Sale_Protocol protocol_for_lambda = protocol;

            var stock_history = _context.Sale_Item_Stock_History.Where(lsh =>
                lsh.sale_item_FKid.Equals(sale_item_in_storage_for_lambda.sale_item_FKid)
            ).ToList();

            var in_storage_stock_history = _context.Sale_Item_In_Storage_Stock_History.Where(ish =>
                ish.sale_item_in_storage_FKid.Equals(sale_item_in_storage_for_lambda.id)
            ).ToList();

            Sale_Item_Stock_History? reference_stock_state;
            Sale_Item_In_Storage_Stock_History? in_storage_reference_stock_state;


            //current stock state
            reference_stock_state = stock_history.Where(sh => sh.timestamp.Equals(protocol_for_lambda.timestamp)).FirstOrDefault();
            if (reference_stock_state == null)
            {
                throw new Exception("19");//not found
            }

            //current in storage stock state
            in_storage_reference_stock_state = in_storage_stock_history.Where(ish => ish.timestamp.Equals(protocol_for_lambda.timestamp)).FirstOrDefault();
            if (in_storage_reference_stock_state == null)
            {
                throw new Exception("19");//not found
            }

            Stock_State_Validator.Validate_Stock_State(reference_stock_state);
            Stock_State_Validator.Validate_Stock_State(in_storage_reference_stock_state);


            List<Sale_Item_Stock_History> stock_history_after_timestamp = stock_history.Where(sh => sh.timestamp > protocol_for_lambda.timestamp).ToList();
            List<Sale_Item_In_Storage_Stock_History> in_storage_stock_history_after_timestamp = in_storage_stock_history.Where(ish => ish.timestamp > protocol_for_lambda.timestamp).ToList();


            if (protocol.state.Equals(Protocol_State.Draft))
            {
                if (reference_stock_state.in_storage_quantity - reference_stock_state.blocked_quantity < sale_item_on_protocol_exists.total_quantity - input_total_quantity)
                {
                    // not enough in storage
                    return_obj.error_object = new Sale_Item_On_Protocol_Error_Model
                    {
                        code = "22",
                        timestamp = reference_stock_state.timestamp,
                        sale_item_in_storage_id = sale_item_in_storage.id,
                        required_quantity = sale_item_on_protocol_exists.total_quantity - input_total_quantity - (reference_stock_state.in_storage_quantity - reference_stock_state.blocked_quantity)
                    };
                    return return_obj;
                }

                if (in_storage_reference_stock_state.in_storage_quantity - in_storage_reference_stock_state.blocked_quantity < sale_item_on_protocol_exists.total_quantity - input_total_quantity)
                {
                    // not enough in storage
                    return_obj.error_object = new Sale_Item_On_Protocol_Error_Model
                    {
                        code = "22",
                        timestamp = in_storage_reference_stock_state.timestamp,
                        sale_item_in_storage_id = sale_item_in_storage.id,
                        required_quantity = sale_item_on_protocol_exists.total_quantity - input_total_quantity - (in_storage_reference_stock_state.in_storage_quantity - in_storage_reference_stock_state.blocked_quantity)
                    };
                    return return_obj;
                }


                var min_stock_state = in_storage_stock_history_after_timestamp.MinBy(ishat => ishat.in_storage_quantity - ishat.blocked_quantity);

                if (min_stock_state != null)
                {
                    if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < sale_item_on_protocol_exists.total_quantity - input_total_quantity)
                    {
                        // not enough in storage
                        return_obj.error_object = new Sale_Item_On_Protocol_Error_Model
                        {
                            code = "22",
                            timestamp = min_stock_state.timestamp,
                            sale_item_in_storage_id = sale_item_in_storage.id,
                            required_quantity = sale_item_on_protocol_exists.total_quantity - input_total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                        };
                        return return_obj;
                    }
                }
            }
            else if (protocol.state.Equals(Protocol_State.Reserved))
            {
                if (reference_stock_state.blocked_quantity < sale_item_on_protocol_exists.total_quantity)
                {
                    // miscalculation (blocked quantity should be equal to input_total_quantity or greater)
                    return_obj.error_object = new Sale_Item_On_Protocol_Error_Model
                    {
                        code = "36",
                        timestamp = reference_stock_state.timestamp,
                        sale_item_in_storage_id = sale_item_in_storage.id,
                        required_quantity = sale_item_on_protocol_exists.total_quantity - reference_stock_state.blocked_quantity
                    };
                    return return_obj;
                }

                if (in_storage_reference_stock_state.blocked_quantity < sale_item_on_protocol_exists.total_quantity)
                {
                    // miscalculation (blocked quantity should be equal to input_total_quantity or greater)
                    return_obj.error_object = new Sale_Item_On_Protocol_Error_Model
                    {
                        code = "36",
                        timestamp = in_storage_reference_stock_state.timestamp,
                        sale_item_in_storage_id = sale_item_in_storage.id,
                        required_quantity = sale_item_on_protocol_exists.total_quantity - in_storage_reference_stock_state.blocked_quantity
                    };
                    return return_obj;
                }

                var min_blocked_state = in_storage_stock_history_after_timestamp.MinBy(ishat => ishat.blocked_quantity);

                if (min_blocked_state != null)
                {
                    if (min_blocked_state.blocked_quantity < sale_item_on_protocol_exists.total_quantity)
                    {
                        // miscalculation (blocked quantity should be equal to input_total_quantity or greater)
                        return_obj.error_object = new Sale_Item_On_Protocol_Error_Model
                        {
                            code = "36",
                            timestamp = min_blocked_state.timestamp,
                            sale_item_in_storage_id = sale_item_in_storage.id,
                            required_quantity = sale_item_on_protocol_exists.total_quantity - min_blocked_state.blocked_quantity
                        };
                        return return_obj;
                    }
                }
            }

            //if (protocol.state.Equals(Protocol_State.Confirmed)) { }
            //for confirmed protocol it isn't possible to check construction site state, so subtracting confirmed sale items means adding them back to the storage



            //for drafts only operation is subtracting item on protocol quantity and total weight so there is no need for separate condition

            if (protocol.state.Equals(Protocol_State.Reserved))
            {
                reference_stock_state.blocked_quantity -= input_total_quantity;
                in_storage_reference_stock_state.blocked_quantity -= input_total_quantity;

                foreach (var next_state in stock_history_after_timestamp)
                {
                    next_state.blocked_quantity -= input_total_quantity;
                }

                foreach (var in_storage_next_state in in_storage_stock_history_after_timestamp)
                {
                    in_storage_next_state.blocked_quantity -= input_total_quantity;
                }
            }

            if (protocol.state.Equals(Protocol_State.Confirmed))
            {

                reference_stock_state.in_storage_quantity += input_total_quantity;

                in_storage_reference_stock_state.in_storage_quantity += input_total_quantity;

                foreach (var next_state in stock_history_after_timestamp)
                {
                    next_state.in_storage_quantity += input_total_quantity;
                }

                foreach (var in_storage_next_state in in_storage_stock_history_after_timestamp)
                {
                    in_storage_next_state.in_storage_quantity += input_total_quantity;
                }
            }


            sale_item_on_protocol_exists.total_quantity -= input_total_quantity;
            sale_item_on_protocol_exists.total_weight_kg -= input_total_quantity * sale_item_in_storage.sale_item_FK.weight_kg;
            sale_item_on_protocol_exists.total_area_m2 -= input_total_quantity * sale_item_in_storage.sale_item_FK.area_m2;
            sale_item_on_protocol_exists.total_worth -= input_total_quantity * sale_item_in_storage.sale_item_FK.price;

            protocol.total_weight_kg -= input_total_quantity * sale_item_in_storage.sale_item_FK.weight_kg;
            protocol.total_area_m2 -= input_total_quantity * sale_item_in_storage.sale_item_FK.area_m2;
            protocol.total_worth -= input_total_quantity * sale_item_in_storage.sale_item_FK.price;


            if (sale_item_on_protocol_exists.total_quantity.Equals(0))
            {
                var stock_state_before_current = stock_history.Where(sh => sh.timestamp < protocol_for_lambda.timestamp).MaxBy(sh => sh.timestamp);
                var in_storage_stock_state_before_current = in_storage_stock_history.Where(ish => ish.timestamp < protocol_for_lambda.timestamp).MaxBy(ish => ish.timestamp);

                if (stock_state_before_current == null || in_storage_stock_state_before_current == null)
                {
                    throw new Exception("19");//not found (should be at least one previus record in history (state after adding item to storage))
                }


                if (
                    stock_state_before_current.total_quantity.Equals(reference_stock_state.total_quantity) &&
                    stock_state_before_current.in_storage_quantity.Equals(reference_stock_state.in_storage_quantity) &&
                    stock_state_before_current.blocked_quantity.Equals(reference_stock_state.blocked_quantity)
                )
                {
                    _context.Sale_Item_Stock_History.Remove(reference_stock_state);
                }
                else
                {
                    //miscallculation (previous history record should differ from current only in item_on_protocol quantity, so if quantity = 0 there should be no difference)
                    return_obj.error_object = new Sale_Item_On_Protocol_Error_Model
                    {
                        code = "36",
                        timestamp = reference_stock_state.timestamp,
                        sale_item_in_storage_id = sale_item_in_storage.id,
                        required_quantity = 0
                    };
                    return return_obj;
                }

                if (
                    in_storage_stock_state_before_current.in_storage_quantity.Equals(in_storage_reference_stock_state.in_storage_quantity) &&
                    in_storage_stock_state_before_current.blocked_quantity.Equals(in_storage_reference_stock_state.blocked_quantity)
                )
                {
                    _context.Sale_Item_In_Storage_Stock_History.Remove(in_storage_reference_stock_state);
                }
                else
                {
                    //miscallculation (previous history record should differ from current only in item_on_protocol quantity, so if quantity = 0 there should be no difference)
                    return_obj.error_object = new Sale_Item_On_Protocol_Error_Model
                    {
                        code = "36",
                        timestamp = in_storage_reference_stock_state.timestamp,
                        sale_item_in_storage_id = sale_item_in_storage.id,
                        required_quantity = 0
                    };
                    return return_obj;
                }

                _context.Sale_Item_On_Protocol.Remove(sale_item_on_protocol_exists);
                return_obj.id = -1;

            }

            _context.SaveChanges();

            return return_obj;
        }



        public async Task<Sale_Item_On_Protocol_Id_Model> Edit_Sale_Item_On_Protocol(Edit_Sale_Item_On_Protocol_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var sale_item_on_protocol_exists = _context.Sale_Item_On_Protocol.Where(sp =>
                    sp.id.Equals(input_obj.id)
                )
                .Include(sp => sp.sale_item_in_storage_FK)
                    .ThenInclude(ls => ls.sale_item_FK)
                .Include(sp => sp.sale_protocol_FK)
                .FirstOrDefault();

                if (
                    sale_item_on_protocol_exists == null ||
                    sale_item_on_protocol_exists.sale_protocol_FK == null ||
                    sale_item_on_protocol_exists.sale_item_in_storage_FK == null ||
                    sale_item_on_protocol_exists.sale_item_in_storage_FK.sale_item_FK == null
                )
                {
                    throw new Exception("19");//not found
                }


                if (sale_item_on_protocol_exists.sale_protocol_FK.state.Equals(Protocol_State.Offer))
                {
                    throw new Exception("19");//not found protocol which isn't in offer state
                }

                Sale_Protocol protocol = sale_item_on_protocol_exists.sale_protocol_FK;
                Sale_Item_In_Storage sale_item_in_storage = sale_item_on_protocol_exists.sale_item_in_storage_FK;

                var comment = await Crypto.Encrypt(session, input_obj.comment);
                if (comment == null)
                {
                    throw new Exception("2");//encryption error
                }

                sale_item_on_protocol_exists.comment = comment;

                if (input_obj.total_quantity > sale_item_on_protocol_exists.total_quantity)
                {
                    decimal quantity_to_add = input_obj.total_quantity - sale_item_on_protocol_exists.total_quantity;

                    return Add_Sale_Item_On_Protocol(session, ref protocol, ref sale_item_in_storage, ref sale_item_on_protocol_exists, quantity_to_add);
                }

                if (input_obj.total_quantity < sale_item_on_protocol_exists.total_quantity)
                {
                    decimal quantity_to_remove = sale_item_on_protocol_exists.total_quantity - input_obj.total_quantity;

                    return Remove_Sale_Item_On_Protocol(session, ref protocol, ref sale_item_in_storage, ref sale_item_on_protocol_exists, quantity_to_remove);
                }

                return new Sale_Item_On_Protocol_Id_Model { id = sale_item_on_protocol_exists.id };
            }

        }


        public List<Sale_Item_On_Protocol_Offer_Model> Get_Available_Sale_Items_To_Offer(Get_Available_Sale_Items_To_Release_Data input_obj, Session_Data session)
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
                    sp.id.Equals(input_obj.protocol_id) &&
                    sp.state.Equals(Protocol_State.Offer) &&
                    sp.deleted.Equals(false)
                )
                .FirstOrDefault();
                if (protocol == null)
                {
                    throw new Exception("19");// protocol not found
                }

                var available_sale_item_list = _context.Sale_Item
                    .Include(si => si.sale_item_in_storage_list_FK)
                        .ThenInclude(sis => sis.storage_FK)
                    .Include(li => li.counting_unit_FK)
                    .ToList();

                List<Sale_Item_On_Protocol_Offer_Model> return_obj = new List<Sale_Item_On_Protocol_Offer_Model>();

                if (available_sale_item_list.Count.Equals(0))
                {
                    return return_obj;
                }

                Sale_Item_On_Protocol_Offer_Model? sale_item_in_return_obj_exists;


                foreach (var sale_item in available_sale_item_list)
                {
                    if (
                        sale_item == null ||
                        sale_item.counting_unit_FK == null ||
                        sale_item.sale_item_in_storage_list_FK == null
                    )
                    {
                        throw new Exception("19");//not found
                    }


                    foreach (var sale_item_in_storage in sale_item.sale_item_in_storage_list_FK)
                    {
                        if (
                            sale_item_in_storage == null ||
                            sale_item_in_storage.storage_FK == null
                        )
                        {
                            throw new Exception("19");//not found
                        }


                        sale_item_in_return_obj_exists = return_obj.Where(ro => ro.sale_item_id.Equals(sale_item.id)).FirstOrDefault();


                        if (sale_item_in_return_obj_exists == null)
                        {
                            return_obj.Add(
                                new Sale_Item_On_Protocol_Offer_Model
                                {
                                    sale_item_id = sale_item.id,
                                    catalog_number = sale_item.catalog_number,
                                    product_name = sale_item.product_name,
                                    counting_unit = sale_item.counting_unit_FK.unit,
                                    in_storage_list =
                                    [
                                        new Sale_Item_On_Protocol_Offer_From_Storage_Model
                                        {
                                            storage_id = sale_item_in_storage.storage_FKid,
                                            storage_number = sale_item_in_storage.storage_FK.number,
                                            storage_name = sale_item_in_storage.storage_FK.name,
                                            sale_item_in_storage_id = sale_item_in_storage.id,
                                            counting_unit = sale_item.counting_unit_FK.unit
                                        }
                                    ]
                                }
                            );

                        }
                        else
                        {
                            sale_item_in_return_obj_exists.in_storage_list.Add(
                                new Sale_Item_On_Protocol_Offer_From_Storage_Model
                                {
                                    storage_id = sale_item_in_storage.storage_FKid,
                                    storage_number = sale_item_in_storage.storage_FK.number,
                                    storage_name = sale_item_in_storage.storage_FK.name,
                                    sale_item_in_storage_id = sale_item_in_storage.id,
                                    counting_unit = sale_item.counting_unit_FK.unit
                                }
                            );
                        }


                    }

                }


                return return_obj;
            }
        }



        public List<Sale_Item_On_Protocol_Release_Available_Model> Get_Available_Sale_Items_To_Release(Get_Available_Sale_Items_To_Release_Data input_obj, Session_Data session)
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
                    sp.id.Equals(input_obj.protocol_id) &&
                    sp.deleted.Equals(false)
                )
                .Include(lp => lp.sale_item_on_protocol_list_FK)
                .FirstOrDefault();
                if (protocol == null || protocol.sale_item_on_protocol_list_FK == null || protocol.state.Equals(Protocol_State.Offer))
                {
                    throw new Exception("19");// protocol not found
                }

                var available_sale_item_list = _context.Sale_Item
                    .Include(si => si.sale_item_in_storage_list_FK)
                        .ThenInclude(sis => sis.storage_FK)
                    .Include(si => si.counting_unit_FK)
                    .ToList();

                List<Sale_Item_On_Protocol_Release_Available_Model> return_obj = new List<Sale_Item_On_Protocol_Release_Available_Model>();

                if (available_sale_item_list.Count.Equals(0))
                {
                    return return_obj;
                }

                Sale_Item_On_Protocol_Release_Available_Model? sale_item_in_return_obj_exists;


                Sale_Item_Stock_History? stock_state;
                Sale_Item_In_Storage_Stock_History? in_storage_stock_state;

                Sale_Item_On_Protocol? item_on_protocol;

                decimal free_quantity = -1;

                foreach (var sale_item in available_sale_item_list)
                {
                    if (
                        sale_item == null ||
                        sale_item.counting_unit_FK == null ||
                        sale_item.sale_item_in_storage_list_FK == null
                    )
                    {
                        throw new Exception("19");//not found
                    }

                    stock_state = _context.Sale_Item_Stock_History.Where(ssh =>
                        ssh.sale_item_FKid.Equals(sale_item.id) &&
                        ssh.timestamp <= protocol.timestamp
                    ).MaxBy(ssh => ssh.timestamp);

                    if (stock_state == null)
                    {
                        continue;
                    }

                    if ((stock_state.in_storage_quantity - stock_state.blocked_quantity).Equals(0))
                    {
                        continue;
                    }

                    foreach (var sale_item_in_storage in sale_item.sale_item_in_storage_list_FK)
                    {
                        if (
                            sale_item_in_storage == null ||
                            sale_item_in_storage.storage_FK == null
                        )
                        {
                            throw new Exception("19");//not found
                        }

                        in_storage_stock_state = _context.Sale_Item_In_Storage_Stock_History.Where(issh =>
                            issh.sale_item_in_storage_FKid.Equals(sale_item_in_storage.id) &&
                            issh.timestamp <= protocol.timestamp
                        ).MaxBy(issh => issh.timestamp);

                        if (in_storage_stock_state == null)
                        {
                            continue;
                        }

                        if ((in_storage_stock_state.in_storage_quantity - in_storage_stock_state.blocked_quantity).Equals(0))
                        {
                            continue;
                        }

                        free_quantity = in_storage_stock_state.in_storage_quantity - in_storage_stock_state.blocked_quantity;

                        if (protocol.state.Equals(Protocol_State.Draft))
                        {
                            item_on_protocol = protocol.sale_item_on_protocol_list_FK.Where(sop => sop.sale_item_in_storage_FKid.Equals(sale_item_in_storage.id)).FirstOrDefault();

                            if (item_on_protocol != null)
                            {
                                free_quantity -= item_on_protocol.total_quantity;

                                if (free_quantity <= 0)
                                {
                                    continue;
                                }
                            }
                        }


                        sale_item_in_return_obj_exists = return_obj.Where(ro => ro.sale_item_id.Equals(sale_item.id)).FirstOrDefault();


                        if (sale_item_in_return_obj_exists == null)
                        {
                            return_obj.Add(
                                new Sale_Item_On_Protocol_Release_Available_Model
                                {
                                    sale_item_id = sale_item.id,
                                    catalog_number = sale_item.catalog_number,
                                    product_name = sale_item.product_name,
                                    counting_unit = sale_item.counting_unit_FK.unit,
                                    total_quantity = free_quantity,
                                    in_storage_list =
                                    [
                                        new Sale_Item_On_Protocol_Release_From_Storage_Model
                                        {
                                            storage_id = sale_item_in_storage.storage_FKid,
                                            storage_number = sale_item_in_storage.storage_FK.number,
                                            storage_name = sale_item_in_storage.storage_FK.name,
                                            sale_item_in_storage_id = sale_item_in_storage.id,
                                            counting_unit = sale_item.counting_unit_FK.unit,
                                            total_quantity = free_quantity
                                        }
                                    ]
                                }
                            );

                        }
                        else
                        {
                            sale_item_in_return_obj_exists.in_storage_list.Add(
                                new Sale_Item_On_Protocol_Release_From_Storage_Model
                                {
                                    storage_id = sale_item_in_storage.storage_FKid,
                                    storage_number = sale_item_in_storage.storage_FK.number,
                                    storage_name = sale_item_in_storage.storage_FK.name,
                                    sale_item_in_storage_id = sale_item_in_storage.id,
                                    counting_unit = sale_item.counting_unit_FK.unit,
                                    total_quantity = free_quantity
                                }
                            );

                            sale_item_in_return_obj_exists.total_quantity += free_quantity;
                        }


                    }

                }


                return return_obj;
            }
        }



        public async Task<List<Sale_Item_On_Protocol_Model>> Get_Sale_Item_On_Protocol_List(Get_Sale_Item_From_Protocol_Data input_obj, Session_Data session)
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
                    sp.id.Equals(input_obj.protocol_id) &&
                    sp.deleted.Equals(false)
                )
                .Include(lp => lp.sale_item_on_protocol_list_FK)
                    .ThenInclude(lop => lop.sale_item_in_storage_FK)
                        .ThenInclude(lis => lis.sale_item_FK)
                            .ThenInclude(li => li.counting_unit_FK)
                .FirstOrDefault();
                if (
                    protocol == null ||
                    protocol.sale_item_on_protocol_list_FK == null
                )
                {
                    throw new Exception("19"); //not found
                }

                List<Sale_Item_On_Protocol_Model> return_obj = new List<Sale_Item_On_Protocol_Model>();

                if (protocol.sale_item_on_protocol_list_FK.Count.Equals(0))
                {
                    return return_obj;
                }

                List<Encrypted_Object> items_on_protocol_comments = new List<Encrypted_Object>();

                foreach (var item_on_protocol in protocol.sale_item_on_protocol_list_FK)
                {
                    if (
                        item_on_protocol == null ||
                        item_on_protocol.sale_item_in_storage_FK == null ||
                        item_on_protocol.sale_item_in_storage_FK.sale_item_FK == null ||
                        item_on_protocol.sale_item_in_storage_FK.sale_item_FK.counting_unit_FK == null
                    )
                    {
                        throw new Exception("19");//not found
                    }

                    return_obj.Add(new Sale_Item_On_Protocol_Model
                    {
                        id = item_on_protocol.id,
                        catalog_number = item_on_protocol.sale_item_in_storage_FK.sale_item_FK.catalog_number,
                        product_name = item_on_protocol.sale_item_in_storage_FK.sale_item_FK.product_name,
                        counting_unit = item_on_protocol.sale_item_in_storage_FK.sale_item_FK.counting_unit_FK.unit,
                        sale_item_id = item_on_protocol.sale_item_in_storage_FK.sale_item_FKid,
                        sale_item_in_storage_id = item_on_protocol.sale_item_in_storage_FKid,
                        total_quantity = item_on_protocol.total_quantity,
                        weight_kg = item_on_protocol.sale_item_in_storage_FK.sale_item_FK.weight_kg,
                        total_weight_kg = item_on_protocol.total_weight_kg,
                        total_worth = item_on_protocol.total_worth,
                        total_area_m2 = item_on_protocol.total_area_m2
                    });

                    items_on_protocol_comments.Add(new Encrypted_Object { id = item_on_protocol.id, encryptedValue = item_on_protocol.comment });
                }


                List<Decrypted_Object> decrypted_items_on_protocol_comments = await Crypto.DecryptList(session, items_on_protocol_comments);

                if (
                    decrypted_items_on_protocol_comments == null ||
                    decrypted_items_on_protocol_comments.Count != items_on_protocol_comments.Count
                )
                {
                    throw new Exception("3");//decryption error
                }

                foreach (var item_on_protocol_model in return_obj)
                {
                    var comment = decrypted_items_on_protocol_comments.Where(d => d.id.Equals(item_on_protocol_model.id)).FirstOrDefault();

                    if (comment != null)
                    {
                        item_on_protocol_model.comment = comment.decryptedValue;
                    }
                    else
                    {
                        throw new Exception("3");//error while decrypting data 
                    }

                }

                return return_obj;
            }

        }


    }
}
