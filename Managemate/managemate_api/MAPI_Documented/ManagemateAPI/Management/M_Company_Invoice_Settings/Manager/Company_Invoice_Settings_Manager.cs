using ManagemateAPI.Database.Context;
using ManagemateAPI.Database.Tables;
using Microsoft.EntityFrameworkCore;
using ManagemateAPI.Management.M_Company_Invoice_Settings.Table_Model;
using ManagemateAPI.Management.M_Company_Invoice_Settings.Input_Objects;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Currency.Table_Model;
using ManagemateAPI.Management.Shared.Static;

namespace ManagemateAPI.Management.M_Company_Invoice_Settings.Manager
{

    /*
     * This is the Company_Invoice_Settings_Manager with methods dedicated to the company_invoice_settings table.
     * 
     * It contains methods to:
     * add records,
     * edit records,
     * get record
     */
    public class Company_Invoice_Settings_Manager
    {

        private DB_Context _context;
        private readonly IConfiguration _configuration;


        public Company_Invoice_Settings_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }



        /* 
         * Add_Company_Invoice_Settings method
         * This method is used to add new records to the company_invoice_settings table.
         * 
         * It accepts Add_Company_Invoice_Settings_Data object as input.
         * It then adds new record with values based on the data given in the input object.
         */
        public string Add_Company_Invoice_Settings(Add_Company_Invoice_Settings_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                if (
                    input_obj.lease_release_protocol_prefix.Equals(input_obj.return_protocol_prefix) ||
                    input_obj.lease_release_protocol_prefix.Equals(input_obj.sale_release_protocol_prefix) ||
                    input_obj.sale_release_protocol_prefix.Equals(input_obj.return_protocol_prefix)
                )
                {
                    throw new Exception("18");//duplicate (protocol prefixes of different types can not be the same)
                }

                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var company_settings_exist = _context.Company_Invoice_Settings.FirstOrDefault();

                if (company_settings_exist == null)
                {
                    var currency = _context.Currency.Where(c => c.id.Equals(input_obj.currency_id_FK)).FirstOrDefault();

                    if(currency == null)
                    {
                        throw new Exception("19");//Selected currency doesn't exist
                    }

                    Company_Invoice_Settings new_company_invoice_settings = new Company_Invoice_Settings();

                    new_company_invoice_settings.decimal_digits = input_obj.decimal_digits;
                    new_company_invoice_settings.invoice_type_division = input_obj.invoice_type_division;
                    new_company_invoice_settings.sale_invoice_prefix = input_obj.sale_invoice_prefix;
                    new_company_invoice_settings.lease_invoice_prefix = input_obj.lease_invoice_prefix;
                    new_company_invoice_settings.currency_FK = currency;

                    new_company_invoice_settings.lease_release_protocol_prefix = input_obj.lease_release_protocol_prefix;
                    new_company_invoice_settings.sale_release_protocol_prefix = input_obj.sale_release_protocol_prefix;
                    new_company_invoice_settings.return_protocol_prefix = input_obj.return_protocol_prefix;

                    _context.Company_Invoice_Settings.Add(new_company_invoice_settings);
                    _context.SaveChanges();

                    return Info.SUCCESSFULLY_ADDED;

                }
                else
                {
                    throw new Exception("23"); //_23_COMPANY_EXISTS_ERROR
                }


            }

        }

        /* 
         * Edit_Company_Invoice_Settings method
         * This method is used to edit a record in the company_invoice_settings table.
         * 
         * It accepts Edit_Company_Invoice_Settings_Data object as input.
         * It then changes values of a record with those given in the input object only if its ID matches the one in the input object.
         */
        public string Edit_Company_Invoice_Settings(Edit_Company_Invoice_Settings_Data input_obj, Session_Data session)
        {
            if (input_obj == null || session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var editing_data = _context.Company_Invoice_Settings.FirstOrDefault();


                if (editing_data != null)
                {
                    var currency = _context.Currency.Where(c => c.id.Equals(input_obj.currency_id_FK)).FirstOrDefault();

                    if (currency == null)
                    {
                        throw new Exception("19");//Selected currency doesn't exist
                    }

                    editing_data.decimal_digits = input_obj.decimal_digits;
                    editing_data.invoice_type_division = input_obj.invoice_type_division;
                    editing_data.sale_invoice_prefix = input_obj.sale_invoice_prefix;
                    editing_data.lease_invoice_prefix = input_obj.lease_invoice_prefix;
                    editing_data.currency_FK = currency;

                    editing_data.lease_release_protocol_prefix = input_obj.lease_release_protocol_prefix;
                    editing_data.sale_release_protocol_prefix = input_obj.sale_release_protocol_prefix;
                    editing_data.return_protocol_prefix = input_obj.return_protocol_prefix;

                    _context.SaveChanges();


                    return Info.SUCCESSFULLY_CHANGED;
                }
                else
                {
                    throw new Exception("19");// objects not found
                }

            }
        }



        /*
         * Get_Company_Invoice_Settings method
         * This method gets a record from the company_invoice_settings table by its ID and returns it.
         * 
         * It accepts Get_Company_Invoice_Settings_Data object as input.
         * Then it gets a records that has the same ID as the ID given in the input object
         */
        public Company_Invoice_Settings_Model Get_Company_Invoice_Settings(Session_Data session)
        {
            if (session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var company_invoice_settings = _context.Company_Invoice_Settings.Include(c => c.currency_FK).FirstOrDefault();

                if (company_invoice_settings == null || company_invoice_settings.currency_FK == null)
                {
                    throw new Exception("19");// company data not found
                }
                else
                {
                    Company_Invoice_Settings_Model company_model = new Company_Invoice_Settings_Model();

                    company_model.decimal_digits = company_invoice_settings.decimal_digits;
                    company_model.invoice_type_division = company_invoice_settings.invoice_type_division;
                    company_model.sale_invoice_prefix = company_invoice_settings.sale_invoice_prefix;
                    company_model.lease_invoice_prefix = company_invoice_settings.lease_invoice_prefix;
                    company_model.currency_id_FK = new Currency_Model
                    {
                        id = company_invoice_settings.currency_FKid,
                        currency_symbol = company_invoice_settings.currency_FK.currency_symbol,
                        currency_hundreth_symbol = company_invoice_settings.currency_FK.currency_hundreth_symbol
                    };

                    company_model.lease_release_protocol_prefix = company_invoice_settings.lease_release_protocol_prefix;
                    company_model.sale_release_protocol_prefix = company_invoice_settings.sale_release_protocol_prefix;
                    company_model.return_protocol_prefix = company_invoice_settings.return_protocol_prefix;

                    return company_model;

                }


            }
        }



    }
}
