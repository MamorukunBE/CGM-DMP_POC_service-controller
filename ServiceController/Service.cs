using constants;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace App.WindowsService_controller;

public sealed class Controller
{
    public string CheckServiceNewVersion(string serviceId)
    {
        ServiceController[] scServices;
        scServices = ServiceController.GetServices();
        string res = "";
        foreach (ServiceController sc in scServices)
        {
            if (sc.ServiceName == serviceId)
            {
                res += $"{sc.ServiceName} - {sc.DisplayName} - {sc.MachineName}\n";
            }
        }
        Debug.WriteLine($"Checking Service updates ({res})");
        return $"Checking Service updates ({res})";
    }
}
