using System.Net;
using System.Net.NetworkInformation;
using System.Text;

class Program
{
    static void Main(string[] args)
    {

        while (true)
        {
            #region Input Prompt
            Console.OutputEncoding = Encoding.UTF8;
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("▶");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("> ");
            Console.ResetColor();
            string? input = Console.ReadLine();
            #endregion

            #region Commands Dispatcher
            // Check if input is null or empty
            if (string.IsNullOrEmpty(input)) continue;

            // Parse the command and args
            string[] cmdArgs = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (cmdArgs.Length == 0) continue;

            string command = cmdArgs[0].ToLower();

            // Check for command-specific help
            if (cmdArgs.Length > 1 && cmdArgs[1].ToLower() == "?")
            {
                HandleHelpCommand(command);
                continue;
            }

            // Dispatch to appropriate command handler
            switch (command)
            {
                case "?":
                    HandleHelpCommand(cmdArgs.Length > 1 ? cmdArgs[1].ToLower() : null);
                    break;
                case "cls":
                case "clear":
                    HandleClearCommand();
                    break;
                case "ping":
                    // Skip the command name and process only the arguments
                    string[] pingArgs = cmdArgs.Length > 1 ?
                        cmdArgs.Skip(1).ToArray() :
                        Array.Empty<string>();
                    HandlePingCommand(pingArgs);
                    break;
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    Console.WriteLine("Type '?' for a list of commands");
                    break;
            }
            #endregion
        }
    }

    #region Command Handlers

    private static void HandleClearCommand()
    {
        Console.Clear();
    }

