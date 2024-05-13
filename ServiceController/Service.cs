using constants;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace App.WindowsService_controller;

public sealed class Controller
{
    private Version CheckServiceCurrentVersion(ServiceController sc)
    {
        string registryPath = @"SYSTEM\CurrentControlSet\Services\" + sc.ServiceName;
        RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default).OpenSubKey(registryPath);
        if (key == null)
            throw new ArgumentException("Non-existent service: " + sc.ServiceName, "sc.ServiceName");
        string value = key.GetValue("ImagePath").ToString();
        key.Close();
        //-----
        Match nameMatch = Regex.Match(value, "^\"([^\"]+)\"");
        string name = nameMatch != null ? nameMatch.Groups[1].Value : value;
        string filePath = Environment.ExpandEnvironmentVariables(name);
        FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(filePath);
        return new Version(fileVersion.FileMajorPart, fileVersion.FileMinorPart, fileVersion.FileBuildPart, fileVersion.FilePrivatePart);
    }

    private async Task<Version> CheckServiceNewestVersionAsync()
    {
        string fileUrl = "http://localhost/";
        using (var httpClient = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(fileUrl);
                if (response.IsSuccessStatusCode)
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        string fileContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"File content length: {fileContent}");
                        if (Regex.Match(fileContent, @"^.d+\.d+\.d+\.d+$", RegexOptions.IgnoreCase) == null)
                            throw new ArgumentException($"Version file content not valid: {fileContent}");
                        return new Version(fileContent);
                    }
                }
                else throw new IOException($"Failed to download file. Status code: {response.StatusCode}");
            }
            catch (HttpRequestException ex)
            { throw new IOException("Error downloading file: network error", ex); }
            catch (Exception ex)
            { throw new IOException("Error downloading file: generic error", ex); }
        }
    }

    public async Task<string> ProcessServiceUpdateAsync(string serviceId)
    {
        ServiceController[] scServices;
        string res = "", errorMsg = null;
        ServiceControllerStatus preStatus, postStatus;
        ServiceController sc = ServiceController.GetServices().Where(s => s.ServiceName == serviceId).FirstOrDefault();
        if (sc != null)
        {
            try {
                Version curVersion = CheckServiceCurrentVersion(sc);
                Version newestVersion = await CheckServiceNewestVersionAsync();
                if (newestVersion > curVersion)
                {
                    preStatus = sc.Status;
                    sc.Stop();
                    sc.Refresh();
                    postStatus = sc.Status;
                    res += $"{sc.ServiceName} - {sc.DisplayName} - {sc.MachineName} - preStatus: {preStatus} - psotStatus: {postStatus}\n";
                }
            }
            catch (ArgumentException ex) { errorMsg = ex.Message; }
        }

        string resultMsg = errorMsg ?? "WELL DONE";
        return $"Checking Service updates ({res})\n{resultMsg}";
    }
}
