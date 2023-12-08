using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;

namespace AIRdrop
{
    public static class RegistryConfig
    {
        public static bool InstallGBHandler()
        {
            string AppPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AIRdrop.exe");
            string protocolName = $"sonic3airdrop";
            try
            {
                var reg = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Sonic3AIRdrop");
                reg.SetValue("", $"URL:{protocolName}");
                reg.SetValue("URL Protocol", "");
                reg = reg.CreateSubKey(@"shell\open\command");
                reg.SetValue("", $"\"{AppPath}\" -download \"%1\"");
                reg.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