    private static void HandlePingCommand(string[] args)
    {
        #region Ping Command

        if (args.Length < 1)
        {
            HandleHelpCommand("ping");
            return;
        }
        if (args[0] == "localhost" ||  args[0] == "local") { args[0] = "127.0.0.1"; }
        string host = args[0];
        bool continuous = false;
        int count = 4; // Default number of tries
        int ttl = 128; // Default TTL
        int timeout = 5000; // Default timeout in milliseconds
        int bufferSize = 32; // Default buffer size
        bool isValidInput = true;

        if (!IsValidHost(host))
        {
            Console.WriteLine($"Could not resolve host: {host}");
            return;
        }
        static bool IsValidHost(string host)
        {
            // First check if the input is a valid IP address
            if (IPAddress.TryParse(host, out _))
            {
                return true;
            }

            // If it's not a valid IP format, try to resolve it as a hostname
            try
            {
                IPAddress[] addresses = Dns.GetHostAddresses(host);
                return addresses.Length > 0;
            }
            catch (Exception)
            {
                // If an exception occurs during resolution, the host is invalid
                return false;
            }
        }


        // Check for additional arguments
        for (int i = 1; i < args.Length; i++)
        {
            string arg = args[i].ToLower(); // Convert to lower case for comparison

            if (arg == "-t")
            {
                continuous = true;
            }
            else if (arg == "-n" && i + 1 < args.Length)
            {
                if (int.TryParse(args[i + 1], out int parsedCount) && parsedCount > 0)
                {
                    count = parsedCount;
                    i++; // Skip the value we just processed
                }
                else
                {
                    NotValidInput("Invalid count value. Must be a positive number.");
                }
            }
            else if (arg == "-i" && i + 1 < args.Length)
            {
                if (int.TryParse(args[i + 1], out int parsedTtl))
                {
                    ttl = parsedTtl;
                    i++; // Skip the value we just processed
                }
                else
                {
                    NotValidInput("Invalid TTL value.");
                }
            }
            else if (arg == "-w" && i + 1 < args.Length)
            {
                if (int.TryParse(args[i + 1], out int parsedTimeout))
                {
                    timeout = parsedTimeout;
                    i++; // Skip the value we just processed
                }
                else
                {
                    NotValidInput("Invalid timeout value.");
                }
            }
            else if (arg == "-l" && i + 1 < args.Length)
            {
                if (int.TryParse(args[i + 1], out int parsedBufferSize) && parsedBufferSize > 0 && parsedBufferSize <= 65500)
                {
                    bufferSize = parsedBufferSize;
                    i++; // Skip the value we just processed
                }
                else
                {
                    NotValidInput("Invalid buffer value. Must be between 1 and 65500.");
                }
            }
            else if (arg.StartsWith("-"))
            {
                NotValidInput($"Unknown argument: {arg}");
            }
        }

        void NotValidInput(string? str = null)
        {
            if (string.IsNullOrEmpty(str))
            {
                Console.WriteLine("Invalid input. Please check the command and try again.");
            }
            else
            {
                Console.WriteLine(str);
            }
            HandleHelpCommand("ping");
            isValidInput = false;
        }

        if (isValidInput)
        {
            try
            {
                Console.WriteLine($"Pinging {host} with {bufferSize} bytes of data:");

                Ping ping = new Ping();
                PingOptions options = new PingOptions
                {
                    Ttl = ttl,
                    DontFragment = true
                };

                // Create buffer with the specified size and fill with data
                byte[] buffer = new byte[bufferSize];
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = (byte)('a' + (i % 26));
                }

                int sent = 0;
                int received = 0;
                long totalTime = 0;
                long minTime = long.MaxValue;
                long maxTime = long.MinValue;

                // If continuous, keep pinging until a key is pressed
                // Otherwise, ping count times
                int pingCount = 0;
                do
                {
                    sent++;
                    PingReply reply = ping.Send(host, timeout, buffer, options);

                    if (reply.Status == IPStatus.Success)
                    {
                        received++;
                        totalTime += reply.RoundtripTime;
                        minTime = Math.Min(minTime, reply.RoundtripTime);
                        maxTime = Math.Max(maxTime, reply.RoundtripTime);

                        Console.WriteLine($"Reply from {reply.Address}: bytes={buffer.Length} time={reply.RoundtripTime}ms TTL={reply.Options.Ttl}");
                    }
                    else
                    {
                        Console.WriteLine($"Request timed out: {reply.Status}");
                    }

                    if (continuous || pingCount < count - 1)
                        Thread.Sleep(1000); // Wait 1 second between pings

                    pingCount++;

                } while ((continuous && !Console.KeyAvailable) || (!continuous && pingCount < count));

                // Consume the key if we're in continuous mode and a key was pressed
                if (continuous && Console.KeyAvailable)
                    Console.ReadKey(true);

                // Display ping statistics
                Console.WriteLine("\nPing statistics for " + host + ":");
                Console.WriteLine($"    Packets: Sent = {sent}, Received = {received}, Lost = {sent - received} ({(sent - received) * 100 / sent}% loss)");

                if (received > 0)
                {
                    Console.WriteLine("Approximate round trip times in milli-seconds:");
                    Console.WriteLine($"    Minimum = {minTime}ms, Maximum = {maxTime}ms, Average = {totalTime / received}ms");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        #endregion
    }

    private static void HandleHelpCommand(string? command)
    {
        #region Help Command
        if (string.IsNullOrEmpty(command))
        {
            // Display all available commands
            Console.WriteLine("Available commands:");
            Console.WriteLine("? - Display this help message");
            Console.WriteLine("ping - Send ICMP echo request to network hosts (type 'ping ?' for more details)");
            Console.WriteLine("cls/clear - Clear the console screen");
            Console.WriteLine("Type '<command> ?' for detailed help on a specific command");
        }
        else
        {
            // Display help for specific command
            switch (command.ToLower())
            {
                case "ping":
                    Console.WriteLine("ping - Send ICMP echo request to network hosts");
                    Console.WriteLine("-t: Continuous ping");
                    Console.WriteLine("-n count: Number of echo requests to send (default is 4)");
                    Console.WriteLine("-i ttl: Set the Time To Live (TTL) value");
                    Console.WriteLine("-w timeout: Set the timeout in milliseconds");
                    Console.WriteLine("-l size: Set the buffer size in bytes");
                    Console.WriteLine("Usage: ping <host> [-t] [-n count] [-i ttl] [-w timeout] [-l size]");
                    break;
                case "cls":
                case "clear":
                    Console.WriteLine("cls/clear - Clear the console screen");
                    Console.WriteLine("Usage: cls or clear");
                    break;
                case "?":
                    Console.WriteLine("help or ? - Display help information");
                    Console.WriteLine("Usage:  [command]");
                    Console.WriteLine("command ? (alternative syntax)");
                    break;
                default:
                    Console.WriteLine($"No help available for '{command}'");
                    break;
            }
        }
        #endregion
    }
    #endregion
}