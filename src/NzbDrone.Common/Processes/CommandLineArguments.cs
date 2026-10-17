using System.Collections.Generic;
using System.Text;

namespace NzbDrone.Common.Processes
{
    public static class CommandLineArguments
    {
        // Splits a command line into individual arguments using the same rules .NET applies to
        // ProcessStartInfo.Arguments on Linux/macOS (which in turn follow the MSVC CRT / CommandLineToArgvW rules),
        // so passing the result to ProcessStartInfo.ArgumentList is equivalent to passing the original string:
        //  - Arguments are separated by spaces or tabs outside of quotes
        //  - Double quotes group characters and are removed; "" inside a quoted section is a literal quote
        //  - 2n backslashes followed by a quote become n backslashes and the quote toggles quoting
        //  - 2n+1 backslashes followed by a quote become n backslashes and a literal quote
        //  - Backslashes not followed by a quote are literal
        public static List<string> Split(string commandLine)
        {
            var args = new List<string>();

            if (commandLine == null)
            {
                return args;
            }

            var i = 0;

            while (i < commandLine.Length)
            {
                while (i < commandLine.Length && (commandLine[i] == ' ' || commandLine[i] == '\t'))
                {
                    i++;
                }

                if (i == commandLine.Length)
                {
                    break;
                }

                args.Add(ReadArgument(commandLine, ref i));
            }

            return args;
        }

        private static string ReadArgument(string commandLine, ref int i)
        {
            var arg = new StringBuilder();
            var inQuotes = false;

            while (i < commandLine.Length)
            {
                var backslashes = 0;

                while (i < commandLine.Length && commandLine[i] == '\\')
                {
                    backslashes++;
                    i++;
                }

                if (backslashes > 0)
                {
                    if (i >= commandLine.Length || commandLine[i] != '"')
                    {
                        arg.Append('\\', backslashes);
                    }
                    else
                    {
                        arg.Append('\\', backslashes / 2);

                        if (backslashes % 2 != 0)
                        {
                            arg.Append('"');
                            i++;
                        }
                    }

                    continue;
                }

                var c = commandLine[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < commandLine.Length && commandLine[i + 1] == '"')
                    {
                        arg.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }

                    i++;
                    continue;
                }

                if ((c == ' ' || c == '\t') && !inQuotes)
                {
                    break;
                }

                arg.Append(c);
                i++;
            }

            return arg.ToString();
        }
    }
}
