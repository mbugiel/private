using ManagemateAPI.Management.Shared.Static;

namespace ManagemateAPI.Controllers
{
    public class Response_Handler
    {
        public static Api_Response GetExceptionResponse(Exception ex)
        {

            Api_Response response = new Api_Response();

            response.code = ex.Message;
            response.responseData = null;

            switch (ex.Message)
            {
                case "1":
                    response.message = Info._1_SESSION_NOT_FOUND; break;

                case "2":
                    response.message = Info._2_ENCRYPTION_ERROR; break;

                case "3":
                    response.message = Info._3_DECRYPTION_ERROR; break;

                case "4":
                    response.message = Info._4_LOGIN_IN_USE; break;

                case "5":
                    response.message = Info._5_EMAIL_IN_USE; break;

                case "6":
                    response.message = Info._6_ADD_USER_ERROR; break;

                case "7":
                    response.message = Info._7_USER_NOT_FOUND; break;

                case "8":
                    response.message = Info._8_PASSWORD_ERROR; break;

                case "9":
                    response.message = Info._9_CONFIRM_CODE_TIME_ERROR; break;

                case "10":
                    response.message = Info._10_CONFIRM_CODE_ATTEMPT_ERROR; break;

                case "11":
                    response.message = Info._11_CONFIRM_CODE_INVALid; break;

                case "12":
                    response.message = Info._12_CONFIRM_CODE_NOT_FOUND; break;

                case "13":
                    response.message = Info._13_NO_ITEMS_ON_RECEIPT; break;

                case "14":
                    response.message = Info._14_NULL_ERROR; break;

                case "15":
                    response.message = Info._15_CONFIRM_CODE_READ_TEMPLATE_ERROR; break;

                case "16":
                    response.message = Info._16_SERVER_RESPONSE_ERROR; break;

                case "17":
                    response.message = Info._17_SESSION_EXPIRED; break;

                case "18":
                    response.message = Info._18_DUPLICATE_ERROR; break;

                case "19":
                    response.message = Info._19_OBJECT_NOT_FOUND; break;

                case "20":
                    response.message = Info._20_CONFIRM_CODE_DELAY_ERROR; break;

                case "21":
                    response.message = Info._21_INVALID_PASSWORD_ERROR; break;

                case "22":
                    response.message = Info._22_NOT_ENOUGH_IN_STOCK; break;

                case "23":
                    response.message = Info._23_COMPANY_EXISTS_ERROR; break;

                case "24":
                    response.message = Info._24_TOO_MANY_RETURNED_ERROR; break;

                case "25":
                    response.message = Info._25_INCORRECT_ITEM_TRADING_ERROR; break;

                case "26":
                    response.message = Info._26_FILE_READ_ERROR; break;

                case "27":
                    response.message = Info._27_FOLDER_CREATE_ERROR; break;

                case "28":
                    response.message = Info._28_JSON_ERROR; break;

                case "29":
                    response.message = Info._29_FILE_TOO_LARGE; break;

                case "30":
                    response.message = Info._30_EMPTY_LOGO_ERROR; break;

                case "31":
                    response.message = Info._31_INVALID_FILE_FORMAT_ERROR; break;

                case "32":
                    response.message = Info._32_COOKIE_ACCESS_ERROR; break;
                
                case "33":
                    response.message = Info._33_NOT_ENOUGH_RESERVED; break;
                
                case "34":
                    response.message = Info._34_RESERVED_RETURN_PROTOCOL_ERROR; break;
                
                case "35":
                    response.message = Info._35_NOT_ENOUGH_UNUSED; break;
                
                case "36":
                    response.message = Info._36_CHECKSUM_ERROR; break;

                case "37":
                    response.message = Info._37_DELETE_USED_ERROR; break;

                case "38":
                    response.message = Info._38_IN_USE_ERROR; break;
                    
                case "39":
                    response.message = Info._39_NEGATIVE_VALUE_ERROR; break;       
                    
                case "40":
                    response.message = Info._40_INVALID_DATE_ERROR; break;
                                        
                case "41":
                    response.message = Info._41_STOCK_STATE_DATE_ERROR; break;
                       
                case "42":
                    response.message = Info._42_TIMESTAMP_DUPLICATE_ERROR; break;

                case "43":
                    response.message = Info._43_NOT_ENOUGH_OUT_OF_STORAGE; break;

                case "44":
                    response.message = Info._44_NOT_ENOUGH_FREE_ITEMS; break;

                case "45":
                    response.message = Info._45_MAX_OFFER_ORDER_ERROR; break;
                    
                case "46":
                    response.message = Info._46_NEW_TIMESTAMP_ERROR; break;
                                        
                case "47":
                    response.message = Info._47_DISCOUNT_TOO_HIGH_ERROR; break;
                                           
                case "48":
                    response.message = Info._48_INVOICE_DATE_ERROR; break;
                                      
                case "49":
                    response.message = Info._49_ORDER_NOT_EMPTY_ERROR; break;
               
                case "50":
                    response.message = Info._50_INVALID_RATE_ERROR; break;
                    
                case "51":
                    response.message = Info._51_SETTINGS_NOT_FOUND_ERROR; break;

                default:
                    response.message = "Failure";
                    break;
            }

            return response;
        }

        public static Api_Response GetAppResponse(ResponseType type, object? contract)
        {
            Api_Response response;

            response = new Api_Response { responseData = contract };
            switch (type)
            {
                case ResponseType.Success:
                    response.code = "0";
                    response.message = "Success";

                    break;
                case ResponseType.NotFound:
                    response.code = "2";
                    response.message = "Not Found";

                    break;
            }
            return response;
        }
    }
}
