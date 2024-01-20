using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;


namespace fStore
{
    class Program
    {
        public static int Main(String[] args)
        {
            StartServer();
            return 0;
        }

        public static void StartServer()
        {
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            try {

                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEndPoint);
                listener.Listen(10);

                Console.WriteLine("Waiting for a connection...");
                Socket handler = listener.Accept();

                // Kliens adathandling
                string data = null;
                byte[] bytes = null;


                // + Fő operációs tömb
                while (true)
                {
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, bytesRec);


                    // + Fájlfogadás klienstől szerverre
                    if (data.Contains("fileSend")){
                        string[] splitData = data.Split(' ');

                        using (FileStream myFile = File.Create(splitData[1])) {
                            //NetworkStream fileData = new NetworkStream(handler);

                            //fileData.CopyTo(myFile);

                            if (splitData[1].Contains(".png") || splitData[1].Contains(".jpg") || splitData[1].Contains(".jpeg")) {
                                var imgBytes = new byte[Convert.ToInt32(splitData[2])];
                                int imBytes = handler.Receive(imgBytes);

                                myFile.Write(imgBytes, 0, imgBytes.Length);
                            } else {
                                var fBytes = new byte[Convert.ToInt32(splitData[2])];
                                int fileBytes = handler.Receive(fBytes);

                                var fileData = Encoding.ASCII.GetString(fBytes, 0, fileBytes);
                                byte[] fileByt = new UTF8Encoding(true).GetBytes(fileData);

                                myFile.Write(fileByt, 0, fileByt.Length);
                            }
                        }

                        Console.WriteLine("Write successful.");
                    

                    // + Fájllekérés szerverről kliensre
                    } else if(data.Contains("fileRecieve")) {
                        try {
                            

                            string CurrDir = Directory.GetCurrentDirectory();
                            var CurrDirFiles = Directory.GetFiles(CurrDir).Select(Path.GetFileName);

                            string fileNames = string.Join("\n", CurrDirFiles);

                            byte[] listBytes = Encoding.ASCII.GetBytes(fileNames);

                            Console.WriteLine(fileNames);
                            handler.Send(listBytes);

                            string fileNameInput = Console.ReadLine();

                            string fileName = Path.GetFileName(fileNameInput);
                            Console.WriteLine("Sending {0} to the requester.", fileName);

                            byte[] test = File.ReadAllBytes(fileName);

                            byte[] fileSendMsg = Encoding.ASCII.GetBytes("fileRecieve " + fileName + " " + test.Length + " <EOF>");

                            int sendFSendBytes = handler.Send(fileSendMsg);

                            handler.Send(test);
                            //sender.SendFile(fileName);

                            Console.WriteLine("File sent!");
                        } catch (Exception e) {
                            Console.WriteLine("ERROR AT FILE SEND, {0}", e.ToString());
                        }
                    } else if (data.IndexOf("<EOF>") > -1)
                    {
                        data.Replace("<EOF>", "");
                        Console.WriteLine("Text received : {0}", data);

                        byte[] msg = Encoding.ASCII.GetBytes(data);
                        handler.Send(msg);
                    }
                }

                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\n Press any key to continue...");
            Console.ReadKey();
        }

    }
}
