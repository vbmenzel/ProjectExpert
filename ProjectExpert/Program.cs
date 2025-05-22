using System.Net;
using System.Net.NetworkInformation;
using System.Text;

/// <summary>
/// An advanced command-line shell providing networking and file system utilities.
/// Features include smart command parsing, async operations, and extensible command architecture.
/// </summary>
class NetworkShell
{
    #region Constants and Configuration
    private static readonly ConsoleColor ErrorColor = ConsoleColor.Red;
    private static readonly ConsoleColor InfoColor = ConsoleColor.Cyan;
    private static readonly ConsoleColor WarningColor = ConsoleColor.Yellow;
    
    private static readonly string PromptArrow = "▶";
    private static readonly string PromptSymbol = "> ";
    #endregion

    #region State Management
    private static CancellationTokenSource _cancellationTokenSource = new();
    public static readonly Dictionary<string, ICommand> Commands = new();
    #endregion

    #region Main Entry Point
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        InitializeCommands();
        SetupCancellationHandler();
        WriteColoredLine("Network Shell v2.0 - Type '?' for help", InfoColor);
        Console.WriteLine();

        await RunCommandLoop();
    }

    private static void InitializeCommands()
    {
        var commands = new ICommand[]
        {
            new HelpCommand(),
            new ClearCommand(),
            new PingCommand(),
            new CatCommand(),
            new DirectoryCommand(),
            new ExitCommand()
        };

        foreach (var command in commands)
        {
            foreach (var alias in command.Aliases)
            {
                Commands[alias.ToLower()] = command;
            }
        }
    }

    private static void SetupCancellationHandler()
    {
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
        };
    }

    private static async Task RunCommandLoop()
    {
        while (true)
        {
            DisplayPrompt();
            var input = Console.ReadLine()?.Trim();
            
            if (string.IsNullOrEmpty(input)) continue;

            var (command, args) = ParseCommand(input);
            
            if (Commands.TryGetValue(command.ToLower(), out var commandHandler))
            {
                try
                {
                    await commandHandler.ExecuteAsync(args, _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    WriteColoredLine("\nOperation canceled.", WarningColor);
                }
                catch (Exception ex)
                {
                    WriteColoredLine($"Error: {ex.Message}", ErrorColor);
                }
            }
            else
            {
                WriteColoredLine($"Unknown command: {command}", ErrorColor);
                WriteColoredLine("Type '?' for available commands", InfoColor);
            }
            
            Console.WriteLine();
        }
    }

    private static void DisplayPrompt()
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write(PromptArrow);
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write(PromptSymbol);
        Console.ResetColor();
    }

    private static (string command, string[] args) ParseCommand(string input)
    {
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 
            ? (parts[0], parts.Skip(1).ToArray()) 
            : (string.Empty, Array.Empty<string>());
    }
    #endregion

    #region Utility Methods
    public static void WriteColoredLine(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void WriteColored(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(message);
        Console.ResetColor();
    }

    public static bool IsValidHost(string host)
    {
        if (IPAddress.TryParse(host, out _)) return true;
        
        try
        {
            var addresses = Dns.GetHostAddresses(host);
            return addresses.Length > 0;
        }
        catch
        {
            return false;
        }
    }
    #endregion
}

#region Command Interface and Base Classes
public interface ICommand
{
    string[] Aliases { get; }
    string Description { get; }
    string Usage { get; }
    Task ExecuteAsync(string[] args, CancellationToken cancellationToken);
}

public abstract class BaseCommand : ICommand
{
    public abstract string[] Aliases { get; }
    public abstract string Description { get; }
    public abstract string Usage { get; }
    
    public virtual Task ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args.Length > 0 && args[0].Equals("?", StringComparison.OrdinalIgnoreCase))
        {
            ShowHelp();
            return Task.CompletedTask;
        }
        
        return ExecuteCommandAsync(args, cancellationToken);
    }
    
    protected abstract Task ExecuteCommandAsync(string[] args, CancellationToken cancellationToken);
    
    protected virtual void ShowHelp()
    {
        NetworkShell.WriteColoredLine($"{Description}", ConsoleColor.Green);
        NetworkShell.WriteColoredLine($"Usage: {Usage}", ConsoleColor.Cyan);
    }
}
#endregion

