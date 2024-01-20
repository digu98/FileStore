using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace fStore_client
{
    class Program
    {
        public static int Main(String[] args)
        {
            StartClient();
            return 0;
        }

        public static void StartClient()
        {
            byte[] bytes = new byte[1024];

            try
            {
                // + Szerverre kapcsolódási kód
                IPHostEntry host = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Fő socket try/catch
                try
                {
                    // Aktuális connect
                    sender.Connect(remoteEP);
                    
                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());


                    // Line testing kód
                    byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");

                    int bytesSent = sender.Send(msg);

                    int bytesRec = sender.Receive(bytes);
                    Console.WriteLine("Echoed test = {0}",
                        Encoding.ASCII.GetString(bytes, 0, bytesRec));


                    // + Fő operációs tömb
                    while (true) {
                        string input = Console.ReadLine();

                        // + Kilépés funkció
                        if(input == "quit") {
                            // Release the socket.
                            //sender.Shutdown(SocketShutdown.Both);
                            //sender.Close();

                        // + Fájllekérés szerverről
                        } else if (input == "fileRecieve"){

                            byte[] msgG = Encoding.ASCII.GetBytes(input);
                            sender.Send(msgG);

                            Console.WriteLine("Specify the file with the path you wish to download:");

                            int RecFromServer = sender.Receive(bytes);

                            string data = Encoding.ASCII.GetString(bytes, 0, RecFromServer);

                            Console.WriteLine(data);

                            string fileNameInput = Console.ReadLine();


                            // # VVV IMPLEMENTÁCIÓS VONAL - IMPLEMENTÁLÁS ALATT INNENTŐL VVV


                            string[] splitData = data.Split(' ');

                            using (FileStream myFile = File.Create(splitData[1])) {
                                
                                if (splitData[1].Contains(".png") || splitData[1].Contains(".jpg") || splitData[1].Contains(".jpeg") || splitData[1].Contains(".mp4")) {
                                    var imgBytes = new byte[Convert.ToInt32(splitData[2])];
                                    int imBytes = sender.Receive(imgBytes);

                                    myFile.Write(imgBytes, 0, imgBytes.Length);
                                } else {
                                    var fBytes = new byte[Convert.ToInt32(splitData[2])];
                                    int fileBytes = sender.Receive(fBytes);

                                    var fileData = Encoding.ASCII.GetString(fBytes, 0, fileBytes);
                                    byte[] fileByt = new UTF8Encoding(true).GetBytes(fileData);

                                    myFile.Write(fileByt, 0, fileByt.Length);
                                }
                            }

                            Console.WriteLine("Write successful.");
                    

                    // + Fájlküldés szervernek
                    } else if (input == "fileSend") {
                            try {
                                Console.WriteLine("Specify the file with the path you wish to upload:");

                                string CurrDir = Directory.GetCurrentDirectory();
                                string[] CurrDirFiles = Directory.GetFiles(CurrDir);

                                foreach (string file in CurrDirFiles) {
                                    Console.WriteLine(file);
                                }

                                string fileNameInput = Console.ReadLine();

                                string fileName = Path.GetFileName(fileNameInput);
                                Console.WriteLine("Sending {0} to the host.", fileName);

                                byte[] test = File.ReadAllBytes(fileName);

                                byte[] fileSendMsg = Encoding.ASCII.GetBytes("fileSend " + fileName + " " + test.Length + " <EOF>");

                                int sendFSendBytes = sender.Send(fileSendMsg);

                                sender.Send(test);
                                //sender.SendFile(fileName);

                                Console.WriteLine("File sent!");
                            } catch (Exception e) {
                                Console.WriteLine("ERROR AT FILE SEND, {0}", e.ToString());
                            }
                        } else {
                            byte[] new_bytes = new byte[1024];

                            if (input.Contains("<EOF>") == false) {
                                input = input + "<EOF>";
                            }

                            byte[] le_msg = Encoding.ASCII.GetBytes(input);

                            int sendBytes = sender.Send(le_msg);
                            Console.WriteLine("Message sent! :)");

                            int recBytes = sender.Receive(new_bytes);

                            Console.WriteLine("Echoed test = {0}",
                                Encoding.ASCII.GetString(new_bytes, 0, recBytes));
                        }
                    }

                }

                // + EXEPCIÓÓÓÓÓÓK
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.ReadLine();
        }
    }
}
