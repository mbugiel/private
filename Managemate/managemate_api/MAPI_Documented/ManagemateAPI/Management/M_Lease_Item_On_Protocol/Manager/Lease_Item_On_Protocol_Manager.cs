using ManagemateAPI.Database.Context;
using ManagemateAPI.Database.Tables;
using ManagemateAPI.Encryption;
using ManagemateAPI.Encryption.Input_Objects;
using ManagemateAPI.Management.M_Lease_Item_On_Protocol.Input_Objects;
using ManagemateAPI.Management.M_Lease_Item_On_Protocol.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.Shared.Enum;
using ManagemateAPI.Management.Shared.Static;
using ManagemateAPI.Management.Shared.Validator;
using Microsoft.EntityFrameworkCore;

/*
 * This is the Lease_Item_On_Receipt_Manager with methods dedicated to the Lease_Item_On_Receipt table.
 * 
 * It contains methods to:
 * add records,
 * edit records,
 * delete records,
 * get record by id,
 */
namespace ManagemateAPI.Management.M_Lease_Item_On_Protocol.Manager
{
    public class Lease_Item_On_Protocol_Manager
    {

        private DB_Context _context;
        private readonly IConfiguration _configuration;


        public Lease_Item_On_Protocol_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /* 
         * Add_Lease_Item_On_Protocol method
         * This method is used to add new records to the lease_item_on_receipt table.
         * 
         * It accepts Add_Lease_Item_On_Receipt_Data object as input.
         * It then adds new record with values based on the data given in the input object.
         */

        public string Add_Lease_Item_On_Offer_Protocol(Add_Lease_Item_On_Protocol_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var protocol = _context.Lease_Protocol.Where(lp =>
                    lp.id.Equals(input_obj.protocol_FK) &&
                    lp.state.Equals(Protocol_State.Offer) &&
                    lp.deleted.Equals(false)
                )
                .FirstOrDefault();

                var lease_item_in_storage = _context.Lease_Item_In_Storage.Where(li =>
                    li.id.Equals(input_obj.lease_item_in_storage_FK)
                )
                .Include(li => li.lease_item_FK)
                .FirstOrDefault();

                if (
                    protocol == null ||
                    lease_item_in_storage == null ||
                    lease_item_in_storage.lease_item_FK == null
                )
                {
                    throw new Exception("19");//not found
                }

                if (
                    protocol.type.Equals(Lease_Protocol_Type.Return)
                )
                {
                    throw new Exception("34");//return protocol can not be in reservation or offer state
                }

                var lease_item_on_protocol_exists = _context.Lease_Item_On_Protocol.Where(iop =>
                    iop.lease_item_in_storage_FKid.Equals(lease_item_in_storage.id) &&
                    iop.lease_protocol_FKid.Equals(protocol.id)
                ).FirstOrDefault();

                
                if(lease_item_on_protocol_exists == null)
                {
                    var new_item_on_protocol = new Lease_Item_On_Protocol
                    {
                        lease_item_in_storage_FKid = lease_item_in_storage.id,
                        lease_protocol_FKid = protocol.id,
                        total_quantity = input_obj.total_quantity,
                        total_weight_kg = input_obj.total_quantity * lease_item_in_storage.lease_item_FK.weight_kg,
                        total_area_m2 = input_obj.total_quantity * lease_item_in_storage.lease_item_FK.area_m2,
                        total_worth = input_obj.total_quantity * lease_item_in_storage.lease_item_FK.price,
                        comment = Array.Empty<byte>()
                    };

                    _context.Lease_Item_On_Protocol.Add(new_item_on_protocol);
                }
                else
                {
                    lease_item_on_protocol_exists.total_quantity += input_obj.total_quantity;                    
                }
                
                _context.SaveChanges();

                return Info.SUCCESSFULLY_ADDED;
            }
        }

