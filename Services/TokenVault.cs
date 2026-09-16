using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DiscordSecurityBot.Services;

public static class TokenVault
{
    public static string? GetDiscordToken(IConfigurationSection config)
    {
        Console.Write("Enter the password for the KeePassXC database: ");
        return FetchFromKeePass(MaskedPasswordInput(), config);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="password"></param>
    /// <param name="config"></param>
    /// <returns>Bot Token</returns>
    private static string? FetchFromKeePass(string password, IConfigurationSection config)
    {
        string? cliPath = config["CliPath"];
        string? dbPath = config["DbPath"];
        string keyFilePath = config["KeyFilePath"] ?? "";
        string entryName = config["EntryName"] ?? "Token";

        if (string.IsNullOrEmpty(dbPath) || string.IsNullOrEmpty(cliPath) || string.IsNullOrEmpty(entryName))
        {
            Console.WriteLine("\nError. Check appsettings.json");
            return null;
        }

        if (!string.IsNullOrWhiteSpace(keyFilePath) && !File.Exists(keyFilePath))
        {
            Console.WriteLine($"\nError. Key file not found: {keyFilePath}");
            return null;
        }

        ProcessStartInfo startInfo = new()
        {
            FileName = cliPath,
            ArgumentList = {
                "show",
                "-a", "Password",
                "-k",
                keyFilePath,
                dbPath,
                entryName
            },
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Start KeePassXC CLI
        using (Process process = new() { StartInfo = startInfo })
        {
            try
            {
                process.Start();

                process.StandardInput.WriteLine(password);
                process.StandardInput.Close();

                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                if (!process.WaitForExit(TimeSpan.FromSeconds(15)))
                {
                    process.Kill(entireProcessTree: true);
                    Console.Error.WriteLine("\nKeePassXC timed out.");
                    return null;
                }

                string token = outputTask.Result.Trim();
                string error = errorTask.Result;

                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"\nKeePassXC Error. Exit Code: {process.ExitCode}");
                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Details: {error}");
                    }
                    return null;
                }

                return token;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                Console.WriteLine($"{cliPath} — not found.");
                return null;
            }
        }
    }

    /// <summary>
    /// Masked password into ****
    /// </summary>
    /// <returns>Password</returns>
    private static string MaskedPasswordInput()
    {
        if (Console.IsInputRedirected)
        {
            return Console.ReadLine() ?? string.Empty;
        }

        StringBuilder sb = new();
        ConsoleKeyInfo key;
        while ((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Backspace)
            {
                if (sb.Length > 0)
                {
                    sb.Length--;
                    Console.Write("\b \b");
                }
                continue;
            }

            if (!char.IsControl(key.KeyChar))
            {
                sb.Append(key.KeyChar);
                Console.Write('*');
            }
        }
        Console.WriteLine();
        return sb.ToString();
    }
}

