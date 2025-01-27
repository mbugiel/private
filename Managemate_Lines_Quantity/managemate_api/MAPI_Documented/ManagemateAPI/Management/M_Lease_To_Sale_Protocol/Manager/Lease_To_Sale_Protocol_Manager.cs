using ManagemateAPI.Database.Context;
using ManagemateAPI.Database.Tables;
using ManagemateAPI.Encryption.Input_Objects;
using ManagemateAPI.Encryption;
using ManagemateAPI.Management.M_Lease_Item_On_Protocol.Table_Model;
using ManagemateAPI.Management.M_Lease_Protocol.Input_Objects;
using ManagemateAPI.Management.M_Lease_To_Sale_Protocol.Input_Objects;
using ManagemateAPI.Management.M_Lease_To_Sale_Protocol.Table_Model;
using ManagemateAPI.Management.M_Sale_Protocol.Manager;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.Shared.Enum;
using ManagemateAPI.Management.Shared.Validator;
using Microsoft.EntityFrameworkCore;
using ManagemateAPI.Management.M_Client.Table_Model;
using ManagemateAPI.Management.M_Construction_Site.Table_Model;
using ManagemateAPI.Management.M_Company.Table_Model;
using ManagemateAPI.Management.Shared.Static;
using PuppeteerSharp.Media;
using PuppeteerSharp;
using ManagemateAPI.Management.Shared.Json_Model;

namespace ManagemateAPI.Management.M_Lease_To_Sale_Protocol.Manager
{
    public class Lease_To_Sale_Protocol_Manager
    {

        private DB_Context _context;
        private readonly IConfiguration _configuration;


