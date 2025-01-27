using ManagemateAPI.Management.Shared.Enum;
using ManagemateAPI.Management.Shared.Json_Model;
using System.Text.Json;

namespace ManagemateAPI.Management.Shared.Static
{
    public static class File_Provider
    {

        private static string? INVOICE_BODY = null;

        private static string? INVOICE_HEADER = null;
        private static string? INVOICE_COMPANY_INFO = null;
        private static string? INVOICE_CLIENT_INFO = null;
        private static string? INVOICE_MAIN_TABLE = null;
        private static string? INVOICE_FOOTER = null;

        private static string? PROTOCOL_HTML_CONTENT = null;


        private static string? EMAIL_HTML_1 = null;
        private static string? EMAIL_HTML_2 = null;



        private static List<(string, string?)> PROTOCOL_LANGUAGE_LIST = 
        [
            ( "pl", null )
        ];

        private static List<(string, string?)> INVOICE_LANGUAGE_LIST =
        [
            ( "pl", null )
        ];  
        
        private static List<(string, string?)> MONTHS_LANGUAGE_LIST =
        [
            ( "pl", null )
        ];



        public static string Get_Protocol_HTML()
        {
            if (PROTOCOL_HTML_CONTENT == null)
            {

                if (File.Exists(System_Path.PROTOCOL_HTML))
                {
                    try
                    {
                        PROTOCOL_HTML_CONTENT = File.ReadAllText(System_Path.PROTOCOL_HTML);
                    }
                    catch (Exception)
                    {
                        throw new Exception("26");//file read error
                    }
                }
                else
                {
                    throw new Exception("26"); // file read error
                }

            }

            return PROTOCOL_HTML_CONTENT;
        }



        public static Protocol_Language_Model Get_Protocol_Language_Data(string language_code)
        {
            if (!PROTOCOL_LANGUAGE_LIST.Any(l => l.Item1.Equals(language_code.ToLower())))
            {
                throw new Exception("19");//not found
            }

            var lang = PROTOCOL_LANGUAGE_LIST.Where(l => l.Item1.Equals(language_code.ToLower())).First();

            if (lang.Item2 == null)
            {

                try
                {
                    lang.Item2 = File.ReadAllText(System_Path.PROTOCOL_LANGUAGE_FOLDER + language_code.ToLower());
                }
                catch (Exception)
                {
                    throw new Exception("26");//file read error
                }

            }

            Protocol_Language_Model? model = JsonSerializer.Deserialize<Protocol_Language_Model>(lang.Item2);

            if (model == null)
            {
                throw new Exception("26");//file read error
            }

            return model;
        }


        public static string Get_Protocol_State_Name(ref Protocol_Language_Model language_model, Protocol_State state)
        {
            if (state.Equals(Protocol_State.Draft))
            {
                return language_model.Draft;
            }
            else if (state.Equals(Protocol_State.Reserved))
            {
                return language_model.Reserved;
            }
            else if (state.Equals(Protocol_State.Offer))
            {
                return language_model.Offer;
            }
            else if (state.Equals(Protocol_State.Confirmed))
            {
                return "";
            }
            else
            {
                throw new Exception("19");//not found
            }
        }


        public static string Get_Base64_Tag(byte[] logo, string file_type)
        {
            return "<img src=\"data:" + file_type + ";base64," + Convert.ToBase64String(logo) + "\" alt=\"logo\" />";
        }





        //INVOICE

        public static Months_Language_Model Get_Months_Language_Data(string language_code)
        {
            if (!MONTHS_LANGUAGE_LIST.Any(l => l.Item1.Equals(language_code.ToLower())))
            {
                throw new Exception("19");//not found
            }

            var lang = MONTHS_LANGUAGE_LIST.Where(l => l.Item1.Equals(language_code.ToLower())).First();

            if (lang.Item2 == null)
            {

                try
                {
                    lang.Item2 = File.ReadAllText(System_Path.INVOICE_LANGUAGE_FOLDER + language_code.ToLower() + System_Path.INVOICE_MONTH_NAMES);
                }
                catch (Exception)
                {
                    throw new Exception("26");//file read error
                }

            }

            Months_Language_Model? model = JsonSerializer.Deserialize<Months_Language_Model>(lang.Item2);

            if (model == null)
            {
                throw new Exception("26");//file read error
            }

            return model;
        }


        public static Invoice_Language_Model Get_Invoice_Language_Data(string language_code)
        {
            if (!INVOICE_LANGUAGE_LIST.Any(l => l.Item1.Equals(language_code.ToLower())))
            {
                throw new Exception("19");//not found
            }

            var lang = INVOICE_LANGUAGE_LIST.Where(l => l.Item1.Equals(language_code.ToLower())).First();

            if (lang.Item2 == null)
            {

                try
                {
                    lang.Item2 = File.ReadAllText(System_Path.INVOICE_LANGUAGE_FOLDER + language_code.ToLower());
                }
                catch (Exception)
                {
                    throw new Exception("26");//file read error
                }

            }

            Invoice_Language_Model? model = JsonSerializer.Deserialize<Invoice_Language_Model>(lang.Item2);

            if (model == null)
            {
                throw new Exception("26");//file read error
            }

            return model;
        }


