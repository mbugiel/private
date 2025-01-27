using ManagemateAPI.Controllers;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using ManagemateAPI.Encryption.Input_Objects;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.Shared.Static;


namespace ManagemateAPI.Encryption
{
    public static class Crypto
    {

        private static byte[] key;
        private static byte[] iv;
        private static string passwd;
        private static string appcode;

        public async static Task<byte[]> Encrypt(Session_Data session, string data_to_encrypt)
        {
            if (session == null || data_to_encrypt == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {

                using (var httpClient = new HttpClient())
                {

                    Encrypt_Data encryptData = new Encrypt_Data { SessionData = session, DataToEncrypt = data_to_encrypt };

                    StringContent content = new StringContent(JsonSerializer.Serialize(encryptData), Encoding.UTF8, "application/json");

                    using (var authResponse = await httpClient.PostAsync(System_Path.AUTHENTICATION_API+"Encrypt_Data", content))
                    {

                        var responseAsString = await authResponse.Content.ReadAsStringAsync();

                        if (responseAsString == null)
                        {
                            throw new Exception("14");//_14_NULL_ERROR
                        }
                        else
                        {

                            Api_Response_Encrypted apiResponse = JsonSerializer.Deserialize<Api_Response_Encrypted>(responseAsString);


                            if (apiResponse != null)
                            {

                                if (apiResponse.code.Equals("1"))
                                {

                                    throw new Exception(apiResponse.message.ToString());

                                }
                                else
                                {
                                    return apiResponse.responseData;
                                }

                            }
                            else
                            {
                                throw new Exception("16");//_16_SERVER_RESPONSE_ERROR
                            }

                        }



                    }



                }

            }

        }

        public async static Task<string> Decrypt(Session_Data session, byte[] data_to_decrypt)
        {
            if (session == null || data_to_decrypt == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {

                using (var httpClient = new HttpClient())
                {
                    Decrypt_Data decryptData = new Decrypt_Data { SessionData = session, EncryptedData = data_to_decrypt };

                    StringContent content = new StringContent(JsonSerializer.Serialize(decryptData), Encoding.UTF8, "application/json");

                    using (var authResponse = await httpClient.PostAsync(System_Path.AUTHENTICATION_API+"Decrypt_Data", content))
                    {

                        var responseAsString = await authResponse.Content.ReadAsStringAsync();

                        if (responseAsString == null)
                        {
                            throw new Exception("14");//_14_NULL_ERROR
                        }
                        else
                        {

                            Api_Response_Decrypted apiResponse = JsonSerializer.Deserialize<Api_Response_Decrypted>(responseAsString);


                            if (apiResponse != null)
                            {

                                if (apiResponse.code.Equals("1"))
                                {

                                    throw new Exception(apiResponse.message.ToString());

                                }
                                else
                                {
                                    return apiResponse.responseData;
                                }

                            }
                            else
                            {
                                throw new Exception("16");//_16_SERVER_RESPONSE_ERROR
                            }

                        }



                    }



                }

            }

        }



        public async static Task<byte[]> EncryptByte(Session_Data session, byte[] data_to_encrypt)
        {
            if (session == null || data_to_encrypt == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {

                using (var httpClient = new HttpClient())
                {

                    Encrypt_Byte_Data encryptData = new Encrypt_Byte_Data { SessionData = session, DataToEncrypt = data_to_encrypt };

                    StringContent content = new StringContent(JsonSerializer.Serialize(encryptData), Encoding.UTF8, "application/json");

                    using (var authResponse = await httpClient.PostAsync(System_Path.AUTHENTICATION_API+"Encrypt_Byte_Data", content))
                    {

                        var responseAsString = await authResponse.Content.ReadAsStringAsync();

                        if (responseAsString == null)
                        {
                            throw new Exception("14");//_14_NULL_ERROR
                        }
                        else
                        {

                            Api_Response_Encrypted apiResponse = JsonSerializer.Deserialize<Api_Response_Encrypted>(responseAsString);


                            if (apiResponse != null)
                            {

                                if (apiResponse.code.Equals("1"))
                                {

                                    throw new Exception(apiResponse.message.ToString());

                                }
                                else
                                {
                                    return apiResponse.responseData;
                                }

                            }
                            else
                            {
                                throw new Exception("16");//_16_SERVER_RESPONSE_ERROR
                            }

                        }



                    }



                }

            }

        }

        public async static Task<byte[]> DecryptByte(Session_Data session, byte[] data_to_decrypt)
        {
            if (session == null || data_to_decrypt == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {

                using (var httpClient = new HttpClient())
                {
                    Decrypt_Byte_Data decryptData = new Decrypt_Byte_Data { SessionData = session, EncryptedData = data_to_decrypt };

                    StringContent content = new StringContent(JsonSerializer.Serialize(decryptData), Encoding.UTF8, "application/json");

                    using (var authResponse = await httpClient.PostAsync(System_Path.AUTHENTICATION_API+"Decrypt_Byte_Data", content))
                    {

                        var responseAsString = await authResponse.Content.ReadAsStringAsync();

                        if (responseAsString == null)
                        {
                            throw new Exception("14");//_14_NULL_ERROR
                        }
                        else
                        {

                            Api_Response_Encrypted apiResponse = JsonSerializer.Deserialize<Api_Response_Encrypted>(responseAsString);


                            if (apiResponse != null)
                            {

                                if (apiResponse.code.Equals("1"))
                                {

                                    throw new Exception(apiResponse.message.ToString());

                                }
                                else
                                {
                                    return apiResponse.responseData;
                                }

                            }
                            else
                            {
                                throw new Exception("16");//_16_SERVER_RESPONSE_ERROR
                            }

                        }



                    }



                }

            }

        }




        public async static Task<List<Encrypted_Object>> EncryptList(Session_Data session, List<Decrypted_Object> list_to_encrypt)
        {
            if (session == null || list_to_encrypt == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {

                using (var httpClient = new HttpClient())
                {
                    Encrypt_Data_List encryptDataList = new Encrypt_Data_List { SessionData = session, DataToEncrypt = list_to_encrypt };

                    StringContent content = new StringContent(JsonSerializer.Serialize(encryptDataList), Encoding.UTF8, "application/json");

                    using (var authResponse = await httpClient.PostAsync(System_Path.AUTHENTICATION_API+"Encrypt_Data_List", content))
                    {

                        var responseAsString = await authResponse.Content.ReadAsStringAsync();

                        if (responseAsString == null)
                        {
                            throw new Exception("14");//_14_NULL_ERROR
                        }
                        else
                        {

                            Api_Response_Encrypted_List apiResponse = JsonSerializer.Deserialize<Api_Response_Encrypted_List>(responseAsString);


                            if (apiResponse != null)
                            {

                                if (apiResponse.code.Equals("1"))
                                {

                                    throw new Exception(apiResponse.message.ToString());

                                }
                                else
                                {
                                    return apiResponse.responseData;
                                }

                            }
                            else
                            {
                                throw new Exception("16");//_16_SERVER_RESPONSE_ERROR
                            }

                        }



                    }



                }

            }

        }

        public async static Task<List<Decrypted_Object>> DecryptList(Session_Data session, List<Encrypted_Object> list_to_decrypt)
        {
            if (session == null || list_to_decrypt == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {

                using (var httpClient = new HttpClient())
                {
                    Decrypt_Data_List decryptDataList = new Decrypt_Data_List { SessionData = session, DataToDecrypt = list_to_decrypt };

                    StringContent content = new StringContent(JsonSerializer.Serialize(decryptDataList), Encoding.UTF8, "application/json");

                    using (var authResponse = await httpClient.PostAsync(System_Path.AUTHENTICATION_API+"Decrypt_Data_List", content))
                    {

                        var responseAsString = await authResponse.Content.ReadAsStringAsync();

                        if (responseAsString == null)
                        {
                            throw new Exception("14");//_14_NULL_ERROR
                        }
                        else
                        {

                            Api_Response_Decrypted_List apiResponse = JsonSerializer.Deserialize<Api_Response_Decrypted_List>(responseAsString);


                            if (apiResponse != null)
                            {

                                if (apiResponse.code.Equals("1"))
                                {

                                    throw new Exception(apiResponse.message.ToString());

                                }
                                else
                                {
                                    return apiResponse.responseData;
                                }

                            }
                            else
                            {
                                throw new Exception("16");//_16_SERVER_RESPONSE_ERROR
                            }

                        }



                    }



                }

            }

        }




        public static byte[] SystemEncrypt(string simpletext, byte[] key, byte[] iv)
        {
            byte[] cipheredtext;
            using (Aes aes = Aes.Create())
            {
                ICryptoTransform encryptor = aes.CreateEncryptor(key, iv);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(simpletext);
                            streamWriter.Flush();
                        }

                        cipheredtext = memoryStream.ToArray();
                    }
                }
            }
            return cipheredtext;
        }


        public static string SystemDecrypt(byte[] cipheredtext, byte[] key, byte[] iv)
        {
            string simpletext = String.Empty;
            using (Aes aes = Aes.Create())
            {
                ICryptoTransform decryptor = aes.CreateDecryptor(key, iv);
                using (MemoryStream memoryStream = new MemoryStream(cipheredtext))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            simpletext = streamReader.ReadToEnd();
                            streamReader.Close();
                        }
                    }
                }
            }
            return simpletext;
        }




