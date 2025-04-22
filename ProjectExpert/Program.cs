using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
while (true)
{
    Console.Write(":-: ");
    string? input = Console.ReadLine();

    #region Commands
    // Check if input is null or empty
    if (string.IsNullOrEmpty(input)) continue;
    // Clear Command
    if (input == "cls" || input == "clear") { Console.Clear(); continue; }
    // Ping Command
    if (input.StartsWith("ping"))
    {
        string helpMsg = "Usage: ping <host> [-t] [-i ttl] [-w timeout]\n" +
            "-t: Continuous ping\n" +
            "-i ttl: Set the Time To Live (TTL) value\n" +
            "-w timeout: Set the timeout in milliseconds";
        string[] pingArgs = input.Split(' ');
        if (pingArgs.Length < 2)
        {
            Console.WriteLine(helpMsg);
            continue;
        }

        string host = pingArgs[1];
        bool continuous = false;
        int ttl = 128; // Default TTL
        int timeout = 5000; // Default timeout in milliseconds
        bool isValidInput = true;

        // Check for additional arguments
        for (int i = 2; i < pingArgs.Length; i++)
        {
            if (pingArgs[i] == "-t")
            {
                continuous = true;
            }
            else if (pingArgs[i] == "-i" && i + 1 < pingArgs.Length)
            {
                if (int.TryParse(pingArgs[i + 1], out int parsedTtl))
                {
                    ttl = parsedTtl;
                    i++; // Skip the value we just processed
                }
                else{
                    NotValidInput("Invalid TTL value.");
                }
            }
            else if (pingArgs[i] == "-w" && i + 1 < pingArgs.Length)
            {
                if (int.TryParse(pingArgs[i + 1], out int parsedTimeout))
                {
                    timeout = parsedTimeout;
                    i++; // Skip the value we just processed
                }
                else
                {
                    NotValidInput("Invalid timeout value.");
                }
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
            Console.WriteLine(helpMsg);
            isValidInput = false;
        }
        if (isValidInput)
        {
            try
            {
                Ping ping = new Ping();
                PingOptions options = new PingOptions
                {
                    Ttl = ttl,
                    DontFragment = true
                };

                // Default payload
                byte[] buffer = new byte[32];

                do
                {
                    PingReply reply = ping.Send(host, timeout, buffer, options);


                    if (reply.Status == IPStatus.Success)
                    {
                        Console.WriteLine($"Reply from {reply.Address}: time={reply.RoundtripTime}ms TTL={reply.Options.Ttl}");
                    }
                    else
                    {
                        Console.WriteLine($"Ping failed: {reply.Status}");
                    }

                    if (continuous)
                        Thread.Sleep(1000); // Wait 1 second between pings in continuous mode

                } while (continuous); // Continue only if -t flag was specified
                for (int i = 2; i < pingArgs.Length; i++)
                {
                    if (pingArgs[i] == "-t")
                    {
                        continuous = true;
                    }
                    else if (pingArgs[i] == "-i")
                    {
                        if (i + 1 < pingArgs.Length && int.TryParse(pingArgs[i + 1], out int parsedTtl))
                        {
                            ttl = parsedTtl;
                            i++; // Skip the value we just processed
                        }
                        else
                        {
                            NotValidInput("Invalid or missing TTL value.");
                            break;
                        }
                    }
                    else if (pingArgs[i] == "-w")
                    {
                        if (i + 1 < pingArgs.Length && int.TryParse(pingArgs[i + 1], out int parsedTimeout))
                        {
                            timeout = parsedTimeout;
                            i++; // Skip the value we just processed
                        }
                        else
                        {
                            NotValidInput("Invalid or missing timeout value.");
                            break;
                        }
                    }
                    else
                    {
                        NotValidInput("Unknown argument.");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
    #endregion
}