        public static string Get_Invoice_Body_HTML()
        {
            if (INVOICE_BODY == null)
            {

                if (File.Exists(System_Path.INVOICE_BODY))
                {
                    try
                    {
                        INVOICE_BODY = File.ReadAllText(System_Path.INVOICE_BODY);
                    }
                    catch (Exception)
                    {
                        throw new Exception("26");//file read error
                    }
                }
                else
                {
                    throw new Exception("26"); // file read error
                }

            }

            return INVOICE_BODY;
        }

        public static string Get_Invoice_Header_HTML()
        {
            if (INVOICE_HEADER == null)
            {

                if (File.Exists(System_Path.INVOICE_HEADER))
                {
                    try
                    {
                        INVOICE_HEADER = File.ReadAllText(System_Path.INVOICE_HEADER);
                    }
                    catch (Exception)
                    {
                        throw new Exception("26");//file read error
                    }
                }
                else
                {
                    throw new Exception("26"); // file read error
                }

            }

            return INVOICE_HEADER;
        }

        public static string Get_Invoice_Company_HTML()
        {
            if (INVOICE_COMPANY_INFO == null)
            {

                if (File.Exists(System_Path.INVOICE_COMPANY_INFO))
                {
                    try
                    {
                        INVOICE_COMPANY_INFO = File.ReadAllText(System_Path.INVOICE_COMPANY_INFO);
                    }
                    catch (Exception)
                    {
                        throw new Exception("26");//file read error
                    }
                }
                else
                {
                    throw new Exception("26"); // file read error
                }

            }

            return INVOICE_COMPANY_INFO;
        }

        public static string Get_Invoice_Client_HTML()
        {
            if (INVOICE_CLIENT_INFO == null)
            {

                if (File.Exists(System_Path.INVOICE_CLIENT_INFO))
                {
                    try
                    {
                        INVOICE_CLIENT_INFO = File.ReadAllText(System_Path.INVOICE_CLIENT_INFO);
                    }
                    catch (Exception)
                    {
                        throw new Exception("26");//file read error
                    }
                }
                else
                {
                    throw new Exception("26"); // file read error
                }

            }

            return INVOICE_CLIENT_INFO;
        }

        public static string Get_Invoice_Main_Table_HTML()
        {
            if (INVOICE_MAIN_TABLE == null)
            {

                if (File.Exists(System_Path.INVOICE_MAIN_TABLE))
                {
                    try
                    {
                        INVOICE_MAIN_TABLE = File.ReadAllText(System_Path.INVOICE_MAIN_TABLE);
                    }
                    catch (Exception)
                    {
                        throw new Exception("26");//file read error
                    }
                }
                else
                {
                    throw new Exception("26"); // file read error
                }

            }

            return INVOICE_MAIN_TABLE;
        }

        public static string Get_Invoice_Footer_HTML()
        {
            if (INVOICE_FOOTER == null)
            {

                if (File.Exists(System_Path.INVOICE_FOOTER))
                {
                    try
                    {
                        INVOICE_FOOTER = File.ReadAllText(System_Path.INVOICE_FOOTER);
                    }
                    catch (Exception)
                    {
                        throw new Exception("26");//file read error
                    }
                }
                else
                {
                    throw new Exception("26"); // file read error
                }

            }

            return INVOICE_FOOTER;
        }


        //EMAIL

        public static string Get_Email_Template_HTML_1()
        {
            if (EMAIL_HTML_1 == null)
            {

                if (File.Exists(System_Path.EMAIL_HTML_1))
                {
                    try
                    {
                        EMAIL_HTML_1 = File.ReadAllText(System_Path.EMAIL_HTML_1);
                    }
                    catch (Exception)
                    {
                        throw new Exception("26");//file read error
                    }
                }
                else
                {
                    throw new Exception("26"); // file read error
                }

            }

            return EMAIL_HTML_1;
        }

        public static string Get_Email_Template_HTML_2()
        {
            if (EMAIL_HTML_2 == null)
            {

                if (File.Exists(System_Path.EMAIL_HTML_2))
                {
                    try
                    {
                        EMAIL_HTML_2 = File.ReadAllText(System_Path.EMAIL_HTML_2);
                    }
                    catch (Exception)
                    {
                        throw new Exception("26");//file read error
                    }
                }
                else
                {
                    throw new Exception("26"); // file read error
                }

            }

            return EMAIL_HTML_2;
        }

    }
}
