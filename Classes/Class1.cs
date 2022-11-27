using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Classes
{
    public class DescXD
    {
        public int Id { get; set; }
        public long TelegramId { get; set; }
        public string Description { get; set; }
    }
   public class _User:IComparable
    {
        public string FirstName { get; set; }
        public string Username { get; set; }
        public long UserId { get; set; }
        public DateTime LastSendMessage { get; set; }
        public bool Admin { get; set; }
        public int CountMsg { get; set; }
        public DateTime Mute { get; set; }

        public int CompareTo(object obj)
        {
            return (obj as _User).CountMsg.CompareTo(CountMsg);
        }
    }

    public class Rootobject
    {
        public Coord coord { get; set; }
        public Weather[] weather { get; set; }
        public string _base { get; set; }
        public Main1 main { get; set; }
        public int visibility { get; set; }
        public Wind wind { get; set; }
        public Clouds clouds { get; set; }
        public int dt { get; set; }
        public Sys sys { get; set; }
        public int timezone { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int cod { get; set; }
    }

    public class Coord
    {
        public float lon { get; set; }
        public float lat { get; set; }
    }

    public class Main1
    {
        public float temp { get; set; }
        public float feels_like { get; set; }
        public float temp_min { get; set; }
        public float temp_max { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
    }

    public class Wind
    {
        public float speed { get; set; }
        public int deg { get; set; }
    }

    public class Clouds
    {
        public int all { get; set; }
    }

    public class Sys
    {
        public int type { get; set; }
        public int id { get; set; }
        public string country { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
    }

    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }
    public static class MD5Encryptor
    {
        public static string MD5Hash(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            text += "jhsdfas234h3v2346dhjad";
            //compute hash from the bytes of text  
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));

            //get hash result after compute it  
            byte[] result = md5.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                //change it into 2 hexadecimal digits  
                //for each byte  
                strBuilder.Append(result[i].ToString("x2"));
            }

            return strBuilder.ToString();
        }
    }
    public static class CesarEncryptor
    {
       public static string CesarEncrypt(string text,int index)
        {
            string EncStr = "";
            for (int i = 0; i < text.Length; i++)
            {
                EncStr +=(char)((int)text[i] + index);
            }
            return EncStr;
        }

        public static string CesarDecrypt(string text, int index)
        {
            string EncStr = "";
            for (int i = 0; i < text.Length; i++)
            {
                EncStr += (char)((int)text[i] - index);
            }
            return EncStr;
        }
    }
}