        public static Keys GetKeys()
        {
            if(key == null || iv == null ||  key.Length == 0 || iv.Length == 0)ReadKeys();

            return new Keys { Key = key, Iv = iv };
        }


        public static string GetPasswd()
        {
            if (passwd == null ||  passwd.Length == 0 || passwd.Equals("") || passwd.Equals(string.Empty)) ReadPasswd();

            return passwd;
        }


        public static string GetAppCode()
        {
            if (appcode == null ||  appcode.Length == 0 || appcode.Equals("") || appcode.Equals(string.Empty)) ReadAppCode();

            return appcode;
        }




        public static void ReadKeys()
        {
            var path = System_Path.ENCRYPT_KEYS_PATH;

            if (File.Exists(path))
            {
                var stream = File.Open(path, FileMode.Open);

                using (BinaryReader reader = new BinaryReader(stream))
                {

                    key = reader.ReadBytes(16);
                    iv = reader.ReadBytes(16);

                    reader.Close();

                }

                stream.Close();


            }
            else
            {
                var stream = File.Create(path);
                using(BinaryWriter writer = new BinaryWriter(stream))
                {
                    RandomNumberGenerator rng = RandomNumberGenerator.Create();
                    key  = new byte[16];
                    iv = new byte[16];

                    rng.GetBytes(key);
                    rng.GetBytes(iv);

                    writer.Write(key);
                    writer.Write(iv);
                    
                    writer.Flush();
                    writer.Close();
                }
                stream.Close();
            }
        }

        

