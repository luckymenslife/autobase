using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekod.Repository.SettingsFile;

namespace Rekod.MapFunctions
{
    public class ProxyParameters
    {
        public static void ProxyApplyChanges()
        {
            var proxy = Program.SettingsXML.LocalParameters.Proxy;
            switch (proxy.Status)
            {
                case eProxyStatus.Non:
                    {
                        Program.mainFrm1.axMapLIb1.ProxyDisable();
                    } break;
                case eProxyStatus.Customer:
                    {
                        string hostname = proxy.IP + ":" + proxy.Port;
                        string access = proxy.Login + ":" + Services.Encrypting.TryDecrypt(proxy.Password, "proxy_pwd");

                        Program.mainFrm1.axMapLIb1.ProxyEnable(hostname, access);
                    } break;
                default:
                    break;
            }
        }

    }
}
