using ManagemateAPI.Database.Context;
using ManagemateAPI.Encryption;
using ManagemateAPI.Database.Tables;
using ManagemateAPI.Encryption.Input_Objects;
using ManagemateAPI.Management.M_Invoice.Input_Objects;
using ManagemateAPI.Management.M_Invoice.Table_Model;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using Humanizer;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Client.Table_Model;
using ManagemateAPI.Management.M_Company.Table_Model;
using ManagemateAPI.Management.Shared.Enum;
using ManagemateAPI.Management.M_Order.Table_Model;
using ManagemateAPI.Management.Shared.Static;
using PuppeteerSharp.Media;
using PuppeteerSharp;
using ManagemateAPI.Management.Shared.Json_Model;

/*
 * This is the Invoice_Manager with methods dedicated to the Invoice table.
 * 
 * It contains methods to:
 * add records,
 * edit records,
 * delete records,
 * get record by id,
 */
namespace ManagemateAPI.Management.M_Invoice.Manager
{
    public class Invoice_Manager
    {

        private DB_Context _context;
        private readonly IConfiguration _configuration;

        public Invoice_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }





        public async Task<List<Order_With_Pending_Invoices_Model>> Get_Order_List_With_Pending_Invoices(Session_Data session)
        {
            if (session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                List<Order_With_Pending_Invoices_Model> return_obj = new List<Order_With_Pending_Invoices_Model>();


                var order_list = _context.Order.Where(o => 
                    o.deleted.Equals(false)
                )
                .Include(o => o.lease_protocol_list_FK)
                .Include(o => o.sale_protocol_list_FK)
                .Include(o => o.lease_item_out_of_storage_list_FK)
                .Include(o => o.client_FK)
                .Include(o => o.invoice_list_FK)
                .ToList();

                if (order_list.Count.Equals(0))
                {
                    return return_obj;
                }

                var settings = _context.Company_Invoice_Settings.FirstOrDefault();
                if(settings == null)
                {
                    throw new Exception("51");//settings not found
                }


                List<Encrypted_Object> encrypted_order_data = new List<Encrypted_Object>();
                List<Encrypted_Object> encrypted_client_data = new List<Encrypted_Object>();
                List<Encrypted_Object> encrypted_client_data_2 = new List<Encrypted_Object>();

                DateTime start_date = new DateTime(); //user current date + one day to ensure that any existing protocol is before it;
                DateTime end_date = new DateTime(); //user current date

                DateTime operational_timestamp;

                List<Year_Month> order_months = new List<Year_Month>();

                Lease_Protocol? lease_protocol_reference;
                Sale_Protocol? sale_protocol_reference;
                Lease_Item_Out_Of_Storage_History? out_of_storage_history;

                bool order_has_pending_invoice = false;
                bool set_start_end_dates = false;
                
                foreach(var order in order_list)
                {
                    order_has_pending_invoice = false;
                    set_start_end_dates = false;

                    if (
                        order == null || 
                        order.lease_protocol_list_FK == null ||
                        order.sale_protocol_list_FK == null ||
                        order.lease_item_out_of_storage_list_FK == null ||
                        order.invoice_list_FK == null ||
                        order.client_FK == null
                    )
                    {
                        throw new Exception("19");//not found
                    }

                    //if nothing was done in order then it is skipped
                    if(order.lease_protocol_list_FK.Count.Equals(0) && order.sale_protocol_list_FK.Count.Equals(0))
                    {
                        continue;
                    }

                    //if at least one lease_protocol exists in order, then the earliest timestamp is assigned to "first timestamp" in order
                    if (order.lease_protocol_list_FK.Count > 0)
                    {
                        lease_protocol_reference = order.lease_protocol_list_FK.MinBy(lp => lp.timestamp);
                        if(lease_protocol_reference == null)
                        {
                            throw new Exception("19");//not found
                        }

                        start_date = lease_protocol_reference.timestamp;

                        lease_protocol_reference = order.lease_protocol_list_FK.MaxBy(lp => lp.timestamp);
                        if (lease_protocol_reference == null)
                        {
                            throw new Exception("19");//not found
                        }

                        end_date = lease_protocol_reference.timestamp;

                        set_start_end_dates = true;
                    }

                    //if at least one sale_protocol exists in order, then the earliest timestamp, if is earlier than previously assigned, is assigned to "first timestamp" in order
                    if (order.sale_protocol_list_FK.Count > 0)
                    {
                        sale_protocol_reference = order.sale_protocol_list_FK.MinBy(sp => sp.timestamp);
                        if(sale_protocol_reference == null)
                        {
                            throw new Exception("19");//not found
                        }

                        if (set_start_end_dates)
                        {
                            if (sale_protocol_reference.timestamp < start_date)
                            {
                                start_date = sale_protocol_reference.timestamp;
                            }

                            sale_protocol_reference = order.sale_protocol_list_FK.MaxBy(sp => sp.timestamp);
                            if (sale_protocol_reference == null)
                            {
                                throw new Exception("19");//not found
                            }

                            if (sale_protocol_reference.timestamp > end_date)
                            {
                                end_date = sale_protocol_reference.timestamp;
                            }
                        }
                        else
                        {
                            start_date = sale_protocol_reference.timestamp;

                            sale_protocol_reference = order.sale_protocol_list_FK.MaxBy(sp => sp.timestamp);
                            if (sale_protocol_reference == null)
                            {
                                throw new Exception("19");//not found
                            }

                            end_date = sale_protocol_reference.timestamp;
                        }


                    }



                    //converting first and last timestamp in order to "all existing months in order" list
                    order_months.Add(new Year_Month { year = end_date.Year, month = end_date.Month });
                    while (start_date < end_date)
                    {
                        if(order_months.Where(om => om.year.Equals(start_date.Year) && om.month.Equals(start_date.Month) ).FirstOrDefault() == null)
                        {
                            order_months.Add(new Year_Month { year = start_date.Year, month = start_date.Month });
                        }

                        start_date = start_date.AddMonths(1);
                    }

                    //each month is checked if there was created confirmed protocol during it OR if at least one item_out_of_storage was present on construction site during that month
                    foreach (var month in order_months) 
                    {
                        if (settings.invoice_type_division)
                        {
                            if (
                                order.invoice_list_FK.Where(i =>
                                    i.invoice_type.Equals(Invoice_Type.Lease) &&
                                    i.year.Equals(month.year) &&
                                    i.month.Equals(month.month)
                                )
                                .FirstOrDefault() != null &&
                                order.invoice_list_FK.Where(i =>
                                    i.invoice_type.Equals(Invoice_Type.Sale) &&
                                    i.year.Equals(month.year) &&
                                    i.month.Equals(month.month)
                                )
                                .FirstOrDefault() != null
                            )
                            {
                                continue;
                            }



                            //if lease invoice has not been issued, month is checked for lease_protocols
                            if (
                                order.invoice_list_FK.Where(i =>
                                    i.invoice_type.Equals(Invoice_Type.Lease) &&
                                    i.year.Equals(month.year) &&
                                    i.month.Equals(month.month)
                                )
                                .FirstOrDefault() == null
                            )
                            {

                                if (
                                    order.lease_protocol_list_FK.Any(lp =>
                                        lp.state.Equals(Protocol_State.Confirmed) &&
                                        lp.lease_item_on_protocol_list_FK.Count > 0 &&
                                        lp.timestamp.Year.Equals(month.year) &&
                                        lp.timestamp.Month.Equals(month.month)
                                    )
                                )
                                {
                                

                                    return_obj.Add(new Order_With_Pending_Invoices_Model
                                    {
                                        order_id = order.id,
                                        client_id = order.client_FKid,
                                        client_number = order.client_FK.number,
                                        client_is_private_person = order.client_FK.is_private_person,
                                        number = order.order_number
                                    }
                                    );

                                    if (encrypted_client_data.Where(ec => ec.id.Equals(order.client_FKid)).FirstOrDefault() == null)
                                    {
                                        if (order.client_FK.is_private_person)
                                        {
                                            encrypted_client_data.Add(new Encrypted_Object { id = order.client_FKid, encryptedValue = order.client_FK.name });
                                            encrypted_client_data_2.Add(new Encrypted_Object { id = order.client_FKid, encryptedValue = order.client_FK.surname });
                                        }
                                        else
                                        {
                                            encrypted_client_data.Add(new Encrypted_Object { id = order.client_FKid, encryptedValue = order.client_FK.company_name });
                                        }
                                    }
                                    encrypted_order_data.Add(new Encrypted_Object { id = order.id, encryptedValue = order.order_name });

                                    break;
                                }


                                //if no lease protocols has been created in that month
                                //loop checks if at least one item was present on construction site during this month
                                operational_timestamp = new DateTime(month.year, month.month, 1);
                                foreach (var item_out_of_storage in order.lease_item_out_of_storage_list_FK)
                                {
                                    if (
                                        item_out_of_storage == null ||
                                        item_out_of_storage.lease_item_out_of_storage_history_FK == null
                                    )
                                    {
                                        throw new Exception("19");//not found
                                    }

                                    out_of_storage_history = item_out_of_storage.lease_item_out_of_storage_history_FK.Where(lioh => lioh.timestamp < operational_timestamp).MaxBy(lioh => lioh.timestamp);

                                    if (out_of_storage_history != null)
                                    {
                                        if (out_of_storage_history.total_quantity > 0)
                                        {
                                            return_obj.Add(new Order_With_Pending_Invoices_Model
                                            {
                                                order_id = order.id,
                                                client_id = order.client_FKid,
                                                client_number = order.client_FK.number,
                                                client_is_private_person = order.client_FK.is_private_person,
                                                number = order.order_number
                                            }
                                            );

                                            if (encrypted_client_data.Where(ec => ec.id.Equals(order.client_FKid)).FirstOrDefault() == null)
                                            {
                                                if (order.client_FK.is_private_person)
                                                {
                                                    encrypted_client_data.Add(new Encrypted_Object { id = order.client_FKid, encryptedValue = order.client_FK.name });
                                                    encrypted_client_data_2.Add(new Encrypted_Object { id = order.client_FKid, encryptedValue = order.client_FK.surname });
                                                }
                                                else
                                                {
                                                    encrypted_client_data.Add(new Encrypted_Object { id = order.client_FKid, encryptedValue = order.client_FK.company_name });
                                                }
                                            }
                                            encrypted_order_data.Add(new Encrypted_Object { id = order.id, encryptedValue = order.order_name });

                                            order_has_pending_invoice = true;
                                            break;

                                        }
                                    }

                                }

                                if (order_has_pending_invoice)
                                {
                                    break;
                                }


                            }



                            //if sale invoice has not been issued, then month is checked for sales
                            if (
                                order.invoice_list_FK.Where(i =>
                                    i.invoice_type.Equals(Invoice_Type.Sale) &&
                                    i.year.Equals(month.year) &&
                                    i.month.Equals(month.month)
                                )
                                .FirstOrDefault() == null
                            )
                            {
                                //if at least one confirmed sale_protocol (with items or services on it) or lease_protocol with services on it exists, order is added to return list
                                if (
                                    order.sale_protocol_list_FK.Any(sp =>
                                        sp.state.Equals(Protocol_State.Confirmed) &&
                                        (
                                            sp.sale_item_on_protocol_list_FK.Count > 0 ||
                                            sp.service_on_sale_protocol_list_FK.Count > 0
                                        ) &&
                                        sp.timestamp.Year.Equals(month.year) &&
                                        sp.timestamp.Month.Equals(month.month)
                                    ) ||
                                    order.lease_protocol_list_FK.Any(lp =>
                                        lp.state.Equals(Protocol_State.Confirmed) &&
                                        (
                                            lp.service_on_lease_protocol_list_FK.Count > 0 ||
                                            lp.lease_to_sale_protocol_FKid != null
                                        ) &&
                                        lp.timestamp.Year.Equals(month.year) &&
                                        lp.timestamp.Month.Equals(month.month)
                                    )
                                )
                                {

                                    return_obj.Add(new Order_With_Pending_Invoices_Model
                                        {
                                            order_id = order.id,
                                            client_id = order.client_FKid,
                                            client_number = order.client_FK.number,
                                            client_is_private_person = order.client_FK.is_private_person,
                                            number = order.order_number
                                        }
                                    );

                                    if (encrypted_client_data.Where(ec => ec.id.Equals(order.client_FKid)).FirstOrDefault() == null)
                                    {
                                        if (order.client_FK.is_private_person)
                                        {
                                            encrypted_client_data.Add(new Encrypted_Object { id = order.client_FKid, encryptedValue = order.client_FK.name });
                                            encrypted_client_data_2.Add(new Encrypted_Object { id = order.client_FKid, encryptedValue = order.client_FK.surname });
                                        }
                                        else
                                        {
                                            encrypted_client_data.Add(new Encrypted_Object { id = order.client_FKid, encryptedValue = order.client_FK.company_name });
                                        }
                                    }
                                    encrypted_order_data.Add(new Encrypted_Object { id = order.id, encryptedValue = order.order_name });

                                    break;
                                }

                            }


                        }
                        else
                        {
                            //if combined invoice related with that month has been issued, further checking is skipped
                            if(
                                order.invoice_list_FK.Where(i => 
                                    i.invoice_type.Equals(Invoice_Type.Combined) && 
                                    i.year.Equals(month.year) && 
                                    i.month.Equals(month.month) 
                                )
                                .FirstOrDefault() != null
                            )
                            {
                                continue;
                            }

                            //if at least one confirmed lease_protocol (with items or services on it) exists, order is added to return list
                            if(
                                order.lease_protocol_list_FK.Any(lp => 
                                    lp.state.Equals(Protocol_State.Confirmed) && 
                                    (
                                        lp.lease_item_on_protocol_list_FK.Count > 0 || 
                                        lp.service_on_lease_protocol_list_FK.Count > 0
                                    ) &&
                                    lp.timestamp.Year.Equals(month.year) &&
                                    lp.timestamp.Month.Equals(month.month)
                                )
                            )
                            {
                                return_obj.Add(new Order_With_Pending_Invoices_Model
                                    {
                                        order_id = order.id,
                                        client_id = order.client_FKid,
                                        client_number = order.client_FK.number,
                                        client_is_private_person = order.client_FK.is_private_person,
                                        number = order.order_number
                                    }
                                );

                                if (encrypted_client_data.Where(ec => ec.id.Equals(order.client_FKid)).FirstOrDefault() == null)
                                {
                                    if (order.client_FK.is_private_person)
                                    {
                                        encrypted_client_data.Add(new Encrypted_Object { id = order.client_FKid, encryptedValue = order.client_FK.name });
                                        encrypted_client_data_2.Add(new Encrypted_Object { id = order.client_FKid, encryptedValue = order.client_FK.surname });
                                    }
                                    else
                                    {
                                        encrypted_client_data.Add(new Encrypted_Object { id = order.client_FKid, encryptedValue = order.client_FK.company_name });
                                    }
                                }
                                encrypted_order_data.Add(new Encrypted_Object { id = order.id, encryptedValue = order.order_name });

                                break;
                            }

                            //if at least one confirmed sale_protocol (with items or services on it) exists, order is added to return list
                            if (
                                order.sale_protocol_list_FK.Any(sp => 
                                    sp.state.Equals(Protocol_State.Confirmed) && 
                                    (
                                        sp.sale_item_on_protocol_list_FK.Count > 0 || 
                                        sp.service_on_sale_protocol_list_FK.Count > 0
                                    ) &&
                                    sp.timestamp.Year.Equals(month.year) &&
                                    sp.timestamp.Month.Equals(month.month)
                                )
                            )
                            {
                                return_obj.Add(new Order_With_Pending_Invoices_Model
                                    {
                                        order_id = order.id,
                                        client_id = order.client_FKid,
                                        client_number = order.client_FK.number,
                                        client_is_private_person = order.client_FK.is_private_person,
                                        number = order.order_number
                                    }
                                );

                                if (encrypted_client_data.Where(ec => ec.id.Equals(order.client_FKid)).FirstOrDefault() == null)
                                {
                                    if (order.client_FK.is_private_person)
                                    {
                                        encrypted_client_data.Add(new Encrypted_Object { id = order.client_FKid, encryptedValue = order.client_FK.name });
                                        encrypted_client_data_2.Add(new Encrypted_Object { id = order.client_FKid, encryptedValue = order.client_FK.surname });
                                    }
                                    else
                                    {
                                        encrypted_client_data.Add(new Encrypted_Object { id = order.client_FKid, encryptedValue = order.client_FK.company_name });
                                    }
                                }
                                encrypted_order_data.Add(new Encrypted_Object { id = order.id, encryptedValue = order.order_name });

                                break;
                            }

                            //if no protocols has been created in that month
                            //loop checks if at least one item was present on construction site during this month
                            operational_timestamp = new DateTime(month.year, month.month, 1);
                            foreach(var item_out_of_storage in order.lease_item_out_of_storage_list_FK)
                            {
                                if(
                                    item_out_of_storage == null || 
                                    item_out_of_storage.lease_item_out_of_storage_history_FK == null
                                )
                                {
                                    throw new Exception("19");//not found
                                }

                                out_of_storage_history = item_out_of_storage.lease_item_out_of_storage_history_FK.Where(lioh => lioh.timestamp < operational_timestamp).MaxBy(lioh => lioh.timestamp);

                                if (out_of_storage_history != null)
                                {
                                    if(out_of_storage_history.total_quantity > 0)
                                    {
                                        return_obj.Add(new Order_With_Pending_Invoices_Model
                                            {
                                                order_id = order.id,
                                                client_id = order.client_FKid,
                                                client_number = order.client_FK.number,
                                                client_is_private_person = order.client_FK.is_private_person,
                                                number = order.order_number
                                            }
                                        );

                                        if (encrypted_client_data.Where(ec => ec.id.Equals(order.client_FKid)).FirstOrDefault() == null)
                                        {
                                            if (order.client_FK.is_private_person)
                                            {
                                                encrypted_client_data.Add(new Encrypted_Object { id = order.client_FKid, encryptedValue = order.client_FK.name });
                                                encrypted_client_data_2.Add(new Encrypted_Object { id = order.client_FKid, encryptedValue = order.client_FK.surname });
                                            }
                                            else
                                            {
                                                encrypted_client_data.Add(new Encrypted_Object { id = order.client_FKid, encryptedValue = order.client_FK.company_name });
                                            }
                                        }
                                        encrypted_order_data.Add(new Encrypted_Object { id = order.id, encryptedValue = order.order_name });

                                        order_has_pending_invoice = true;
                                        break;

                                    }
                                }

                            }

                            if (order_has_pending_invoice)
                            {
                                break;
                            }

                        }



                    }                   
                    


                }

                var decrypted_order_data = await Crypto.DecryptList(session, encrypted_order_data);
                var decrypted_client_data = await Crypto.DecryptList(session, encrypted_client_data);
                var decrypted_client_data_2 = await Crypto.DecryptList(session, encrypted_client_data_2);

                if (
                    decrypted_order_data == null || decrypted_order_data.Count != encrypted_order_data.Count ||
                    decrypted_client_data == null || decrypted_client_data.Count != encrypted_client_data.Count ||
                    decrypted_client_data_2 == null || decrypted_client_data_2.Count != encrypted_client_data_2.Count
                )
                {
                    throw new Exception("3");//decryption error
                }

                Decrypted_Object? order_name;
                Decrypted_Object? client_info;
                Decrypted_Object? client_info_2;
                foreach(var order_model in return_obj)
                {
                    order_name = decrypted_order_data.Where(o => o.id.Equals(order_model.order_id)).FirstOrDefault();
                    if(order_name == null)
                    {
                        throw new Exception("3");//decryption error
                    }
                    order_model.name = order_name.decryptedValue;

                    client_info = decrypted_client_data.Where(d => d.id.Equals(order_model.client_id)).FirstOrDefault();
                    if (client_info == null)
                    {
                        throw new Exception("3");//decryption error
                    }
                    order_model.client_info = client_info.decryptedValue;


                    if (order_model.client_is_private_person)
                    {
                        client_info_2 = decrypted_client_data_2.Where(d => d.id.Equals(order_model.client_id)).FirstOrDefault();
                        if (client_info_2 == null)
                        {
                            throw new Exception("3");//decryption error
                        }

                        order_model.client_info += " " + client_info_2.decryptedValue;
                    }

                }

                return return_obj;

            }
        }