        public static void ReadPasswd()
        {

            var path = System_Path.PASSWD_PATH;
            Keys keysPasswd = GetKeys();

            if (File.Exists(path))
            {
                var stream = File.Open(path, FileMode.Open);

                using (BinaryReader reader = new BinaryReader(stream))
                {

                    byte[] pass = reader.ReadBytes(128);

                    passwd = SystemDecrypt(pass, keysPasswd.Key, keysPasswd.Iv);

                    reader.Close();

                }

                stream.Close();


            }
            else
            {
                var stream = File.Create(path);
                using (BinaryWriter writer = new BinaryWriter(stream))
                {

                    StreamReader reader = new StreamReader(path+".txt");

                    passwd = reader.ReadToEnd();
                    reader.Close();

                    byte[] pass = SystemEncrypt(passwd, keysPasswd.Key, keysPasswd.Iv);

                    writer.Write(pass);

                    writer.Flush();
                    writer.Close();
                }
                stream.Close();
            }
        }




        public static void ReadAppCode()
        {

            var path = System_Path.APPCODE_PATH;
            Keys keysAppCode = GetKeys();

            if (File.Exists(path))
            {
                var stream = File.Open(path, FileMode.Open);

                using (BinaryReader reader = new BinaryReader(stream))
                {

                    byte[] code = reader.ReadBytes(128);

                    appcode = SystemDecrypt(code, keysAppCode.Key, keysAppCode.Iv);

                    reader.Close();

                }

                stream.Close();


            }
            else
            {
                var stream = File.Create(path);
                using (BinaryWriter writer = new BinaryWriter(stream))
                {

                    StreamReader reader = new StreamReader(path+".txt");

                    appcode = reader.ReadToEnd();
                    reader.Close();

                    byte[] code = SystemEncrypt(appcode, keysAppCode.Key, keysAppCode.Iv);

                    writer.Write(code);

                    writer.Flush();
                    writer.Close();
                }
                stream.Close();
            }
        }



        public static decimal Round(decimal value, int precision)
        {

            return decimal.Round(value, precision,MidpointRounding.AwayFromZero);

        }




    }
}
