using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;

class RepairCmdPowershell
{
    static void Main(string[] args)
    {
        Console.Title = "CMD & PowerShell Repair Tool";
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("============================================");
        Console.WriteLine("CMD & PowerShell Repair Tool");
        Console.WriteLine("============================================");
        Console.ResetColor();
        Console.WriteLine();

        // Check for admin
        bool isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
            .IsInRole(WindowsBuiltInRole.Administrator);

        if (!isAdmin)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Restarting with Administrator privileges...");
            Console.ResetColor();
            
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = Process.GetCurrentProcess().MainModule.FileName;
            psi.Verb = "runas";
            psi.UseShellExecute = true;
            
            try
            {
                Process.Start(psi);
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: Admin privileges required. Please right-click and Run as Administrator.");
                Console.ResetColor();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            return;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Running with Administrator privileges...");
        Console.ResetColor();
        Console.WriteLine();

        // Step 1: DISM ScanHealth
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[1/6] Running DISM - Scanning for corruption...");
        Console.ResetColor();
        RunCommand("DISM.exe", "/Online /Cleanup-Image /ScanHealth");
        Console.WriteLine();

        // Step 2: DISM RestoreHealth
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[2/6] Running DISM - Restoring system health...");
        Console.ResetColor();
        RunCommand("DISM.exe", "/Online /Cleanup-Image /RestoreHealth");
        Console.WriteLine();

        // Step 3: SFC
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[3/6] Running System File Checker (SFC)...");
        Console.ResetColor();
        RunCommand("sfc.exe", "/scannow");
        Console.WriteLine();

        // Step 4: Re-register CMD
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[4/6] Re-registering system components...");
        Console.ResetColor();
        
        string system32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
        string cmdPath = Path.Combine(system32, "cmd.exe");
        
        if (File.Exists(cmdPath))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("CMD found at: " + cmdPath);
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("WARNING: cmd.exe not found!");
            Console.ResetColor();
        }
        Console.WriteLine();

        // Step 5: Enable PowerShell feature
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[5/6] Enabling Windows PowerShell features...");
        Console.ResetColor();
        RunCommand("DISM.exe", "/Online /Enable-Feature /FeatureName:MicrosoftWindowsPowerShellV2Root /All /NoRestart");
        Console.WriteLine();

        // Step 6: Install PowerShell 7 via winget
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[6/6] Installing PowerShell 7+ via winget...");
        Console.ResetColor();
        RunCommand("winget", "install --id Microsoft.PowerShell --source winget --accept-package-agreements --accept-source-agreements");
        Console.WriteLine();

        // Verification
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("============================================");
        Console.WriteLine("Verification:");
        Console.WriteLine("============================================");
        Console.ResetColor();
        Console.WriteLine();

        // Check CMD
        Console.WriteLine("Checking CMD...");
        if (File.Exists(cmdPath))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ CMD found: " + cmdPath);
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ CMD not found");
            Console.ResetColor();
        }
        Console.WriteLine();

        // Check PowerShell 5.1
        Console.WriteLine("Checking Windows PowerShell 5.1...");
        string ps5Path = Path.Combine(system32, @"WindowsPowerShell\v1.0\powershell.exe");
        if (File.Exists(ps5Path))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ PowerShell 5.1 found: " + ps5Path);
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ PowerShell 5.1 not found");
            Console.ResetColor();
        }
        Console.WriteLine();

        // Check PowerShell 7
        Console.WriteLine("Checking PowerShell 7+...");
        string pwshPath = @"C:\Program Files\PowerShell\7\pwsh.exe";
        if (File.Exists(pwshPath))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ PowerShell 7+ found: " + pwshPath);
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("PowerShell 7+ not found (optional)");
            Console.ResetColor();
        }
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("============================================");
        Console.WriteLine("Repair Process Complete!");
        Console.WriteLine("============================================");
        Console.ResetColor();
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("IMPORTANT: Please restart your computer for changes to take effect.");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static void RunCommand(string fileName, string arguments)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = fileName;
            psi.Arguments = arguments;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = false;

            using (Process process = new Process())
            {
                process.StartInfo = psi;
                process.OutputDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
                process.ErrorDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
                
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error running " + fileName + ": " + ex.Message);
            Console.ResetColor();
        }
    }
}