#region Command Implementations
public class HelpCommand : BaseCommand
{
    public override string[] Aliases => new[] { "help", "?", "commands" };
    public override string Description => "Display help information";
    public override string Usage => "help [command]";

    protected override Task ExecuteCommandAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args.Length > 0)
        {
            ShowSpecificHelp(args[0]);
        }
        else
        {
            ShowGeneralHelp();
        }
        return Task.CompletedTask;
    }

    private void ShowGeneralHelp()
    {
        NetworkShell.WriteColoredLine("Available Commands:", ConsoleColor.Yellow);
        Console.WriteLine();
        
        var commandGroups = NetworkShell.Commands.Values
            .Distinct()
            .GroupBy(cmd => cmd.GetType())
            .Select(g => g.First());

        foreach (var command in commandGroups)
        {
            NetworkShell.WriteColored($"  {string.Join(", ", command.Aliases)}", ConsoleColor.Cyan);
            Console.WriteLine($" - {command.Description}");
        }
        
        Console.WriteLine();
        NetworkShell.WriteColoredLine("Type '<command> ?' for detailed help on a specific command", ConsoleColor.Gray);
    }

    private void ShowSpecificHelp(string commandName)
    {
        if (NetworkShell.Commands.TryGetValue(commandName.ToLower(), out var command))
        {
            command.ExecuteAsync(new[] { "?" }, CancellationToken.None);
        }
        else
        {
            NetworkShell.WriteColoredLine($"No help available for '{commandName}'", ConsoleColor.Red);
        }
    }
}

public class ClearCommand : BaseCommand
{
    public override string[] Aliases => new[] { "cls", "clear" };
    public override string Description => "Clear the console screen";
    public override string Usage => "cls";

    protected override Task ExecuteCommandAsync(string[] args, CancellationToken cancellationToken)
    {
        Console.Clear();
        return Task.CompletedTask;
    }
}

public class ExitCommand : BaseCommand
{
    public override string[] Aliases => new[] { "exit", "quit", "q" };
    public override string Description => "Exit the shell";
    public override string Usage => "exit";

    protected override Task ExecuteCommandAsync(string[] args, CancellationToken cancellationToken)
    {
        Environment.Exit(0);
        return Task.CompletedTask;
    }
}

public class DirectoryCommand : BaseCommand
{
    public override string[] Aliases => new[] { "ls", "dir" };
    public override string Description => "List directory contents";
    public override string Usage => "ls [path]";

    protected override Task ExecuteCommandAsync(string[] args, CancellationToken cancellationToken)
    {
        var path = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
        
        if (!Directory.Exists(path))
        {
            NetworkShell.WriteColoredLine($"Directory not found: {path}", ConsoleColor.Red);
            return Task.CompletedTask;
        }

        try
        {
            NetworkShell.WriteColoredLine($"Directory: {Path.GetFullPath(path)}", ConsoleColor.Yellow);
            Console.WriteLine();
            
            var entries = Directory.GetFileSystemEntries(path)
                .OrderBy(entry => Directory.Exists(entry) ? 0 : 1)
                .ThenBy(Path.GetFileName);

            foreach (var entry in entries)
            {
                var name = Path.GetFileName(entry);
                var color = Directory.Exists(entry) ? ConsoleColor.Blue : ConsoleColor.White;
                var prefix = Directory.Exists(entry) ? "[DIR] " : "      ";
                
                NetworkShell.WriteColored(prefix, ConsoleColor.Gray);
                NetworkShell.WriteColoredLine(name, color);
            }
        }
        catch (UnauthorizedAccessException)
        {
            NetworkShell.WriteColoredLine("Access denied to directory", ConsoleColor.Red);
        }
        catch (Exception ex)
        {
            NetworkShell.WriteColoredLine($"Error listing directory: {ex.Message}", ConsoleColor.Red);
        }
        
        return Task.CompletedTask;
    }
}

public class CatCommand : BaseCommand
{
    public override string[] Aliases => new[] { "cat", "type" };
    public override string Description => "Display file contents";
    public override string Usage => "cat <filename> [filename2] ...";

