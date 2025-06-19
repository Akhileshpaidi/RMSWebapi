using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
//using System.Web.Script.Serialization;


namespace ITR_TelementaryAPI
{
    public class Auth_ServiceResponse
    {
        public string Flag { get; set; }
        public string Status { get; set; }
        public string oAuth { get; set; }
    }
    public class ClsGlobal
    {
        //public static string AuthURL = ConfigurationManager.AppSettings["AuthURL"];
        public static string EncryptionKey = "(y6er1@n$1234567";
        public static string Salt = "0001000100010001";

        public string EncryptAES(string clearText)
        {
            byte[] clearBytes = Encoding.UTF8.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                encryptor.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                encryptor.IV = Encoding.UTF8.GetBytes(Salt);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public string DecryptAES(string cipherText)
        {

            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                encryptor.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                encryptor.IV = Encoding.UTF8.GetBytes(Salt);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
        public string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }
        public static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            string returnValue = System.Text.ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
            return returnValue;
        }
        //public Auth_ServiceResponse IsAuthenticated_WebService_JAVA(string encrypted_Token)
        //{
        //    Auth_ServiceResponse Obj_ServiceResponse = new Auth_ServiceResponse();
        //    try
        //    {
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //        var httpWebRequest = (HttpWebRequest)WebRequest.Create(ClsGlobal.AuthURL);
        //        httpWebRequest.ContentType = "application/json";
        //        httpWebRequest.Method = "POST";
        //        using (var streamWriter = new

        //        StreamWriter(httpWebRequest.GetRequestStream()))
        //        {
        //            string json = "{\"tokenid\":\"" + encrypted_Token + "\"}";
        //            streamWriter.Write(json);
        //        }
        //        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        //        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        //        {
        //            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
        //            Object ServiceResponse = jsSerializer.DeserializeObject(streamReader.ReadToEnd());
        //            System.Collections.Generic.Dictionary<string, object> obj2 = new System.Collections.Generic.Dictionary<string, object>();
        //            obj2 = (System.Collections.Generic.Dictionary<string, object>)(ServiceResponse);
        //            string status = obj2["access_token"].ToString();
        //            if (status.ToUpper() == "FAIL")
        //            {
        //                Obj_ServiceResponse.Flag = "1";
        //                Obj_ServiceResponse.Status = "Login Authentication Failed!!!";
        //            }
        //            else
        //            {
        //                Obj_ServiceResponse.Flag = "0";
        //                Obj_ServiceResponse.Status = "Success";
        //                Obj_ServiceResponse.oAuth = status;
        //            }
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        Obj_ServiceResponse.Flag = "2";
        //        Obj_ServiceResponse.Status = ex.Message;
        //    }
        //    return Obj_ServiceResponse;
        //}
    }
}