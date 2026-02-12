using System.Net.NetworkInformation;

namespace DeMasterProCloud.Common.LicenseHelpers
{
    public class MachineHelpers
    {
        public static string GetLocalMacAddress()
        {
            string macAddresses = "";
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Only consider Ethernet network interfaces, thereby ignoring any
                // loopback devices etc.
                if (nic.NetworkInterfaceType != NetworkInterfaceType.Ethernet) continue;
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    macAddresses += nic.GetPhysicalAddress().ToString();
                    break;
                }
            }
            return macAddresses;
        }

        public static string GetLocalOsDescription()
        {
            return System.Runtime.InteropServices.RuntimeInformation.OSDescription;
        }

        public static string GetLocalOsIdentifier()
        {
            return System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier;
        }

        public static string GetLocalMachineName()
        {
            return System.Environment.MachineName;
        }
    }
}