    protected override async Task ExecuteCommandAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            return;
        }

        foreach (var filePath in args)
        {
            await DisplayFile(filePath, cancellationToken);
            if (args.Length > 1) Console.WriteLine();
        }
    }

    private async Task DisplayFile(string filePath, CancellationToken cancellationToken)
    {
        var actualPath = FindFile(filePath);
        
        if (actualPath == null)
        {
            NetworkShell.WriteColoredLine($"File not found: {filePath}", ConsoleColor.Red);
            return;
        }

        try
        {
            NetworkShell.WriteColoredLine($"=== {actualPath} ===", ConsoleColor.Green);
            
            var content = await File.ReadAllTextAsync(actualPath, cancellationToken);
            Console.WriteLine(content);
        }
        catch (UnauthorizedAccessException)
        {
            NetworkShell.WriteColoredLine($"Access denied: {actualPath}", ConsoleColor.Red);
        }
        catch (Exception ex)
        {
            NetworkShell.WriteColoredLine($"Error reading file: {ex.Message}", ConsoleColor.Red);
        }
    }

    private string? FindFile(string filePath)
    {
        if (File.Exists(filePath)) return filePath;
        
        var withTxtExtension = filePath + ".txt";
        if (File.Exists(withTxtExtension)) return withTxtExtension;
        
        return null;
    }
}

public class PingCommand : BaseCommand
{
    public override string[] Aliases => new[] { "ping" };
    public override string Description => "Send ICMP echo requests to network hosts";
    public override string Usage => "ping <host> [-t] [-n count] [-i ttl] [-w timeout] [-l size] [| filename]";

    protected override async Task ExecuteCommandAsync(string[] args, CancellationToken cancellationToken)
    {
        var options = ParsePingOptions(args);
        if (options == null) return;

        await ExecutePing(options, cancellationToken);
    }

    protected override void ShowHelp()
    {
        base.ShowHelp();
        Console.WriteLine();
        NetworkShell.WriteColoredLine("Options:", ConsoleColor.Yellow);
        Console.WriteLine("  -t           Ping continuously until stopped");
        Console.WriteLine("  -n count     Number of echo requests (default: 4)");
        Console.WriteLine("  -i ttl       Time To Live value (default: 128)");
        Console.WriteLine("  -w timeout   Timeout in milliseconds (default: 5000)");
        Console.WriteLine("  -l size      Buffer size in bytes (default: 32)");
        Console.WriteLine("  | filename   Save output to file");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  ping google.com");
        Console.WriteLine("  ping 8.8.8.8 -t");
        Console.WriteLine("  ping localhost -n 10 -l 64");
    }