        public async Task<string> Edit_Lease_Item_On_Offer_Protocol(Edit_Lease_Item_On_Protocol_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var lease_item_on_protocol_exists = _context.Lease_Item_On_Protocol.Where(lp =>
                    lp.id.Equals(input_obj.id)
                )
                .Include(lp => lp.lease_protocol_FK)
                .FirstOrDefault();

                if (
                    lease_item_on_protocol_exists == null ||
                    lease_item_on_protocol_exists.lease_protocol_FK == null
                )
                {
                    throw new Exception("19");//not found
                }

                if (!lease_item_on_protocol_exists.lease_protocol_FK.state.Equals(Protocol_State.Offer))
                {
                    throw new Exception("19");//not found
                }

                if (
                    lease_item_on_protocol_exists.lease_protocol_FK.type.Equals(Lease_Protocol_Type.Return)
                )
                {
                    throw new Exception("34");//return protocol can not be in reservation or offer state
                }


                var comment = await Crypto.Encrypt(session, input_obj.comment);
                if (comment == null)
                {
                    throw new Exception("2");//encryption error
                }

                lease_item_on_protocol_exists.comment = comment;

                if (input_obj.total_quantity > lease_item_on_protocol_exists.total_quantity)
                {
                    decimal quantity_to_add = input_obj.total_quantity - lease_item_on_protocol_exists.total_quantity;

                    lease_item_on_protocol_exists.total_quantity += quantity_to_add;
                }

                if (input_obj.total_quantity < lease_item_on_protocol_exists.total_quantity)
                {
                    decimal quantity_to_remove = lease_item_on_protocol_exists.total_quantity - input_obj.total_quantity;

                    if (quantity_to_remove > lease_item_on_protocol_exists.total_quantity || quantity_to_remove < 0)
                    {
                        throw new Exception("39");// negative quantity
                    }

                    lease_item_on_protocol_exists.total_quantity -= quantity_to_remove;

                    if (lease_item_on_protocol_exists.total_quantity.Equals(0))
                    {
                        _context.Lease_Item_On_Protocol.Remove(lease_item_on_protocol_exists);
                    }                    
                }

                _context.SaveChanges();

                return Info.SUCCESSFULLY_CHANGED;
            }
        }

        public Lease_Item_On_Protocol_Id_Model Add_Lease_Item_On_Protocol(Add_Lease_Item_On_Protocol_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var protocol = _context.Lease_Protocol.Where(lp => 
                    lp.id.Equals(input_obj.protocol_FK) && 
                    lp.deleted.Equals(false) &&
                    lp.lease_to_sale_protocol_FKid == null
                )
                .FirstOrDefault();


                var lease_item_in_storage = _context.Lease_Item_In_Storage.Where(li => 
                    li.id.Equals(input_obj.lease_item_in_storage_FK)
                )
                .Include(li => li.lease_item_FK)
                .FirstOrDefault();

                if(
                    protocol == null ||
                    lease_item_in_storage == null ||
                    lease_item_in_storage.lease_item_FK == null
                )
                {
                    throw new Exception("19");//not found
                }

                if (
                    protocol.type.Equals(Lease_Protocol_Type.Return) &&
                    (
                        protocol.state.Equals(Protocol_State.Reserved) ||
                        protocol.state.Equals(Protocol_State.Offer)
                    )
                )
                {
                    throw new Exception("34");//return protocol can not be in reservation or offer state
                }

                if(protocol.state.Equals(Protocol_State.Offer))
                {
                    throw new Exception("19");//not found protocol which isn't in offer state
                }

                var lease_item_on_protocol_exists = _context.Lease_Item_On_Protocol.Where(iop =>
                    iop.lease_item_in_storage_FKid.Equals(lease_item_in_storage.id) &&
                    iop.lease_protocol_FKid.Equals(protocol.id)
                ).FirstOrDefault();

                
                return Add_Lease_Item_On_Protocol(session, ref protocol, ref lease_item_in_storage, ref lease_item_on_protocol_exists, input_obj.total_quantity);
            }

        }


        private Lease_Item_On_Protocol_Id_Model Add_Lease_Item_On_Protocol(Session_Data session, ref Lease_Protocol protocol, ref Lease_Item_In_Storage lease_item_in_storage, ref Lease_Item_On_Protocol? lease_item_on_protocol_exists, decimal input_total_quantity)
        {
            DateTime protocolTimestamp = protocol.timestamp;
            Lease_Item_In_Storage lease_item_in_storage_for_lambda = lease_item_in_storage;
            Lease_Protocol protocol_for_lambda = protocol;

            var stock_history = _context.Lease_Item_Stock_History.Where(lsh =>
                lsh.lease_item_FKid.Equals(lease_item_in_storage_for_lambda.lease_item_FKid)
            ).ToList();

            var in_storage_stock_history = _context.Lease_Item_In_Storage_Stock_History.Where(ish =>
                ish.lease_item_in_storage_FKid.Equals(lease_item_in_storage_for_lambda.id)
            ).ToList();

            Lease_Item_Stock_History? reference_stock_state;
            Lease_Item_In_Storage_Stock_History? in_storage_reference_stock_state;

            Lease_Item_Out_Of_Storage? out_of_storage_related_object = null;
            Lease_Item_Out_Of_Storage_History? out_of_storage_reference = null;
            List<Lease_Item_Out_Of_Storage_History> out_of_storage_history_after_timestamp = new List<Lease_Item_Out_Of_Storage_History>();

            if (lease_item_on_protocol_exists == null)
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


            Lease_Item_On_Protocol_Id_Model return_obj = new Lease_Item_On_Protocol_Id_Model();

            if (protocol.type.Equals(Lease_Protocol_Type.Release))
            {
                if (lease_item_on_protocol_exists != null && protocol.state.Equals(Protocol_State.Draft))
                {
                    if (reference_stock_state.in_storage_quantity - reference_stock_state.blocked_quantity - lease_item_on_protocol_exists.total_quantity < input_total_quantity)
                    {
                        //not enough in stock
                        return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "22",
                            timestamp = reference_stock_state.timestamp,
                            lease_item_in_storage_id = lease_item_in_storage.id,
                            required_quantity = input_total_quantity - (reference_stock_state.in_storage_quantity - reference_stock_state.blocked_quantity - lease_item_on_protocol_exists.total_quantity)
                        };
                        return return_obj;
                    }

                    if (in_storage_reference_stock_state.in_storage_quantity - in_storage_reference_stock_state.blocked_quantity - lease_item_on_protocol_exists.total_quantity < input_total_quantity)
                    {
                        //not enough in stock
                        return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "22",
                            timestamp = in_storage_reference_stock_state.timestamp,
                            lease_item_in_storage_id = lease_item_in_storage.id,
                            required_quantity = input_total_quantity - (in_storage_reference_stock_state.in_storage_quantity - in_storage_reference_stock_state.blocked_quantity - lease_item_on_protocol_exists.total_quantity)
                        };
                        return return_obj;
                    }
                }
                else
                {
                    if (reference_stock_state.in_storage_quantity - reference_stock_state.blocked_quantity < input_total_quantity)
                    {
                        //not enough in stock
                        return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "22",
                            timestamp = reference_stock_state.timestamp,
                            lease_item_in_storage_id = lease_item_in_storage.id,
                            required_quantity = input_total_quantity - (reference_stock_state.in_storage_quantity - reference_stock_state.blocked_quantity)
                        };
                        return return_obj;
                    }

                    if (in_storage_reference_stock_state.in_storage_quantity - in_storage_reference_stock_state.blocked_quantity < input_total_quantity)
                    {
                        //not enough in stock
                        return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "22",
                            timestamp = in_storage_reference_stock_state.timestamp,
                            lease_item_in_storage_id = lease_item_in_storage.id,
                            required_quantity = input_total_quantity - (in_storage_reference_stock_state.in_storage_quantity - in_storage_reference_stock_state.blocked_quantity)
                        };
                        return return_obj;
                    }
                }

                var min_stock_state = in_storage_stock_history_after_timestamp.MinBy(ishat => ishat.in_storage_quantity - ishat.blocked_quantity);

                if (min_stock_state != null)
                {
                    if (lease_item_on_protocol_exists != null && protocol.state.Equals(Protocol_State.Draft))
                    {
                        if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity - lease_item_on_protocol_exists.total_quantity < input_total_quantity)
                        {
                            // not enough in storage
                            return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                            {
                                code = "22",
                                timestamp = min_stock_state.timestamp,
                                lease_item_in_storage_id = lease_item_in_storage.id,
                                required_quantity = input_total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity - lease_item_on_protocol_exists.total_quantity)
                            };
                            return return_obj;
                        }
                    }
                    else
                    {
                        if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < input_total_quantity)
                        {
                            // not enough in storage
                            return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                            {
                                code = "22",
                                timestamp = min_stock_state.timestamp,
                                lease_item_in_storage_id = lease_item_in_storage.id,
                                required_quantity = input_total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                            };
                            return return_obj;
                        }
                    }
                }

            }

            if (protocol.type.Equals(Lease_Protocol_Type.Return) || (protocol.type.Equals(Lease_Protocol_Type.Release) && protocol.state.Equals(Protocol_State.Confirmed)))
            {
                out_of_storage_related_object = _context.Lease_Item_Out_Of_Storage.Where(lios =>
                    lios.lease_item_in_storage_FKid.Equals(lease_item_in_storage_for_lambda.id) &&
                    lios.order_FKid.Equals(protocol_for_lambda.order_FKid)
                )
                .Include(lios => lios.lease_item_out_of_storage_history_FK)
                .FirstOrDefault();
            }


            if (protocol.type.Equals(Lease_Protocol_Type.Return))
            {
                if (
                    out_of_storage_related_object == null ||
                    out_of_storage_related_object.lease_item_out_of_storage_history_FK == null
                )
                {
                    throw new Exception("19");//not found
                }


                if (lease_item_on_protocol_exists == null)
                {
                    Timestamp_Validator.Validate_Protocol_Timestamp(out_of_storage_related_object.lease_item_out_of_storage_history_FK, protocol.timestamp);

                    //latest out of storage state before protocol timestamp
                    out_of_storage_reference = out_of_storage_related_object.lease_item_out_of_storage_history_FK.Where(liosh =>
                        liosh.timestamp < protocol_for_lambda.timestamp
                    ).MaxBy(liosh => liosh.timestamp);
                    if (out_of_storage_reference == null)
                    {
                        throw new Exception("19");//not found
                    }
                }
                else
                {
                    //current out of storage state (at protocol timestamp)
                    out_of_storage_reference = out_of_storage_related_object.lease_item_out_of_storage_history_FK.Where(liosh =>
                        liosh.timestamp.Equals(protocol_for_lambda.timestamp)
                    ).FirstOrDefault();
                    if (out_of_storage_reference == null)
                    {
                        throw new Exception("19");//not found
                    }
                }

                Stock_State_Validator.Validate_Out_Of_Storage_State(out_of_storage_reference, in_storage_reference_stock_state.out_of_storage_quantity);


                if (lease_item_on_protocol_exists != null && protocol.state.Equals(Protocol_State.Draft))
                {
                    if (reference_stock_state.out_of_storage_quantity - lease_item_on_protocol_exists.total_quantity < input_total_quantity)
                    {
                        //not enough out of storage
                        return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "43",
                            timestamp = reference_stock_state.timestamp,
                            lease_item_in_storage_id = lease_item_in_storage.id,
                            required_quantity = input_total_quantity - (reference_stock_state.out_of_storage_quantity - lease_item_on_protocol_exists.total_quantity)
                        };
                        return return_obj;
                    }

                    if (in_storage_reference_stock_state.out_of_storage_quantity - lease_item_on_protocol_exists.total_quantity < input_total_quantity)
                    {
                        //not enough out of storage
                        return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "43",
                            timestamp = in_storage_reference_stock_state.timestamp,
                            lease_item_in_storage_id = lease_item_in_storage.id,
                            required_quantity = input_total_quantity - (in_storage_reference_stock_state.out_of_storage_quantity - lease_item_on_protocol_exists.total_quantity)
                        };
                        return return_obj;
                    }

                    if (out_of_storage_reference.total_quantity - lease_item_on_protocol_exists.total_quantity < input_total_quantity)
                    {
                        //not enough out of storage
                        return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "43",
                            timestamp = out_of_storage_reference.timestamp,
                            lease_item_in_storage_id = lease_item_in_storage.id,
                            required_quantity = input_total_quantity - (out_of_storage_reference.total_quantity - lease_item_on_protocol_exists.total_quantity)
                        };
                        return return_obj;
                    }
                }
                else
                {
                    if (reference_stock_state.out_of_storage_quantity < input_total_quantity)
                    {
                        //not enough out of storage
                        return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "43",
                            timestamp = reference_stock_state.timestamp,
                            lease_item_in_storage_id = lease_item_in_storage.id,
                            required_quantity = input_total_quantity - reference_stock_state.out_of_storage_quantity
                        };
                        return return_obj;
                    }

                    if (in_storage_reference_stock_state.out_of_storage_quantity < input_total_quantity)
                    {
                        //not enough out of storage
                        return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "43",
                            timestamp = in_storage_reference_stock_state.timestamp,
                            lease_item_in_storage_id = lease_item_in_storage.id,
                            required_quantity = input_total_quantity - in_storage_reference_stock_state.out_of_storage_quantity
                        };
                        return return_obj;
                    }

                    if (out_of_storage_reference.total_quantity < input_total_quantity)
                    {
                        //not enough out of storage
                        return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "43",
                            timestamp = out_of_storage_reference.timestamp,
                            lease_item_in_storage_id = lease_item_in_storage.id,
                            required_quantity = input_total_quantity - out_of_storage_reference.total_quantity
                        };
                        return return_obj;
                    }
                }

                out_of_storage_history_after_timestamp = out_of_storage_related_object.lease_item_out_of_storage_history_FK.Where(loh => loh.timestamp > protocol_for_lambda.timestamp).ToList();

                var min_out_of_storage_reference = out_of_storage_history_after_timestamp.MinBy(loh => loh.total_quantity);

                if (min_out_of_storage_reference != null)
                {
                    if (lease_item_on_protocol_exists != null && protocol.state.Equals(Protocol_State.Draft))
                    {
                        if (min_out_of_storage_reference.total_quantity - lease_item_on_protocol_exists.total_quantity < input_total_quantity)
                        {
                            // not enough items out of storage
                            return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                            {
                                code = "43",
                                timestamp = min_out_of_storage_reference.timestamp,
                                lease_item_in_storage_id = lease_item_in_storage.id,
                                required_quantity = input_total_quantity - (min_out_of_storage_reference.total_quantity - lease_item_on_protocol_exists.total_quantity)
                            };
                            return return_obj;
                        }
                    }
                    else
                    {
                        if (min_out_of_storage_reference.total_quantity < input_total_quantity)
                        {
                            // not enough items out of storage
                            return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                            {
                                code = "43",
                                timestamp = min_out_of_storage_reference.timestamp,
                                lease_item_in_storage_id = lease_item_in_storage.id,
                                required_quantity = input_total_quantity - min_out_of_storage_reference.total_quantity
                            };
                            return return_obj;
                        }
                    }
                }
            }

            Lease_Item_On_Protocol new_item_on_protocol = new Lease_Item_On_Protocol();

            if (protocol.state.Equals(Protocol_State.Draft))
            {
                if (protocol.type.Equals(Lease_Protocol_Type.Return))
                {
                    if (lease_item_on_protocol_exists == null)
                    {
                        new_item_on_protocol = new Lease_Item_On_Protocol
                        {
                            lease_item_in_storage_FKid = lease_item_in_storage.id,
                            lease_protocol_FKid = protocol.id,
                            total_quantity = input_total_quantity,
                            total_weight_kg = input_total_quantity * lease_item_in_storage.lease_item_FK.weight_kg,
                            total_area_m2 = input_total_quantity * lease_item_in_storage.lease_item_FK.area_m2,
                            total_worth = input_total_quantity * lease_item_in_storage.lease_item_FK.price,
                            comment = Array.Empty<byte>()
                        };

                        Lease_Item_Stock_History stock_update = new Lease_Item_Stock_History
                        {
                            lease_item_FKid = lease_item_in_storage.lease_item_FKid,
                            total_quantity = reference_stock_state.total_quantity,
                            in_storage_quantity = reference_stock_state.in_storage_quantity,
                            blocked_quantity = reference_stock_state.blocked_quantity,
                            out_of_storage_quantity = reference_stock_state.out_of_storage_quantity,
                            timestamp = protocol.timestamp
                        };

                        Lease_Item_In_Storage_Stock_History in_storage_stock_update = new Lease_Item_In_Storage_Stock_History
                        {
                            lease_item_in_storage_FKid = lease_item_in_storage.id,
                            total_quantity = in_storage_reference_stock_state.total_quantity,
                            in_storage_quantity = in_storage_reference_stock_state.in_storage_quantity,
                            blocked_quantity = in_storage_reference_stock_state.blocked_quantity,
                            out_of_storage_quantity = in_storage_reference_stock_state.out_of_storage_quantity,
                            timestamp = protocol.timestamp
                        };

                        Lease_Item_Out_Of_Storage_History out_of_storage_update = new Lease_Item_Out_Of_Storage_History
                        {
                            lease_item_out_of_storage_FKid = out_of_storage_related_object.id,
                            total_quantity = out_of_storage_reference.total_quantity,
                            timestamp = protocol.timestamp
                        };

                        _context.Lease_Item_Out_Of_Storage_History.Add(out_of_storage_update);
                        _context.Lease_Item_Stock_History.Add(stock_update);
                        _context.Lease_Item_In_Storage_Stock_History.Add(in_storage_stock_update);
                    }

                }


                if (protocol.type.Equals(Lease_Protocol_Type.Release))
                {
                    if (lease_item_on_protocol_exists == null)
                    {
                        new_item_on_protocol = new Lease_Item_On_Protocol
                        {
                            lease_item_in_storage_FKid = lease_item_in_storage.id,
                            lease_protocol_FKid = protocol.id,
                            total_quantity = input_total_quantity,
                            total_weight_kg = input_total_quantity * lease_item_in_storage.lease_item_FK.weight_kg,
                            total_area_m2 = input_total_quantity * lease_item_in_storage.lease_item_FK.area_m2,
                            total_worth = input_total_quantity * lease_item_in_storage.lease_item_FK.price,
                            comment = Array.Empty<byte>()
                        };

                        Lease_Item_Stock_History stock_update = new Lease_Item_Stock_History
                        {
                            lease_item_FKid = lease_item_in_storage.lease_item_FKid,
                            total_quantity = reference_stock_state.total_quantity,
                            in_storage_quantity = reference_stock_state.in_storage_quantity,
                            blocked_quantity = reference_stock_state.blocked_quantity,
                            out_of_storage_quantity = reference_stock_state.out_of_storage_quantity,
                            timestamp = protocol.timestamp
                        };

                        Lease_Item_In_Storage_Stock_History in_storage_stock_update = new Lease_Item_In_Storage_Stock_History
                        {
                            lease_item_in_storage_FKid = lease_item_in_storage.id,
                            total_quantity = in_storage_reference_stock_state.total_quantity,
                            in_storage_quantity = in_storage_reference_stock_state.in_storage_quantity,
                            blocked_quantity = in_storage_reference_stock_state.blocked_quantity,
                            out_of_storage_quantity = in_storage_reference_stock_state.out_of_storage_quantity,
                            timestamp = protocol.timestamp
                        };

                        _context.Lease_Item_Stock_History.Add(stock_update);
                        _context.Lease_Item_In_Storage_Stock_History.Add(in_storage_stock_update);
                    }
                }

            }
            else if (protocol.state.Equals(Protocol_State.Reserved))
            {
                if (protocol.type.Equals(Lease_Protocol_Type.Release))
                {
                    if (lease_item_on_protocol_exists != null)
                    {
                        reference_stock_state.blocked_quantity += input_total_quantity;
                        in_storage_reference_stock_state.blocked_quantity += input_total_quantity;
                    }
                    else
                    {
                        new_item_on_protocol = new Lease_Item_On_Protocol
                        {
                            lease_item_in_storage_FKid = lease_item_in_storage.id,
                            lease_protocol_FKid = protocol.id,
                            total_quantity = input_total_quantity,
                            total_weight_kg = input_total_quantity * lease_item_in_storage.lease_item_FK.weight_kg,
                            total_area_m2 = input_total_quantity * lease_item_in_storage.lease_item_FK.area_m2,
                            total_worth = input_total_quantity * lease_item_in_storage.lease_item_FK.price,
                            comment = Array.Empty<byte>()
                        };

                        Lease_Item_Stock_History stock_update = new Lease_Item_Stock_History
                        {
                            lease_item_FKid = lease_item_in_storage.lease_item_FKid,
                            total_quantity = reference_stock_state.total_quantity,
                            in_storage_quantity = reference_stock_state.in_storage_quantity,
                            blocked_quantity = reference_stock_state.blocked_quantity + input_total_quantity,
                            out_of_storage_quantity = reference_stock_state.out_of_storage_quantity,
                            timestamp = protocol.timestamp
                        };

                        Lease_Item_In_Storage_Stock_History in_storage_stock_update = new Lease_Item_In_Storage_Stock_History
                        {
                            lease_item_in_storage_FKid = lease_item_in_storage.id,
                            total_quantity = in_storage_reference_stock_state.total_quantity,
                            in_storage_quantity = in_storage_reference_stock_state.in_storage_quantity,
                            blocked_quantity = in_storage_reference_stock_state.blocked_quantity + input_total_quantity,
                            out_of_storage_quantity = in_storage_reference_stock_state.out_of_storage_quantity,
                            timestamp = protocol.timestamp
                        };

                        _context.Lease_Item_Stock_History.Add(stock_update);
                        _context.Lease_Item_In_Storage_Stock_History.Add(in_storage_stock_update);
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
            }
            else if (protocol.state.Equals(Protocol_State.Confirmed))
            {
                if (protocol.type.Equals(Lease_Protocol_Type.Return))
                {
                    if (lease_item_on_protocol_exists != null)
                    {
                        //moving items from construction site to theirs storage
                        reference_stock_state.out_of_storage_quantity -= input_total_quantity;
                        reference_stock_state.in_storage_quantity += input_total_quantity;

                        in_storage_reference_stock_state.out_of_storage_quantity -= input_total_quantity;
                        in_storage_reference_stock_state.in_storage_quantity += input_total_quantity;

                        out_of_storage_reference.total_quantity -= input_total_quantity;
                    }
                    else
                    {
                        new_item_on_protocol = new Lease_Item_On_Protocol
                        {
                            lease_item_in_storage_FKid = lease_item_in_storage.id,
                            lease_protocol_FKid = protocol.id,
                            total_quantity = input_total_quantity,
                            total_weight_kg = input_total_quantity * lease_item_in_storage.lease_item_FK.weight_kg,
                            total_area_m2 = input_total_quantity * lease_item_in_storage.lease_item_FK.area_m2,
                            total_worth = input_total_quantity * lease_item_in_storage.lease_item_FK.price,
                            comment = Array.Empty<byte>()
                        };

                        Lease_Item_Stock_History stock_update = new Lease_Item_Stock_History
                        {
                            lease_item_FKid = lease_item_in_storage.lease_item_FKid,
                            total_quantity = reference_stock_state.total_quantity,
                            in_storage_quantity = reference_stock_state.in_storage_quantity + input_total_quantity,
                            blocked_quantity = reference_stock_state.blocked_quantity,
                            out_of_storage_quantity = reference_stock_state.out_of_storage_quantity - input_total_quantity,
                            timestamp = protocol.timestamp
                        };

                        Lease_Item_In_Storage_Stock_History in_storage_stock_update = new Lease_Item_In_Storage_Stock_History
                        {
                            lease_item_in_storage_FKid = lease_item_in_storage.id,
                            total_quantity = in_storage_reference_stock_state.total_quantity,
                            in_storage_quantity = in_storage_reference_stock_state.in_storage_quantity + input_total_quantity,
                            blocked_quantity = in_storage_reference_stock_state.blocked_quantity,
                            out_of_storage_quantity = in_storage_reference_stock_state.out_of_storage_quantity - input_total_quantity,
                            timestamp = protocol.timestamp
                        };

                        Lease_Item_Out_Of_Storage_History out_of_storage_update = new Lease_Item_Out_Of_Storage_History
                        {
                            lease_item_out_of_storage_FKid = out_of_storage_related_object.id,
                            total_quantity = out_of_storage_reference.total_quantity - input_total_quantity,
                            timestamp = protocol.timestamp
                        };

                        _context.Lease_Item_Out_Of_Storage_History.Add(out_of_storage_update);
                        _context.Lease_Item_Stock_History.Add(stock_update);
                        _context.Lease_Item_In_Storage_Stock_History.Add(in_storage_stock_update);
                    }

                    foreach (var next_state in stock_history_after_timestamp)
                    {
                        next_state.out_of_storage_quantity -= input_total_quantity;
                        next_state.in_storage_quantity += input_total_quantity;
                    }

                    foreach (var in_storage_next_state in in_storage_stock_history_after_timestamp)
                    {
                        in_storage_next_state.out_of_storage_quantity -= input_total_quantity;
                        in_storage_next_state.in_storage_quantity += input_total_quantity;
                    }

                    foreach (var out_of_storage_next_state in out_of_storage_history_after_timestamp)
                    {
                        out_of_storage_next_state.total_quantity -= input_total_quantity;
                    }

                }

                if (protocol.type.Equals(Lease_Protocol_Type.Release))
                {
                    if (lease_item_on_protocol_exists != null)
                    {
                        if (
                            out_of_storage_related_object == null ||
                            out_of_storage_related_object.lease_item_out_of_storage_history_FK == null
                        )
                        {
                            throw new Exception("19");//not found
                        }

                        //current out of storage state (at protocol timestamp)
                        out_of_storage_reference = out_of_storage_related_object.lease_item_out_of_storage_history_FK.Where(liosh =>
                            liosh.timestamp.Equals(protocol_for_lambda.timestamp)
                        ).FirstOrDefault();
                        if (out_of_storage_reference == null)
                        {
                            throw new Exception("19");//not found
                        }

                        Stock_State_Validator.Validate_Out_Of_Storage_State(out_of_storage_reference, in_storage_reference_stock_state.out_of_storage_quantity);

                        reference_stock_state.in_storage_quantity -= input_total_quantity;
                        reference_stock_state.out_of_storage_quantity += input_total_quantity;

                        in_storage_reference_stock_state.in_storage_quantity -= input_total_quantity;
                        in_storage_reference_stock_state.out_of_storage_quantity += input_total_quantity;

                        out_of_storage_reference.total_quantity += input_total_quantity;
                    }
                    else
                    {
                        new_item_on_protocol = new Lease_Item_On_Protocol
                        {
                            lease_item_in_storage_FKid = lease_item_in_storage.id,
                            lease_protocol_FKid = protocol.id,
                            total_quantity = input_total_quantity,
                            total_weight_kg = input_total_quantity * lease_item_in_storage.lease_item_FK.weight_kg,
                            total_area_m2 = input_total_quantity * lease_item_in_storage.lease_item_FK.area_m2,
                            total_worth = input_total_quantity * lease_item_in_storage.lease_item_FK.price,
                            comment = Array.Empty<byte>()
                        };

                        Lease_Item_Stock_History stock_update = new Lease_Item_Stock_History
                        {
                            lease_item_FKid = lease_item_in_storage.lease_item_FKid,
                            total_quantity = reference_stock_state.total_quantity,
                            in_storage_quantity = reference_stock_state.in_storage_quantity - input_total_quantity,
                            blocked_quantity = reference_stock_state.blocked_quantity,
                            out_of_storage_quantity = reference_stock_state.out_of_storage_quantity + input_total_quantity,
                            timestamp = protocol.timestamp
                        };

                        Lease_Item_In_Storage_Stock_History in_storage_stock_update = new Lease_Item_In_Storage_Stock_History
                        {
                            lease_item_in_storage_FKid = lease_item_in_storage.id,
                            total_quantity = in_storage_reference_stock_state.total_quantity,
                            in_storage_quantity = in_storage_reference_stock_state.in_storage_quantity - input_total_quantity,
                            blocked_quantity = in_storage_reference_stock_state.blocked_quantity,
                            out_of_storage_quantity = in_storage_reference_stock_state.out_of_storage_quantity + input_total_quantity,
                            timestamp = protocol.timestamp
                        };

                        _context.Lease_Item_Stock_History.Add(stock_update);
                        _context.Lease_Item_In_Storage_Stock_History.Add(in_storage_stock_update);

                        if (out_of_storage_related_object == null)
                        {
                            Lease_Item_Out_Of_Storage new_out_of_storage_object = new Lease_Item_Out_Of_Storage
                            {
                                lease_item_in_storage_FKid = lease_item_in_storage.id,
                                order_FKid = protocol.order_FKid,
                                lease_item_out_of_storage_history_FK = new List<Lease_Item_Out_Of_Storage_History>
                                    {
                                        new Lease_Item_Out_Of_Storage_History
                                        {
                                            total_quantity = input_total_quantity,
                                            timestamp = protocol.timestamp
                                        }
                                    }
                            };
                            _context.Lease_Item_Out_Of_Storage.Add(new_out_of_storage_object);
                        }
                        else
                        {
                            Timestamp_Validator.Validate_Protocol_Timestamp(out_of_storage_related_object.lease_item_out_of_storage_history_FK, protocol.timestamp);

                            //if object exists, it has to have at least one record in history
                            if (out_of_storage_related_object.lease_item_out_of_storage_history_FK.Count == 0)
                            {
                                throw new Exception("19");//not found
                            }

                            //latest out of storage state before protocol timestamp
                            out_of_storage_reference = out_of_storage_related_object.lease_item_out_of_storage_history_FK.Where(liosh =>
                                liosh.timestamp < protocol_for_lambda.timestamp
                            ).MaxBy(liosh => liosh.timestamp);

                            Lease_Item_Out_Of_Storage_History out_of_storage_update;

                            if (out_of_storage_reference == null)
                            {
                                out_of_storage_update = new Lease_Item_Out_Of_Storage_History
                                {
                                    lease_item_out_of_storage_FKid = out_of_storage_related_object.id,
                                    total_quantity = input_total_quantity,
                                    timestamp = protocol.timestamp
                                };
                            }
                            else
                            {
                                Stock_State_Validator.Validate_Out_Of_Storage_State(out_of_storage_reference, in_storage_reference_stock_state.out_of_storage_quantity);

                                out_of_storage_update = new Lease_Item_Out_Of_Storage_History
                                {
                                    lease_item_out_of_storage_FKid = out_of_storage_related_object.id,
                                    total_quantity = out_of_storage_reference.total_quantity + input_total_quantity,
                                    timestamp = protocol.timestamp
                                };
                            }

                            _context.Lease_Item_Out_Of_Storage_History.Add(out_of_storage_update);
                        }

                    }

                    if (out_of_storage_related_object != null)
                    {
                        out_of_storage_history_after_timestamp = out_of_storage_related_object.lease_item_out_of_storage_history_FK.Where(loh => loh.timestamp > protocol_for_lambda.timestamp).ToList();

                        foreach (var out_of_storage_next_state in out_of_storage_history_after_timestamp)
                        {
                            out_of_storage_next_state.total_quantity += input_total_quantity;
                        }
                    }

                    foreach (var next_state in stock_history_after_timestamp)
                    {
                        next_state.out_of_storage_quantity += input_total_quantity;
                        next_state.in_storage_quantity -= input_total_quantity;
                    }

                    foreach (var in_storage_next_state in in_storage_stock_history_after_timestamp)
                    {
                        in_storage_next_state.out_of_storage_quantity += input_total_quantity;
                        in_storage_next_state.in_storage_quantity -= input_total_quantity;
                    }
                }
            }

            
            protocol.total_weight_kg += input_total_quantity * lease_item_in_storage.lease_item_FK.weight_kg;
            protocol.total_area_m2 += input_total_quantity * lease_item_in_storage.lease_item_FK.area_m2;
            protocol.total_worth += input_total_quantity * lease_item_in_storage.lease_item_FK.price;

            if (lease_item_on_protocol_exists != null)
            {
                lease_item_on_protocol_exists.total_quantity += input_total_quantity;
                lease_item_on_protocol_exists.total_weight_kg += input_total_quantity * lease_item_in_storage.lease_item_FK.weight_kg;
                lease_item_on_protocol_exists.total_area_m2 += input_total_quantity * lease_item_in_storage.lease_item_FK.area_m2;
                lease_item_on_protocol_exists.total_worth += input_total_quantity * lease_item_in_storage.lease_item_FK.price;

                return_obj.id = lease_item_on_protocol_exists.id;
                
                _context.SaveChanges();
            }
            else
            {
                if(new_item_on_protocol.lease_protocol_FK == null)
                {
                    throw new Exception("19"); // not found (if item on protocol doesn't exist it should be created in one of if statement above)
                }

                _context.Lease_Item_On_Protocol.Add(new_item_on_protocol);
                _context.SaveChanges();

                return_obj.id = new_item_on_protocol.id;
            }

            return return_obj;

        }

        private Lease_Item_On_Protocol_Id_Model Remove_Lease_Item_On_Protocol(Session_Data session, ref Lease_Protocol protocol, ref Lease_Item_In_Storage lease_item_in_storage, ref Lease_Item_On_Protocol lease_item_on_protocol_exists, decimal input_total_quantity)
        {
            if(input_total_quantity > lease_item_on_protocol_exists.total_quantity || input_total_quantity < 0)
            {
                throw new Exception("39");// negative quantity
            }

            Lease_Item_On_Protocol_Id_Model return_obj = new Lease_Item_On_Protocol_Id_Model
            {
                id = lease_item_on_protocol_exists.id
            };


            DateTime protocolTimestamp = protocol.timestamp;
            Lease_Item_In_Storage lease_item_in_storage_for_lambda = lease_item_in_storage;
            Lease_Protocol protocol_for_lambda = protocol;

            var stock_history = _context.Lease_Item_Stock_History.Where(lsh =>
                lsh.lease_item_FKid.Equals(lease_item_in_storage_for_lambda.lease_item_FKid)
            ).ToList();

            var in_storage_stock_history = _context.Lease_Item_In_Storage_Stock_History.Where(ish =>
                ish.lease_item_in_storage_FKid.Equals(lease_item_in_storage_for_lambda.id)
            ).ToList();

            Lease_Item_Stock_History? reference_stock_state;
            Lease_Item_In_Storage_Stock_History? in_storage_reference_stock_state;

            Lease_Item_Out_Of_Storage? out_of_storage_related_object = null;
            Lease_Item_Out_Of_Storage_History? out_of_storage_reference = null;
            List<Lease_Item_Out_Of_Storage_History> out_of_storage_history_after_timestamp = new List<Lease_Item_Out_Of_Storage_History>();

            
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


            List<Lease_Item_Stock_History>  stock_history_after_timestamp = stock_history.Where(sh => sh.timestamp > protocol_for_lambda.timestamp).ToList();
            List<Lease_Item_In_Storage_Stock_History>  in_storage_stock_history_after_timestamp = in_storage_stock_history.Where(ish => ish.timestamp > protocol_for_lambda.timestamp).ToList();
            

            if (protocol.type.Equals(Lease_Protocol_Type.Return) || (protocol.type.Equals(Lease_Protocol_Type.Release) && protocol.state.Equals(Protocol_State.Confirmed)))
            {
                out_of_storage_related_object = _context.Lease_Item_Out_Of_Storage.Where(lios =>
                    lios.lease_item_in_storage_FKid.Equals(lease_item_in_storage_for_lambda.id) &&
                    lios.order_FKid.Equals(protocol_for_lambda.order_FKid)
                )
                .Include(lios => lios.lease_item_out_of_storage_history_FK)
                .FirstOrDefault();
            }


            if (protocol.type.Equals(Lease_Protocol_Type.Release))
            {
                if (protocol.state.Equals(Protocol_State.Draft))
                {
                    if (reference_stock_state.in_storage_quantity - reference_stock_state.blocked_quantity < lease_item_on_protocol_exists.total_quantity - input_total_quantity)
                    {
                        // not enough in storage
                        return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "22",
                            timestamp = reference_stock_state.timestamp,
                            lease_item_in_storage_id = lease_item_in_storage.id,
                            required_quantity = lease_item_on_protocol_exists.total_quantity - input_total_quantity - (reference_stock_state.in_storage_quantity - reference_stock_state.blocked_quantity)
                        };
                        return return_obj;
                    }

                    if (in_storage_reference_stock_state.in_storage_quantity - in_storage_reference_stock_state.blocked_quantity < lease_item_on_protocol_exists.total_quantity - input_total_quantity)
                    {
                        // not enough in storage
                        return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "22",
                            timestamp = in_storage_reference_stock_state.timestamp,
                            lease_item_in_storage_id = lease_item_in_storage.id,
                            required_quantity = lease_item_on_protocol_exists.total_quantity - input_total_quantity - (in_storage_reference_stock_state.in_storage_quantity - in_storage_reference_stock_state.blocked_quantity)
                        };
                        return return_obj;
                    }


                    var min_stock_state = in_storage_stock_history_after_timestamp.MinBy(ishat => ishat.in_storage_quantity - ishat.blocked_quantity);

                    if (min_stock_state != null)
                    {
                        if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < lease_item_on_protocol_exists.total_quantity - input_total_quantity)
                        {
                            // not enough in storage
                            return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                            {
                                code = "22",
                                timestamp = min_stock_state.timestamp,
                                lease_item_in_storage_id = lease_item_in_storage.id,
                                required_quantity = lease_item_on_protocol_exists.total_quantity - input_total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                            };
                            return return_obj;
                        }
                    }
                }
                else if(protocol.state.Equals(Protocol_State.Reserved))
                {
                    if(reference_stock_state.blocked_quantity < lease_item_on_protocol_exists.total_quantity)
                    {
                        // miscalculation (blocked quantity should be equal to input_total_quantity or greater)
                        return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "36",
                            timestamp = reference_stock_state.timestamp,
                            lease_item_in_storage_id = lease_item_in_storage.id,
                            required_quantity = lease_item_on_protocol_exists.total_quantity - reference_stock_state.blocked_quantity
                        };
                        return return_obj;
                    }

                    if(in_storage_reference_stock_state.blocked_quantity < lease_item_on_protocol_exists.total_quantity)
                    {
                        // miscalculation (blocked quantity should be equal to input_total_quantity or greater)
                        return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "36",
                            timestamp = in_storage_reference_stock_state.timestamp,
                            lease_item_in_storage_id = lease_item_in_storage.id,
                            required_quantity = lease_item_on_protocol_exists.total_quantity - in_storage_reference_stock_state.blocked_quantity
                        };
                        return return_obj;
                    }

                    var min_blocked_state = in_storage_stock_history_after_timestamp.MinBy(ishat => ishat.blocked_quantity);

                    if (min_blocked_state != null)
                    {
                        if (min_blocked_state.blocked_quantity < lease_item_on_protocol_exists.total_quantity)
                        {
                            // miscalculation (blocked quantity should be equal to input_total_quantity or greater)
                            return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                            {
                                code = "36",
                                timestamp = min_blocked_state.timestamp,
                                lease_item_in_storage_id = lease_item_in_storage.id,
                                required_quantity = lease_item_on_protocol_exists.total_quantity - min_blocked_state.blocked_quantity
                            };
                            return return_obj;
                        }
                    }
                }
                else if (protocol.state.Equals(Protocol_State.Confirmed))
                {
                    if (
                        out_of_storage_related_object == null ||
                        out_of_storage_related_object.lease_item_out_of_storage_history_FK == null
                    )
                    {
                        throw new Exception("19");//not found
                    }

                    //current out of storage state (at protocol timestamp)
                    out_of_storage_reference = out_of_storage_related_object.lease_item_out_of_storage_history_FK.Where(liosh =>
                        liosh.timestamp.Equals(protocol_for_lambda.timestamp)
                    ).FirstOrDefault();
                    if (out_of_storage_reference == null)
                    {
                        throw new Exception("19");//not found
                    }

                    Stock_State_Validator.Validate_Out_Of_Storage_State(out_of_storage_reference, in_storage_reference_stock_state.out_of_storage_quantity);

                    if(reference_stock_state.out_of_storage_quantity < lease_item_on_protocol_exists.total_quantity)
                    {
                        //miscalcullation (at timestamp of protocol out_of_storage_quantity should be equal to or greater than item on protocol quantity)
                        return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "36",
                            timestamp = reference_stock_state.timestamp,
                            lease_item_in_storage_id = lease_item_in_storage.id,
                            required_quantity = lease_item_on_protocol_exists.total_quantity - reference_stock_state.out_of_storage_quantity
                        };
                        return return_obj;
                    }

                    if(in_storage_reference_stock_state.out_of_storage_quantity < lease_item_on_protocol_exists.total_quantity)
                    {
                        //miscalcullation (at timestamp of protocol out_of_storage_quantity should be equal to or greater than item on protocol quantity)
                        return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "36",
                            timestamp = in_storage_reference_stock_state.timestamp,
                            lease_item_in_storage_id = lease_item_in_storage.id,
                            required_quantity = lease_item_on_protocol_exists.total_quantity - in_storage_reference_stock_state.out_of_storage_quantity
                        };
                        return return_obj;
                    }

                    if(out_of_storage_reference.total_quantity < lease_item_on_protocol_exists.total_quantity)
                    {
                        //miscalcullation (at timestamp of protocol out_of_storage_quantity should be equal to or greater than item on protocol quantity)
                        return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "36",
                            timestamp = out_of_storage_reference.timestamp,
                            lease_item_in_storage_id = lease_item_in_storage.id,
                            required_quantity = lease_item_on_protocol_exists.total_quantity - out_of_storage_reference.total_quantity
                        };
                        return return_obj;
                    }


                    out_of_storage_history_after_timestamp = out_of_storage_related_object.lease_item_out_of_storage_history_FK.Where(loh => loh.timestamp > protocol_for_lambda.timestamp).ToList();

                    var min_out_of_storage_reference = out_of_storage_history_after_timestamp.MinBy(loh => loh.total_quantity);

                    if (min_out_of_storage_reference != null)
                    {
                        if (min_out_of_storage_reference.total_quantity < input_total_quantity)
                        {
                            // not enough items out of storage
                            return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                            {
                                code = "43",
                                timestamp = min_out_of_storage_reference.timestamp,
                                lease_item_in_storage_id = lease_item_in_storage.id,
                                required_quantity = input_total_quantity - min_out_of_storage_reference.total_quantity
                            };
                            return return_obj;
                        }
                    }

                }                

            }


            if (protocol.type.Equals(Lease_Protocol_Type.Return))
            {
                if (
                    out_of_storage_related_object == null ||
                    out_of_storage_related_object.lease_item_out_of_storage_history_FK == null
                )
                {
                    throw new Exception("19");//not found
                }

                //current out of storage state (at protocol timestamp)
                out_of_storage_reference = out_of_storage_related_object.lease_item_out_of_storage_history_FK.Where(liosh =>
                    liosh.timestamp.Equals(protocol_for_lambda.timestamp)
                ).FirstOrDefault();
                if (out_of_storage_reference == null)
                {
                    throw new Exception("19");//not found
                }

                Stock_State_Validator.Validate_Out_Of_Storage_State(out_of_storage_reference, in_storage_reference_stock_state.out_of_storage_quantity);


                if (protocol.state.Equals(Protocol_State.Draft))
                {
                    if (reference_stock_state.out_of_storage_quantity < lease_item_on_protocol_exists.total_quantity - input_total_quantity)
                    {
                        // not enough out of storage
                        return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "43",
                            timestamp = reference_stock_state.timestamp,
                            lease_item_in_storage_id = lease_item_in_storage.id,
                            required_quantity = lease_item_on_protocol_exists.total_quantity - input_total_quantity - reference_stock_state.out_of_storage_quantity
                        };
                        return return_obj;
                    }

                    if (in_storage_reference_stock_state.out_of_storage_quantity < lease_item_on_protocol_exists.total_quantity - input_total_quantity)
                    {
                        // not enough out of storage
                        return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "43",
                            timestamp = in_storage_reference_stock_state.timestamp,
                            lease_item_in_storage_id = lease_item_in_storage.id,
                            required_quantity = lease_item_on_protocol_exists.total_quantity - input_total_quantity - in_storage_reference_stock_state.out_of_storage_quantity
                        };
                        return return_obj;
                    }

                    if (out_of_storage_reference.total_quantity < lease_item_on_protocol_exists.total_quantity - input_total_quantity)
                    {
                        // not enough out of storage
                        return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "43",
                            timestamp = out_of_storage_reference.timestamp,
                            lease_item_in_storage_id = lease_item_in_storage.id,
                            required_quantity = lease_item_on_protocol_exists.total_quantity - input_total_quantity - out_of_storage_reference.total_quantity
                        };
                        return return_obj;
                    }

                    out_of_storage_history_after_timestamp = out_of_storage_related_object.lease_item_out_of_storage_history_FK.Where(loh => loh.timestamp > protocol_for_lambda.timestamp).ToList();

                    var min_out_of_storage_reference = out_of_storage_history_after_timestamp.MinBy(loh => loh.total_quantity);

                    if (min_out_of_storage_reference != null)
                    {
                        if (min_out_of_storage_reference.total_quantity < lease_item_on_protocol_exists.total_quantity - input_total_quantity)
                        {
                            // not enough out of storage
                            return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                            {
                                code = "43",
                                timestamp = min_out_of_storage_reference.timestamp,
                                lease_item_in_storage_id = lease_item_in_storage.id,
                                required_quantity = lease_item_on_protocol_exists.total_quantity - input_total_quantity - min_out_of_storage_reference.total_quantity
                            };
                            return return_obj;
                        }
                    }

                }
                else if(protocol.state.Equals(Protocol_State.Confirmed))
                {
                    if (reference_stock_state.in_storage_quantity - reference_stock_state.blocked_quantity < lease_item_on_protocol_exists.total_quantity)
                    {
                        //miscallculation (at protocol timestamp free quantity should be equal to or greater than item on protocol quantity)
                        return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "36",
                            timestamp = reference_stock_state.timestamp,
                            lease_item_in_storage_id = lease_item_in_storage.id,
                            required_quantity = lease_item_on_protocol_exists.total_quantity - (reference_stock_state.in_storage_quantity - reference_stock_state.blocked_quantity)
                        };
                        return return_obj;
                    }

                    if (in_storage_reference_stock_state.in_storage_quantity - in_storage_reference_stock_state.blocked_quantity < lease_item_on_protocol_exists.total_quantity)
                    {
                        //miscallculation (at protocol timestamp free quantity should be equal to or greater than item on protocol quantity)
                        return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                        {
                            code = "36",
                            timestamp = in_storage_reference_stock_state.timestamp,
                            lease_item_in_storage_id = lease_item_in_storage.id,
                            required_quantity = lease_item_on_protocol_exists.total_quantity - (in_storage_reference_stock_state.in_storage_quantity - in_storage_reference_stock_state.blocked_quantity)
                        };
                        return return_obj;
                    }

                    var min_stock_state = in_storage_stock_history_after_timestamp.MinBy(ishat => ishat.in_storage_quantity - ishat.blocked_quantity);

                    if (min_stock_state != null)
                    {
                        if (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity < input_total_quantity)
                        {
                            // not enough in storage
                            return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                            {
                                code = "22",
                                timestamp = min_stock_state.timestamp,
                                lease_item_in_storage_id = lease_item_in_storage.id,
                                required_quantity = input_total_quantity - (min_stock_state.in_storage_quantity - min_stock_state.blocked_quantity)
                            };
                            return return_obj;
                        }
                    }

                }

                
            }


            //for drafts only operation is subtracting item on protocol quantity and total weight so there is no need for separate condition

            if (protocol.state.Equals(Protocol_State.Reserved))
            {
                if (protocol.type.Equals(Lease_Protocol_Type.Release))
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
            }

            if (protocol.state.Equals(Protocol_State.Confirmed))
            {
                if (protocol.type.Equals(Lease_Protocol_Type.Return))
                {
                    //subtracting returned items is moving them from theirs storage to construction site again
                    reference_stock_state.out_of_storage_quantity += input_total_quantity;
                    reference_stock_state.in_storage_quantity -= input_total_quantity;

                    in_storage_reference_stock_state.out_of_storage_quantity += input_total_quantity;
                    in_storage_reference_stock_state.in_storage_quantity -= input_total_quantity;

                    out_of_storage_reference.total_quantity += input_total_quantity;
                    

                    foreach (var next_state in stock_history_after_timestamp)
                    {
                        next_state.out_of_storage_quantity += input_total_quantity;
                        next_state.in_storage_quantity -= input_total_quantity;
                    }

                    foreach (var in_storage_next_state in in_storage_stock_history_after_timestamp)
                    {
                        in_storage_next_state.out_of_storage_quantity += input_total_quantity;
                        in_storage_next_state.in_storage_quantity -= input_total_quantity;
                    }

                    foreach (var out_of_storage_next_state in out_of_storage_history_after_timestamp)
                    {
                        out_of_storage_next_state.total_quantity += input_total_quantity;
                    }
                }

                if (protocol.type.Equals(Lease_Protocol_Type.Release))
                {
                    reference_stock_state.in_storage_quantity += input_total_quantity;
                    reference_stock_state.out_of_storage_quantity -= input_total_quantity;

                    in_storage_reference_stock_state.in_storage_quantity += input_total_quantity;
                    in_storage_reference_stock_state.out_of_storage_quantity -= input_total_quantity;

                    out_of_storage_reference.total_quantity -= input_total_quantity;
                                        

                    foreach (var out_of_storage_next_state in out_of_storage_history_after_timestamp)
                    {
                        out_of_storage_next_state.total_quantity -= input_total_quantity;
                    }

                    foreach (var next_state in stock_history_after_timestamp)
                    {
                        next_state.out_of_storage_quantity -= input_total_quantity;
                        next_state.in_storage_quantity += input_total_quantity;
                    }

                    foreach (var in_storage_next_state in in_storage_stock_history_after_timestamp)
                    {
                        in_storage_next_state.out_of_storage_quantity -= input_total_quantity;
                        in_storage_next_state.in_storage_quantity += input_total_quantity;
                    }
                }
            }


            lease_item_on_protocol_exists.total_quantity -= input_total_quantity;
            lease_item_on_protocol_exists.total_weight_kg -= input_total_quantity * lease_item_in_storage.lease_item_FK.weight_kg;
            lease_item_on_protocol_exists.total_area_m2 -= input_total_quantity * lease_item_in_storage.lease_item_FK.area_m2;
            lease_item_on_protocol_exists.total_worth -= input_total_quantity * lease_item_in_storage.lease_item_FK.price;

            protocol.total_weight_kg -= input_total_quantity * lease_item_in_storage.lease_item_FK.weight_kg;
            protocol.total_area_m2 -= input_total_quantity * lease_item_in_storage.lease_item_FK.area_m2;
            protocol.total_worth -= input_total_quantity * lease_item_in_storage.lease_item_FK.price;


            if (lease_item_on_protocol_exists.total_quantity == 0)
            {
                var stock_state_before_current = stock_history.Where(sh => sh.timestamp < protocol_for_lambda.timestamp).MaxBy(sh => sh.timestamp);
                var in_storage_stock_state_before_current = in_storage_stock_history.Where(ish => ish.timestamp < protocol_for_lambda.timestamp).MaxBy(ish => ish.timestamp);

                if(stock_state_before_current == null || in_storage_stock_state_before_current == null)
                {
                    throw new Exception("19");//not found (should be at least one previus record in history (state after adding item to storage))
                }

                if(out_of_storage_reference != null)
                {
                    var out_of_storage_before_current = out_of_storage_related_object.lease_item_out_of_storage_history_FK.Where(osh => osh.timestamp < protocol_for_lambda.timestamp).MaxBy(osh => osh.timestamp);
                    
                    if(out_of_storage_before_current != null)
                    {
                        if (out_of_storage_reference.total_quantity.Equals(out_of_storage_before_current.total_quantity))
                        {
                            _context.Lease_Item_Out_Of_Storage_History.Remove(out_of_storage_reference);
                        }
                        else
                        {
                            //miscallculation (previous history record should differ from current only in item_on_protocol quantity, so if quantity = 0 there should be no difference)
                            return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                            {
                                code = "36",
                                timestamp = out_of_storage_reference.timestamp,
                                lease_item_in_storage_id = lease_item_in_storage.id,
                                required_quantity = 0
                            };
                            return return_obj;
                        }
                    }
                    else
                    {
                        //if current record is the only one in out_of_storage history
                        if(out_of_storage_related_object.lease_item_out_of_storage_history_FK.Count.Equals(1))
                        {
                            _context.Lease_Item_Out_Of_Storage_History.Remove(out_of_storage_reference);
                            _context.Lease_Item_Out_Of_Storage.Remove(out_of_storage_related_object);
                        }
                        else //if current record is before the rest of history records
                        {
                            _context.Lease_Item_Out_Of_Storage_History.Remove(out_of_storage_reference);
                        }
                    }
                }

                if (
                    stock_state_before_current.total_quantity.Equals(reference_stock_state.total_quantity) &&
                    stock_state_before_current.in_storage_quantity.Equals(reference_stock_state.in_storage_quantity) &&
                    stock_state_before_current.blocked_quantity.Equals(reference_stock_state.blocked_quantity) &&
                    stock_state_before_current.out_of_storage_quantity.Equals(reference_stock_state.out_of_storage_quantity)
                )
                {
                    _context.Lease_Item_Stock_History.Remove(reference_stock_state);
                }
                else
                {
                    //miscallculation (previous history record should differ from current only in item_on_protocol quantity, so if quantity = 0 there should be no difference)
                    return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                    {
                        code = "36",
                        timestamp = reference_stock_state.timestamp,
                        lease_item_in_storage_id = lease_item_in_storage.id,
                        required_quantity = 0
                    };
                    return return_obj;
                }

                if (
                    in_storage_stock_state_before_current.total_quantity.Equals(in_storage_reference_stock_state.total_quantity) &&
                    in_storage_stock_state_before_current.in_storage_quantity.Equals(in_storage_reference_stock_state.in_storage_quantity) &&
                    in_storage_stock_state_before_current.blocked_quantity.Equals(in_storage_reference_stock_state.blocked_quantity) &&
                    in_storage_stock_state_before_current.out_of_storage_quantity.Equals(in_storage_reference_stock_state.out_of_storage_quantity)
                )
                {
                    _context.Lease_Item_In_Storage_Stock_History.Remove(in_storage_reference_stock_state);
                }
                else
                {
                    //miscallculation (previous history record should differ from current only in item_on_protocol quantity, so if quantity = 0 there should be no difference)
                    return_obj.error_object = new Lease_Item_On_Protocol_Error_Model
                    {
                        code = "36",
                        timestamp = in_storage_reference_stock_state.timestamp,
                        lease_item_in_storage_id = lease_item_in_storage.id,
                        required_quantity = 0
                    };
                    return return_obj;
                }

                _context.Lease_Item_On_Protocol.Remove(lease_item_on_protocol_exists);
                return_obj.id = -1;

            }

            _context.SaveChanges();

            return return_obj;
        }


        /* 
         * Edit_Lease_Item_On_Protocol method
         * This method is used to edit a record in the lease_item_on_receipt table.
         * 
         * It accepts Edit_Lease_Item_On_Protocol_Data object as input.
         * It then changes values of a record with those given in the input object and modifies its quantity values according to the protocol state.
         */
        public async Task<Lease_Item_On_Protocol_Id_Model> Edit_Lease_Item_On_Protocol(Edit_Lease_Item_On_Protocol_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var lease_item_on_protocol_exists = _context.Lease_Item_On_Protocol.Where(lp => 
                    lp.id.Equals(input_obj.id)
                )
                .Include(lp => lp.lease_item_in_storage_FK)
                    .ThenInclude(ls => ls.lease_item_FK)
                .Include(lp => lp.lease_protocol_FK)
                .FirstOrDefault();

                if(
                    lease_item_on_protocol_exists == null ||
                    lease_item_on_protocol_exists.lease_protocol_FK == null ||
                    lease_item_on_protocol_exists.lease_item_in_storage_FK == null ||
                    lease_item_on_protocol_exists.lease_item_in_storage_FK.lease_item_FK == null ||
                    lease_item_on_protocol_exists.lease_protocol_FK.lease_to_sale_protocol_FKid != null
                )
                {
                    throw new Exception("19");//not found
                }

                if(
                    lease_item_on_protocol_exists.lease_protocol_FK.type.Equals(Lease_Protocol_Type.Return) && 
                    (
                        lease_item_on_protocol_exists.lease_protocol_FK.state.Equals(Protocol_State.Reserved) ||
                        lease_item_on_protocol_exists.lease_protocol_FK.state.Equals(Protocol_State.Offer)
                    )
                )
                {
                    throw new Exception("34");//return protocol can not be in reservation or offer state
                }

                if (lease_item_on_protocol_exists.lease_protocol_FK.state.Equals(Protocol_State.Offer))
                {
                    throw new Exception("19");//not found protocol which isn't in offer state
                }


                Lease_Protocol protocol = lease_item_on_protocol_exists.lease_protocol_FK;
                Lease_Item_In_Storage lease_item_in_storage = lease_item_on_protocol_exists.lease_item_in_storage_FK;

                var comment = await Crypto.Encrypt(session, input_obj.comment);
                if (comment == null)
                {
                    throw new Exception("2");//encryption error
                }

                lease_item_on_protocol_exists.comment = comment;

                if (input_obj.total_quantity > lease_item_on_protocol_exists.total_quantity)
                {
                    decimal quantity_to_add = input_obj.total_quantity - lease_item_on_protocol_exists.total_quantity;
                                        
                    return Add_Lease_Item_On_Protocol(session, ref protocol, ref lease_item_in_storage, ref lease_item_on_protocol_exists, quantity_to_add);
                }

                if(input_obj.total_quantity < lease_item_on_protocol_exists.total_quantity)
                {
                    decimal quantity_to_remove = lease_item_on_protocol_exists.total_quantity - input_obj.total_quantity;

                    return Remove_Lease_Item_On_Protocol(session, ref protocol, ref lease_item_in_storage, ref lease_item_on_protocol_exists, quantity_to_remove);
                }

                return new Lease_Item_On_Protocol_Id_Model { id = lease_item_on_protocol_exists.id };
            }

        }



        /*
         * Get_Available_Lease_Items_To_Return method
         * This method produces list of available items to be returned to the storage and returns it.
         * 
         * It accepts Get_Available_Lease_Item_To_Return_Data object as input.
         * Then it collects information about previousely released and returned items and returns available items in the list.
         */
        public List<Lease_Item_On_Protocol_Return_Available_Model> Get_Available_Lease_Items_To_Return(Get_Available_Lease_Items_To_Return_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var protocol = _context.Lease_Protocol.Where(lp => 
                    lp.id.Equals(input_obj.protocol_id) &&
                    lp.type.Equals(Lease_Protocol_Type.Return) &&
                    lp.deleted.Equals(false) &&
                    lp.lease_to_sale_protocol_FKid == null
                )
                .Include(lp => lp.lease_item_on_protocol_list_FK)
                .FirstOrDefault();
                if (protocol == null || protocol.lease_item_on_protocol_list_FK == null)
                {
                    throw new Exception("19");// protocol not found
                }

                List<Lease_Item_On_Protocol_Return_Available_Model> return_obj = new List<Lease_Item_On_Protocol_Return_Available_Model>();

                var out_of_storage_lease_item_objects = _context.Lease_Item_Out_Of_Storage.Where(los =>
                    los.order_FKid.Equals(protocol.order_FKid)
                )
                .Include(los => los.lease_item_in_storage_FK)
                    .ThenInclude(lis => lis.lease_item_FK)
                            .ThenInclude(li => li.counting_unit_FK)
                .Include(los => los.lease_item_in_storage_FK)
                    .ThenInclude(lis => lis.storage_FK)
                .ToList();

                if(out_of_storage_lease_item_objects.Count == 0)
                {
                    return return_obj;
                }


                decimal out_of_storage_quantity = 0;
                Lease_Item_On_Protocol? item_on_protocol;

                Lease_Item_Out_Of_Storage_History? out_of_storage_state;

                foreach (Lease_Item_Out_Of_Storage lease_item_out_of_storage in out_of_storage_lease_item_objects)
                {
                    if(
                        lease_item_out_of_storage.lease_item_in_storage_FK == null || 
                        lease_item_out_of_storage.lease_item_in_storage_FK.lease_item_FK == null || 
                        lease_item_out_of_storage.lease_item_in_storage_FK.lease_item_FK.counting_unit_FK == null ||
                        lease_item_out_of_storage.lease_item_in_storage_FK.storage_FK == null
                    )
                    {
                        throw new Exception("19");//not found
                    }

                    out_of_storage_state = _context.Lease_Item_Out_Of_Storage_History.Where(osh =>
                        osh.lease_item_out_of_storage_FKid.Equals(lease_item_out_of_storage.id) &&
                        osh.timestamp <= protocol.timestamp
                    ).MaxBy(osh => osh.timestamp);
                    if( out_of_storage_state == null)
                    {
                        continue;
                    }

                    out_of_storage_quantity = out_of_storage_state.total_quantity;

                    if (protocol.state.Equals(Protocol_State.Draft))
                    {
                        item_on_protocol = protocol.lease_item_on_protocol_list_FK.Where(li => li.lease_item_in_storage_FKid.Equals(lease_item_out_of_storage.lease_item_in_storage_FKid)).FirstOrDefault();
                        if (item_on_protocol != null)
                        {
                            out_of_storage_quantity -= item_on_protocol.total_quantity;

                            if(out_of_storage_quantity <= 0)
                            {
                                continue;
                            }
                        }
                    }

                    var lease_item_in_return_obj_exists = return_obj.Where(ro => ro.lease_item_id.Equals(lease_item_out_of_storage.lease_item_in_storage_FK.lease_item_FKid)).FirstOrDefault();

                                        
                    if(lease_item_in_return_obj_exists != null)
                    {
                        lease_item_in_return_obj_exists.from_storage_list.Add
                        (
                            new Lease_Item_On_Protocol_Return_From_Storage_Model 
                            { 
                                lease_item_in_storage_id = lease_item_out_of_storage.lease_item_in_storage_FKid,
                                storage_id = lease_item_out_of_storage.lease_item_in_storage_FK.storage_FKid,
                                storage_name = lease_item_out_of_storage.lease_item_in_storage_FK.storage_FK.name,
                                storage_number = lease_item_out_of_storage.lease_item_in_storage_FK.storage_FK.number,
                                counting_unit = lease_item_out_of_storage.lease_item_in_storage_FK.lease_item_FK.counting_unit_FK.unit,
                                total_quantity = out_of_storage_quantity,
                                total_weight_kg = out_of_storage_quantity * lease_item_out_of_storage.lease_item_in_storage_FK.lease_item_FK.weight_kg
                            }
                        );

                        lease_item_in_return_obj_exists.total_quantity += out_of_storage_quantity;
                        lease_item_in_return_obj_exists.total_weight_kg += out_of_storage_quantity * lease_item_out_of_storage.lease_item_in_storage_FK.lease_item_FK.weight_kg;
                    }
                    else
                    {
                        return_obj.Add
                        (
                            new Lease_Item_On_Protocol_Return_Available_Model
                            {
                                lease_item_id = lease_item_out_of_storage.lease_item_in_storage_FK.lease_item_FKid,
                                catalog_number = lease_item_out_of_storage.lease_item_in_storage_FK.lease_item_FK.catalog_number,
                                product_name = lease_item_out_of_storage.lease_item_in_storage_FK.lease_item_FK.product_name,
                                counting_unit = lease_item_out_of_storage.lease_item_in_storage_FK.lease_item_FK.counting_unit_FK.unit,
                                total_quantity = out_of_storage_quantity,
                                total_weight_kg = out_of_storage_quantity * lease_item_out_of_storage.lease_item_in_storage_FK.lease_item_FK.weight_kg,
                                from_storage_list = 
                                [
                                    new Lease_Item_On_Protocol_Return_From_Storage_Model 
                                    {
                                        lease_item_in_storage_id = lease_item_out_of_storage.lease_item_in_storage_FKid,
                                        storage_id = lease_item_out_of_storage.lease_item_in_storage_FK.storage_FKid,
                                        storage_name = lease_item_out_of_storage.lease_item_in_storage_FK.storage_FK.name,
                                        counting_unit = lease_item_out_of_storage.lease_item_in_storage_FK.lease_item_FK.counting_unit_FK.unit,
                                        storage_number = lease_item_out_of_storage.lease_item_in_storage_FK.storage_FK.number,
                                        total_quantity = out_of_storage_quantity,
                                        total_weight_kg = out_of_storage_quantity * lease_item_out_of_storage.lease_item_in_storage_FK.lease_item_FK.weight_kg
                                    }
                                ]
                            }
                        );

                    }

                }


                return return_obj;
            }

        }

        public List<Lease_Item_On_Protocol_Offer_Model> Get_Available_Lease_Items_To_Offer(Get_Available_Lease_Items_To_Release_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var protocol = _context.Lease_Protocol.Where(lp =>
                    lp.id.Equals(input_obj.protocol_id) &&
                    lp.type.Equals(Lease_Protocol_Type.Release) &&
                    lp.state.Equals(Protocol_State.Offer) &&
                    lp.deleted.Equals(false)
                )
                .FirstOrDefault();
                if (protocol == null)
                {
                    throw new Exception("19");// protocol not found
                }

                var available_lease_item_list = _context.Lease_Item
                    .Include(li => li.lease_item_in_storage_list_FK)
                        .ThenInclude(lis => lis.storage_FK)
                    .Include(li => li.counting_unit_FK)
                    .ToList();

                List<Lease_Item_On_Protocol_Offer_Model> return_obj = new List<Lease_Item_On_Protocol_Offer_Model>();

                if (available_lease_item_list.Count.Equals(0))
                {
                    return return_obj;
                }

                Lease_Item_On_Protocol_Offer_Model? lease_item_in_return_obj_exists;


                foreach (var lease_item in available_lease_item_list)
                {
                    if (
                        lease_item == null ||
                        lease_item.counting_unit_FK == null ||
                        lease_item.lease_item_in_storage_list_FK == null
                    )
                    {
                        throw new Exception("19");//not found
                    }


                    foreach (var lease_item_in_storage in lease_item.lease_item_in_storage_list_FK)
                    {
                        if (
                            lease_item_in_storage == null ||
                            lease_item_in_storage.storage_FK == null
                        )
                        {
                            throw new Exception("19");//not found
                        }


                        lease_item_in_return_obj_exists = return_obj.Where(ro => ro.lease_item_id.Equals(lease_item.id)).FirstOrDefault();


                        if (lease_item_in_return_obj_exists == null)
                        {
                            return_obj.Add(
                                new Lease_Item_On_Protocol_Offer_Model
                                {
                                    lease_item_id = lease_item.id,
                                    catalog_number = lease_item.catalog_number,
                                    product_name = lease_item.product_name,
                                    counting_unit = lease_item.counting_unit_FK.unit,
                                    in_storage_list =
                                    [
                                        new Lease_Item_On_Protocol_Offer_From_Storage_Model
                                        {
                                            storage_id = lease_item_in_storage.storage_FKid,
                                            storage_number = lease_item_in_storage.storage_FK.number,
                                            storage_name = lease_item_in_storage.storage_FK.name,
                                            lease_item_in_storage_id = lease_item_in_storage.id,
                                            counting_unit = lease_item.counting_unit_FK.unit
                                        }
                                    ]
                                }
                            );

                        }
                        else
                        {
                            lease_item_in_return_obj_exists.in_storage_list.Add(
                                new Lease_Item_On_Protocol_Offer_From_Storage_Model
                                {
                                    storage_id = lease_item_in_storage.storage_FKid,
                                    storage_number = lease_item_in_storage.storage_FK.number,
                                    storage_name = lease_item_in_storage.storage_FK.name,
                                    lease_item_in_storage_id = lease_item_in_storage.id,
                                    counting_unit = lease_item.counting_unit_FK.unit
                                }
                            );
                        }


                    }

                }


                return return_obj;
            }
        }

        public List<Lease_Item_On_Protocol_Release_Available_Model> Get_Available_Lease_Items_To_Release(Get_Available_Lease_Items_To_Release_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var protocol = _context.Lease_Protocol.Where(lp =>
                    lp.id.Equals(input_obj.protocol_id) &&
                    lp.type.Equals(Lease_Protocol_Type.Release) &&
                    lp.deleted.Equals(false)
                )
                .Include(lp => lp.lease_item_on_protocol_list_FK)
                .FirstOrDefault();
                if (protocol == null || protocol.lease_item_on_protocol_list_FK == null || protocol.state.Equals(Protocol_State.Offer))
                {
                    throw new Exception("19");// protocol not found
                }

                var available_lease_item_list = _context.Lease_Item
                    .Include(li => li.lease_item_in_storage_list_FK)
                        .ThenInclude(lis => lis.storage_FK)
                    .Include(li => li.counting_unit_FK)
                    .ToList();

                List<Lease_Item_On_Protocol_Release_Available_Model> return_obj = new List<Lease_Item_On_Protocol_Release_Available_Model>();

                if (available_lease_item_list.Count.Equals(0))
                {
                    return return_obj;
                }

                Lease_Item_On_Protocol_Release_Available_Model? lease_item_in_return_obj_exists;


                Lease_Item_Stock_History? stock_state;
                Lease_Item_In_Storage_Stock_History? in_storage_stock_state;

                Lease_Item_On_Protocol? item_on_protocol;

                decimal free_quantity = -1;

                foreach(var lease_item in available_lease_item_list)
                {
                    if(
                        lease_item == null || 
                        lease_item.counting_unit_FK == null || 
                        lease_item.lease_item_in_storage_list_FK == null
                    )
                    {
                        throw new Exception("19");//not found
                    }

                    stock_state = _context.Lease_Item_Stock_History.Where(lsh => 
                        lsh.lease_item_FKid.Equals(lease_item.id) &&
                        lsh.timestamp <= protocol.timestamp
                    ).MaxBy(lsh => lsh.timestamp);

                    if(stock_state == null)
                    {
                        continue;
                    }

                    if((stock_state.in_storage_quantity - stock_state.blocked_quantity).Equals(0))
                    {
                        continue;
                    }

                    foreach(var lease_item_in_storage in lease_item.lease_item_in_storage_list_FK)
                    {
                        if(
                            lease_item_in_storage == null ||
                            lease_item_in_storage.storage_FK == null
                        )
                        {
                            throw new Exception("19");//not found
                        }

                        in_storage_stock_state = _context.Lease_Item_In_Storage_Stock_History.Where(ilsh => 
                            ilsh.lease_item_in_storage_FKid.Equals(lease_item_in_storage.id) &&
                            ilsh.timestamp <= protocol.timestamp
                        ).MaxBy(ilsh => ilsh.timestamp);

                        if(in_storage_stock_state == null) 
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
                            item_on_protocol = protocol.lease_item_on_protocol_list_FK.Where(lop => lop.lease_item_in_storage_FKid.Equals(lease_item_in_storage.id)).FirstOrDefault();
                            
                            if(item_on_protocol != null)
                            {
                                free_quantity -= item_on_protocol.total_quantity;

                                if (free_quantity <= 0)
                                {
                                    continue;
                                }
                            }
                        }


                        lease_item_in_return_obj_exists = return_obj.Where(ro => ro.lease_item_id.Equals(lease_item.id)).FirstOrDefault();


                        if(lease_item_in_return_obj_exists == null)
                        {
                            return_obj.Add(
                                new Lease_Item_On_Protocol_Release_Available_Model
                                {
                                    lease_item_id = lease_item.id,
                                    catalog_number = lease_item.catalog_number,
                                    product_name = lease_item.product_name,
                                    counting_unit = lease_item.counting_unit_FK.unit,
                                    total_quantity = free_quantity,
                                    in_storage_list = 
                                    [
                                        new Lease_Item_On_Protocol_Release_From_Storage_Model 
                                        {
                                            storage_id = lease_item_in_storage.storage_FKid,
                                            storage_number = lease_item_in_storage.storage_FK.number,
                                            storage_name = lease_item_in_storage.storage_FK.name,
                                            lease_item_in_storage_id = lease_item_in_storage.id,
                                            counting_unit = lease_item.counting_unit_FK.unit,
                                            total_quantity = free_quantity
                                        }
                                    ]
                                }
                            );

                        }
                        else
                        {
                            lease_item_in_return_obj_exists.in_storage_list.Add(
                                new Lease_Item_On_Protocol_Release_From_Storage_Model
                                {
                                    storage_id = lease_item_in_storage.storage_FKid,
                                    storage_number = lease_item_in_storage.storage_FK.number,
                                    storage_name = lease_item_in_storage.storage_FK.name,
                                    lease_item_in_storage_id = lease_item_in_storage.id,
                                    counting_unit = lease_item.counting_unit_FK.unit,
                                    total_quantity = free_quantity
                                }
                            );

                            lease_item_in_return_obj_exists.total_quantity += free_quantity;
                        }


                    }

                }


                return return_obj;
            }
        }

        /*
         * Get_Lease_Item_On_Protocol_List method
         * This method gets a list of items that are on protocol (protocol id given in input) and returns it.
         * 
         * It accepts Get_Lease_Item_From_Protocol_Data object as input.
         * Then it gets a list of records that are associated with specified protocol
         */
        public async Task<List<Lease_Item_On_Protocol_Model>> Get_Lease_Item_On_Protocol_List(Get_Lease_Item_From_Protocol_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var protocol = _context.Lease_Protocol.Where(lp => 
                    lp.id.Equals(input_obj.protocol_id) && 
                    lp.deleted.Equals(false)
                )
                .Include(lp => lp.lease_item_on_protocol_list_FK)
                    .ThenInclude(lop => lop.lease_item_in_storage_FK)
                        .ThenInclude(lis => lis.lease_item_FK)
                            .ThenInclude(li => li.counting_unit_FK)
                .FirstOrDefault();
                if(
                    protocol == null ||
                    protocol.lease_item_on_protocol_list_FK == null
                )
                {
                    throw new Exception("19"); //not found
                }

                List<Lease_Item_On_Protocol_Model> return_obj = new List<Lease_Item_On_Protocol_Model>();

                if(protocol.lease_item_on_protocol_list_FK.Count == 0)
                {
                    return return_obj;
                }

                List<Encrypted_Object> items_on_protocol_comments = new List<Encrypted_Object>();

                foreach (var item_on_protocol in protocol.lease_item_on_protocol_list_FK)
                {
                    if(
                        item_on_protocol == null || 
                        item_on_protocol.lease_item_in_storage_FK == null || 
                        item_on_protocol.lease_item_in_storage_FK.lease_item_FK == null ||
                        item_on_protocol.lease_item_in_storage_FK.lease_item_FK.counting_unit_FK == null
                    )
                    {
                        throw new Exception("19");//not found
                    }

                    return_obj.Add(new Lease_Item_On_Protocol_Model
                    {
                        id = item_on_protocol.id,
                        catalog_number = item_on_protocol.lease_item_in_storage_FK.lease_item_FK.catalog_number,
                        product_name = item_on_protocol.lease_item_in_storage_FK.lease_item_FK.product_name,
                        counting_unit = item_on_protocol.lease_item_in_storage_FK.lease_item_FK.counting_unit_FK.unit,
                        lease_item_id = item_on_protocol.lease_item_in_storage_FK.lease_item_FKid,
                        lease_item_in_storage_id = item_on_protocol.lease_item_in_storage_FKid,
                        total_quantity = item_on_protocol.total_quantity,
                        weight_kg = item_on_protocol.lease_item_in_storage_FK.lease_item_FK.weight_kg,
                        total_weight_kg = item_on_protocol.total_weight_kg,
                        total_worth = item_on_protocol.total_worth,
                        total_area_m2 = item_on_protocol.total_area_m2
                    });

                    items_on_protocol_comments.Add(new Encrypted_Object { id = item_on_protocol.id, encryptedValue = item_on_protocol.comment });
                }


                List<Decrypted_Object> decrypted_items_on_protocol_comments = await Crypto.DecryptList(session, items_on_protocol_comments);

                if(
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
