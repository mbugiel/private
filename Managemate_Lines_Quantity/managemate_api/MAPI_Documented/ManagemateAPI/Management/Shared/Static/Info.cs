namespace ManagemateAPI.Management.Shared.Static
{
    public static class Info
    {
        //ERROR

        public static string _1_SESSION_NOT_FOUND = "session not found";

        public static string _2_ENCRYPTION_ERROR = "encryption error";
        public static string _3_DECRYPTION_ERROR = "decryption error";

        public static string _4_LOGIN_IN_USE = "login in use";
        public static string _5_EMAIL_IN_USE = "e-mail in use";
        public static string _6_ADD_USER_ERROR = "error while adding user";

        public static string _7_USER_NOT_FOUND = "user not found";
        public static string _8_PASSWORD_ERROR = "incorrect password";

        public static string _9_CONFIRM_CODE_TIME_ERROR = "confirmation code has expired";
        public static string _10_CONFIRM_CODE_ATTEMPT_ERROR = "too many incorrect attempts";
        public static string _11_CONFIRM_CODE_INVALid = "incorrect confirmation code";
        public static string _12_CONFIRM_CODE_NOT_FOUND = "confirmation code not found";

        public static string _13_NO_ITEMS_ON_RECEIPT = "no items on protocol";

        public static string _14_NULL_ERROR = "null error";

        public static string _15_CONFIRM_CODE_READ_TEMPLATE_ERROR = "error while reading e-mail template";

        public static string _16_SERVER_RESPONSE_ERROR = "server response error";

        public static string _17_SESSION_EXPIRED = "session has expired";

        public static string _18_DUPLICATE_ERROR = "duplicate error";

        public static string _19_OBJECT_NOT_FOUND = "object not found";

        public static string _20_CONFIRM_CODE_DELAY_ERROR = "too short time period between sending e-mails";

        public static string _21_INVALID_PASSWORD_ERROR = "Password is invalid";

        public static string _23_COMPANY_EXISTS_ERROR = "company data already exists";

        public static string _24_TOO_MANY_RETURNED_ERROR = "can't return more than was released";
        public static string _25_INCORRECT_ITEM_TRADING_ERROR = "can't return item that was sold";

        public static string _26_FILE_READ_ERROR = "error occured when opening an file";
        public static string _27_FOLDER_CREATE_ERROR = "error occured when creating an folder";
        public static string _28_JSON_ERROR = "error occured when serializating or deserialitating a json";

        public static string _29_FILE_TOO_LARGE = "file is to large or has no length";

        public static string _30_EMPTY_LOGO_ERROR = "empty logo in database";

        public static string _31_INVALID_FILE_FORMAT_ERROR = "invalid file format";

        public static string _32_COOKIE_ACCESS_ERROR = "error while setting or getting cookies";

        public static string _22_NOT_ENOUGH_IN_STOCK = "not enough items in stock";
        public static string _33_NOT_ENOUGH_RESERVED = "not enough items in reservation";
        public static string _35_NOT_ENOUGH_UNUSED = "new quantity is less than currently used";
        public static string _38_IN_USE_ERROR = "object is used by another(s)";
        public static string _36_CHECKSUM_ERROR = "miscalculation error";
        public static string _37_DELETE_USED_ERROR = "can not delete object that is currently in use";
        public static string _34_RESERVED_RETURN_PROTOCOL_ERROR = "return protocol can not be in reserved or offer state";
        public static string _39_NEGATIVE_VALUE_ERROR = "quantity cannot be negative";
        public static string _40_INVALID_DATE_ERROR = "specified date is later than first protocol date";
        public static string _41_STOCK_STATE_DATE_ERROR = "specified date is earlier than latest stock state date";
        public static string _42_TIMESTAMP_DUPLICATE_ERROR = "timestamp duplicate in stock history";
        public static string _43_NOT_ENOUGH_OUT_OF_STORAGE = "not enough items out of storage";
        public static string _44_NOT_ENOUGH_FREE_ITEMS = "not enough items that are not in any storage";
        public static string _45_MAX_OFFER_ORDER_ERROR = "all offer protocols should be related with the same order";
        public static string _46_NEW_TIMESTAMP_ERROR = "given date is later than now";
        public static string _47_DISCOUNT_TOO_HIGH_ERROR = "given discount is higher than net worth";
        public static string _48_INVOICE_DATE_ERROR = "invoice date error";
        public static string _49_ORDER_NOT_EMPTY_ERROR = "order is not empty";
        public static string _50_INVALID_RATE_ERROR = "invalid rate value";
        public static string _51_SETTINGS_NOT_FOUND_ERROR = "invoice settings not found";


        //SUCCESS
        public static string CONFIRM_CODE_SENT = "confirmation code sent";

        public static string SUCCESSFULLY_ADDED = "successfully added";
        public static string SUCCESSFULLY_CHANGED = "successfully changed";
        public static string SUCCESSFULLY_DELETED = "successfully deleted";

        public static string SUCCESSFULLY_LOGGED_IN = "successfully logged in";


        //EMAIL
        public static string ADD_USER_SUBJECT = "Weryfikacja adresu e-mail";
        public static string TWO_STEP_LOGIN_SUBJECT = "Logowanie dwuetapowe";
        public static string MAIL_SidE_TEXT = "Dodatkowy tekst na potrzeby niektórych serwerów.";
        public static string VALidATION_CODE = "bYUITVyuvvyuVYU6756vkhv76ugyUGYACF2782167B33D6E31AF6AEAF700A1CABA32555F55372150D353113C9F5150F7281A8967C79180FE88E7143EEF1A1BDADB50F54C6476547148CEC1BF7F6B6D7FFB69303DDAF06EE9B11BEFA099FB1DD64F99A24EA236006B9A9790B9A4141CDE8D86085B2D5C71A0954E4CB17E05BE590935FFB87F6768E21C828D4A12BCD4B5747318D95C2CA603085AE7591E67E5B61491919358073A8575AF4D44F36C9C3B2517B1D605222C92AAD96A5BB3E914CFDCD10684A975DE943B7B104010B27754BDC91694ADA3DDD6AE39EB5831104FCF38530A7B922AC626793F66A2070239B3BC35C47D33B360E6A2BDE946BF1FBF09925134CE6AD51012F660D47A2B44DE81LL8901huiREDYXD+5Ypo+9jgvhc89+JG233278F";


    }
}