        public async Task<Order_Pending_Invoice_List_Model> Get_Order_Pending_Invoice_List(Get_Order_Pending_Invoice_List_Data input_obj, Session_Data session)
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
                )
                .Include(o => o.lease_protocol_list_FK)
                .Include(o => o.sale_protocol_list_FK)
                .Include(o => o.lease_item_out_of_storage_list_FK)
                .Include(o => o.client_FK)
                .Include(o => o.invoice_list_FK)
                .FirstOrDefault();

                if(
                    order == null ||
                    order.lease_protocol_list_FK == null ||
                    order.sale_protocol_list_FK == null ||
                    order.lease_item_out_of_storage_list_FK == null ||
                    order.client_FK == null ||
                    order.invoice_list_FK == null
                )
                {
                    throw new Exception("19");//not found
                }


                var settings = _context.Company_Invoice_Settings.FirstOrDefault();
                if (settings == null)
                {
                    throw new Exception("51");//settings not found
                }


                Order_Pending_Invoice_List_Model return_obj = new Order_Pending_Invoice_List_Model
                {
                    order_id = order.id,
                    number = order.order_number,
                    client_id = order.client_FKid,
                    client_number = order.client_FK.number,
                    pending_invoice_list = new List<Pending_Invoice_Model>()
                };


                Task get_order_info = Task.Run(async () =>
                {
                    List<Encrypted_Object> encrypted_fields = new List<Encrypted_Object>()
                    {
                        new Encrypted_Object { id = 1, encryptedValue = order.order_name }
                    };

                    if (order.client_FK.is_private_person)
                    {
                        encrypted_fields.Add(new Encrypted_Object { id = 2, encryptedValue = order.client_FK.name });
                        encrypted_fields.Add(new Encrypted_Object { id = 3, encryptedValue = order.client_FK.surname });
                    }
                    else
                    {
                        encrypted_fields.Add(new Encrypted_Object { id = 2, encryptedValue = order.client_FK.company_name });
                    }

                    var decrypted_fields = await Crypto.DecryptList(session, encrypted_fields);

                    if(
                        decrypted_fields == null || 
                        decrypted_fields.Count != encrypted_fields.Count
                    )
                    {
                        throw new Exception("3");//decryption error
                    }


                    string surname = "";

                    foreach(var field in decrypted_fields)
                    {
                        if(field == null)
                        {
                            throw new Exception("3");//decryption error
                        }

                        switch (field.id)
                        {
                            case 1:
                                return_obj.name = field.decryptedValue; 
                                break;

                            case 2:
                                return_obj.client_info = field.decryptedValue;
                                break;

                            case 3:
                                surname = field.decryptedValue;
                                break;

                            default:
                                throw new Exception("3");//decryption error
                        }
                    }

                    if (order.client_FK.is_private_person)
                    {
                        return_obj.client_info += " " + surname;
                    }


                });


                //if nothing was done in order then list is empty
                if (order.lease_protocol_list_FK.Count.Equals(0) && order.sale_protocol_list_FK.Count.Equals(0))
                {
                    return return_obj;
                }

                DateTime start_date = new DateTime(); //user current date + one day to ensure that any existing protocol is before it;
                DateTime end_date = new DateTime(); //user current date

                Lease_Protocol? lease_protocol_reference;
                Sale_Protocol? sale_protocol_reference;
                bool set_start_end_dates = false;

                //if at least one lease_protocol exists in order, then the earliest timestamp is assigned to "first timestamp" in order
                if (order.lease_protocol_list_FK.Count > 0)
                {
                    lease_protocol_reference = order.lease_protocol_list_FK.MinBy(lp => lp.timestamp);
                    if (lease_protocol_reference == null)
                    {
                        throw new Exception("19");//not found
                    }

                    start_date = lease_protocol_reference.timestamp;

                    lease_protocol_reference = order.lease_protocol_list_FK.MaxBy(lp => lp.timestamp);
                    if (lease_protocol_reference == null)
                    {
                        throw new Exception("19");//not found
                    }

                    end_date = lease_protocol_reference.timestamp;

                    set_start_end_dates = true;
                }

                //if at least one sale_protocol exists in order, then the earliest timestamp, if is earlier than previously assigned, is assigned to "first timestamp" in order
                if (order.sale_protocol_list_FK.Count > 0)
                {
                    sale_protocol_reference = order.sale_protocol_list_FK.MinBy(sp => sp.timestamp);
                    if (sale_protocol_reference == null)
                    {
                        throw new Exception("19");//not found
                    }

                    if (set_start_end_dates)
                    {
                        if (sale_protocol_reference.timestamp < start_date)
                        {
                            start_date = sale_protocol_reference.timestamp;
                        }

                        sale_protocol_reference = order.sale_protocol_list_FK.MaxBy(sp => sp.timestamp);
                        if (sale_protocol_reference == null)
                        {
                            throw new Exception("19");//not found
                        }

                        if (sale_protocol_reference.timestamp > end_date)
                        {
                            end_date = sale_protocol_reference.timestamp;
                        }
                    }
                    else
                    {
                        start_date = sale_protocol_reference.timestamp;

                        sale_protocol_reference = order.sale_protocol_list_FK.MaxBy(sp => sp.timestamp);
                        if (sale_protocol_reference == null)
                        {
                            throw new Exception("19");//not found
                        }

                        end_date = sale_protocol_reference.timestamp;
                    }


                }

                //converting first and last timestamp in order to "all existing months in order" list
                List<Year_Month> order_months = new List<Year_Month>
                {
                    new Year_Month { year = end_date.Year, month = end_date.Month }
                };


                while (start_date < end_date)
                {
                    if (order_months.Where(om => om.year.Equals(start_date.Year) && om.month.Equals(start_date.Month)).FirstOrDefault() == null)
                    {
                        order_months.Add(new Year_Month { year = start_date.Year, month = start_date.Month });
                    }

                    start_date = start_date.AddMonths(1);
                }

                DateTime operational_timestamp;
                bool lease_invoice_added = false;
                Lease_Item_Out_Of_Storage_History? out_of_storage_history;

                //each month is checked if there was created confirmed protocol during it OR if at least one item_out_of_storage was present on construction site during that month
                foreach (var month in order_months)
                {
                    if (settings.invoice_type_division)
                    {
                        if (
                            order.invoice_list_FK.Where(i =>
                                i.invoice_type.Equals(Invoice_Type.Lease) &&
                                i.year.Equals(month.year) &&
                                i.month.Equals(month.month)
                            )
                            .FirstOrDefault() != null &&
                            order.invoice_list_FK.Where(i =>
                                i.invoice_type.Equals(Invoice_Type.Sale) &&
                                i.year.Equals(month.year) &&
                                i.month.Equals(month.month)
                            )
                            .FirstOrDefault() != null
                        )
                        {
                            continue;
                        }



                        //if both invoices have not been issued, month is checked for lease invoice separately
                        if (
                            order.invoice_list_FK.Where(i =>
                                i.invoice_type.Equals(Invoice_Type.Lease) &&
                                i.year.Equals(month.year) &&
                                i.month.Equals(month.month)
                            )
                            .FirstOrDefault() == null
                        )
                        {
                            lease_invoice_added = false;

                            if (
                                order.lease_protocol_list_FK.Any(lp =>
                                    lp.state.Equals(Protocol_State.Confirmed) &&
                                    lp.lease_item_on_protocol_list_FK.Count > 0 &&
                                    lp.timestamp.Year.Equals(month.year) &&
                                    lp.timestamp.Month.Equals(month.month)
                                )
                            )
                            {

                                return_obj.pending_invoice_list.Add(new Pending_Invoice_Model
                                    {
                                       order_id = order.id,
                                       invoice_type = Invoice_Type.Lease,
                                       year = month.year,
                                       month = month.month
                                    }
                                );

                                lease_invoice_added = true;

                            }


                            //if no lease protocols has been created in that month
                            //loop checks if at least one item was present on construction site during this month
                            if (!lease_invoice_added)
                            {
                                operational_timestamp = new DateTime(month.year, month.month, 1);
                                foreach (var item_out_of_storage in order.lease_item_out_of_storage_list_FK)
                                {
                                    if (
                                        item_out_of_storage == null ||
                                        item_out_of_storage.lease_item_out_of_storage_history_FK == null
                                    )
                                    {
                                        throw new Exception("19");//not found
                                    }

                                    out_of_storage_history = item_out_of_storage.lease_item_out_of_storage_history_FK.Where(lioh => lioh.timestamp < operational_timestamp).MaxBy(lioh => lioh.timestamp);

                                    if (out_of_storage_history != null)
                                    {
                                        if (out_of_storage_history.total_quantity > 0)
                                        {
                                            return_obj.pending_invoice_list.Add(new Pending_Invoice_Model
                                                {
                                                    order_id = order.id,
                                                    invoice_type = Invoice_Type.Lease,
                                                    year = month.year,
                                                    month = month.month
                                                }
                                            );

                                            break;

                                        }
                                    }

                                }

                            }



                        }



                        //if sale invoice has not been issued, then month is checked for sales
                        if (
                            order.invoice_list_FK.Where(i =>
                                i.invoice_type.Equals(Invoice_Type.Sale) &&
                                i.year.Equals(month.year) &&
                                i.month.Equals(month.month)
                            )
                            .FirstOrDefault() == null
                        )
                        {
                            //if at least one confirmed sale_protocol (with items or services on it) or lease_protocol with services on it exists, order is added to return list
                            if (
                                order.sale_protocol_list_FK.Any(sp =>
                                    sp.state.Equals(Protocol_State.Confirmed) &&
                                    (
                                        sp.sale_item_on_protocol_list_FK.Count > 0 ||
                                        sp.service_on_sale_protocol_list_FK.Count > 0
                                    ) &&
                                    sp.timestamp.Year.Equals(month.year) &&
                                    sp.timestamp.Month.Equals(month.month)
                                ) ||
                                order.lease_protocol_list_FK.Any(lp =>
                                    lp.state.Equals(Protocol_State.Confirmed) &&
                                    (
                                        lp.service_on_lease_protocol_list_FK.Count > 0 ||
                                        lp.lease_to_sale_protocol_FKid != null
                                    ) &&
                                    lp.timestamp.Year.Equals(month.year) &&
                                    lp.timestamp.Month.Equals(month.month)
                                )
                            )
                            {
                                return_obj.pending_invoice_list.Add(new Pending_Invoice_Model
                                    {
                                        order_id = order.id,
                                        invoice_type = Invoice_Type.Sale,
                                        year = month.year,
                                        month = month.month
                                    }
                                );

                            }

                        }


                    }
                    else
                    {
                        //if combined invoice related with that month has been issued, further checking is skipped
                        if (
                            order.invoice_list_FK.Where(i =>
                                i.invoice_type.Equals(Invoice_Type.Combined) &&
                                i.year.Equals(month.year) &&
                                i.month.Equals(month.month)
                            )
                            .FirstOrDefault() != null
                        )
                        {
                            continue;
                        }

                        //if at least one confirmed lease_protocol (with items or services on it) exists, order is added to return list
                        if (
                            order.lease_protocol_list_FK.Any(lp =>
                                lp.state.Equals(Protocol_State.Confirmed) &&
                                (
                                    lp.lease_item_on_protocol_list_FK.Count > 0 ||
                                    lp.service_on_lease_protocol_list_FK.Count > 0
                                ) &&
                                lp.timestamp.Year.Equals(month.year) &&
                                lp.timestamp.Month.Equals(month.month)
                            )
                        )
                        {
                            return_obj.pending_invoice_list.Add(new Pending_Invoice_Model
                                {
                                    order_id = order.id,
                                    invoice_type = Invoice_Type.Combined,
                                    year = month.year,
                                    month = month.month
                                }
                            );

                            continue;
                        }

                        //if at least one confirmed sale_protocol (with items or services on it) exists, order is added to return list
                        if (
                            order.sale_protocol_list_FK.Any(sp =>
                                sp.state.Equals(Protocol_State.Confirmed) &&
                                (
                                    sp.sale_item_on_protocol_list_FK.Count > 0 ||
                                    sp.service_on_sale_protocol_list_FK.Count > 0
                                ) &&
                                sp.timestamp.Year.Equals(month.year) &&
                                sp.timestamp.Month.Equals(month.month)
                            )
                        )
                        {
                            return_obj.pending_invoice_list.Add(new Pending_Invoice_Model
                            {
                                order_id = order.id,
                                invoice_type = Invoice_Type.Combined,
                                year = month.year,
                                month = month.month
                            }
                            );                            

                            continue;
                        }

                        //if no protocols has been created in that month
                        //loop checks if at least one item was present on construction site during this month
                        operational_timestamp = new DateTime(month.year, month.month, 1);
                        foreach (var item_out_of_storage in order.lease_item_out_of_storage_list_FK)
                        {
                            if (
                                item_out_of_storage == null ||
                                item_out_of_storage.lease_item_out_of_storage_history_FK == null
                            )
                            {
                                throw new Exception("19");//not found
                            }

                            out_of_storage_history = item_out_of_storage.lease_item_out_of_storage_history_FK.Where(lioh => lioh.timestamp < operational_timestamp).MaxBy(lioh => lioh.timestamp);

                            if (out_of_storage_history != null)
                            {
                                if (out_of_storage_history.total_quantity > 0)
                                {
                                    return_obj.pending_invoice_list.Add(new Pending_Invoice_Model
                                        {
                                            order_id = order.id,
                                            invoice_type = Invoice_Type.Combined,
                                            year = month.year,
                                            month = month.month
                                        }
                                    );
                                    
                                    break;

                                }
                            }

                        }


                    }



                }


                await get_order_info;

                return return_obj;

            }
        }



        public async Task<Invoice_Id_Model> Create_Invoice(Create_Invoice_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                if(!Enum.IsDefined(typeof(Invoice_Type), input_obj.invoice_type))
                {
                    throw new Exception("19");//not found
                }


                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var invoice_exists = _context.Invoice.Where(i => 
                    i.order_FKid.Equals(input_obj.order_id) && 
                    i.year.Equals(input_obj.year) && 
                    i.month.Equals(input_obj.month) &&
                    i.invoice_type.Equals(input_obj.invoice_type)
                ).FirstOrDefault();

                if(invoice_exists != null)
                {
                    if (input_obj.overwrite)
                    {
                        _context.Invoice.Remove(invoice_exists);
                    }
                    else
                    {
                        throw new Exception("18");//invoice already exists
                    }
                }

                var invoice_settings = _context.Company_Invoice_Settings.Include(i => i.currency_FK).FirstOrDefault();
                if(invoice_settings == null || invoice_settings.currency_FK == null)
                {
                    throw new Exception("51");//settings not found
                }


                string lease_for_month_name = "";

                Order? order;

                Invoice new_invoice;
                List<Invoice_Row> new_invoice_rows = new List<Invoice_Row>();

                int row_number = 1;

                decimal invoice_total_net_worth = 0;
                decimal invoice_total_tax_worth = 0;


                switch (input_obj.invoice_type)
                {

                    case Invoice_Type.Lease:

                        order = _context.Order.Where(o => 
                            o.id.Equals(input_obj.order_id) && 
                            o.deleted.Equals(false)
                        )
                        .Include(o => 
                            o.lease_protocol_list_FK.Where(lp => 
                                lp.state.Equals(Protocol_State.Confirmed) && 
                                lp.timestamp.Year.Equals(input_obj.year) && 
                                lp.timestamp.Month.Equals(input_obj.month) &&
                                lp.lease_item_on_protocol_list_FK.Count > 0
                            )
                        )
                        .ThenInclude(lp => lp.lease_item_on_protocol_list_FK)
                            .ThenInclude(li => li.lease_item_in_storage_FK)
                        .FirstOrDefault();

                        if(
                            order == null ||
                            order.lease_protocol_list_FK == null ||
                            order.lease_protocol_list_FK.Count.Equals(0)
                        )
                        {
                            throw new Exception("19");//not found
                        }

                        var first_lease_protocol = order.lease_protocol_list_FK.MinBy(lp => lp.timestamp);
                        if(first_lease_protocol != null)
                        {
                            if(
                                input_obj.issue_date < first_lease_protocol.timestamp ||
                                input_obj.sale_date < first_lease_protocol.timestamp ||
                                input_obj.payment_date < first_lease_protocol.timestamp
                            )
                            {
                                throw new Exception("48");//cannot issue invoice before first protocol
                            }
                        }


                        List<Lease_Value_Model> lease_values = Calculate_Lease(ref order, input_obj.year, input_obj.month);

                        /////////////////////////////////////
                        ///Hardcoded "pl"
                        lease_for_month_name = Get_Lease_For_Month_Name(input_obj.month, "pl");


                        foreach (var lease_value in lease_values)
                        {
                            Invoice_Row new_row = new Invoice_Row
                            {
                                row_number = row_number,
                                name = lease_for_month_name,
                                total_quantity = 1,
                                counting_unit = "-",
                                tax_pct = lease_value.tax_pct
                            };

                            new_row.net_worth = lease_value.net_worth;


                            if (input_obj.use_discount)
                            {
                                new_row.use_discount = true;

                                if (input_obj.discount_is_in_pct)
                                {
                                    new_row.discount_is_in_pct = true;
                                    new_row.discount_value = input_obj.discount_value;

                                    new_row.net_worth_after_discount = lease_value.net_worth * ((100 - input_obj.discount_value) / 100);
                                }
                                else
                                {
                                    new_row.discount_is_in_pct = false;
                                    new_row.discount_value = input_obj.discount_value;

                                    if(input_obj.discount_value >= lease_value.net_worth)
                                    {
                                        throw new Exception("47");//discount too high
                                    }

                                    new_row.net_worth_after_discount = lease_value.net_worth - input_obj.discount_value;
                                }
                            }
                            else
                            {
                                new_row.use_discount = false;
                                new_row.discount_is_in_pct = false;
                                new_row.discount_value = 0;

                                new_row.net_worth_after_discount = lease_value.net_worth;
                            }


                            new_row.net_worth_total = new_row.net_worth_after_discount * new_row.total_quantity;
                                
                            new_row.tax_worth = new_row.net_worth_total * (new_row.tax_pct / 100);

                            new_row.gross_worth = new_row.net_worth_total + new_row.tax_worth;


                            invoice_total_net_worth += new_row.net_worth_total;
                            invoice_total_tax_worth += new_row.tax_worth;

                            new_invoice_rows.Add(new_row);
                            row_number++;
                        }                        


                        break;

                    case Invoice_Type.Sale:

                        order = _context.Order.Where(o =>
                            o.id.Equals(input_obj.order_id) &&
                            o.deleted.Equals(false)
                        )
                        .Include(o =>
                            o.lease_protocol_list_FK.Where(lp =>
                                lp.state.Equals(Protocol_State.Confirmed) &&
                                lp.timestamp.Year.Equals(input_obj.year) &&
                                lp.timestamp.Month.Equals(input_obj.month) &&
                                lp.service_on_lease_protocol_list_FK.Count > 0
                            )
                        )
                        .ThenInclude(lp => lp.service_on_lease_protocol_list_FK)
                            .ThenInclude(s => s.service_FK)
                        .Include(o => 
                            o.lease_to_sale_protocol_list_FK.Where(ls =>
                                ls.timestamp.Year.Equals(input_obj.year) &&
                                ls.timestamp.Month.Equals(input_obj.month) &&
                                ls.lease_item_on_lease_to_sale_protocol_list_FK.Count > 0
                            )
                        )
                        .ThenInclude(ls => ls.lease_item_on_lease_to_sale_protocol_list_FK)
                            .ThenInclude(lils => lils.lease_item_in_storage_FK)
                        .Include(o => 
                            o.sale_protocol_list_FK.Where(sp =>
                                sp.state.Equals(Protocol_State.Confirmed) &&
                                sp.timestamp.Year.Equals(input_obj.year) &&
                                sp.timestamp.Month.Equals(input_obj.month) &&
                                sp.sale_item_on_protocol_list_FK.Count > 0
                            )
                        )
                        .ThenInclude(sp => sp.sale_item_on_protocol_list_FK)
                            .ThenInclude(si => si.sale_item_in_storage_FK)
                        .Include(o =>
                            o.sale_protocol_list_FK.Where(sp =>
                                sp.state.Equals(Protocol_State.Confirmed) &&
                                sp.timestamp.Year.Equals(input_obj.year) &&
                                sp.timestamp.Month.Equals(input_obj.month) &&
                                sp.service_on_sale_protocol_list_FK.Count > 0
                            )
                        )
                        .ThenInclude(sp => sp.service_on_sale_protocol_list_FK)
                            .ThenInclude(s => s.service_FK)
                        .FirstOrDefault();

                        if (
                            order == null ||
                            order.lease_protocol_list_FK == null ||
                            order.lease_to_sale_protocol_list_FK == null ||
                            order.sale_protocol_list_FK == null ||
                            (
                                order.lease_protocol_list_FK.Count.Equals(0) && 
                                order.lease_to_sale_protocol_list_FK.Count.Equals(0) && 
                                order.sale_protocol_list_FK.Count.Equals(0)
                            )
                        )
                        {
                            throw new Exception("19");//not found
                        }


                        var first_lease_protocol_in_sale = order.lease_protocol_list_FK.MinBy(lp => lp.timestamp);
                        var first_sale_protocol_in_sale = order.sale_protocol_list_FK.MinBy(lp => lp.timestamp);
                        var first_lease_to_sale_protocol_in_sale = order.lease_to_sale_protocol_list_FK.MinBy(lp => lp.timestamp);

                        bool before_lease = true;
                        bool before_sale = true;
                        bool before_lease_to_sale = true;

                        if (first_lease_protocol_in_sale != null)
                        {
                            if (!(input_obj.issue_date < first_lease_protocol_in_sale.timestamp || input_obj.sale_date < first_lease_protocol_in_sale.timestamp || input_obj.payment_date < first_lease_protocol_in_sale.timestamp))
                            {
                                before_lease = false;
                            }
                        }

                        if (first_sale_protocol_in_sale != null)
                        {
                            if (!(input_obj.issue_date < first_sale_protocol_in_sale.timestamp || input_obj.sale_date < first_sale_protocol_in_sale.timestamp || input_obj.payment_date < first_sale_protocol_in_sale.timestamp))
                            {
                                before_sale = false;
                            }
                        }

                        if (first_lease_to_sale_protocol_in_sale != null)
                        {
                            if (!(input_obj.issue_date < first_lease_to_sale_protocol_in_sale.timestamp || input_obj.sale_date < first_lease_to_sale_protocol_in_sale.timestamp || input_obj.payment_date < first_lease_to_sale_protocol_in_sale.timestamp))
                            {
                                before_lease_to_sale = false;
                            }
                        }

                        if(before_lease == before_sale == before_lease_to_sale == true)
                        {
                            throw new Exception("48");//invoice cannot be issued before first protocol date
                        }



                        List<Sale_Value_Model> sale_values = Calculate_Sale(ref order, input_obj.year, input_obj.month);
                              

                        foreach (var sale_value in sale_values)
                        {
                            Invoice_Row new_row = new Invoice_Row
                            {
                                row_number = row_number,
                                name = sale_value.sale_object_name,
                                total_quantity = sale_value.total_quantity,
                                counting_unit = sale_value.counting_unit,
                                tax_pct = sale_value.tax_pct
                            };

                            new_row.net_worth = sale_value.net_worth;


                            if (input_obj.use_discount)
                            {
                                new_row.use_discount = true;

                                if (input_obj.discount_is_in_pct)
                                {
                                    new_row.discount_is_in_pct = true;
                                    new_row.discount_value = input_obj.discount_value;

                                    new_row.net_worth_after_discount = sale_value.net_worth * ((100 - input_obj.discount_value) / 100);
                                }
                                else
                                {
                                    new_row.discount_is_in_pct = false;
                                    new_row.discount_value = input_obj.discount_value;

                                    if (input_obj.discount_value >= sale_value.net_worth)
                                    {
                                        throw new Exception("47");//discount too high
                                    }

                                    new_row.net_worth_after_discount = sale_value.net_worth - input_obj.discount_value;
                                }
                            }
                            else
                            {
                                new_row.use_discount = false;
                                new_row.discount_is_in_pct = false;
                                new_row.discount_value = 0;

                                new_row.net_worth_after_discount = sale_value.net_worth;
                            }


                            new_row.net_worth_total = new_row.net_worth_after_discount * new_row.total_quantity;

                            new_row.tax_worth = new_row.net_worth_total * (new_row.tax_pct / 100);

                            new_row.gross_worth = new_row.net_worth_total + new_row.tax_worth;


                            invoice_total_net_worth += new_row.net_worth_total;
                            invoice_total_tax_worth += new_row.tax_worth;

                            new_invoice_rows.Add(new_row);
                            row_number++;
                        }

                        
                        break;

                    case Invoice_Type.Combined:

                        order = _context.Order.Where(o =>
                            o.id.Equals(input_obj.order_id) &&
                            o.deleted.Equals(false)
                        )
                        .Include(o =>
                            o.lease_protocol_list_FK.Where(lp =>
                                lp.state.Equals(Protocol_State.Confirmed) &&
                                lp.timestamp.Year.Equals(input_obj.year) &&
                                lp.timestamp.Month.Equals(input_obj.month) &&
                                lp.service_on_lease_protocol_list_FK.Count > 0
                            )
                        )
                        .ThenInclude(lp => lp.service_on_lease_protocol_list_FK)
                            .ThenInclude(s => s.service_FK)
                        .Include(o =>
                            o.lease_protocol_list_FK.Where(lp =>
                                lp.state.Equals(Protocol_State.Confirmed) &&
                                lp.timestamp.Year.Equals(input_obj.year) &&
                                lp.timestamp.Month.Equals(input_obj.month) &&
                                lp.lease_item_on_protocol_list_FK.Count > 0
                            )
                        )
                        .ThenInclude(lp => lp.lease_item_on_protocol_list_FK)
                            .ThenInclude(li => li.lease_item_in_storage_FK)
                        .Include(o =>
                            o.lease_to_sale_protocol_list_FK.Where(ls =>
                                ls.timestamp.Year.Equals(input_obj.year) &&
                                ls.timestamp.Month.Equals(input_obj.month) &&
                                ls.lease_item_on_lease_to_sale_protocol_list_FK.Count > 0
                            )
                        )
                        .ThenInclude(ls => ls.lease_item_on_lease_to_sale_protocol_list_FK)
                            .ThenInclude(lils => lils.lease_item_in_storage_FK)
                        .Include(o =>
                            o.sale_protocol_list_FK.Where(sp =>
                                sp.state.Equals(Protocol_State.Confirmed) &&
                                sp.timestamp.Year.Equals(input_obj.year) &&
                                sp.timestamp.Month.Equals(input_obj.month) &&
                                sp.sale_item_on_protocol_list_FK.Count > 0
                            )
                        )
                        .ThenInclude(sp => sp.sale_item_on_protocol_list_FK)
                            .ThenInclude(si => si.sale_item_in_storage_FK)
                        .Include(o =>
                            o.sale_protocol_list_FK.Where(sp =>
                                sp.state.Equals(Protocol_State.Confirmed) &&
                                sp.timestamp.Year.Equals(input_obj.year) &&
                                sp.timestamp.Month.Equals(input_obj.month) &&
                                sp.service_on_sale_protocol_list_FK.Count > 0
                            )
                        )
                        .ThenInclude(sp => sp.service_on_sale_protocol_list_FK)
                            .ThenInclude(s => s.service_FK)
                        .FirstOrDefault();

                        if (
                            order == null ||
                            order.lease_protocol_list_FK == null ||
                            order.lease_to_sale_protocol_list_FK == null ||
                            order.sale_protocol_list_FK == null ||
                            (
                                order.lease_protocol_list_FK.Count.Equals(0) &&
                                order.lease_to_sale_protocol_list_FK.Count.Equals(0) &&
                                order.sale_protocol_list_FK.Count.Equals(0)
                            )
                        )
                        {
                            throw new Exception("19");//not found
                        }

                        var first_lease_protocol_in_combined = order.lease_protocol_list_FK.MinBy(lp => lp.timestamp);
                        var first_sale_protocol_in_combined = order.sale_protocol_list_FK.MinBy(lp => lp.timestamp);
                        var first_lease_to_sale_protocol_in_combined = order.lease_to_sale_protocol_list_FK.MinBy(lp => lp.timestamp);

                        bool before_lease_combined = true;
                        bool before_sale_combined = true;
                        bool before_lease_to_sale_combined = true;

                        if (first_lease_protocol_in_combined != null)
                        {
                            if (!(input_obj.issue_date < first_lease_protocol_in_combined.timestamp || input_obj.sale_date < first_lease_protocol_in_combined.timestamp || input_obj.payment_date < first_lease_protocol_in_combined.timestamp))
                            {
                                before_lease_combined = false;
                            }
                        }

                        if (first_sale_protocol_in_combined != null)
                        {
                            if (!(input_obj.issue_date < first_sale_protocol_in_combined.timestamp || input_obj.sale_date < first_sale_protocol_in_combined.timestamp || input_obj.payment_date < first_sale_protocol_in_combined.timestamp))
                            {
                                before_sale_combined = false;
                            }
                        }

                        if (first_lease_to_sale_protocol_in_combined != null)
                        {
                            if (!(input_obj.issue_date < first_lease_to_sale_protocol_in_combined.timestamp || input_obj.sale_date < first_lease_to_sale_protocol_in_combined.timestamp || input_obj.payment_date < first_lease_to_sale_protocol_in_combined.timestamp))
                            {
                                before_lease_to_sale_combined = false;
                            }
                        }

                        if (before_lease_combined == before_sale_combined == before_lease_to_sale_combined == true)
                        {
                            throw new Exception("48");//invoice cannot be issued before first protocol date
                        }


                        List<Lease_Value_Model> lease_values_for_combined = Calculate_Lease(ref order, input_obj.year, input_obj.month);

                        List<Sale_Value_Model> sale_values_for_combined = Calculate_Sale(ref order, input_obj.year, input_obj.month);
                   

                        /////////////////////////////////////
                        ///Hardcoded "pl"
                        lease_for_month_name = Get_Lease_For_Month_Name(input_obj.month, "pl");


                        //Calculating lease values

                        foreach (var lease_value in lease_values_for_combined)
                        {
                            Invoice_Row new_row = new Invoice_Row
                            {
                                row_number = row_number,
                                name = lease_for_month_name,
                                total_quantity = 1,
                                counting_unit = "-",
                                tax_pct = lease_value.tax_pct
                            };

                            new_row.net_worth = lease_value.net_worth;


                            if (input_obj.use_discount)
                            {
                                new_row.use_discount = true;

                                if (input_obj.discount_is_in_pct)
                                {
                                    new_row.discount_is_in_pct = true;
                                    new_row.discount_value = input_obj.discount_value;

                                    new_row.net_worth_after_discount = lease_value.net_worth * ((100 - input_obj.discount_value) / 100);
                                }
                                else
                                {
                                    new_row.discount_is_in_pct = false;
                                    new_row.discount_value = input_obj.discount_value;

                                    if (input_obj.discount_value >= lease_value.net_worth)
                                    {
                                        throw new Exception("47");//discount too high
                                    }

                                    new_row.net_worth_after_discount = lease_value.net_worth - input_obj.discount_value;
                                }
                            }
                            else
                            {
                                new_row.use_discount = false;
                                new_row.discount_is_in_pct = false;
                                new_row.discount_value = 0;

                                new_row.net_worth_after_discount = lease_value.net_worth;
                            }


                            new_row.net_worth_total = new_row.net_worth_after_discount * new_row.total_quantity;

                            new_row.tax_worth = new_row.net_worth_total * (new_row.tax_pct / 100);

                            new_row.gross_worth = new_row.net_worth_total + new_row.tax_worth;


                            invoice_total_net_worth += new_row.net_worth_total;
                            invoice_total_tax_worth += new_row.tax_worth;

                            new_invoice_rows.Add(new_row);
                            row_number++;
                        }


                        //Calculating sale values

                        foreach (var sale_value in sale_values_for_combined)
                        {
                            Invoice_Row new_row = new Invoice_Row
                            {
                                row_number = row_number,
                                name = sale_value.sale_object_name,
                                total_quantity = sale_value.total_quantity,
                                counting_unit = sale_value.counting_unit,
                                tax_pct = sale_value.tax_pct
                            };

                            new_row.net_worth = sale_value.net_worth;


                            if (input_obj.use_discount)
                            {
                                new_row.use_discount = true;

                                if (input_obj.discount_is_in_pct)
                                {
                                    new_row.discount_is_in_pct = true;
                                    new_row.discount_value = input_obj.discount_value;

                                    new_row.net_worth_after_discount = sale_value.net_worth * ((100 - input_obj.discount_value) / 100);
                                }
                                else
                                {
                                    new_row.discount_is_in_pct = false;
                                    new_row.discount_value = input_obj.discount_value;

                                    if (input_obj.discount_value >= sale_value.net_worth)
                                    {
                                        throw new Exception("47");//discount too high
                                    }

                                    new_row.net_worth_after_discount = sale_value.net_worth - input_obj.discount_value;
                                }
                            }
                            else
                            {
                                new_row.use_discount = false;
                                new_row.discount_is_in_pct = false;
                                new_row.discount_value = 0;

                                new_row.net_worth_after_discount = sale_value.net_worth;
                            }


                            new_row.net_worth_total = new_row.net_worth_after_discount * new_row.total_quantity;

                            new_row.tax_worth = new_row.net_worth_total * (new_row.tax_pct / 100);

                            new_row.gross_worth = new_row.net_worth_total + new_row.tax_worth;


                            invoice_total_net_worth += new_row.net_worth_total;
                            invoice_total_tax_worth += new_row.tax_worth;

                            new_invoice_rows.Add(new_row);
                            row_number++;
                        }


                        break;

                    default:
                        throw new Exception("19");//not found

                }

                var comment = await Crypto.Encrypt(session, input_obj.comment);
                if(comment == null)
                {
                    throw new Exception("2");//encryption error
                }

                new_invoice = new Invoice
                {
                    invoice_type = input_obj.invoice_type,
                    order_FKid = order.id,
                    year = input_obj.year,
                    month = input_obj.month,
                    number = Get_Invoice_Number(input_obj.invoice_type, input_obj.year, input_obj.month),
                    issue_date = input_obj.issue_date,
                    sale_date = input_obj.sale_date,
                    payment_date = input_obj.payment_date,
                    net_worth = invoice_total_net_worth,
                    tax_worth = invoice_total_tax_worth,
                    gross_worth = invoice_total_net_worth + invoice_total_tax_worth,
                    payment_method = input_obj.payment_method,
                    comment = comment,

                    invoice_row_list_FK = new_invoice_rows
                };

                new_invoice.full_number = Get_Full_Invoice_Number(input_obj.invoice_type, input_obj.year, input_obj.month, new_invoice.number);

                string gross_worth_string = Crypto.Round(new_invoice.gross_worth, 2).ToString(CultureInfo.InvariantCulture);
                string[] parts = gross_worth_string.Split(".");
                //HARDCODED pl
                CultureInfo language = new CultureInfo("pl");

                new_invoice.gross_worth_in_words =
                        Convert.ToInt64(parts[0], CultureInfo.InvariantCulture).ToWords(language) + " " +
                        invoice_settings.currency_FK.currency_symbol;

                if (parts.Length > 1)
                {
                    new_invoice.gross_worth_in_words += " " +
                        Convert.ToInt64(parts[1], CultureInfo.InvariantCulture).ToWords(language) + " " +
                        invoice_settings.currency_FK.currency_hundreth_symbol;
                }


                _context.Invoice.Add(new_invoice);

                _context.SaveChanges();

                return new Invoice_Id_Model { invoice_id = new_invoice.id };
            }
        }


        public async Task<Invoice_Id_Model> Edit_Invoice(Edit_Invoice_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var invoice_exists = _context.Invoice.Where(i =>
                    i.id.Equals(input_obj.id)
                ).FirstOrDefault();

                if (invoice_exists == null)
                {
                    throw new Exception("19");//not found
                }

                var comment = await Crypto.Encrypt(session, input_obj.comment);
                if (comment == null)
                {
                    throw new Exception("2");//encryption error
                }

                invoice_exists.comment = comment;
                invoice_exists.payment_method = input_obj.payment_method;

                _context.SaveChanges();

                return new Invoice_Id_Model { invoice_id = invoice_exists.id };

            }
        }


        private List<Lease_Value_Model> Calculate_Lease(ref Order order, int year, int month)
        {
            //include items out of storage -> items in storage

            List<DateTime> days_in_month = AllDaysInMonth(year, month);

            DateTime first_timestamp_in_month = new DateTime(year, month, 1, 0, 0, 0, 0, 0);

            List<Lease_Item_Calculation_Model> calculation_list = new List<Lease_Item_Calculation_Model>();

            List<Lease_Value_Model> return_list = new List<Lease_Value_Model>();


            Lease_Item_Out_Of_Storage_History? out_of_storage_state;

            Lease_Item_Calculation_Model? already_calculated;

            //adding items that are already on construction site
            foreach (var item_out_of_storage in order.lease_item_out_of_storage_list_FK)
            {
                if(
                    item_out_of_storage == null ||
                    item_out_of_storage.lease_item_out_of_storage_history_FK == null
                )
                {
                    throw new Exception("19");//not found
                }

                out_of_storage_state = item_out_of_storage.lease_item_out_of_storage_history_FK.Where(h => h.timestamp < first_timestamp_in_month).MaxBy(h => h.timestamp);

                if(out_of_storage_state != null)
                {
                    already_calculated = calculation_list.Where(a => a.lease_item_id.Equals(item_out_of_storage.lease_item_in_storage_FK.lease_item_FKid)).FirstOrDefault();

                    if(already_calculated == null)
                    {
                        calculation_list.Add(
                            new Lease_Item_Calculation_Model
                            {
                                lease_item_id = item_out_of_storage.lease_item_in_storage_FK.lease_item_FKid,
                                total_quantity = out_of_storage_state.total_quantity,
                                days_on_construction_site = 0,
                                overwritten = false
                            }
                        );
                    }
                    else
                    {
                        already_calculated.total_quantity += out_of_storage_state.total_quantity;
                    }

                }


            }



            foreach(var day in days_in_month)
            {
                //protocol list already filtered in caller method
                foreach(var lease_protocol in order.lease_protocol_list_FK.Where(lp => lp.timestamp.Day.Equals(day.Day)))
                {

                    if(
                        lease_protocol == null || 
                        lease_protocol.lease_item_on_protocol_list_FK == null
                    )
                    {
                        throw new Exception("19");// not found
                    }

                    if (lease_protocol.type.Equals(Lease_Protocol_Type.Release))
                    {

                        foreach(var item_on_protocol in lease_protocol.lease_item_on_protocol_list_FK)
                        {
                            if(item_on_protocol == null || item_on_protocol.lease_item_in_storage_FK == null)
                            {
                                throw new Exception("19");// not found
                            }

                            already_calculated = calculation_list.Where(c => 
                                c.lease_item_id.Equals(item_on_protocol.lease_item_in_storage_FK.lease_item_FKid) && 
                                c.overwritten.Equals(false)
                            ).FirstOrDefault();

                            if(already_calculated == null)
                            {
                                calculation_list.Add(
                                    new Lease_Item_Calculation_Model
                                    {
                                        lease_item_id = item_on_protocol.lease_item_in_storage_FK.lease_item_FKid,
                                        total_quantity = item_on_protocol.total_quantity,
                                        days_on_construction_site = 0,
                                        overwritten = false
                                    }
                                );
                            }
                            else
                            {
                                calculation_list.Add(
                                    new Lease_Item_Calculation_Model
                                    {
                                        lease_item_id = item_on_protocol.lease_item_in_storage_FK.lease_item_FKid,
                                        total_quantity = already_calculated.total_quantity + item_on_protocol.total_quantity,
                                        days_on_construction_site = 0,
                                        overwritten = false
                                    }
                                );

                                already_calculated.overwritten = true;
                            }


                        }

                    }
                    else if (lease_protocol.type.Equals(Lease_Protocol_Type.Return))
                    {

                        foreach (var item_on_protocol in lease_protocol.lease_item_on_protocol_list_FK)
                        {
                            if (item_on_protocol == null || item_on_protocol.lease_item_in_storage_FK == null)
                            {
                                throw new Exception("19");// not found
                            }

                            already_calculated = calculation_list.Where(c =>
                                c.lease_item_id.Equals(item_on_protocol.lease_item_in_storage_FK.lease_item_FKid) &&
                                c.overwritten.Equals(false)
                            ).FirstOrDefault();

                            //return protocol affects lease calculation only from the day after current,
                            //so previous state (already_calculated) is valid one day more
                            //and
                            //new state has days_on_construction_site = -1 to start calculation from the next day
                            if (already_calculated == null)
                            {
                                calculation_list.Add(
                                    new Lease_Item_Calculation_Model
                                    {
                                        lease_item_id = item_on_protocol.lease_item_in_storage_FK.lease_item_FKid,
                                        total_quantity = item_on_protocol.total_quantity * -1,
                                        days_on_construction_site = -1,
                                        overwritten = false
                                    }
                                );
                            }
                            else
                            {
                                calculation_list.Add(
                                    new Lease_Item_Calculation_Model
                                    {
                                        lease_item_id = item_on_protocol.lease_item_in_storage_FK.lease_item_FKid,
                                        total_quantity = already_calculated.total_quantity - item_on_protocol.total_quantity,
                                        days_on_construction_site = -1,
                                        overwritten = false
                                    }
                                );

                                already_calculated.days_on_construction_site += 1;
                                already_calculated.overwritten = true;
                            }


                        }

                    }
                    else
                    {
                        throw new Exception("36");// miscallculation error
                    }


                }


                foreach(var calculated_item in calculation_list)
                {
                    if (calculated_item.overwritten.Equals(false))
                    {
                        calculated_item.days_on_construction_site += 1;
                    }
                }


            }



            Lease_Value_Model? tax_pct_exists;
            Lease_Item? lease_item_refernece;

            decimal net_worth;
            decimal tax_worth;

            foreach(var calculated_lease_item in calculation_list)
            {
                lease_item_refernece = _context.Lease_Item.Where(li => li.id.Equals(calculated_lease_item.lease_item_id)).Include(li => li.lease_group_FK).FirstOrDefault();

                if(lease_item_refernece == null || lease_item_refernece.lease_group_FK == null)
                {
                    throw new Exception("19");// not found
                }

                tax_pct_exists = return_list.Where(r => r.tax_pct.Equals(lease_item_refernece.lease_group_FK.tax_pct)).FirstOrDefault();


                if (order.use_static_rate)
                {
                    net_worth = ((calculated_lease_item.total_quantity * lease_item_refernece.price * order.static_rate) / 30) * calculated_lease_item.days_on_construction_site;
                }
                else
                {
                    net_worth = ((calculated_lease_item.total_quantity * lease_item_refernece.price * lease_item_refernece.lease_group_FK.rate) / 30) * calculated_lease_item.days_on_construction_site;
                }

                tax_worth = net_worth * (lease_item_refernece.lease_group_FK.tax_pct / 100);

                if (tax_pct_exists == null)
                {
                    return_list.Add(
                        new Lease_Value_Model
                        {
                            tax_pct = lease_item_refernece.lease_group_FK.tax_pct,
                            net_worth = net_worth,
                            tax_worth = tax_worth,
                            gross_worth = net_worth + tax_worth
                        }
                    );
                }
                else
                {
                    tax_pct_exists.net_worth += net_worth;
                    tax_pct_exists.tax_worth += tax_worth;
                    tax_pct_exists.gross_worth += net_worth + tax_worth;
                }

            }


            return return_list;
        }


        private List<Sale_Value_Model> Calculate_Sale(ref Order order, int year, int month)
        {

            //include items out of storage -> items in storage

            List<DateTime> days_in_month = AllDaysInMonth(year, month);

            DateTime first_timestamp_in_month = new DateTime(year, month, 1, 0, 0, 0, 0, 0);

            List<Sale_Item_Calculation_Model> sale_item_calculation_list = new List<Sale_Item_Calculation_Model>();
            List<Lease_To_Sale_Item_Calculation_Model> lease_to_sale_item_calculation_list = new List<Lease_To_Sale_Item_Calculation_Model>();
            List<Service_Calculation_Model> service_calculation_list = new List<Service_Calculation_Model>();

            List<Sale_Value_Model> return_list = new List<Sale_Value_Model>();


            Sale_Item_Calculation_Model? sale_item_already_calculated;
            Lease_To_Sale_Item_Calculation_Model? lease_to_sale_item_already_calculated;
            Service_Calculation_Model? service_already_calculated;

            
            foreach (var day in days_in_month)
            {
                //protocol list already filtered in caller method
                foreach (var lease_protocol in order.lease_protocol_list_FK.Where(lp => lp.timestamp.Day.Equals(day.Day)))
                {

                    if (
                        lease_protocol == null ||
                        lease_protocol.service_on_lease_protocol_list_FK == null
                    )
                    {
                        throw new Exception("19");// not found
                    }


                    foreach (var service_on_protocol in lease_protocol.service_on_lease_protocol_list_FK)
                    {
                        if (service_on_protocol == null)
                        {
                            throw new Exception("19");// not found
                        }

                        service_already_calculated = service_calculation_list.Where(s =>
                            s.service_id.Equals(service_on_protocol.service_FKid)
                        ).FirstOrDefault();

                        if (service_already_calculated == null)
                        {
                            service_calculation_list.Add(
                                new Service_Calculation_Model
                                {
                                    service_id = service_on_protocol.service_FKid,
                                    total_worth = service_on_protocol.net_worth
                                }
                            );
                        }
                        else
                        {
                            service_already_calculated.total_worth += service_on_protocol.net_worth;
                        }

                    }



                }

                //protocol list already filtered in caller method
                foreach (var sale_protocol in order.sale_protocol_list_FK.Where(lp => lp.timestamp.Day.Equals(day.Day)))
                {

                    if (
                        sale_protocol == null ||
                        sale_protocol.service_on_sale_protocol_list_FK == null ||
                        sale_protocol.sale_item_on_protocol_list_FK == null
                    )
                    {
                        throw new Exception("19");// not found
                    }


                    foreach (var service_on_protocol in sale_protocol.service_on_sale_protocol_list_FK)
                    {
                        if (service_on_protocol == null)
                        {
                            throw new Exception("19");// not found
                        }

                        service_already_calculated = service_calculation_list.Where(s =>
                            s.service_id.Equals(service_on_protocol.service_FKid)
                        ).FirstOrDefault();

                        if (service_already_calculated == null)
                        {
                            service_calculation_list.Add(
                                new Service_Calculation_Model
                                {
                                    service_id = service_on_protocol.service_FKid,
                                    total_worth = service_on_protocol.net_worth
                                }
                            );
                        }
                        else
                        {
                            service_already_calculated.total_worth += service_on_protocol.net_worth;
                        }

                    }

                    foreach(var sale_item_on_protocol in sale_protocol.sale_item_on_protocol_list_FK)
                    {
                        if (sale_item_on_protocol == null || sale_item_on_protocol.sale_item_in_storage_FK == null)
                        {
                            throw new Exception("19");// not found
                        }

                        sale_item_already_calculated = sale_item_calculation_list.Where(s =>
                            s.sale_item_id.Equals(sale_item_on_protocol.sale_item_in_storage_FK.sale_item_FKid)
                        ).FirstOrDefault();

                        if (sale_item_already_calculated == null)
                        {
                            sale_item_calculation_list.Add(
                                new Sale_Item_Calculation_Model
                                {
                                    sale_item_id = sale_item_on_protocol.sale_item_in_storage_FK.sale_item_FKid,
                                    total_quantity = sale_item_on_protocol.total_quantity
                                }
                            );
                        }
                        else
                        {
                            sale_item_already_calculated.total_quantity += sale_item_on_protocol.total_quantity;
                        }
                    }

                }


                foreach(var lease_to_sale_protocol in order.lease_to_sale_protocol_list_FK.Where(lp => lp.timestamp.Day.Equals(day.Day)))
                {
                    if (
                        lease_to_sale_protocol == null ||
                        lease_to_sale_protocol.lease_item_on_lease_to_sale_protocol_list_FK == null
                    )
                    {
                        throw new Exception("19");// not found
                    }


                    foreach (var lease_to_sale_item_on_protocol in lease_to_sale_protocol.lease_item_on_lease_to_sale_protocol_list_FK)
                    {
                        if (lease_to_sale_item_on_protocol == null || lease_to_sale_item_on_protocol.lease_item_in_storage_FK == null)
                        {
                            throw new Exception("19");// not found
                        }

                        lease_to_sale_item_already_calculated = lease_to_sale_item_calculation_list.Where(s =>
                            s.lease_item_id.Equals(lease_to_sale_item_on_protocol.lease_item_in_storage_FK.lease_item_FKid)
                        ).FirstOrDefault();

                        if (lease_to_sale_item_already_calculated == null)
                        {
                            lease_to_sale_item_calculation_list.Add(
                                new Lease_To_Sale_Item_Calculation_Model
                                {
                                    lease_item_id = lease_to_sale_item_on_protocol.lease_item_in_storage_FK.lease_item_FKid,
                                    total_quantity = lease_to_sale_item_on_protocol.total_quantity
                                }
                            );
                        }
                        else
                        {
                            lease_to_sale_item_already_calculated.total_quantity += lease_to_sale_item_on_protocol.total_quantity;
                        }
                    }
                }


            }


            Sale_Item? sale_item_reference;
            Service? service_reference;
            Lease_Item? lease_item_reference;

            decimal net_worth;
            decimal tax_worth;


            foreach (var calculated_sale_item in sale_item_calculation_list)
            {
                sale_item_reference = _context.Sale_Item.Where(si => 
                    si.id.Equals(calculated_sale_item.sale_item_id)
                )
                .Include(li => li.sale_group_FK)
                .Include(li => li.counting_unit_FK)
                .FirstOrDefault();

                if (
                    sale_item_reference == null || 
                    sale_item_reference.sale_group_FK == null ||
                    sale_item_reference.counting_unit_FK == null
                )
                {
                    throw new Exception("19");// not found
                }

                net_worth = sale_item_reference.price;
                tax_worth = net_worth * (sale_item_reference.sale_group_FK.tax_pct / 100);


                return_list.Add(
                    new Sale_Value_Model
                    {
                        tax_pct = sale_item_reference.sale_group_FK.tax_pct,
                        net_worth = net_worth,
                        tax_worth = tax_worth,
                        gross_worth = net_worth + tax_worth,
                        total_quantity = calculated_sale_item.total_quantity,
                        counting_unit = sale_item_reference.counting_unit_FK.unit,
                        sale_object_name = sale_item_reference.catalog_number + " " + sale_item_reference.product_name
                    }
                );
            }


            foreach (var calculated_lease_to_sale_item in lease_to_sale_item_calculation_list)
            {
                lease_item_reference = _context.Lease_Item.Where(li => 
                    li.id.Equals(calculated_lease_to_sale_item.lease_item_id)
                )
                .Include(li => li.lease_group_FK)
                .Include(li => li.counting_unit_FK)
                .FirstOrDefault();

                if (
                    lease_item_reference == null || 
                    lease_item_reference.lease_group_FK == null || 
                    lease_item_reference.counting_unit_FK == null
                )
                {
                    throw new Exception("19");// not found
                }

                net_worth = lease_item_reference.price;
                tax_worth = net_worth * (lease_item_reference.lease_group_FK.tax_pct / 100);


                return_list.Add(
                    new Sale_Value_Model
                    {
                        tax_pct = lease_item_reference.lease_group_FK.tax_pct,
                        net_worth = net_worth,
                        tax_worth = tax_worth,
                        gross_worth = net_worth + tax_worth,
                        total_quantity = calculated_lease_to_sale_item.total_quantity,
                        counting_unit = lease_item_reference.counting_unit_FK.unit,
                        sale_object_name = lease_item_reference.catalog_number + " " + lease_item_reference.product_name
                    }
                );
            }



            foreach (var calculated_service in service_calculation_list)
            {
                service_reference = _context.Service.Where(s => s.id.Equals(calculated_service.service_id)).Include(s => s.service_group_FK).FirstOrDefault();

                if (service_reference == null || service_reference.service_group_FK == null)
                {
                    throw new Exception("19");// not found
                }

                net_worth = calculated_service.total_worth;
                tax_worth = net_worth * (service_reference.service_group_FK.tax_pct / 100);

                 
                return_list.Add(
                    new Sale_Value_Model
                    {
                        tax_pct = service_reference.service_group_FK.tax_pct,
                        net_worth = net_worth,
                        tax_worth = tax_worth,
                        gross_worth = net_worth + tax_worth,
                        total_quantity = 1,
                        sale_object_name = service_reference.service_number + " " + service_reference.service_name
                    }
                );

            }


            return return_list;

        }


        /*
         * Delete_Invoice method
         * This method is used to a record from the Invoice table.
         *  
         * It accepts Delete_Invoice_Data object as input.
         * Then it deletes a record if its ID matches the one given in the input object.
         */
        public string Delete_Invoice(Delete_Invoice_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var invoice_exits = _context.Invoice.Where(i => i.id.Equals(input_obj.invoice_id)).FirstOrDefault();

                if (invoice_exits == null)
                {
                    throw new Exception("19");// invoice not found in db
                }

                _context.Invoice.Remove(invoice_exits);
                _context.SaveChanges();

                return Info.SUCCESSFULLY_DELETED;
            }
        }


        public Invoice_Row_Id_Model Edit_Invoice_Row(Edit_Invoice_Row_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var invoice_row_exits = _context.Invoice_Row.Where(i => i.id.Equals(input_obj.id)).Include(i => i.invoice_FK).FirstOrDefault();
                if (invoice_row_exits == null || invoice_row_exits.invoice_FK == null)
                {
                    throw new Exception("19");// invoice row not found in db
                }

                var invoice_settings = _context.Company_Invoice_Settings.Include(i => i.currency_FK).FirstOrDefault();
                if (invoice_settings == null || invoice_settings.currency_FK == null)
                {
                    throw new Exception("51");//settings not found
                }


                invoice_row_exits.name = input_obj.name;
                invoice_row_exits.counting_unit = input_obj.counting_unit;

                invoice_row_exits.invoice_FK.net_worth -= invoice_row_exits.net_worth_total;
                invoice_row_exits.invoice_FK.tax_worth -= invoice_row_exits.tax_worth;
                invoice_row_exits.invoice_FK.gross_worth -= invoice_row_exits.gross_worth;


                if (input_obj.use_discount)
                {
                    invoice_row_exits.use_discount = true;

                    if (input_obj.discount_is_in_pct)
                    {
                        invoice_row_exits.discount_is_in_pct = true;
                        invoice_row_exits.discount_value = input_obj.discount_value;

                        invoice_row_exits.net_worth_after_discount = invoice_row_exits.net_worth * ((100 - input_obj.discount_value) / 100);
                    }
                    else
                    {
                        invoice_row_exits.discount_is_in_pct = false;
                        invoice_row_exits.discount_value = input_obj.discount_value;

                        if (input_obj.discount_value >= invoice_row_exits.net_worth)
                        {
                            throw new Exception("47");//discount too high
                        }

                        invoice_row_exits.net_worth_after_discount = invoice_row_exits.net_worth - input_obj.discount_value;
                    }
                }
                else
                {
                    invoice_row_exits.use_discount = false;
                    invoice_row_exits.discount_is_in_pct = false;
                    invoice_row_exits.discount_value = 0;

                    invoice_row_exits.net_worth_after_discount = invoice_row_exits.net_worth;
                }

                invoice_row_exits.net_worth_total = invoice_row_exits.net_worth_after_discount * invoice_row_exits.total_quantity;

                invoice_row_exits.tax_worth = invoice_row_exits.net_worth_total * (invoice_row_exits.tax_pct / 100);

                invoice_row_exits.gross_worth = invoice_row_exits.net_worth_total + invoice_row_exits.tax_worth;


                invoice_row_exits.invoice_FK.net_worth += invoice_row_exits.net_worth_total;
                invoice_row_exits.invoice_FK.tax_worth += invoice_row_exits.tax_worth;
                invoice_row_exits.invoice_FK.gross_worth += invoice_row_exits.gross_worth;


                string gross_worth_string = Crypto.Round(invoice_row_exits.invoice_FK.gross_worth, 2).ToString(CultureInfo.InvariantCulture);
                string[] parts = gross_worth_string.Split(".");
                //HARDCODED pl
                CultureInfo language = new CultureInfo("pl");

                invoice_row_exits.invoice_FK.gross_worth_in_words =
                        Convert.ToInt64(parts[0], CultureInfo.InvariantCulture).ToWords(language) + " " +
                        invoice_settings.currency_FK.currency_symbol;

                if (parts.Length > 1)
                {
                    invoice_row_exits.invoice_FK.gross_worth_in_words += " " +
                        Convert.ToInt64(parts[1], CultureInfo.InvariantCulture).ToWords(language) + " " +
                        invoice_settings.currency_FK.currency_hundreth_symbol;
                }


                _context.SaveChanges();

                return new Invoice_Row_Id_Model { invoice_row_id = invoice_row_exits.id };
            }
        }





        /*
         * Get_Invoice_By_Id method
         * This method gets a record from the Invoice table by its ID and returns it.
         * 
         * It accepts Get_Invoice_By_Id_Data object as input.
         * Then it gets a records that has the same ID as the ID given in the input object
         */
        public async Task<Invoice_Model> Get_Invoice_By_Id(Get_Invoice_By_Id_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var selected_invoice = _context.Invoice.Where(i => 
                    i.id.Equals(input_obj.id_to_get)
                )
                .Include(i => i.invoice_row_list_FK)
                .Include(i => i.order_FK)
                    .ThenInclude(o => o.client_FK)
                .Include(i => i.order_FK)
                    .ThenInclude(o => o.construction_site_FK)
                .FirstOrDefault();

                if (
                    selected_invoice == null || 
                    selected_invoice.order_FK == null || 
                    selected_invoice.order_FK.client_FK == null || 
                    selected_invoice.order_FK.construction_site_FK == null || 
                    selected_invoice.invoice_row_list_FK == null
                )
                {
                    throw new Exception("19"); //Invoice not found in DB
                }


                List<Encrypted_Object> encrypted_fields = [
                    new Encrypted_Object { id = 1, encryptedValue = selected_invoice.comment },
                    new Encrypted_Object { id = 2, encryptedValue = selected_invoice.order_FK.order_name },
                    new Encrypted_Object { id = 3, encryptedValue = selected_invoice.order_FK.comment },
                    new Encrypted_Object { id = 4, encryptedValue = selected_invoice.order_FK.client_FK.name },
                    new Encrypted_Object { id = 5, encryptedValue = selected_invoice.order_FK.client_FK.surname },

                    new Encrypted_Object { id = 7, encryptedValue = selected_invoice.order_FK.construction_site_FK.construction_site_name }
                ];

                if (!selected_invoice.order_FK.client_FK.is_private_person)
                {
                    encrypted_fields.Add(new Encrypted_Object { id = 6, encryptedValue = selected_invoice.order_FK.client_FK.company_name });
                }


                Invoice_Model return_obj = new Invoice_Model
                {
                    id = selected_invoice.id,
                    invoice_type = selected_invoice.invoice_type,
                    issue_date = selected_invoice.issue_date,
                    sale_date = selected_invoice.sale_date,
                    payment_date = selected_invoice.payment_date,
                    payment_method = selected_invoice.payment_method,
                    year = selected_invoice.year,
                    month = selected_invoice.month,
                    full_number = selected_invoice.full_number,
                    order_id = selected_invoice.order_FKid,
                    net_worth = selected_invoice.net_worth,
                    tax_worth = selected_invoice.tax_worth,
                    gross_worth = selected_invoice.gross_worth,
                    gross_worth_in_words = selected_invoice.gross_worth_in_words,
                    order = new Order_Model_List
                    {
                        id = selected_invoice.order_FKid,
                        order_number = selected_invoice.order_FK.order_number,
                        state = selected_invoice.order_FK.state,
                        default_payment_method = selected_invoice.order_FK.default_payment_method,
                        default_payment_date_offset = selected_invoice.order_FK.default_payment_date_offset,
                        default_discount = selected_invoice.order_FK.default_discount,
                        use_static_rate = selected_invoice.order_FK.use_static_rate,
                        static_rate = selected_invoice.order_FK.static_rate,
                        timestamp = selected_invoice.order_FK.timestamp,

                        client_id = selected_invoice.order_FK.client_FKid,
                        client_number = selected_invoice.order_FK.client_FK.number,
                        client_is_private_person = selected_invoice.order_FK.client_FK.is_private_person,
                        client_company_name = "-",

                        construction_site_id = selected_invoice.order_FK.construction_site_FKid,
                        construction_site_number = selected_invoice.order_FK.construction_site_FK.number
                    },
                    invoice_row_list = []
                };

                var decrypted_fields = await Crypto.DecryptList(session, encrypted_fields);

                if (decrypted_fields == null || decrypted_fields.Count != encrypted_fields.Count)
                {
                    throw new Exception("3");
                }

                foreach(var field in decrypted_fields)
                {
                    if(field == null)
                    {
                        throw new Exception("3");//decryption error
                    }

                    switch (field.id)
                    {
                        case 1:
                            return_obj.comment = field.decryptedValue;
                            break;

                        case 2:
                            return_obj.order.order_name = field.decryptedValue;
                            break;

                        case 3:
                            return_obj.order.comment = field.decryptedValue;
                            break;

                        case 4:
                            return_obj.order.client_name = field.decryptedValue;
                            break;

                        case 5:
                            return_obj.order.client_surname = field.decryptedValue;
                            break;

                        case 6:
                            return_obj.order.client_company_name = field.decryptedValue;
                            break;

                        case 7:
                            return_obj.order.construction_site_name = field.decryptedValue;
                            break;

                        default:
                            throw new Exception("3");
                    }

                }

                var settings = _context.Company_Invoice_Settings.FirstOrDefault();
                if(settings == null)
                {
                    throw new Exception("51");//settings not found
                }

                foreach(var invoice_row in selected_invoice.invoice_row_list_FK)
                {
                    if(invoice_row == null)
                    {
                        throw new Exception("19");//not found
                    }

                    return_obj.invoice_row_list.Add(
                        new Invoice_Row_Model
                        {
                            id = invoice_row.id,
                            row_number = invoice_row.row_number,
                            name = invoice_row.name,
                            total_quantity = invoice_row.total_quantity,
                            counting_unit = invoice_row.counting_unit,
                            use_discount = invoice_row.use_discount,
                            discount_is_in_pct = invoice_row.discount_is_in_pct,
                            discount_value = invoice_row.discount_value,
                            net_worth = Crypto.Round(invoice_row.net_worth, settings.decimal_digits),
                            net_worth_after_discount = Crypto.Round(invoice_row.net_worth_after_discount, settings.decimal_digits),
                            net_worth_total = Crypto.Round(invoice_row.net_worth_total, settings.decimal_digits),
                            tax_pct = invoice_row.tax_pct,
                            tax_worth = Crypto.Round(invoice_row.tax_worth, settings.decimal_digits),
                            gross_worth = Crypto.Round(invoice_row.gross_worth, settings.decimal_digits)
                        }
                    );
                }

                return return_obj;

            }
        }



        public async Task<Order_With_Invoice_List_Model> Get_Invoice_List(Get_Invoice_List_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var selected_order = _context.Order.Where(o => 
                    o.id.Equals(input_obj.order_id)
                )
                .Include(o => o.invoice_list_FK)
                .FirstOrDefault();

                if(selected_order == null || selected_order.invoice_list_FK == null)
                {
                    throw new Exception("19");//not found
                }

                var order_name = await Crypto.Decrypt(session, selected_order.order_name);
                if(order_name == null)
                {
                    throw new Exception("3");//decryption error
                }

                Order_With_Invoice_List_Model return_obj = new Order_With_Invoice_List_Model
                {
                    id = selected_order.id,
                    order_number = selected_order.order_number,
                    order_name = order_name,
                    state = selected_order.state,
                    timestamp = selected_order.timestamp,
                    invoice_list = []
                };

                foreach(var invoice in selected_order.invoice_list_FK)
                {
                    if(invoice == null)
                    {
                        throw new Exception("19");//not found
                    }

                    return_obj.invoice_list.Add(
                        new Invoice_List_Model
                        {
                            id = invoice.id,
                            order_id = invoice.order_FKid,
                            year = invoice.year,
                            month = invoice.month,
                            full_number = invoice.full_number,
                            invoice_type = invoice.invoice_type,
                            issue_date = invoice.issue_date,
                            sale_date = invoice.sale_date,
                            payment_date = invoice.payment_date,
                            payment_method = invoice.payment_method,
                            is_printed = invoice.invoice_printed_data_FKid != null,
                            net_worth = invoice.net_worth,
                            tax_worth = invoice.tax_worth,
                            gross_worth = invoice.gross_worth
                        }
                    );

                }


                return return_obj;

            }
        }



        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++\\
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++\\
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++\\
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++\\
        /*
         * Invoice_Issuer
         */
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++\\
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++\\
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++\\
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++\\
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++\\


        public async Task<Invoice_Print_Model> Print_Invoice(Print_Invoice_Data input_obj, Session_Data session)
        {
            if(input_obj == null || session == null)
            {
                throw new Exception("14");
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                if (!input_obj.print_new)
                {
                    var invoice = _context.Invoice.Where(i =>
                        i.id.Equals(input_obj.invoice_id)
                    )
                    .Include(i => i.invoice_printed_data_FK)
                    .FirstOrDefault();

                    if (invoice == null)
                    {
                        throw new Exception("19");
                    }

                    if (invoice.invoice_printed_data_FK != null)
                    {

                        var binary_data = _context.Invoice_Binary_Data.Where(ib => ib.invoice_printed_data_FKid.Equals(invoice.invoice_printed_data_FKid)).FirstOrDefault();

                        if (binary_data == null)
                        {
                            throw new Exception("19"); // binary data should exist if printed data exists
                        }

                        byte[] file = await Crypto.DecryptByte(session, binary_data.invoice_bytes);
                        if (file == null)
                        {
                            throw new Exception("3");//decryption
                        }

                        return new Invoice_Print_Model { invoice_bytes = file, invoice_file_name = invoice.invoice_printed_data_FK.invoice_file_name };

                    }
                }



                var selected_invoice = _context.Invoice.Where(i => 
                    i.id.Equals(input_obj.invoice_id)
                )
                .Include(i => i.invoice_row_list_FK)
                .Include(i => i.order_FK)
                    .ThenInclude(o => o.client_FK)
                .FirstOrDefault();

                if(
                    selected_invoice == null ||
                    selected_invoice.invoice_row_list_FK == null ||
                    selected_invoice.order_FK == null ||
                    selected_invoice.order_FK.client_FK == null
                )
                {
                    throw new Exception("19");//not found
                }


                var settings = _context.Company_Invoice_Settings.Include(s => s.currency_FK).FirstOrDefault();
                if (settings == null || settings.currency_FK == null)
                {
                    throw new Exception("51");//settings not found
                }


                var company_data = _context.Company_Info.FirstOrDefault();
                if (company_data == null)
                {
                    throw new Exception("19");//not found
                }

                Company_Info_Model company_info = new Company_Info_Model();

                Client_Model_List client_info = new Client_Model_List
                {
                    id = selected_invoice.order_FK.client_FKid,
                    number = selected_invoice.order_FK.client_FK.number,
                    is_private_person = selected_invoice.order_FK.client_FK.is_private_person
                };

                string comment = "";

                Task get_company_info = Task.Run(async () =>
                {
                    List<Encrypted_Object> encrypted_fields =
                    [
                        new Encrypted_Object { id = 1, encryptedValue = company_data.name },
                        new Encrypted_Object { id = 2, encryptedValue = company_data.surname },
                        new Encrypted_Object { id = 3, encryptedValue = company_data.company_name },
                        new Encrypted_Object { id = 4, encryptedValue = company_data.nip },
                        new Encrypted_Object { id = 5, encryptedValue = company_data.phone_number },
                        new Encrypted_Object { id = 6, encryptedValue = company_data.email },
                        new Encrypted_Object { id = 7, encryptedValue = company_data.address },
                        new Encrypted_Object { id = 8, encryptedValue = company_data.bank_name },
                        new Encrypted_Object { id = 9, encryptedValue = company_data.bank_number },
                        new Encrypted_Object { id = 10, encryptedValue = company_data.web_page },

                        new Encrypted_Object { id = 11, encryptedValue = selected_invoice.comment }
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
                                    company_info.name = field.decryptedValue; break;

                                case 2:
                                    company_info.surname = field.decryptedValue; break;

                                case 3:
                                    company_info.company_name = field.decryptedValue; break;

                                case 4:
                                    company_info.nip = field.decryptedValue; break;

                                case 5:
                                    company_info.phone_number = field.decryptedValue; break;

                                case 6:
                                    company_info.email = field.decryptedValue; break;

                                case 7:
                                    company_info.address = field.decryptedValue; break;

                                case 8:
                                    company_info.bank_name = field.decryptedValue; break;

                                case 9:
                                    company_info.bank_number = field.decryptedValue; break;

                                case 10:
                                    company_info.web_page = field.decryptedValue; break;

                                case 11:
                                    comment = field.decryptedValue; break;

                                default:
                                    throw new Exception("3");//error while decrypting data 
                            }
                        }
                    }

                });


                Task get_client_info = Task.Run(async () =>
                {

                    List<Encrypted_Object> encrypted_items = [
                        new Encrypted_Object { id = 1, encryptedValue = selected_invoice.order_FK.client_FK.surname },
                        new Encrypted_Object { id = 2, encryptedValue = selected_invoice.order_FK.client_FK.name },
                        new Encrypted_Object { id = 5, encryptedValue = selected_invoice.order_FK.client_FK.phone_number },
                        new Encrypted_Object { id = 6, encryptedValue = selected_invoice.order_FK.client_FK.email },
                        new Encrypted_Object { id = 7, encryptedValue = selected_invoice.order_FK.client_FK.address },
                        new Encrypted_Object { id = 8, encryptedValue = selected_invoice.order_FK.client_FK.comment }
                    ];

                    if (!selected_invoice.order_FK.client_FK.is_private_person)
                    {
                        encrypted_items.Add(new Encrypted_Object { id = 3, encryptedValue = selected_invoice.order_FK.client_FK.company_name });
                        encrypted_items.Add(new Encrypted_Object { id = 4, encryptedValue = selected_invoice.order_FK.client_FK.nip });
                    }

                    List<Decrypted_Object> decrypted_items = await Crypto.DecryptList(session, encrypted_items);


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

                            case 4:
                                client_info.nip = field.decryptedValue;
                                break;

                            case 5:
                                client_info.phone_number = field.decryptedValue;
                                break;

                            case 6:
                                client_info.email = field.decryptedValue;
                                break;

                            case 7:
                                client_info.address = field.decryptedValue;
                                break;

                            case 8:
                                client_info.comment = field.decryptedValue;
                                break;

                            default:
                                throw new Exception("3");

                        }

                    }

                    if (selected_invoice.order_FK.client_FK.is_private_person)
                    {
                        client_info.company_name = "";
                        client_info.nip = "";
                    }

                });


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


                //string invoice_PDF_file;


                //hardcoded pl
                Invoice_Language_Model language = File_Provider.Get_Invoice_Language_Data("pl");

                string invoice_body = File_Provider.Get_Invoice_Body_HTML();
                string invoice_header = File_Provider.Get_Invoice_Header_HTML();
                string invoice_company = File_Provider.Get_Invoice_Company_HTML();
                string invoice_client = File_Provider.Get_Invoice_Client_HTML();
                string invoice_main_table = File_Provider.Get_Invoice_Main_Table_HTML();
                string invoice_footer = File_Provider.Get_Invoice_Footer_HTML();



                invoice_header = string.Format(invoice_header, 
                [
                    language.invoice,
                    selected_invoice.full_number,
                    language.issue_date,
                    language.sale_date,
                    language.payment_date,
                    language.payment_method,
                    selected_invoice.issue_date,
                    selected_invoice.sale_date,
                    selected_invoice.payment_date,
                    selected_invoice.payment_method,
                    File_Provider.Get_Base64_Tag(decrypted_logo, company_logo_data.file_type)                    
                ]);

                await get_company_info;
                await get_client_info;

                invoice_company = string.Format(invoice_company,
                [
                    language.seller,
                    company_info.company_name,
                    company_info.address,
                    company_info.nip,
                    company_info.web_page,
                    company_info.email,
                    company_info.phone_number,
                    company_info.bank_name,
                    company_info.bank_number
                ]);

                invoice_client = string.Format(invoice_client,
                [
                    language.client,
                    client_info.company_name.Equals("") ? client_info.name + " " + client_info.surname : client_info.company_name,
                    client_info.address,
                    client_info.nip
                ]);


                invoice_footer = string.Format(invoice_footer,
                [
                    language.comments,
                    comment,
                    language.to_pay,
                    Crypto.Round(selected_invoice.gross_worth, 2).ToString()+" "+settings.currency_FK.currency_symbol + ", " + selected_invoice.gross_worth_in_words,

                    language.name_and_surname_of_invoice_recipient,
                    language.name_and_surname_of_invoice_issuer,
                    input_obj.auto_sign ? company_info.name + " " + company_info.surname : ""
                ]);


                string invoice_rows_html = "";

                decimal total_net_worth = 0;
                decimal total_tax_worth = 0;
                decimal total_gross_worth = 0;

                decimal operational_net_worth;
                decimal operational_net_worth_after_discount;
                decimal operational_net_worth_total;
                decimal operational_tax_worth;
                decimal operational_gross_worth;

                List<Lease_Value_Model> including_rows = new List<Lease_Value_Model>();

                Lease_Value_Model? pct_exists;

                bool display_discount_columns = selected_invoice.invoice_row_list_FK.Any(r => r.use_discount);

                var sorted_invoice_rows = selected_invoice.invoice_row_list_FK.OrderBy(r => r.row_number);

                foreach (var row in sorted_invoice_rows)
                {
                    if(row == null)
                    {
                        throw new Exception("19");//not found
                    }


                    operational_net_worth = Crypto.Round(row.net_worth, settings.decimal_digits);

                    if (row.use_discount)
                    {
                        if (row.discount_is_in_pct)
                        {
                            operational_net_worth_after_discount = Crypto.Round(operational_net_worth * ((100 - row.discount_value) / 100), settings.decimal_digits);
                        }
                        else
                        {
                            operational_net_worth_after_discount = Crypto.Round(operational_net_worth - row.discount_value, settings.decimal_digits);
                        }
                    }
                    else
                    {
                        operational_net_worth_after_discount = operational_net_worth;
                    }

                    operational_net_worth_total = Crypto.Round(operational_net_worth_after_discount * row.total_quantity, settings.decimal_digits);

                    operational_tax_worth = Crypto.Round(operational_net_worth_total * (row.tax_pct / 100), settings.decimal_digits);
                    
                    operational_gross_worth = operational_net_worth_total + operational_tax_worth;


                    pct_exists = including_rows.Where(r => r.tax_pct.Equals(row.tax_pct)).FirstOrDefault();

                    if (pct_exists == null)
                    {
                        including_rows.Add(
                            new Lease_Value_Model
                            {
                                tax_pct = row.tax_pct,
                                net_worth = operational_net_worth_total,
                                tax_worth = operational_tax_worth,
                                gross_worth = operational_gross_worth
                            }
                        );
                    }
                    else
                    {
                        pct_exists.net_worth += operational_net_worth_total;
                        pct_exists.tax_worth += operational_tax_worth;
                        pct_exists.gross_worth += operational_gross_worth;
                    }

                    total_net_worth += operational_net_worth_total;
                    total_tax_worth += operational_tax_worth;
                    total_gross_worth += operational_gross_worth;

                    if (display_discount_columns)
                    {
                        invoice_rows_html += @"
                        <tr>
                            <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;"">" + row.row_number.ToString() + @"</td>
                            <td style=""border: 2px solid silver;text-align: left;padding: 0.1rem 0.2rem;""> " + row.name + @" </td>
                            <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;""> " + (row.use_discount ? (row.discount_is_in_pct ? row.discount_value.ToString() + "%" : row.discount_value.ToString()) : "-") + @" </td>
                            <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;""> " + row.total_quantity.ToString() + @" </td>
                            <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;""> " + operational_net_worth.ToString() + @" </td>
                            <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;""> " + operational_net_worth_after_discount.ToString() + @" </td>
                            <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;""> " + operational_net_worth_total.ToString() + @" </td>
                            <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;""> " + row.tax_pct.ToString() + @" </td>
                            <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;""> " + operational_tax_worth.ToString() + @" </td>
                            <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;""> " + operational_gross_worth.ToString() + @" </td>
                        </tr>";
                    }
                    else
                    {
                        invoice_rows_html += @"
                        <tr>
                            <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;"">"+ row.row_number.ToString() + @"</td>
                            <td style=""border: 2px solid silver;text-align: left;padding: 0.1rem 0.2rem;""> "+ row.name+@" </td>
                            <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;""> "+ row.total_quantity.ToString() + @" </td>
                            <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;""> "+ operational_net_worth.ToString() + @" </td>
                            <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;""> "+ operational_net_worth_total.ToString() + @" </td>
                            <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;""> "+ row.tax_pct.ToString() + @" </td>
                            <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;""> "+ operational_tax_worth.ToString() + @" </td>
                            <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;""> "+ operational_gross_worth.ToString() + @" </td>
                        </tr>";
                    }

                }


                string main_table_headers_html = display_discount_columns ? (@"
                    <tr style=""background-color: #DDD;"">
                        <th style=""width: 5%;border: 2px solid silver;text-align: center;"">"+ language.number +@"</th>
                        <th style=""width: 45%;border: 2px solid silver;text-align: center;"">"+ language.name + @"</th>
                        <th style=""width: 10%;border: 2px solid silver;text-align: center;"">"+ language.discount + @"</th>
                        <th style=""width: 45%;border: 2px solid silver;text-align: center;"">"+ language.quantity + @"</th>
                        <th style=""width: 45%;border: 2px solid silver;text-align: center;"">"+ language.net_price + @"</th>
                        <th style=""width: 10%;border: 2px solid silver;text-align: center;"">"+ language.net_price_after_discount + @"</th>
                        <th style=""width: 5%;border: 2px solid silver;text-align: center;"">"+ language.net_value + @"</th>
                        <th style=""width: 10%;border: 2px solid silver;text-align: center;"">"+ language.vat_percent + @"</th>
                        <th style=""width: 10%;border: 2px solid silver;text-align: center;"">"+ language.vat_value + @"</th>
                        <th style=""width: 5%;border: 2px solid silver;text-align: center;"">"+ language.gross_value + @"</th>
                    </tr>") 
                    : (@"
                    <tr style=""background-color: #DDD;"">
                        <th style=""width: 5%;border: 2px solid silver;text-align: center;"">"+ language.number +@"</th>
                        <th style=""width: 45%;border: 2px solid silver;text-align: center;"">"+ language.name + @"</th>
                        <th style=""width: 45%;border: 2px solid silver;text-align: center;"">"+ language.quantity + @"</th>
                        <th style=""width: 45%;border: 2px solid silver;text-align: center;"">"+ language.net_price + @"</th>
                        <th style=""width: 5%;border: 2px solid silver;text-align: center;"">"+ language.net_value + @"</th>
                        <th style=""width: 10%;border: 2px solid silver;text-align: center;"">"+ language.vat_percent + @"</th>
                        <th style=""width: 10%;border: 2px solid silver;text-align: center;"">"+ language.vat_value + @"</th>
                        <th style=""width: 5%;border: 2px solid silver;text-align: center;"">"+ language.gross_value + @"</th>
                    </tr>");


                string including_html = "";

                foreach(var including in including_rows)
                {
                    including_html += @"<tr>";

                    if (display_discount_columns)
                    {
                        including_html += @"<td colspan=""5"" style=""border-bottom: 2px solid transparent; border-left: 2px solid transparent;""></td>";
                    }
                    else
                    {
                        including_html += @"<td colspan=""3"" style=""border-bottom: 2px solid transparent; border-left: 2px solid transparent;""></td>";
                    }

                    including_html += @"
                        <th style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;"">"+ language.including +@"</th>
                        <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;"">"+ including.net_worth.ToString() +@"</td>
                        <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;"">"+ including.tax_pct.ToString() +@"</td>
                        <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;"">"+ including.tax_worth.ToString() +@"</td>
                        <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;"">"+ including.gross_worth.ToString() +@"</td>
                    </tr>";
                }


                string summary_html = "";

                if (display_discount_columns)
                {
                    summary_html = @"
                        <tr>
                            <td colspan=""5"" style=""border-bottom: 2px solid transparent; border-left: 2px solid transparent;""></td>";
                }
                else
                {
                    summary_html = @"
                        <tr>
                            <td colspan=""3"" style=""border-bottom: 2px solid transparent; border-left: 2px solid transparent;""></td>";
                }

                summary_html += @"
                        <th style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;"">"+language.total +@"</th>
                        <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;"">"+ total_net_worth + @"</td>
                        <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;"">-</td>
                        <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;"">"+ total_tax_worth + @"</td>
                        <td style=""border: 2px solid silver;text-align: right;padding: 0.1rem 0.2rem;"">"+ total_gross_worth + @"</td>
                    </tr>";



                invoice_main_table = string.Format(invoice_main_table,
                [
                    main_table_headers_html,
                    invoice_rows_html,
                    including_html,
                    summary_html,
                    language.net_value,
                    total_net_worth,
                    language.vat_value,
                    total_tax_worth,
                    language.gross_value,
                    total_gross_worth
                ]);                



                invoice_body = string.Format(invoice_body, new object[]
                {
                    invoice_header,
                    invoice_company,
                    invoice_client,
                    invoice_main_table,
                    invoice_footer
                });


                BrowserFetcher browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
                using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true }))
                {
                    using (var page = await browser.NewPageAsync())
                    {
                        await page.SetContentAsync(invoice_body);
                        await page.EvaluateExpressionAsync("document.title = '" + selected_invoice.full_number + "';");

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

                        Invoice_Printed_Data printed_data = new Invoice_Printed_Data
                        {
                            invoice_file_name = selected_invoice.full_number.Replace("/", "_"),
                            invoice_FKid = selected_invoice.id,
                            print_timestamp = input_obj.user_current_timestamp,
                            invoice_binary_data_FK = new Invoice_Binary_Data
                            {
                                invoice_bytes = encrypted_pdf
                            }
                        };

                        _context.Invoice_Printed_Data.Add(printed_data);
                        _context.SaveChanges();

                        return new Invoice_Print_Model { invoice_file_name = printed_data.invoice_file_name, invoice_bytes = pdf };

                    }
                }





            }

        }



        private string Get_Lease_For_Month_Name(int month_number, string language_code)
        {

            Months_Language_Model? month_names = File_Provider.Get_Months_Language_Data(language_code);


            switch (month_number)
            {
                case 1:
                    return month_names.leaseFor + " " + month_names.jan;

                case 2:
                    return month_names.leaseFor + " " + month_names.feb;

                case 3:
                    return month_names.leaseFor + " " + month_names.mar;

                case 4:
                    return month_names.leaseFor + " " + month_names.apr;

                case 5:
                    return month_names.leaseFor + " " + month_names.may;

                case 6:
                    return month_names.leaseFor + " " + month_names.jun;

                case 7:
                    return month_names.leaseFor + " " + month_names.jul;

                case 8:
                    return month_names.leaseFor + " " + month_names.aug;

                case 9:
                    return month_names.leaseFor + " " + month_names.sep;

                case 10:
                    return month_names.leaseFor + " " + month_names.oct;

                case 11:
                    return month_names.leaseFor + " " + month_names.nov;

                case 12:
                    return month_names.leaseFor + " " + month_names.dec;

                default:
                    throw new Exception("19");
            }


        }


        /*
         * AllDaysInMonth method
         * This method returns list of all days in month specified in parameter
         */
        private List<DateTime> AllDaysInMonth(int year, int month)
        {

            int days = DateTime.DaysInMonth(year, month);

            List<DateTime> result = new List<DateTime>();

            for (int day = 1; day <= days; day++)
            {
                result.Add(new DateTime(year, month, day));
            }

            return result;

        }



        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++\\
        //Invoice number row
        private int Get_Invoice_Number(Invoice_Type invoice_type, int year, int month)
        {
            var new_invoice_number = _context.Invoice.Where(i => 
                i.month.Equals(month) && 
                i.year.Equals(year) && 
                i.invoice_type.Equals(invoice_type)
            ).MaxBy(f => f.number);

            if (new_invoice_number == null)
            {
                return 1;
            }
            else
            {
                return new_invoice_number.number + 1;
            }
        }

        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++\\
        //Full invoice number
        private string Get_Full_Invoice_Number(Invoice_Type invoice_type, long year, long month, int invoice_number)
        {
            var invoice_settings = _context.Company_Invoice_Settings.FirstOrDefault();

            if (invoice_settings != null)
            {
                if (invoice_type.Equals(Invoice_Type.Sale))
                {
                    return invoice_settings.sale_invoice_prefix + "/" + year + "/" + month + "/" + invoice_number;
                }
                else
                {
                    //for Lease and Combined invoice type
                    return invoice_settings.lease_invoice_prefix + "/" + year + "/" + month + "/" + invoice_number;
                }
            }
            else
            {
                throw new Exception("51");//settings not found
            }
        }


    
    }
}
