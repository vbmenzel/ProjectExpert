using System.Net;
using System.Net.NetworkInformation;
using System.Text;

/// <summary>
/// A simple command-line shell that provides basic networking and file system utilities.
/// Supports commands like ping, cat, ls/dir, and basic shell operations.
/// </summary>
class Program
{
    #region Constants and Static Fields
    /// <summary>
    /// Default console color for displaying error messages throughout the application
    /// </summary>
    static ConsoleColor errorColor = ConsoleColor.Red;

    /// <summary>
    /// Global flag to handle graceful interruption via Ctrl+C
    /// Set to true when user requests cancellation, checked in long-running operations
    /// </summary>
    static bool cancelRequested = false;
    #endregion

    #region Main Entry Point
    /// <summary>
    /// Main application entry point. Sets up the command loop and handles user input.
    /// </summary>
    static void Main(string[] args)
    {
        // Set up graceful Ctrl+C handling to prevent abrupt termination
        // This allows long-running commands (like continuous ping) to be interrupted cleanly
        Console.CancelKeyPress += (sender, e) =>
        {
            cancelRequested = true;
            e.Cancel = true; // Prevents immediate program termination
        };

        // Main command processing loop - continues until user exits
        while (true)
        {
            #region Display Command Prompt
            // Configure console for UTF-8 output to support special characters
            Console.OutputEncoding = Encoding.UTF8;

            // Create a visually appealing command prompt with colors
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("▶");  // Arrow indicator
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("> "); // Prompt symbol
            Console.ResetColor(); // Return to default colors

            // Read user input from console
            string? input = Console.ReadLine();
            #endregion

            #region Parse and Dispatch Commands
            // Skip processing if user entered empty input
            if (string.IsNullOrEmpty(input)) continue;

            // Split input into command and arguments, removing empty entries
            // This handles multiple spaces between arguments gracefully
            string[] cmdArgs = SplitCommandLine(input!);
            if (cmdArgs.Length == 0) continue;

            // Extract the main command (case-insensitive)
            string command = cmdArgs[0].ToLower();

            // Handle command-specific help requests (e.g., "ping ?")
            if (cmdArgs.Length > 1 && cmdArgs[1].ToLower() == "?")
            {
                HandleHelpCommand(command);
                continue;
            }

            // Main command dispatcher - routes commands to appropriate handlers
            switch (command)
            {
                // Help and information commands
                case "help":
                case "command":
                case "commands":
                case "?":
                    // Pass specific command name if provided, otherwise show general help
                    HandleHelpCommand(cmdArgs.Length > 1 ? cmdArgs[1].ToLower() : null);
                    break;

                // Screen management commands
                case "cls":
                case "clear":
                    HandleClearCommand();
                    break;

                // Network utilities
                case "ping":
                    // Extract ping arguments (everything after the command)
                    string[] pingArgs = cmdArgs.Length > 1 ?
                        cmdArgs[1..] : Array.Empty<string>();
                    HandlePingCommand(pingArgs);
                    break;

                // File system utilities
                case "cat":
                    // Extract file arguments for cat command
                    string[] catArgs = cmdArgs.Length > 1 ?
                        cmdArgs[1..] : Array.Empty<string>();
                    HandleCatCommand(catArgs);
                    break;

                case "ls":
                case "dir":
                    // Extract directory arguments for listing command
                    string[] dirArgs = cmdArgs.Length > 1 ?
                        cmdArgs[1..] : Array.Empty<string>();
                    HandleDirCommand(dirArgs);
                    break;

                // Exit commands
                case "quit":
                case "exit":
                case "q":
                    Environment.Exit(0);
                    break;

                // Handle unknown commands
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    Console.WriteLine("Type '?' for a list of commands");
                    break;
            }
            #endregion
        }
    }
    #endregion

    #region Command Handler Methods

    #region Screen Management Commands
    /// <summary>
    /// Handles the 'cls' or 'clear' command to clear the console screen.
    /// Provides a clean slate for continued interaction.
    /// </summary>
    private static void HandleClearCommand()
    {
        Console.Clear();
    }
    #endregion

