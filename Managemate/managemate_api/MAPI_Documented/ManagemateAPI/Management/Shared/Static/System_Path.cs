using ManagemateAPI.Management.M_Session.Input_Objects;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace ManagemateAPI.Management.Shared.Static
{
    public static class System_Path
    {
        public static string CURRENT_DIRECTORY = Directory.GetCurrentDirectory();
        public static string WRITE_DIRECTORY = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public static string AUTHENTICATION_API = "https://ftp.managemate.eu/api/";//"http://localhost:5001/api/";//"https://localhost:7046/api/";//
        public static string EMAIL_HTML_1 = CURRENT_DIRECTORY + "/Templates/email.html";//@"\Templates\email.html";//
        public static string EMAIL_HTML_2 = CURRENT_DIRECTORY + "/Templates/loginEmail.html";// @"\Templates\loginEmail.html";//
        public static string ENCRYPT_KEYS_PATH = WRITE_DIRECTORY + "/keys";//@"F:\Projekty\Magazyn\sandbox\AuthenticationAPI 2.0\Postgres\keys";//
        public static string PASSWD_PATH = WRITE_DIRECTORY + "/passwd";//@"F:\Projekty\Magazyn\sandbox\AuthenticationAPI 2.0\Postgres\passwd";//
        public static string APPCODE_PATH = WRITE_DIRECTORY + "/appcode";//@"F:\Projekty\Magazyn\sandbox\AuthenticationAPI 2.0\Postgres\appcode";//


        public static string COMPANY_LOGO_NAME = "company_logo";

        //INVOICE\\
        public static string INVOICE_ELEMENTS_ROOT = CURRENT_DIRECTORY + "/Templates/invoice/";
        public static string PROTOCOL_ELEMENTS_ROOT = CURRENT_DIRECTORY + "/Templates/protocol/";

        public static string INVOICE_MONTH_NAMES = INVOICE_ELEMENTS_ROOT + "_months.json";

        public static string INVOICE_BODY = INVOICE_ELEMENTS_ROOT + "invoice_body.html";
        public static string INVOICE_HEADER = INVOICE_ELEMENTS_ROOT + "invoice_header.html";
        public static string INVOICE_COMPANY_INFO = INVOICE_ELEMENTS_ROOT + "invoice_company.html";
        public static string INVOICE_CLIENT_INFO = INVOICE_ELEMENTS_ROOT + "invoice_client.html";
        public static string INVOICE_MAIN_TABLE = INVOICE_ELEMENTS_ROOT + "invoice_main_table.html";
        public static string INVOICE_FOOTER = INVOICE_ELEMENTS_ROOT + "invoice_footer.html";

        public static string PROTOCOL_HTML = PROTOCOL_ELEMENTS_ROOT + "protocol.html";

        public static string INVOICE_LANGUAGE_FOLDER = INVOICE_ELEMENTS_ROOT + "Language/";
        public static string PROTOCOL_LANGUAGE_FOLDER = PROTOCOL_ELEMENTS_ROOT + "Language/";


        //HTTP ONLY COOKIES\\
        public static string COOKIE_TOKEN_NAME = "session_one";
        public static string COOKIE_USERID_NAME = "session_two";


        public static List<string[]> CURRENCY_LIST =
        [
            ["PLN", "gr"],
            ["GBP", "p"],
            ["EUR", "c"],
            ["USD", "c"]
        ];

    }
}
