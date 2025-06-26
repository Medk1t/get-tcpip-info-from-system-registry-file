using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace GetCompInfo
{
    public class RegistryLoader
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int RegLoadKey(UIntPtr hKey, string lpSubKey, string lpFile);
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int RegUnLoadKey(UIntPtr hKey, string lpSubKey);
        private static readonly UIntPtr HKEY_LOCAL_MACHINE = (UIntPtr)0x80000002;
        public static void LoadAndReadSystemHive(string hivePath)
        {
            string tempKeyName = "TempSysHive";
            int result = RegLoadKey(HKEY_LOCAL_MACHINE, tempKeyName, hivePath);
            if (result != 0)
            {
                Console.WriteLine($"Ошибка загрузки реестра: {result}");
                return;
            }

            try
            {
                string parametersKeypath = $@"{tempKeyName}\ControlSet001\Services\Tcpip\Parameters";
                string hostname = string.Empty;
                string dhcpDomain = string.Empty;
                string dhcpNameServer = string.Empty;
                string dhcpIpAdress = string.Empty;
                string dhcpDefaultGateway = string.Empty;
                string dhcpSubnetMask = string.Empty;
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(parametersKeypath))
                {
                    if (key != null)
                    {
                        using (FileStream stream = new FileStream($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\{key.GetValue("Hostname") ?? "Tcpip"}.txt", FileMode.Create, FileAccess.Write))
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            //foreach (var item in key.GetValueNames())
                            //{
                            //    writer.WriteLine($"{item} = {key.GetValue(item)}");
                            //}
                            hostname = key.GetValue("Hostname")?.ToString();
                            dhcpDomain = (key.GetValue("DhcpDomain") ?? key.GetValue("Domain"))?.ToString();
                            using (RegistryKey interfacesKeys = key.OpenSubKey("Interfaces"))
                            {
                                foreach (var item in interfacesKeys.GetSubKeyNames())
                                {
                                    using (RegistryKey interfaceKey = interfacesKeys.OpenSubKey(item))
                                    {
                                        //foreach (var itm in interfaceKey.GetValueNames())
                                        //{
                                        //    writer.WriteLine($"\t{itm} = {interfaceKey.GetValue(itm)}");
                                        //}
                                        if ((interfaceKey.GetValue("DhcpIPAddress") ?? interfaceKey.GetValue("IPAddress")) != null)
                                            dhcpIpAdress += interfaceKey.GetValue("DhcpIPAddress") as string ?? string.Join("", (interfaceKey.GetValue("IPAddress") as string[]) ?? new string[1]);
                                        if ((interfaceKey.GetValue("DhcpSubnetMask") ?? interfaceKey.GetValue("SubnetMask")) != null)
                                            dhcpSubnetMask += (interfaceKey.GetValue("DhcpSubnetMask") as string ?? string.Join("", (interfaceKey.GetValue("SubnetMask") as string[]) ?? new string[1]));
                                        if ((interfaceKey.GetValue("DhcpNameServer") ?? interfaceKey.GetValue("NameServer")) != null)
                                            dhcpNameServer += (interfaceKey.GetValue("DhcpNameServer") as string ?? interfaceKey.GetValue("NameServer") as string);
                                        if ((interfaceKey.GetValue("DhcpDefaultGateway") ?? interfaceKey.GetValue("DefaultGateway")) != null)
                                            dhcpDefaultGateway += interfaceKey.GetValue("DhcpDefaultGateway") as string ?? string.Join("", (interfaceKey.GetValue("DefaultGateway") as string[])?? new string[1]);
                                    }
                                }
                            }
                            writer.WriteLine($"Hostname = {hostname}");
                            writer.WriteLine($"DhcpDomain = {dhcpDomain}");
                            writer.WriteLine($"DhcpIPAddress = {dhcpIpAdress}");
                            writer.WriteLine($"DhcpDefaultGateway = {dhcpDefaultGateway}");
                            writer.WriteLine($"DhcpSubnetMask = {dhcpSubnetMask}");
                            writer.WriteLine($"DhcpNameServer = {dhcpNameServer}");
                            writer.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки значения реестра {ex.Message}");
                throw;
            }
            RegUnLoadKey(HKEY_LOCAL_MACHINE, tempKeyName);
        }
    }
}