    #region File System Commands
    /// <summary>
    /// Handles the 'ls' or 'dir' command to list directory contents.
    /// Shows all files and subdirectories in the specified or current directory.
    /// </summary>
    /// <param name="args">Command arguments - first argument is the directory path (optional)</param>
    private static void HandleDirCommand(string[] args)
    {
        // Use current directory if no path is specified
        string path = args.Length > 0 ? args[0].Trim('"') : Directory.GetCurrentDirectory();

        // Validate that the specified directory exists
        if (!Directory.Exists(path))
        {
            Console.ForegroundColor = errorColor;
            Console.WriteLine($"Directory not found: {path}");
            Console.ResetColor();
            return;
        }

        // Display header with directory being listed
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Listing for {path}:");
        Console.ResetColor();

        try
        {
            // Get and display all file system entries (files and directories)
            foreach (var entry in Directory.GetFileSystemEntries(path))
            {
                Console.WriteLine(entry);
            }
        }
        catch (Exception ex)
        {
            // Handle access denied or other file system errors
            Console.ForegroundColor = errorColor;
            Console.WriteLine($"Error listing directory: {ex.Message}");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// Handles the 'cat' command to display file contents.
    /// Can display multiple files in sequence and automatically tries .txt extension if file not found.
    /// Now supports quoted filenames with spaces.
    /// </summary>
    /// <param name="args">Array of file paths to display</param>
    private static void HandleCatCommand(string[] args)
    {
        // Require at least one file argument
        if (args.Length == 0)
        {
            HandleHelpCommand("cat");
            return;
        }

        // Process each file specified in the arguments
        foreach (string rawFilePath in args)
        {
            // Remove surrounding quotes if present
            string filePath = rawFilePath.Trim('"');

            // Try to read the file as specified
            if (File.Exists(filePath))
            {
                try
                {
                    string content = File.ReadAllText(filePath);

                    // Display file header
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Contents of {filePath}:");
                    Console.ResetColor();

                    // Display file content
                    Console.WriteLine(content);
                }
                catch (Exception ex)
                {
                    // Handle file access errors (permissions, locked files, etc.)
                    Console.ForegroundColor = errorColor;
                    Console.WriteLine($"Error reading file '{filePath}': {ex.Message}");
                    Console.ResetColor();
                }
            }
            // Try with .txt extension if original file not found
            else if (File.Exists(filePath + ".txt"))
            {
                try
                {
                    string content = File.ReadAllText(filePath + ".txt");

                    // Display file header with .txt extension
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Contents of {filePath + ".txt"}:");
                    Console.ResetColor();

                    // Display file content
                    Console.WriteLine(content);
                }
                catch (Exception ex)
                {
                    // Handle file access errors for .txt version
                    Console.ForegroundColor = errorColor;
                    Console.WriteLine($"Error reading file '{filePath + ".txt"}': {ex.Message}");
                    Console.ResetColor();
                }
            }
            else
            {
                // File not found with or without .txt extension
                Console.ForegroundColor = errorColor;
                Console.WriteLine($"File not found: {filePath}");
                Console.ResetColor();
            }
        }
    }

    #endregion
    /// <summary>
    /// Splits a command line string into arguments, respecting quoted substrings.
    /// </summary>
    private static string[] SplitCommandLine(string input)
    {
        var args = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (current.Length > 0)
                {
                    args.Add(current.ToString());
                    current.Clear();
                }
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0)
            args.Add(current.ToString());

        return args.ToArray();
    }


    #region Network Commands
    /// <summary>
    /// Handles the 'ping' command with full support for various options and output redirection.
    /// Supports continuous ping, custom packet counts, TTL settings, timeouts, buffer sizes,
    /// and file output redirection.
    /// </summary>
    /// <param name="args">Command arguments including host and optional parameters</param>
    private static void HandlePingCommand(string[] args)
    {
        #region Input Validation and Setup
        // Require at least a hostname/IP address
        if (args.Length < 1)
        {
            HandleHelpCommand("ping");
            return;
        }

        // Handle common localhost aliases for convenience
        if (args[0] == "localhost" || args[0] == "local")
        {
            args[0] = "127.0.0.1";
        }

        string host = args[0];

        // Initialize ping parameters with default values
        bool continuous = false;        // -t flag: ping continuously
        int count = 4;                 // -n flag: number of ping attempts
        int ttl = 128;                 // -i flag: Time To Live value
        int timeout = 5000;            // -w flag: timeout in milliseconds
        int bufferSize = 32;           // -l flag: ping packet size in bytes
        bool isValidInput = true;      // Flag to track parameter validation
        string? outputFile = null;     // File path for output redirection

        // Validate that the host is reachable (IP address or resolvable hostname)
        if (!IsValidHost(host))
        {
            Console.ForegroundColor = errorColor;
            Console.WriteLine($"Could not resolve host: {host}");
            Console.ResetColor();
            return;
        }

        /// <summary>
        /// Local helper function to validate if a host is reachable.
        /// Accepts both IP addresses and hostnames that can be resolved via DNS.
        /// </summary>
        /// <param name="host">Hostname or IP address to validate</param>
        /// <returns>True if host is valid and reachable, false otherwise</returns>
        static bool IsValidHost(string host)
        {
            // Check if it's a valid IP address
            if (IPAddress.TryParse(host, out _))
                return true;

            // Try to resolve hostname via DNS
            try
            {
                IPAddress[] addresses = Dns.GetHostAddresses(host);
                return addresses.Length > 0;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Command Line Argument Parsing
        // Process each argument after the hostname
        for (int i = 1; i < args.Length; i++)
        {
            string arg = args[i].ToLower();

            // Continuous ping flag
            if (arg == "-t")
            {
                continuous = true;
            }
            // Packet count parameter
            else if (arg == "-n" && i + 1 < args.Length)
            {
                if (int.TryParse(args[i + 1], out int parsedCount) && parsedCount > 0)
                {
                    count = parsedCount;
                    i++; // Skip the next argument as it's the count value
                }
                else
                {
                    NotValidInput("Invalid count value. Must be a positive number.");
                }
            }
            // TTL (Time To Live) parameter
            else if (arg == "-i" && i + 1 < args.Length)
            {
                if (int.TryParse(args[i + 1], out int parsedTtl))
                {
                    ttl = parsedTtl;
                    i++; // Skip the next argument as it's the TTL value
                }
                else
                {
                    NotValidInput("Invalid TTL value.");
                }
            }
            // Timeout parameter
            else if (arg == "-w" && i + 1 < args.Length)
            {
                if (int.TryParse(args[i + 1], out int parsedTimeout))
                {
                    timeout = parsedTimeout;
                    i++; // Skip the next argument as it's the timeout value
                }
                else
                {
                    NotValidInput("Invalid timeout value.");
                }
            }
            // Buffer size parameter
            else if (arg == "-l" && i + 1 < args.Length)
            {
                if (int.TryParse(args[i + 1], out int parsedBufferSize) &&
                    parsedBufferSize > 0 && parsedBufferSize <= 65500)
                {
                    bufferSize = parsedBufferSize;
                    i++; // Skip the next argument as it's the buffer size value
                }
                else
                {
                    NotValidInput("Invalid buffer value. Must be between 1 and 65500.");
                }
            }
            // Output redirection to file
            else if (arg == "|" && i + 1 < args.Length)
            {
                outputFile = args[i + 1];

                // Validate and prepare the output file path
                try
                {
                    // Check if the directory exists
                    string? directory = Path.GetDirectoryName(outputFile);
                    if (directory is not null && directory != string.Empty && !Directory.Exists(directory))
                    {
                        NotValidInput($"Directory does not exist: {directory}");
                        break;
                    }

                    // Add .txt extension if no extension is provided
                    if (!Path.HasExtension(outputFile))
                    {
                        outputFile += ".txt";
                    }

                    // Test file writability by creating and immediately deleting
                    using (FileStream fs = File.Create(outputFile)) { }
                    File.Delete(outputFile);
                }
                catch (Exception ex)
                {
                    NotValidInput($"Invalid file path: {ex.Message}");
                    break;
                }
                i++; // Skip the filename we just processed
            }
            // Handle unknown parameters
            else if (arg.StartsWith("-"))
            {
                NotValidInput($"Unknown argument: {arg}");
            }
        }

        /// <summary>
        /// Local helper function to handle invalid input and display appropriate error messages.
        /// Sets the validation flag to false and optionally displays a custom error message.
        /// </summary>
        /// <param name="str">Custom error message (optional)</param>
        void NotValidInput(string? str = null)
        {
            Console.ForegroundColor = errorColor;
            if (string.IsNullOrEmpty(str))
            {
                Console.WriteLine("Invalid input. Please check the command and try again.");
            }
            else
            {
                Console.WriteLine(str);
            }
            Console.ResetColor();
            HandleHelpCommand("ping");
            isValidInput = false;
        }
        #endregion

        #region Ping Execution
        // Only proceed with ping execution if all parameters are valid
        if (isValidInput)
        {
            try
            {
                // StringBuilder to collect output for file redirection
                StringBuilder outputBuilder = new StringBuilder();

                // Display and optionally save ping header
                string pingHeader = $"Pinging {host} with {bufferSize} bytes of data:";
                Console.WriteLine(pingHeader);
                if (outputFile != null)
                {
                    outputBuilder.AppendLine(pingHeader);
                }

                // Initialize ping object and options
                Ping ping = new Ping();
                PingOptions options = new PingOptions
                {
                    Ttl = ttl,                    // Set Time To Live
                    DontFragment = true           // Don't allow packet fragmentation
                };

                // Create data buffer with specified size and fill with pattern
                // Uses repeating alphabet pattern for consistent data
                byte[] buffer = new byte[bufferSize];
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = (byte)('a' + (i % 26));
                }

                // Initialize statistics tracking variables
                int sent = 0;           // Total packets sent
                int received = 0;       // Total packets received successfully
                long totalTime = 0;     // Sum of all round-trip times
                long minTime = long.MaxValue;  // Minimum round-trip time
                long maxTime = long.MinValue;  // Maximum round-trip time
                int pingCount = 0;      // Current ping iteration

                cancelRequested = false; // Reset cancellation flag

                #region Main Ping Loop
                // Continue pinging until count reached or user cancels (Ctrl+C)
                do
                {
                    if (cancelRequested) break; // Allow graceful interruption

                    sent++;

                    // Send ping and process response
                    PingReply reply = ping.Send(host, timeout, buffer, options);
                    string replyMessage;

                    if (reply.Status == IPStatus.Success)
                    {
                        // Successful ping - update statistics and format response
                        received++;
                        totalTime += reply.RoundtripTime;
                        minTime = Math.Min(minTime, reply.RoundtripTime);
                        maxTime = Math.Max(maxTime, reply.RoundtripTime);
                        replyMessage = $"Reply from {reply.Address}: bytes={buffer.Length} time={reply.RoundtripTime}ms TTL={reply.Options.Ttl}";
                    }
                    else
                    {
                        // Failed ping - show error status
                        replyMessage = $"Request timed out: {reply.Status}";
                    }

                    // Display result and optionally save to output buffer
                    Console.WriteLine(replyMessage);
                    if (outputFile != null)
                    {
                        outputBuilder.AppendLine(replyMessage);
                    }

                    // Wait between pings (except for the last one)
                    if ((continuous || pingCount < count - 1) && !cancelRequested)
                        Thread.Sleep(1000); // 1 second delay between pings

                    pingCount++;
                } while ((continuous && !cancelRequested) || (!continuous && pingCount < count));
                #endregion

                #region Statistics Display and File Output
                // Calculate and display ping statistics
                string statsHeader = $"\nPing statistics for {host}:";
                string packetStats = $"    Packets: Sent = {sent}, Received = {received}, Lost = {sent - received} ({(sent - received) * 100 / sent}% loss)";

                Console.WriteLine(statsHeader);
                Console.WriteLine(packetStats);

                // Add statistics to file output if redirection is enabled
                if (outputFile != null)
                {
                    outputBuilder.AppendLine(statsHeader);
                    outputBuilder.AppendLine(packetStats);
                }

                // Display timing statistics if any packets were received
                if (received > 0)
                {
                    string timesHeader = "Approximate round trip times in milli-seconds:";
                    string timesStats = $"    Minimum = {minTime}ms, Maximum = {maxTime}ms, Average = {totalTime / received}ms";

                    Console.WriteLine(timesHeader);
                    Console.WriteLine(timesStats);

                    // Add timing statistics to file output
                    if (outputFile != null)
                    {
                        outputBuilder.AppendLine(timesHeader);
                        outputBuilder.AppendLine(timesStats);
                    }
                }

                // Write collected output to file if redirection was requested
                if (outputFile != null)
                {
                    try
                    {
                        File.WriteAllText(outputFile, outputBuilder.ToString());
                        Console.WriteLine($"\nOutput saved to {Path.GetFullPath(outputFile)}");
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = errorColor;
                        Console.WriteLine($"Error writing to file: {ex.Message}");
                        Console.ResetColor();
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors during ping execution
                Console.ForegroundColor = errorColor;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }
        }
        #endregion
    }
    #endregion

    #region Help System
    /// <summary>
    /// Handles the 'help' command and provides command-specific help information.
    /// Can display general help for all commands or detailed help for specific commands.
    /// </summary>
    /// <param name="command">Specific command to show help for (null for general help)</param>
    private static void HandleHelpCommand(string? command)
    {
        if (string.IsNullOrEmpty(command))
        {
            #region General Help Display
            // Display overview of all available commands
            Console.WriteLine("Available commands:");
            Console.WriteLine("? - Display this help message");
            Console.WriteLine("ping - Send ICMP echo request to network hosts (type 'ping ?' for more details)");
            Console.WriteLine("cat - Display the contents of a file (type 'cat ?' for more details)");
            Console.WriteLine("ls/dir - List files and directories (type 'ls ?' for more details)");
            Console.WriteLine("cls/clear - Clear the console screen");
            Console.WriteLine("exit/quit/q - Exit the shell");
            Console.WriteLine("Type '<command> ?' for detailed help on a specific command");
            #endregion
        }
        else
        {
            #region Command-Specific Help
            // Display detailed help for the specified command
            switch (command.ToLower())
            {
                case "ping":
                    Console.WriteLine("ping - Send ICMP echo request to network hosts");
                    Console.WriteLine("-t: Continuous ping (break with Ctrl+C)");
                    Console.WriteLine("-n count: Number of echo requests to send (default is 4)");
                    Console.WriteLine("-i ttl: Set the Time To Live (TTL) value");
                    Console.WriteLine("-w timeout: Set the timeout in milliseconds");
                    Console.WriteLine("-l size: Set the buffer size in bytes");
                    Console.WriteLine("| filename: Pipe output to a file (adds .txt extension if none provided)");
                    Console.WriteLine("Usage: ping <host> [-t] [-n count] [-i ttl] [-w timeout] [-l size] [| filename]");
                    break;

                case "cls":
                case "clear":
                    Console.WriteLine("cls/clear - Clear the console screen");
                    Console.WriteLine("Usage: cls or clear");
                    break;

                case "load":
                case "read":
                case "cat":
                    Console.WriteLine("cat - Display the contents of a file");
                    Console.WriteLine("Automatically tries .txt extension if file not found");
                    Console.WriteLine("Can display multiple files in sequence");
                    Console.WriteLine("Usage: cat <filename> [filename2] [filename3] ...");
                    break;

                case "ls":
                case "dir":
                    Console.WriteLine("ls/dir - List files and directories");
                    Console.WriteLine("Shows all files and subdirectories in the specified path");
                    Console.WriteLine("Uses current directory if no path is provided");
                    Console.WriteLine("Usage: ls [path] or dir [path]");
                    break;

                case "exit":
                case "quit":
                case "q":
                    Console.WriteLine("exit/quit/q - Exit the shell");
                    Console.WriteLine("Terminates the application immediately");
                    Console.WriteLine("Usage: exit or quit or q");
                    break;

                case "?":
                case "help":
                    Console.WriteLine("help or ? - Display help information");
                    Console.WriteLine("Shows general help or specific command details");
                    Console.WriteLine("Usage: help [command] or ? [command]");
                    Console.WriteLine("Alternative syntax: <command> ?");
                    break;

                default:
                    Console.WriteLine($"No help available for '{command}'");
                    Console.WriteLine("Type '?' to see all available commands");
                    break;
            }
            #endregion
        }
    }
    #endregion

    #endregion
}