    private PingOptions? ParsePingOptions(string[] args)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            return null;
        }

        var host = args[0];
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            host = "127.0.0.1";

        if (!NetworkShell.IsValidHost(host))
        {
            NetworkShell.WriteColoredLine($"Cannot resolve host: {host}", ConsoleColor.Red);
            return null;
        }

        var options = new PingOptions
        {
            Host = host,
            Continuous = false,
            Count = 4,
            Ttl = 128,
            Timeout = 5000,
            BufferSize = 32,
            OutputFile = null
        };

        for (int i = 1; i < args.Length; i++)
        {
            var arg = args[i].ToLower();
            
            switch (arg)
            {
                case "-t":
                    options.Continuous = true;
                    break;
                case "-n" when i + 1 < args.Length && int.TryParse(args[i + 1], out int count) && count > 0:
                    options.Count = count;
                    i++;
                    break;
                case "-i" when i + 1 < args.Length && int.TryParse(args[i + 1], out int ttl):
                    options.Ttl = ttl;
                    i++;
                    break;
                case "-w" when i + 1 < args.Length && int.TryParse(args[i + 1], out int timeout):
                    options.Timeout = timeout;
                    i++;
                    break;
                case "-l" when i + 1 < args.Length && int.TryParse(args[i + 1], out int size) && size > 0 && size <= 65500:
                    options.BufferSize = size;
                    i++;
                    break;
                case "|" when i + 1 < args.Length:
                    options.OutputFile = args[i + 1];
                    if (!Path.HasExtension(options.OutputFile))
                        options.OutputFile += ".txt";
                    i++;
                    break;
                default:
                    NetworkShell.WriteColoredLine($"Invalid argument: {arg}", ConsoleColor.Red);
                    ShowHelp();
                    return null;
            }
        }

        return options;
    }

    private async Task ExecutePing(PingOptions options, CancellationToken cancellationToken)
    {
        var output = new StringBuilder();
        var stats = new PingStatistics();
        
        using var ping = new Ping();
        var pingOptions = new System.Net.NetworkInformation.PingOptions
        {
            Ttl = options.Ttl,
            DontFragment = true
        };

        var buffer = CreateBuffer(options.BufferSize);
        var header = $"Pinging {options.Host} with {options.BufferSize} bytes of data:";
        
        Console.WriteLine(header);
        output.AppendLine(header);

        try
        {
            var pingCount = 0;
            var startTime = DateTime.UtcNow;

            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                stats.Sent++;
                var reply = await ping.SendPingAsync(options.Host, options.Timeout, buffer, pingOptions);
                
                var message = FormatPingReply(reply, buffer.Length);
                Console.WriteLine(message);
                output.AppendLine(message);

                if (reply.Status == IPStatus.Success)
                {
                    stats.UpdateSuccess(reply.RoundtripTime);
                }

                if ((options.Continuous && !cancellationToken.IsCancellationRequested) || 
                    (!options.Continuous && ++pingCount < options.Count))
                {
                    await Task.Delay(1000, cancellationToken);
                }
                else
                {
                    break;
                }
            } while (true);
        }
        catch (OperationCanceledException)
        {
            // Graceful cancellation
        }

        DisplayStatistics(options.Host, stats, output);
        await SaveOutputIfRequested(options.OutputFile, output.ToString());
    }

    private byte[] CreateBuffer(int size)
    {
        var buffer = new byte[size];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)('a' + (i % 26));
        }
        return buffer;
    }

    private string FormatPingReply(PingReply reply, int bufferSize)
    {
        return reply.Status == IPStatus.Success
            ? $"Reply from {reply.Address}: bytes={bufferSize} time={reply.RoundtripTime}ms TTL={reply.Options?.Ttl ?? 0}"
            : $"Request failed: {reply.Status}";
    }

    private void DisplayStatistics(string host, PingStatistics stats, StringBuilder output)
    {
        var lossPercent = stats.Sent > 0 ? (stats.Sent - stats.Received) * 100 / stats.Sent : 0;
        
        var statsHeader = $"\nPing statistics for {host}:";
        var packetStats = $"    Packets: Sent = {stats.Sent}, Received = {stats.Received}, Lost = {stats.Sent - stats.Received} ({lossPercent}% loss)";
        
        Console.WriteLine(statsHeader);
        Console.WriteLine(packetStats);
        output.AppendLine(statsHeader);
        output.AppendLine(packetStats);

        if (stats.Received > 0)
        {
            var timingHeader = "Approximate round trip times in milli-seconds:";
            var timingStats = $"    Minimum = {stats.MinTime}ms, Maximum = {stats.MaxTime}ms, Average = {stats.TotalTime / stats.Received}ms";
            
            Console.WriteLine(timingHeader);
            Console.WriteLine(timingStats);
            output.AppendLine(timingHeader);
            output.AppendLine(timingStats);
        }
    }

    private async Task SaveOutputIfRequested(string? filename, string content)
    {
        if (string.IsNullOrEmpty(filename)) return;

        try
        {
            await File.WriteAllTextAsync(filename, content);
            NetworkShell.WriteColoredLine($"\nOutput saved to {Path.GetFullPath(filename)}", ConsoleColor.Green);
        }
        catch (Exception ex)
        {
            NetworkShell.WriteColoredLine($"Error saving file: {ex.Message}", ConsoleColor.Red);
        }
    }

    private class PingOptions
    {
        public string Host { get; set; } = "";
        public bool Continuous { get; set; }
        public int Count { get; set; }
        public int Ttl { get; set; }
        public int Timeout { get; set; }
        public int BufferSize { get; set; }
        public string? OutputFile { get; set; }
    }

    private class PingStatistics
    {
        public int Sent { get; set; }
        public int Received { get; set; }
        public long TotalTime { get; set; }
        public long MinTime { get; set; } = long.MaxValue;
        public long MaxTime { get; set; } = long.MinValue;

        public void UpdateSuccess(long roundtripTime)
        {
            Received++;
            TotalTime += roundtripTime;
            MinTime = Math.Min(MinTime, roundtripTime);
            MaxTime = Math.Max(MaxTime, roundtripTime);
        }
    }
}
#endregion