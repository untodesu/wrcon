using System.IO;
using System.Security.Cryptography;
using System;
using System.Text;

namespace WRcon.Core
{
    internal class Config
    {
        public string Address { get; set; } = "127.0.0.1";
        public string Port { get; set; } = "27015";
        public string Password { get; set; } = "";

        public readonly string Filename;

        // MD5 result has length of 128 bits. So does AES key.
        // I ain't aiming for too much security for a fucking rcon client.
        private readonly byte[] key = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Environment.UserName));

        public Config(string filename = ".wrcon")
        {
            Filename = filename;
            if(File.Exists(Filename)) {
                try {
                    using(FileStream fs = new FileStream(Filename, FileMode.Open, FileAccess.Read)) {
                        using(Aes aes = Aes.Create()) {
                            byte[] iv = new byte[aes.IV.Length];
                            fs.Read(iv, 0, iv.Length);
                            using(CryptoStream cs = new CryptoStream(fs, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read)) {
                                using(StreamReader sr = new StreamReader(cs)) {
                                    Address = sr.ReadLine();
                                    Port = sr.ReadLine();
                                    Password = sr.ReadLine();
                                }
                            }
                        }
                    }
                }
                catch { }
            }
        }

        public void Save()
        {
            try {
                using(FileStream fs = new FileStream(Filename, FileMode.OpenOrCreate, FileAccess.Write)) {
                    using(Aes aes = Aes.Create()) {
                        aes.Key = key;
                        fs.Write(aes.IV, 0, aes.IV.Length);
                        using(CryptoStream cs = new CryptoStream(fs, aes.CreateEncryptor(), CryptoStreamMode.Write)) {
                            using(StreamWriter sw = new StreamWriter(cs)) {
                                sw.WriteLine(Address);
                                sw.WriteLine(Port);
                                sw.WriteLine(Password);
                            }
                        }
                    }
                }

            }
            catch { }
        }
    }
}