        public Lease_To_Sale_Protocol_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public Lease_To_Sale_Protocol_Id_Model Create_Lease_To_Sale_Protocol(Create_Lease_To_Sale_Protocol_Data input_obj, Session_Data session)
        {
            if(input_obj == null || session == null)
            {
                throw new Exception("14");//null error
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();


                var return_draft_protocol = _context.Lease_Protocol.Where(lp => 
                    lp.id.Equals(input_obj.return_lease_protocol_id) &&
                    lp.type.Equals(Lease_Protocol_Type.Return) && 
                    lp.state.Equals(Protocol_State.Draft) && 
                    lp.deleted.Equals(false) && 
                    lp.lease_item_on_protocol_list_FK.Count > 0 &&
                    lp.service_on_lease_protocol_list_FK.Count.Equals(0)
                )
                .Include(lp => lp.lease_item_on_protocol_list_FK)
                    .ThenInclude(li => li.lease_item_in_storage_FK)
                        .ThenInclude(lis => lis.lease_item_FK)
                .FirstOrDefault();

                if(return_draft_protocol == null || return_draft_protocol.lease_item_on_protocol_list_FK == null)
                {
                    throw new Exception("19");//not found
                }


                List<Lease_Item_On_Lease_To_Sale_Protocol> lease_to_sale_list = new List<Lease_Item_On_Lease_To_Sale_Protocol>();



                Lease_To_Sale_Protocol_Id_Model return_obj = new Lease_To_Sale_Protocol_Id_Model
                {
                    lease_to_sale_protocol_id = -1,
                    error_list = new List<Lease_Item_On_Protocol_Error_Model>()
                };


                List<Lease_Item_Stock_History> stock_history;
                List<Lease_Item_In_Storage_Stock_History> in_storage_stock_history;

                Lease_Item_Stock_History? current_stock_state;
                Lease_Item_In_Storage_Stock_History? in_storage_current_stock_state;
                Lease_Item_In_Storage_Stock_History? min_stock_state = null;

                Lease_Item_Out_Of_Storage? out_of_storage_object = null;
                Lease_Item_Out_Of_Storage_History? out_of_storage_state = null;
                Lease_Item_Out_Of_Storage_History? min_out_of_storage = null;


                List<Lease_Item_Out_Of_Storage_History> out_of_storage_history_operational;
                List<Lease_Item_Stock_History> stock_history_operational;
                List<Lease_Item_In_Storage_Stock_History> in_storage_stock_history_operational;


                foreach (var lease_item_on_protocol in return_draft_protocol.lease_item_on_protocol_list_FK)
                {

                    if (
                        lease_item_on_protocol == null ||
                        lease_item_on_protocol.lease_item_in_storage_FK == null ||
                        lease_item_on_protocol.lease_item_in_storage_FK.lease_item_FK == null
                    )
                    {
                        throw new Exception("19");//not found
                    }

                    stock_history = _context.Lease_Item_Stock_History.Where(lsh =>
                        lsh.lease_item_FKid.Equals(lease_item_on_protocol.lease_item_in_storage_FK.lease_item_FKid) &&
                        lsh.timestamp >= return_draft_protocol.timestamp
                    ).ToList();

                    in_storage_stock_history = _context.Lease_Item_In_Storage_Stock_History.Where(ilsh =>
                        ilsh.lease_item_in_storage_FKid.Equals(lease_item_on_protocol.lease_item_in_storage_FKid) &&
                        ilsh.timestamp >= return_draft_protocol.timestamp
                    ).ToList();



                    current_stock_state = stock_history.Where(lsh => lsh.timestamp.Equals(return_draft_protocol.timestamp)).FirstOrDefault();
                    if (current_stock_state == null)
                    {
                        throw new Exception("19");//not found
                    }

                    in_storage_current_stock_state = in_storage_stock_history.Where(ilsh => ilsh.timestamp.Equals(return_draft_protocol.timestamp)).FirstOrDefault();
                    if (in_storage_current_stock_state == null)
                    {
                        throw new Exception("19");//not found
                    }

                    Stock_State_Validator.Validate_Stock_State(current_stock_state);
                    Stock_State_Validator.Validate_Stock_State(in_storage_current_stock_state);

                    out_of_storage_object = _context.Lease_Item_Out_Of_Storage.Where(los =>
                        los.order_FKid.Equals(return_draft_protocol.order_FKid) &&
                        los.lease_item_in_storage_FKid.Equals(lease_item_on_protocol.lease_item_in_storage_FKid)
                    )
                    .Include(los => los.lease_item_out_of_storage_history_FK)
                    .FirstOrDefault();



                    if (out_of_storage_object == null)
                    {
                        // miscalcullation (if return protocol is in draft state it means that out_of_storage object (with at least one history record) should exist)
                        return_obj.error_list.Clear();
                        return_obj.error_list.Add(
                            new Lease_Item_On_Protocol_Error_Model
                            {
                                code = "36",
                                lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                timestamp = return_draft_protocol.timestamp,
                                required_quantity = 0
                            }
                        );

                        return return_obj;
                    }

                    if (
                        out_of_storage_object.lease_item_out_of_storage_history_FK == null ||
                        out_of_storage_object.lease_item_out_of_storage_history_FK.Count == 0
                    )
                    {
                        throw new Exception("19");//not found
                    }


                    out_of_storage_state = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp.Equals(return_draft_protocol.timestamp)).FirstOrDefault();

                    if (out_of_storage_state == null)
                    {
                        throw new Exception("19");//not found
                    }

                    Stock_State_Validator.Validate_Out_Of_Storage_State(out_of_storage_state, in_storage_current_stock_state.out_of_storage_quantity);




                    if (current_stock_state.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                    {
                        //not enough out of storage
                        return_obj.error_list.Add(
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
                        return_obj.error_list.Add(
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










                    if (out_of_storage_state.total_quantity < lease_item_on_protocol.total_quantity)
                    {
                        //not enough out of storage
                        return_obj.error_list.Add(
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
                    out_of_storage_history_operational = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp > return_draft_protocol.timestamp).ToList();


                    min_out_of_storage = out_of_storage_history_operational.MinBy(ous => ous.total_quantity);

                    if (min_out_of_storage != null)
                    {
                        if (min_out_of_storage.total_quantity < lease_item_on_protocol.total_quantity)
                        {
                            //not enough out of storage
                            return_obj.error_list.Add(
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
                    in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > return_draft_protocol.timestamp).ToList();


                    min_stock_state = in_storage_stock_history_operational.MinBy(sh => sh.out_of_storage_quantity);
                    if (min_stock_state != null)
                    {
                        if (min_stock_state.out_of_storage_quantity < lease_item_on_protocol.total_quantity)
                        {
                            // miscallculation (if out_of_storage_quantity is less than on_protocol quantity, then min_out_of_storage quantity (checked above) should also be less)
                            return_obj.error_list.Clear();
                            return_obj.error_list.Add(
                                new Lease_Item_On_Protocol_Error_Model
                                {
                                    code = "36",
                                    lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                    timestamp = min_stock_state.timestamp,
                                    required_quantity = 0
                                }
                            );

                            return return_obj;
                        }
                    }

                    //stock history after previous_timestamp
                    stock_history_operational = stock_history.Where(sh => sh.timestamp > return_draft_protocol.timestamp).ToList();


                    out_of_storage_state.total_quantity -= lease_item_on_protocol.total_quantity;

                    current_stock_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                    current_stock_state.total_quantity -= lease_item_on_protocol.total_quantity;
                    //current_stock_state.in_storage_quantity += lease_item_on_protocol.total_quantity;

                    in_storage_current_stock_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                    in_storage_current_stock_state.total_quantity -= lease_item_on_protocol.total_quantity;
                    //in_storage_current_stock_state.in_storage_quantity += lease_item_on_protocol.total_quantity;

                    foreach (var out_of_storage_next_state in out_of_storage_history_operational)
                    {
                        out_of_storage_next_state.total_quantity -= lease_item_on_protocol.total_quantity;
                    }

                    foreach (var next_state in stock_history_operational)
                    {
                        next_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                        next_state.total_quantity -= lease_item_on_protocol.total_quantity;
                        //next_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                    }

                    foreach (var in_storage_next_state in in_storage_stock_history_operational)
                    {
                        in_storage_next_state.out_of_storage_quantity -= lease_item_on_protocol.total_quantity;
                        in_storage_next_state.total_quantity -= lease_item_on_protocol.total_quantity;
                        //in_storage_next_state.in_storage_quantity += lease_item_on_protocol.total_quantity;
                    }

                    if (return_obj.error_list.Count.Equals(0))
                    {
                        var new_item_on_lease_to_sale = new Lease_Item_On_Lease_To_Sale_Protocol
                        {
                            lease_item_in_storage_FKid = lease_item_on_protocol.lease_item_in_storage_FKid,
                            total_quantity = lease_item_on_protocol.total_quantity,
                            total_weight_kg = lease_item_on_protocol.total_weight_kg,
                            total_area_m2 = lease_item_on_protocol.total_area_m2,
                            comment = lease_item_on_protocol.comment
                        };

                        if (input_obj.discount_is_in_pct)
                        {
                            if(input_obj.discount_value < 0 || input_obj.discount_value >= 100)
                            {
                                throw new Exception("36");//miscallculation
                            }

                            new_item_on_lease_to_sale.total_worth = lease_item_on_protocol.total_worth * ((100 - input_obj.discount_value) / 100);

                        }
                        else
                        {
                            if(input_obj.discount_value < 0 || input_obj.discount_value >= (lease_item_on_protocol.total_worth / lease_item_on_protocol.total_quantity))
                            {
                                throw new Exception("36");//miscallculation
                            }

                            new_item_on_lease_to_sale.total_worth = lease_item_on_protocol.total_worth - input_obj.discount_value;

                        }
                        

                        lease_to_sale_list.Add(new_item_on_lease_to_sale);

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

                Lease_To_Sale_Protocol new_lease_to_sale_protocol = new Lease_To_Sale_Protocol
                {
                    number = Sale_Protocol_Manager.Get_Latest_Sale_Protocol_Number(_context, return_draft_protocol.timestamp.Year) + 1,
                    year = return_draft_protocol.timestamp.Year,
                    prefix = settings.sale_release_protocol_prefix,
                    timestamp = return_draft_protocol.timestamp,
                    order_FKid = return_draft_protocol.order_FKid,
                    return_lease_protocol_FK = return_draft_protocol
                };
                new_lease_to_sale_protocol.full_number = new_lease_to_sale_protocol.prefix + "-" + new_lease_to_sale_protocol.number.ToString("D5") + "/" + new_lease_to_sale_protocol.year.ToString();

                _context.Lease_To_Sale_Protocol.Add(new_lease_to_sale_protocol);
                _context.SaveChanges();

                foreach(var lease_item_for_sale in lease_to_sale_list)
                {
                    lease_item_for_sale.lease_to_sale_protocol_FKid = new_lease_to_sale_protocol.id;
                }

                return_draft_protocol.state = Protocol_State.Confirmed;

                _context.Lease_Item_On_Lease_To_Sale_Protocol.AddRange(lease_to_sale_list);
                _context.SaveChanges();

                return_obj.lease_to_sale_protocol_id = new_lease_to_sale_protocol.id;


                return return_obj;

            }

        }



        public Lease_Protocol_Id_Model_After_Remove_Data Remove_Lease_To_Sale_Protocol(Remove_Lease_To_Sale_Protocol_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//null error
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();


                var lease_to_sale_protocol = _context.Lease_To_Sale_Protocol.Where(ltsp => 
                    ltsp.id.Equals(input_obj.lease_to_sale_protocol_id)
                )
                .Include(ltsp => ltsp.return_lease_protocol_FK)
                .Include(ltsp => ltsp.lease_item_on_lease_to_sale_protocol_list_FK)
                    .ThenInclude(ltsp => ltsp.lease_item_in_storage_FK)
                .Include(ltsp => ltsp.lease_to_sale_protocol_printed_data_FK)
                .FirstOrDefault();

                if (
                    lease_to_sale_protocol == null ||
                    lease_to_sale_protocol.return_lease_protocol_FK == null ||
                    lease_to_sale_protocol.lease_item_on_lease_to_sale_protocol_list_FK == null
                )
                {
                    throw new Exception("19");//not found
                }


                Lease_Protocol_Id_Model_After_Remove_Data return_obj = new Lease_Protocol_Id_Model_After_Remove_Data
                {
                    protocol_id = lease_to_sale_protocol.return_lease_protocol_FKid,
                    error_list = new List<Lease_Item_On_Protocol_Error_Model>()
                };


                List<Lease_Item_Stock_History> stock_history;
                List<Lease_Item_In_Storage_Stock_History> in_storage_stock_history;

                Lease_Item_Stock_History? current_stock_state;
                Lease_Item_In_Storage_Stock_History? in_storage_current_stock_state;

                Lease_Item_Out_Of_Storage? out_of_storage_object = null;
                Lease_Item_Out_Of_Storage_History? out_of_storage_state = null;


                List<Lease_Item_Out_Of_Storage_History> out_of_storage_history_operational;
                List<Lease_Item_Stock_History> stock_history_operational;
                List<Lease_Item_In_Storage_Stock_History> in_storage_stock_history_operational;


                foreach (var lease_item_on_protocol in lease_to_sale_protocol.lease_item_on_lease_to_sale_protocol_list_FK)
                {

                    if (
                        lease_item_on_protocol == null ||
                        lease_item_on_protocol.lease_item_in_storage_FK == null
                    )
                    {
                        throw new Exception("19");//not found
                    }

                    stock_history = _context.Lease_Item_Stock_History.Where(lsh =>
                        lsh.lease_item_FKid.Equals(lease_item_on_protocol.lease_item_in_storage_FK.lease_item_FKid) &&
                        lsh.timestamp >= lease_to_sale_protocol.timestamp
                    ).ToList();

                    in_storage_stock_history = _context.Lease_Item_In_Storage_Stock_History.Where(ilsh =>
                        ilsh.lease_item_in_storage_FKid.Equals(lease_item_on_protocol.lease_item_in_storage_FKid) &&
                        ilsh.timestamp >= lease_to_sale_protocol.timestamp
                    ).ToList();



                    current_stock_state = stock_history.Where(lsh => lsh.timestamp.Equals(lease_to_sale_protocol.timestamp)).FirstOrDefault();
                    if (current_stock_state == null)
                    {
                        throw new Exception("19");//not found
                    }

                    in_storage_current_stock_state = in_storage_stock_history.Where(ilsh => ilsh.timestamp.Equals(lease_to_sale_protocol.timestamp)).FirstOrDefault();
                    if (in_storage_current_stock_state == null)
                    {
                        throw new Exception("19");//not found
                    }

                    Stock_State_Validator.Validate_Stock_State(current_stock_state);
                    Stock_State_Validator.Validate_Stock_State(in_storage_current_stock_state);

                    out_of_storage_object = _context.Lease_Item_Out_Of_Storage.Where(los =>
                        los.order_FKid.Equals(lease_to_sale_protocol.order_FKid) &&
                        los.lease_item_in_storage_FKid.Equals(lease_item_on_protocol.lease_item_in_storage_FKid)
                    )
                    .Include(los => los.lease_item_out_of_storage_history_FK)
                    .FirstOrDefault();



                    if (out_of_storage_object == null)
                    {
                        // miscalcullation (if return protocol is in confirmed state it means that out_of_storage object (with at least one history record) should exist)
                        return_obj.error_list.Clear();
                        return_obj.error_list.Add(
                            new Lease_Item_On_Protocol_Error_Model
                            {
                                code = "36",
                                lease_item_in_storage_id = lease_item_on_protocol.lease_item_in_storage_FKid,
                                timestamp = lease_to_sale_protocol.timestamp,
                                required_quantity = 0
                            }
                        );

                        return return_obj;
                    }

                    if (
                        out_of_storage_object.lease_item_out_of_storage_history_FK == null ||
                        out_of_storage_object.lease_item_out_of_storage_history_FK.Count == 0
                    )
                    {
                        throw new Exception("19");//not found
                    }


                    out_of_storage_state = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp.Equals(lease_to_sale_protocol.timestamp)).FirstOrDefault();

                    if (out_of_storage_state == null)
                    {
                        throw new Exception("19");//not found
                    }

                    Stock_State_Validator.Validate_Out_Of_Storage_State(out_of_storage_state, in_storage_current_stock_state.out_of_storage_quantity);



                    //out of storage states after previous_timestamp
                    out_of_storage_history_operational = out_of_storage_object.lease_item_out_of_storage_history_FK.Where(losh => losh.timestamp > lease_to_sale_protocol.timestamp).ToList();

                    //in storage stock history after previous_timestamp
                    in_storage_stock_history_operational = in_storage_stock_history.Where(ish => ish.timestamp > lease_to_sale_protocol.timestamp).ToList();

                    //stock history after previous_timestamp
                    stock_history_operational = stock_history.Where(sh => sh.timestamp > lease_to_sale_protocol.timestamp).ToList();


                    out_of_storage_state.total_quantity += lease_item_on_protocol.total_quantity;

                    current_stock_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                    current_stock_state.total_quantity += lease_item_on_protocol.total_quantity;

                    in_storage_current_stock_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                    in_storage_current_stock_state.total_quantity += lease_item_on_protocol.total_quantity;

                    foreach (var out_of_storage_next_state in out_of_storage_history_operational)
                    {
                        out_of_storage_next_state.total_quantity += lease_item_on_protocol.total_quantity;
                    }

                    foreach (var next_state in stock_history_operational)
                    {
                        next_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                        next_state.total_quantity += lease_item_on_protocol.total_quantity;
                    }

                    foreach (var in_storage_next_state in in_storage_stock_history_operational)
                    {
                        in_storage_next_state.out_of_storage_quantity += lease_item_on_protocol.total_quantity;
                        in_storage_next_state.total_quantity += lease_item_on_protocol.total_quantity;
                    }

                }

                if (return_obj.error_list.Count > 0)
                {
                    return return_obj;
                }


                lease_to_sale_protocol.return_lease_protocol_FK.state = Protocol_State.Draft;

                _context.SaveChanges();

                if(lease_to_sale_protocol.lease_to_sale_protocol_printed_data_FK != null)
                {
                    _context.Lease_To_Sale_Protocol_Printed_Data.Remove(lease_to_sale_protocol.lease_to_sale_protocol_printed_data_FK);
                }

                _context.Lease_To_Sale_Protocol.Remove(lease_to_sale_protocol);

                _context.SaveChanges();


                return return_obj;

            }

        }





        public async Task<Lease_To_Sale_Protocol_Model> Get_Lease_Protocol_By_Id(Get_Lease_Protocol_By_Id_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var selected_protocol = _context.Lease_To_Sale_Protocol.Where(ltsp =>
                    ltsp.id.Equals(input_obj.id_to_get)
                )
                .Include(lp => lp.lease_item_on_lease_to_sale_protocol_list_FK)
                    .ThenInclude(i => i.lease_item_in_storage_FK)
                        .ThenInclude(li => li.lease_item_FK)
                            .ThenInclude(li => li.counting_unit_FK)
                .Include(ltsp => ltsp.return_lease_protocol_FK)
                .FirstOrDefault();

                if (
                    selected_protocol == null ||
                    selected_protocol.lease_item_on_lease_to_sale_protocol_list_FK == null ||
                    selected_protocol.return_lease_protocol_FK == null
                )
                {
                    throw new Exception("19");//protocol not found
                }

                Lease_To_Sale_Protocol_Model return_obj = new Lease_To_Sale_Protocol_Model
                {
                    id = selected_protocol.id,
                    full_number = selected_protocol.full_number,
                    order_id = selected_protocol.order_FKid,
                    timestamp = selected_protocol.timestamp,
                    total_weight_kg = selected_protocol.return_lease_protocol_FK.total_weight_kg,
                    total_area_m2 = selected_protocol.return_lease_protocol_FK.total_area_m2,
                    total_worth = selected_protocol.return_lease_protocol_FK.total_worth,
                    return_lease_protocol_id = selected_protocol.return_lease_protocol_FKid
                };


                Task get_protocol_info = Task.Run(async () =>
                {
                    List<Encrypted_Object> encrypted_fields =
                    [
                        new Encrypted_Object { id = 1, encryptedValue = selected_protocol.return_lease_protocol_FK.element },
                        new Encrypted_Object { id = 2, encryptedValue = selected_protocol.return_lease_protocol_FK.transport },
                        new Encrypted_Object { id = 3, encryptedValue = selected_protocol.return_lease_protocol_FK.comment }
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
                                return_obj.element = field.decryptedValue; break;

                            case 2:
                                return_obj.transport = field.decryptedValue; break;

                            case 3:
                                return_obj.comment = field.decryptedValue; break;

                            default:
                                throw new Exception("3");//error while decrypting data 
                        }
                    }

                });


                Task get_items_on_protocol_info = Task.Run(async () =>
                {
                    List<Lease_Item_On_lease_To_Sale_Protocol_Model> items_on_protocol_model = new List<Lease_Item_On_lease_To_Sale_Protocol_Model>();

                    if (selected_protocol.lease_item_on_lease_to_sale_protocol_list_FK.Count == 0)
                    {
                        return_obj.lease_item_on_lease_to_sale_protocol_list_FK = items_on_protocol_model;
                        return;
                    }

                    List<Encrypted_Object> encrypted_comment_list = new List<Encrypted_Object>();

                    foreach (var item in selected_protocol.lease_item_on_lease_to_sale_protocol_list_FK)
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

                        items_on_protocol_model.Add(new Lease_Item_On_lease_To_Sale_Protocol_Model
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

                    return_obj.lease_item_on_lease_to_sale_protocol_list_FK = items_on_protocol_model;

                });


                await get_protocol_info;
                await get_items_on_protocol_info;

                return return_obj;
            }

        }


        public async Task<Lease_To_Sale_Protocol_Print_Model> Print_Lease_To_Sale_Protocol(Print_Lease_To_Sale_Protocol_Data input_obj, Session_Data session)
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
                    var protocol = _context.Lease_To_Sale_Protocol.Where(lps =>
                        lps.id.Equals(input_obj.lease_to_sale_protocol_id)
                    )
                    .Include(lp => lp.lease_item_on_lease_to_sale_protocol_list_FK)
                    .FirstOrDefault();

                    if (protocol == null)
                    {
                        throw new Exception("19");
                    }

                    if (protocol.lease_to_sale_protocol_printed_data_FK != null)
                    {

                        var binary_data = _context.Lease_To_Sale_Protocol_Binary_Data.Where(lsb => lsb.lease_to_sale_protocol_printed_data_FKid.Equals(protocol.lease_to_sale_protocol_printed_data_FKid)).FirstOrDefault();

                        if (binary_data == null)
                        {
                            throw new Exception("19"); // binary data should exist if printed data exists
                        }

                        byte[] file = await Crypto.DecryptByte(session, binary_data.lease_to_sale_protocol_bytes);
                        if (file == null)
                        {
                            throw new Exception("3");//decryption
                        }

                        return new Lease_To_Sale_Protocol_Print_Model { protocol_bytes = file, protocol_file_name = protocol.lease_to_sale_protocol_printed_data_FK.lease_to_sale_protocol_file_name };

                    }
                }



                var selected_protocol = _context.Lease_To_Sale_Protocol.Where(ltsp =>
                    ltsp.id.Equals(input_obj.lease_to_sale_protocol_id)
                )
                .Include(lp => lp.order_FK)
                    .ThenInclude(o => o.client_FK)
                .Include(lp => lp.order_FK)
                    .ThenInclude(o => o.construction_site_FK)
                .Include(lp => lp.lease_item_on_lease_to_sale_protocol_list_FK)
                    .ThenInclude(i => i.lease_item_in_storage_FK)
                        .ThenInclude(li => li.lease_item_FK)
                            .ThenInclude(li => li.counting_unit_FK)
                .Include(ltsp => ltsp.return_lease_protocol_FK)
                .FirstOrDefault();

                if (
                    selected_protocol == null ||
                    selected_protocol.order_FK == null ||
                    selected_protocol.order_FK.client_FK == null ||
                    selected_protocol.order_FK.construction_site_FK == null ||
                    selected_protocol.lease_item_on_lease_to_sale_protocol_list_FK == null ||
                    selected_protocol.return_lease_protocol_FK == null
                )
                {
                    throw new Exception("19");//protocol not found
                }

                Lease_To_Sale_Protocol_Model protocol_info = new Lease_To_Sale_Protocol_Model
                {
                    id = selected_protocol.id,
                    full_number = selected_protocol.full_number,
                    order_id = selected_protocol.order_FKid,
                    timestamp = selected_protocol.timestamp,
                    total_weight_kg = selected_protocol.return_lease_protocol_FK.total_weight_kg,
                    total_area_m2 = selected_protocol.return_lease_protocol_FK.total_area_m2,
                    total_worth = selected_protocol.return_lease_protocol_FK.total_worth,
                    return_lease_protocol_id = selected_protocol.return_lease_protocol_FKid
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


                var company_data = _context.Company_Info.FirstOrDefault();
                if (company_data == null)
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

                List<Lease_Item_On_lease_To_Sale_Protocol_Model> items_on_protocol_model = new List<Lease_Item_On_lease_To_Sale_Protocol_Model>();


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
                    if (decrypted_items == null || decrypted_items.Count != encrypted_items.Count)
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


                Task get_protocol_info = Task.Run(async () =>
                {
                    List<Encrypted_Object> encrypted_fields =
                    [
                        new Encrypted_Object { id = 1, encryptedValue = selected_protocol.return_lease_protocol_FK.element },
                        new Encrypted_Object { id = 2, encryptedValue = selected_protocol.return_lease_protocol_FK.transport },
                        new Encrypted_Object { id = 3, encryptedValue = selected_protocol.return_lease_protocol_FK.comment }
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

                    if (selected_protocol.lease_item_on_lease_to_sale_protocol_list_FK.Count.Equals(0))
                    {
                        return;
                    }

                    List<Encrypted_Object> encrypted_comment_list = new List<Encrypted_Object>();

                    foreach (var item in selected_protocol.lease_item_on_lease_to_sale_protocol_list_FK)
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

                        items_on_protocol_model.Add(new Lease_Item_On_lease_To_Sale_Protocol_Model
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

                foreach (var protocol_row in items_on_protocol_model)
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
                if (company_logo_data == null || company_logo_data.company_logo == null || company_logo_data.file_type == null)
                {
                    throw new Exception("19");//not found
                }

                byte[] decrypted_logo = await Crypto.DecryptByte(session, company_logo_data.company_logo);
                if (decrypted_logo == null)
                {
                    throw new Exception("3");//decryption
                }

                string client_name = client_info.company_name.Equals("") ? client_info.name + " " + client_info.surname : client_info.company_name;

                protocol_info.comment = protocol_info.comment == "" ? "" :
                    @"<h5>Uwagi:</h5>
                    <div>" + protocol_info.comment + "</div>";


                protocol_html = string.Format(protocol_html,
                [
                    protocol_info.full_number,
                    language_model.LeaseToSale,
                    protocol_info.timestamp.ToString("dd.MM.yyyy HH:mm"),
                    company_info.company_name,
                    company_info.phone_number,
                    company_info.email,
                    company_info.web_page,
                    language_model.Release + language_model.FromDay,
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
                    ""
                ]);


                BrowserFetcher browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
                using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true }))
                {
                    using (var page = await browser.NewPageAsync())
                    {
                        await page.SetContentAsync(protocol_html);
                        await page.EvaluateExpressionAsync("document.title = '" + protocol_info.full_number + "';");

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
                        if (encrypted_pdf == null)
                        {
                            throw new Exception("2");//encryption
                        }

                        Lease_To_Sale_Protocol_Printed_Data printed_data = new Lease_To_Sale_Protocol_Printed_Data
                        {
                            lease_to_sale_protocol_file_name = protocol_info.full_number.Replace("/", "_"),
                            lease_to_sale_protocol_FKid = protocol_info.id,
                            print_timestamp = input_obj.user_current_timestamp,
                            lease_to_sale_protocol_binary_data_FK = new Lease_To_Sale_Protocol_Binary_Data
                            {
                                lease_to_sale_protocol_bytes = encrypted_pdf
                            }
                        };

                        _context.Lease_To_Sale_Protocol_Printed_Data.Add(printed_data);
                        _context.SaveChanges();

                        return new Lease_To_Sale_Protocol_Print_Model { protocol_file_name = printed_data.lease_to_sale_protocol_file_name, protocol_bytes = pdf };

                    }
                }





            }

        }



        public async Task<List<Lease_To_Sale_Protocol_Model_List>> Get_All_Lease_To_Sale_Protocol(Get_All_Lease_To_Sale_Protocol_Data input_obj, Session_Data session)
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

                
                var protocol_list = _context.Lease_To_Sale_Protocol.Where(ltsp =>
                    ltsp.order_FKid.Equals(order.id)
                )
                .Include(pl => pl.return_lease_protocol_FK)
                .ToList();
                

                List<Lease_To_Sale_Protocol_Model_List> lease_protocol_list_model = new List<Lease_To_Sale_Protocol_Model_List>();

                if (protocol_list.Count == 0)
                {
                    return lease_protocol_list_model;
                }

                List<Encrypted_Object> encrypted_protocol_element = new List<Encrypted_Object>();
                List<Encrypted_Object> encrypted_protocol_transport = new List<Encrypted_Object>();
                List<Encrypted_Object> encrypted_comment = new List<Encrypted_Object>();


                foreach (var protocol in protocol_list)
                {
                    if(protocol.return_lease_protocol_FK == null)
                    {
                        throw new Exception("19");//not found
                    }

                    lease_protocol_list_model.Add(new Lease_To_Sale_Protocol_Model_List
                    {
                        id = protocol.id,
                        timestamp = protocol.timestamp,
                        full_number = protocol.full_number,
                        total_weight_kg = protocol.return_lease_protocol_FK.total_weight_kg,
                        total_area_m2 = protocol.return_lease_protocol_FK.total_area_m2,
                        total_worth = protocol.return_lease_protocol_FK.total_worth,
                        lease_protocol_id = protocol.return_lease_protocol_FKid
                    });

                    encrypted_protocol_element.Add(new Encrypted_Object { id = protocol.id, encryptedValue = protocol.return_lease_protocol_FK.element });
                    encrypted_protocol_transport.Add(new Encrypted_Object { id = protocol.id, encryptedValue = protocol.return_lease_protocol_FK.transport });
                    encrypted_comment.Add(new Encrypted_Object { id = protocol.id, encryptedValue = protocol.return_lease_protocol_FK.comment });
                }

                var decrypted_protocol_element = await Crypto.DecryptList(session, encrypted_protocol_element);
                var decrypted_protocol_transport = await Crypto.DecryptList(session, encrypted_protocol_transport);
                var decrypted_comment = await Crypto.DecryptList(session, encrypted_comment);

                if (
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



    }
}
