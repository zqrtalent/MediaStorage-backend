using System;
using System.IO;
using System.Diagnostics;
using MediaStorage.Encoder.Extensions;
using MediaStorage.IO;
using MediaStorage.IO.FileStream;
using MediaStorage.IO.GoogleDrive;

namespace MediaStorage.Encoder.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string mp3FilePath = args[0];

            var sw = new Stopwatch();
            sw.Start();

            string stateJsonInit = string.Empty;
             // Read json state.
            using(var fs = File.OpenRead($"{mp3FilePath}.json"))
            {
                byte[] arrJson = new byte[fs.Length];
                fs.Read(arrJson, 0, arrJson.Length);
                stateJsonInit = System.Text.ASCIIEncoding.UTF8.GetString(arrJson, 0, arrJson.Length);
            }

            using(IStorage storage = GoogleDriveStorageFactory.Create("", "", ""))
            //using(IStorage storage = new LocalStorage(""))
            {
                using( var file = storage.Open("/Wovenwar/Honor is Dead/World on Fire.mp3"))
                //using(var file = storage.Open("World on Fire.mp3"))
                {
                    using(var encoder = MediaEncoderExtension.EncoderByMediaType("mp3"))
                    {
                        if(encoder.Init(file, false, stateJsonInit))
                        {
                            var packets = encoder.ReadPackets(100, 50);

                            // // Save state json
                            // string stateJson;
                            // encoder.SaveStateIntoJson(true, out stateJson);

                            // // Save json state.
                            // using(var fs = File.OpenWrite($"{mp3FilePath}.json"))
                            // {
                            //     byte[] arrJson = System.Text.ASCIIEncoding.UTF8.GetBytes(stateJson);
                            //     fs.Write(arrJson, 0, arrJson.Length);
                            // }
                            
                            sw.Stop();
                            
                            Console.WriteLine($"Elapsed {sw.ElapsedMilliseconds}ms");
                        }
                    }
                }
            }
        }
    }
}
