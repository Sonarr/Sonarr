//  **********************************************************************************
//  CassiniDev - http://cassinidev.codeplex.com
// 
//  Copyright (c) 2010 Sky Sanders. All rights reserved.
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  
//  This source code is subject to terms and conditions of the Microsoft Public
//  License (Ms-PL). A copy of the license can be found in the license.txt file
//  included in this distribution.
//  
//  You must not remove this notice, or any other, from this software.
//  
//  **********************************************************************************

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace CassiniDev
{
    /// <summary>
    /// Parser for command line arguments.
    ///
    /// The parser specification is infered from the instance fields of the object
    /// specified as the destination of the parse.
    /// Valid argument types are: int, uint, string, bool, enums
    /// Also argument types of Array of the above types are also valid.
    /// 
    /// Error checking options can be controlled by adding a ArgumentAttribute
    /// to the instance fields of the destination object.
    ///
    /// At most one field may be marked with the DefaultArgumentAttribute
    /// indicating that arguments without a '-' or '/' prefix will be parsed as that argument.
    ///
    /// If not specified then the parser will infer default options for parsing each
    /// instance field. The default long name of the argument is the field name. The
    /// default short name is the first character of the long name. Long names and explicitly
    /// specified short names must be unique. Default short names will be used provided that
    /// the default short name does not conflict with a long name or an explicitly
    /// specified short name.
    ///
    /// Arguments which are array types are collection arguments. Collection
    /// arguments can be specified multiple times.
    /// 
    /// 
    ///    Usage
    ///    -----
    ///
    ///    Parsing command line arguments to a console application is a common problem. 
    ///    This library handles the common task of reading arguments from a command line 
    ///    and filling in the values in a type.
    ///
    ///    To use this library, define a class whose fields represent the data that your 
    ///    application wants to receive from arguments on the command line. Then call 
    ///    CommandLine.ParseArguments() to fill the object with the data 
    ///    from the command line. Each field in the class defines a command line argument. 
    ///    The type of the field is used to validate the data read from the command line. 
    ///    The name of the field defines the name of the command line option.
    ///
    ///    The parser can handle fields of the following types:
    ///
    ///    - string
    ///    - int
    ///    - uint
    ///    - bool
    ///    - enum
    ///    - array of the above type
    ///
    ///    For example, suppose you want to read in the argument list for wc (word count). 
    ///    wc takes three optional boolean arguments: -l, -w, and -c and a list of files.
    ///
    ///    You could parse these arguments using the following code:
    ///
    ///    class WCArguments
    ///    {
    ///        public bool lines;
    ///        public bool words;
    ///        public bool chars;
    ///        public string[] files;
    ///    }
    ///
    ///    class WC
    ///    {
    ///        static void Main(string[] args)
    ///        {
    ///            if (CommandLine.ParseArgumentsWithUsage(args, parsedArgs))
    ///            {
    ///            //     insert application code here
    ///            }
    ///        }
    ///    }
    ///
    ///    So you could call this aplication with the following command line to count 
    ///    lines in the foo and bar files:
    ///
    ///        wc.exe /lines /files:foo /files:bar
    ///
    ///    The program will display the following usage message when bad command line 
    ///    arguments are used:
    ///
    ///        wc.exe -x
    ///
    ///    Unrecognized command line argument '-x'
    ///        /lines[+|-]                         short form /l
    ///        /words[+|-]                         short form /w
    ///        /chars[+|-]                         short form /c
    ///        /files:&lt;string>                     short form /f
    ///        @&lt;file>                             Read response file for more options
    ///
    ///    That was pretty easy. However, you realy want to omit the "/files:" for the 
    ///    list of files. The details of field parsing can be controled using custom 
    ///    attributes. The attributes which control parsing behaviour are:
    ///
    ///    ArgumentAttribute 
    ///        - controls short name, long name, required, allow duplicates, default value
    ///        and help text
    ///    DefaultArgumentAttribute 
    ///        - allows omition of the "/name".
    ///        - This attribute is allowed on only one field in the argument class.
    ///
    ///    So for the wc.exe program we want this:
    ///
    ///    using System;
    ///    using Utilities;
    ///
    ///    class WCArguments
    ///    {
    ///        [Argument(ArgumentType.AtMostOnce, HelpText="Count number of lines in the input text.")]
    ///        public bool lines;
    ///        [Argument(ArgumentType.AtMostOnce, HelpText="Count number of words in the input text.")]
    ///        public bool words;
    ///        [Argument(ArgumentType.AtMostOnce, HelpText="Count number of chars in the input text.")]
    ///        public bool chars;
    ///        [DefaultArgument(ArgumentType.MultipleUnique, HelpText="Input files to count.")]
    ///        public string[] files;
    ///    }
    ///
    ///    class WC
    ///    {
    ///        static void Main(string[] args)
    ///        {
    ///            WCArguments parsedArgs = new WCArguments();
    ///            if (CommandLine.ParseArgumentsWithUsage(args, parsedArgs))
    ///            {
    ///            //     insert application code here
    ///            }
    ///        }
    ///    }
    ///
    ///
    ///
    ///    So now we have the command line we want:
    ///
    ///        wc.exe /lines foo bar
    ///
    ///    This will set lines to true and will set files to an array containing the 
    ///    strings "foo" and "bar".
    ///
    ///    The new usage message becomes:
    ///
    ///        wc.exe -x
    ///
    ///    Unrecognized command line argument '-x'
    ///    /lines[+|-]  Count number of lines in the input text. (short form /l)
    ///    /words[+|-]  Count number of words in the input text. (short form /w)
    ///    /chars[+|-]  Count number of chars in the input text. (short form /c)
    ///    @&lt;file>      Read response file for more options
    ///    &lt;files>      Input files to count. (short form /f)
    ///
    ///    If you want more control over how error messages are reported, how /help is 
    ///    dealt with, etc you can instantiate the CommandLine.Parser class.
    ///
    ///
    ///
    ///    Cheers,
    ///    Peter Hallam
    ///    C# Compiler Developer
    ///    Microsoft Corp.
    ///
    ///
    ///
    ///
    ///    Release Notes
    ///    -------------
    ///
    ///    10/02/2002 Initial Release
    ///    10/14/2002 Bug Fix
    ///    01/08/2003 Bug Fix in @ include files
    ///    10/23/2004 Added user specified help text, formatting of help text to 
    ///            screen width. Added ParseHelp for /?.
    ///    11/23/2004 Added support for default values.
    ///    02/23/2005 Fix bug with short name and default arguments.
    ///
    ///    12/24/2009 sky: Added ushort as valid argument type. 
    ///    12/27/2009 sky: todo: expose out and err to enable use in forms app.
    ///    12/29/2009 sky: added ArgumentsAttribute and GetGenericUsageString to allow attaching generic help text
    ///    01/01/2010 sky: split classes into seperate files
    ///    01/01/2010 sky: cleaned up Parser.cs
    ///    05/22/2010 sky: major cleanup - more to come.
    /// </summary>
    public sealed class CommandLineParser
    {
        /// <summary>
        /// The System Defined new line string.
        /// </summary>
        public const string NewLine = "\r\n";

        private const int SpaceBeforeParam = 2;

        private const int StdOutputHandle = -11;

        private readonly Hashtable _argumentMap;

        private readonly ArrayList _arguments;

        private readonly Type _argumentSpecification;

        private readonly Argument _defaultArgument;

        private readonly ErrorReporter _reporter;

        /// <summary>
        /// Don't ever call this.
        /// </summary>
        // ReSharper disable UnusedMember.Local
        private CommandLineParser()
            // ReSharper restore UnusedMember.Local
        {
            //noop
        }

        /// <summary>
        /// Creates a new command line argument parser.
        /// </summary>
        /// <param name="argumentSpecification"> The type of object to  parse. </param>
        /// <param name="reporter"> The destination for parse errors. </param>
        public CommandLineParser(Type argumentSpecification, ErrorReporter reporter)
        {
            _argumentSpecification = argumentSpecification;
            _reporter = reporter;
            _arguments = new ArrayList();
            _argumentMap = new Hashtable();

            foreach (FieldInfo field in argumentSpecification.GetFields())
            {
                if (!field.IsStatic && !field.IsInitOnly && !field.IsLiteral)
                {
                    ArgumentAttribute attribute = GetAttribute(field);
                    if (attribute is DefaultArgumentAttribute)
                    {
                        Debug.Assert(_defaultArgument == null);
                        _defaultArgument = new Argument(attribute, field, reporter);
                    }
                    else
                    {
                        _arguments.Add(new Argument(attribute, field, reporter));
                    }
                }
            }

            // add explicit names to map
            foreach (Argument argument in _arguments)
            {
                Debug.Assert(!_argumentMap.ContainsKey(argument.LongName));
                _argumentMap[argument.LongName] = argument;
                if (argument.ExplicitShortName)
                {
                    if (!string.IsNullOrEmpty(argument.ShortName))
                    {
                        Debug.Assert(!_argumentMap.ContainsKey(argument.ShortName));
                        _argumentMap[argument.ShortName] = argument;
                    }
                    else
                    {
                        argument.ClearShortName();
                    }
                }
            }

            // add implicit names which don't collide to map
            foreach (Argument argument in _arguments)
            {
                if (!argument.ExplicitShortName)
                {
                    if (!string.IsNullOrEmpty(argument.ShortName) && !_argumentMap.ContainsKey(argument.ShortName))
                        _argumentMap[argument.ShortName] = argument;
                    else
                        argument.ClearShortName();
                }
            }
        }

        /// <summary>
        /// Does this parser have a default argument.
        /// </summary>
        /// <value> Does this parser have a default argument. </value>
        public bool HasDefaultArgument
        {
            get { return _defaultArgument != null; }
        }

        /// <summary>
        /// Returns a Usage string for command line argument parsing.
        /// Use ArgumentAttributes to control parsing behaviour.
        /// Formats the output to the width of the current console window.
        /// </summary>
        /// <param name="argumentType"> The type of the arguments to display usage for. </param>
        /// <returns> Printable string containing a user friendly description of command line arguments. </returns>
        public static string ArgumentsUsage(Type argumentType)
        {
            int screenWidth = GetConsoleWindowWidth();
            if (screenWidth == 0)
                screenWidth = 80;
            return ArgumentsUsage(argumentType, screenWidth);
        }

        /// <summary>
        /// Returns a Usage string for command line argument parsing.
        /// Use ArgumentAttributes to control parsing behaviour.
        /// </summary>
        /// <param name="argumentType"> The type of the arguments to display usage for. </param>
        /// <param name="columns"> The number of columns to format the output to. </param>
        /// <returns> Printable string containing a user friendly description of command line arguments. </returns>
        public static string ArgumentsUsage(Type argumentType, int columns)
        {
            return (new CommandLineParser(argumentType, null)).GetUsageString(columns);
        }

        /// <summary>
        /// Returns the number of columns in the current console window
        /// </summary>
        /// <returns>Returns the number of columns in the current console window</returns>
        public static int GetConsoleWindowWidth()
        {
            Interop.CONSOLE_SCREEN_BUFFER_INFO csbi = new Interop.CONSOLE_SCREEN_BUFFER_INFO();

#pragma warning disable 168
            int rc = Interop.GetConsoleScreenBufferInfo(Interop.GetStdHandle(StdOutputHandle), ref csbi);
#pragma warning restore 168
            int screenWidth = csbi.dwSize.x;
            return screenWidth;
        }

        /// <summary>
        /// A user firendly usage string describing the command line argument syntax.
        /// </summary>
        public string GetUsageString(int screenWidth)
        {
            ArgumentHelpStrings[] strings = GetAllHelpStrings();

            int maxParamLen = 0;
            foreach (ArgumentHelpStrings helpString in strings)
            {
                maxParamLen = Math.Max(maxParamLen, helpString.syntax.Length);
            }

            const int minimumNumberOfCharsForHelpText = 10;
            const int minimumHelpTextColumn = 5;
            const int minimumScreenWidth = minimumHelpTextColumn + minimumNumberOfCharsForHelpText;

            int idealMinimumHelpTextColumn = maxParamLen + SpaceBeforeParam;
            screenWidth = Math.Max(screenWidth, minimumScreenWidth);
            int helpTextColumn = screenWidth < (idealMinimumHelpTextColumn + minimumNumberOfCharsForHelpText)
                                     ? minimumHelpTextColumn
                                     : idealMinimumHelpTextColumn;

            const string newLine = "\n";
            StringBuilder builder = new StringBuilder();

            // 01/01/2010 sky
            string genericUsage = GetGenericUsageString(_argumentSpecification, screenWidth);
            if (!string.IsNullOrEmpty(genericUsage))
            {
                builder.AppendLine(genericUsage);
            }

            foreach (ArgumentHelpStrings helpStrings in strings)
            {
                // add syntax string
                int syntaxLength = helpStrings.syntax.Length;
                builder.Append(helpStrings.syntax);

                // start help text on new line if syntax string is too long
                int currentColumn = syntaxLength;
                if (syntaxLength >= helpTextColumn)
                {
                    builder.Append(newLine);
                    currentColumn = 0;
                }

                // add help text broken on spaces
                int charsPerLine = screenWidth - helpTextColumn;
                int index = 0;
                while (index < helpStrings.help.Length)
                {
                    // tab to start column
                    builder.Append(' ', helpTextColumn - currentColumn);
                    currentColumn = helpTextColumn;

                    // find number of chars to display on this line
                    int endIndex = index + charsPerLine;
                    if (endIndex >= helpStrings.help.Length)
                    {
                        // rest of text fits on this line
                        endIndex = helpStrings.help.Length;
                    }
                    else
                    {
                        endIndex = helpStrings.help.LastIndexOf(' ', endIndex - 1,
                                                                Math.Min(endIndex - index, charsPerLine));
                        if (endIndex <= index)
                        {
                            // no spaces on this line, append full set of chars
                            endIndex = index + charsPerLine;
                        }
                    }

                    // add chars
                    builder.Append(helpStrings.help, index, endIndex - index);
                    index = endIndex;

                    // do new line
                    AddNewLine(newLine, builder, ref currentColumn);

                    // don't start a new line with spaces
                    while (index < helpStrings.help.Length && helpStrings.help[index] == ' ')
                        index++;
                }

                // add newline if there's no help text                
                if (helpStrings.help.Length == 0)
                {
                    builder.Append(newLine);
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Searches a StringBuilder for a character
        /// </summary>
        /// <param name="text"> The text to search. </param>
        /// <param name="value"> The character value to search for. </param>
        /// <param name="startIndex"> The index to stat searching at. </param>
        /// <returns> The index of the first occurence of value or -1 if it is not found. </returns>
        public static int IndexOf(StringBuilder text, char value, int startIndex)
        {
            for (int index = startIndex; index < text.Length; index++)
            {
                if (text[index] == value)
                    return index;
            }

            return -1;
        }

        /// <summary>
        /// Searches a StringBuilder for a character in reverse
        /// </summary>
        /// <param name="text"> The text to search. </param>
        /// <param name="value"> The character to search for. </param>
        /// <param name="startIndex"> The index to start the search at. </param>
        /// <returns>The index of the last occurence of value in text or -1 if it is not found. </returns>
        public static int LastIndexOf(StringBuilder text, char value, int startIndex)
        {
            for (int index = Math.Min(startIndex, text.Length - 1); index >= 0; index--)
            {
                if (text[index] == value)
                    return index;
            }

            return -1;
        }

        /// <summary>
        /// Parses an argument list.
        /// </summary>
        /// <param name="args"> The arguments to parse. </param>
        /// <param name="destination"> The destination of the parsed arguments. </param>
        /// <returns> true if no parse errors were encountered. </returns>
        public bool Parse(string[] args, object destination)
        {
            bool hadError = ParseArgumentList(args, destination);

            // check for missing required arguments
            foreach (Argument arg in _arguments)
            {
                hadError |= arg.Finish(destination);
            }
            if (_defaultArgument != null)
            {
                hadError |= _defaultArgument.Finish(destination);
            }

            return !hadError;
        }

        /// <summary>
        /// Parses Command Line Arguments. 
        /// Errors are output on Console.Error.
        /// Use ArgumentAttributes to control parsing behaviour.
        /// </summary>
        /// <param name="arguments"> The actual arguments. </param>
        /// <param name="destination"> The resulting parsed arguments. </param>
        /// <returns> true if no errors were detected. </returns>
        public static bool ParseArguments(string[] arguments, object destination)
        {
            return ParseArguments(arguments, destination, Console.Error.WriteLine);
        }

        /// <summary>
        /// Parses Command Line Arguments. 
        /// Use ArgumentAttributes to control parsing behaviour.
        /// </summary>
        /// <param name="arguments"> The actual arguments. </param>
        /// <param name="destination"> The resulting parsed arguments. </param>
        /// <param name="reporter"> The destination for parse errors. </param>
        /// <returns> true if no errors were detected. </returns>
        public static bool ParseArguments(string[] arguments, object destination, ErrorReporter reporter)
        {
            CommandLineParser parser = new CommandLineParser(destination.GetType(), reporter);
            return parser.Parse(arguments, destination);
        }

        /// <summary>
        /// Parses Command Line Arguments. Displays usage message to Console.Out
        /// if /?, /help or invalid arguments are encounterd.
        /// Errors are output on Console.Error.
        /// Use ArgumentAttributes to control parsing behaviour.
        /// </summary>
        /// <param name="arguments"> The actual arguments. </param>
        /// <param name="destination"> The resulting parsed arguments. </param>
        /// <returns> true if no errors were detected. </returns>
        public static bool ParseArgumentsWithUsage(string[] arguments, object destination)
        {
            if (ParseHelp(arguments) || !ParseArguments(arguments, destination))
            {
                // error encountered in arguments. Display usage message
                Console.Write(ArgumentsUsage(destination.GetType()));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if a set of arguments asks for help.
        /// </summary>
        /// <param name="args"> Args to check for help. </param>
        /// <returns> Returns true if args contains /? or /help. </returns>
        public static bool ParseHelp(string[] args)
        {
            CommandLineParser helpParser = new CommandLineParser(typeof (HelpArgument), NullErrorReporter);
            HelpArgument helpArgument = new HelpArgument();
            helpParser.Parse(args, helpArgument);
            return helpArgument.help;
        }

        private static void AddNewLine(string newLine, StringBuilder builder, ref int currentColumn)
        {
            builder.Append(newLine);
            currentColumn = 0;
        }

        private static object DefaultValue(ArgumentAttribute attribute)
        {
            return (attribute == null || !attribute.HasDefaultValue) ? null : attribute.DefaultValue;
        }

        private static Type ElementType(FieldInfo field)
        {
            return IsCollectionType(field.FieldType) ? field.FieldType.GetElementType() : null;
        }

        private static bool ExplicitShortName(ArgumentAttribute attribute)
        {
            return (attribute != null && !attribute.DefaultShortName);
        }

        private static ArgumentType Flags(ArgumentAttribute attribute, FieldInfo field)
        {
            if (attribute != null)
            {
                return attribute.Type;
            }

            return IsCollectionType(field.FieldType) ? ArgumentType.MultipleUnique : ArgumentType.AtMostOnce;
        }

        private ArgumentHelpStrings[] GetAllHelpStrings()
        {
            ArgumentHelpStrings[] strings = new ArgumentHelpStrings[NumberOfParametersToDisplay()];

            int index = 0;
            foreach (Argument arg in _arguments)
            {
                strings[index] = GetHelpStrings(arg);
                index++;
            }
            strings[index++] = new ArgumentHelpStrings("@<file>", "Read response file for more options");
            if (_defaultArgument != null)
            {
                strings[index] = GetHelpStrings(_defaultArgument);
            }

            return strings;
        }

        private static ArgumentAttribute GetAttribute(ICustomAttributeProvider field)
        {
            object[] attributes = field.GetCustomAttributes(typeof (ArgumentAttribute), false);
            if (attributes.Length == 1)
                return (ArgumentAttribute) attributes[0];

            Debug.Assert(attributes.Length == 0);
            return null;
        }

        /// <summary>
        /// 01/01/2010 sky
        /// </summary>
        private static string GetGenericUsageString(ICustomAttributeProvider type, int cols)
        {
            object[] attributes = type.GetCustomAttributes(typeof (ArgumentsAttribute), true);
            if (attributes.Length == 0 || !((ArgumentsAttribute) attributes[0]).HasHelpText)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            string usage = ((ArgumentsAttribute) attributes[0]).HelpText;
            // simple width formatter
            string[] lines = Regex.Split(usage, Environment.NewLine);
            foreach (string line in lines)
            {
                string[] words = Regex.Split(line, " ");
                string currentLine = string.Empty;
                foreach (string word in words)
                {
                    if (currentLine.Length + word.Length + 1 > cols)
                    {
                        // start new line
                        sb.AppendLine(currentLine);
                        currentLine = word + " ";
                    }
                    else
                    {
                        currentLine += (word + " ");
                    }
                }
                sb.AppendLine(currentLine);
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }

        private static ArgumentHelpStrings GetHelpStrings(Argument arg)
        {
            return new ArgumentHelpStrings(arg.SyntaxHelp, arg.FullHelpText);
        }

        private static bool HasHelpText(ArgumentAttribute attribute)
        {
            return (attribute != null && attribute.HasHelpText);
        }

        private static string HelpText(ArgumentAttribute attribute)
        {
            return attribute == null ? null : attribute.HelpText;
        }

        private static bool IsCollectionType(Type type)
        {
            return type.IsArray;
        }

        private static bool IsValidElementType(Type type)
        {
            //SKY:12/25/09 - added ushort
            return type != null && (
                                       type == typeof (int) ||
                                       type == typeof (uint) ||
                                       type == typeof (ushort) ||
                                       type == typeof (string) ||
                                       type == typeof (bool) ||
                                       type.IsEnum);
        }

        private bool LexFileArguments(string fileName, out string[] argumentsOut)
        {
            string args;

            try
            {
                using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    args = (new StreamReader(file)).ReadToEnd();
                }
            }
            catch (Exception e)
            {
                _reporter(string.Format("Error: Can't open command line argument file '{0}' : '{1}'", fileName,
                                        e.Message));
                argumentsOut = null;
                return false;
            }

            bool hadError = false;
            ArrayList argArray = new ArrayList();
            StringBuilder currentArg = new StringBuilder();
            bool inQuotes = false;
            int index = 0;

            // while (index < args.Length)
            try
            {
                while (true)
                {
                    // skip whitespace
                    while (char.IsWhiteSpace(args[index]))
                    {
                        index += 1;
                    }

                    // # - comment to end of line
                    if (args[index] == '#')
                    {
                        index += 1;
                        while (args[index] != '\n')
                        {
                            index += 1;
                        }
                        continue;
                    }

                    // do one argument
                    do
                    {
                        if (args[index] == '\\')
                        {
                            int cSlashes = 1;
                            index += 1;
                            while (index == args.Length && args[index] == '\\')
                            {
                                cSlashes += 1;
                            }

                            if (index == args.Length || args[index] != '"')
                            {
                                currentArg.Append('\\', cSlashes);
                            }
                            else
                            {
                                currentArg.Append('\\', (cSlashes >> 1));
                                if (0 != (cSlashes & 1))
                                {
                                    currentArg.Append('"');
                                }
                                else
                                {
                                    inQuotes = !inQuotes;
                                }
                            }
                        }
                        else if (args[index] == '"')
                        {
                            inQuotes = !inQuotes;
                            index += 1;
                        }
                        else
                        {
                            currentArg.Append(args[index]);
                            index += 1;
                        }
                    } while (!char.IsWhiteSpace(args[index]) || inQuotes);
                    argArray.Add(currentArg.ToString());
                    currentArg.Length = 0;
                }
            }
            catch (IndexOutOfRangeException)
            {
                // got EOF 
                if (inQuotes)
                {
                    _reporter(string.Format("Error: Unbalanced '\"' in command line argument file '{0}'", fileName));
                    hadError = true;
                }
                else if (currentArg.Length > 0)
                {
                    // valid argument can be terminated by EOF
                    argArray.Add(currentArg.ToString());
                }
            }

            argumentsOut = (string[]) argArray.ToArray(typeof (string));
            return hadError;
        }

        private static string LongName(ArgumentAttribute attribute, FieldInfo field)
        {
            return (attribute == null || attribute.DefaultLongName) ? field.Name : attribute.LongName;
        }

        private static void NullErrorReporter(string message)
        {
        }

        private int NumberOfParametersToDisplay()
        {
            int numberOfParameters = _arguments.Count + 1;
            if (HasDefaultArgument)
            {
                numberOfParameters += 1;
            }
            return numberOfParameters;
        }

        /// <summary>
        /// Parses an argument list into an object
        /// </summary>
        /// <param name="args"></param>
        /// <param name="destination"></param>
        /// <returns> true if an error occurred </returns>
        private bool ParseArgumentList(IEnumerable<string> args, object destination)
        {
            bool hadError = false;
            if (args != null)
            {
                foreach (string argument in args)
                {
                    if (argument.Length > 0)
                    {
                        switch (argument[0])
                        {
                            case '-':
                            case '/':
                                int endIndex = argument.IndexOfAny(new[] {':', '+', '-'}, 1);
                                string option = argument.Substring(1,
                                                                   endIndex == -1 ? argument.Length - 1 : endIndex - 1);
                                string optionArgument;
                                if (option.Length + 1 == argument.Length)
                                {
                                    optionArgument = null;
                                }
                                else if (argument.Length > 1 + option.Length && argument[1 + option.Length] == ':')
                                {
                                    optionArgument = argument.Substring(option.Length + 2);
                                }
                                else
                                {
                                    optionArgument = argument.Substring(option.Length + 1);
                                }

                                Argument arg = (Argument) _argumentMap[option];
                                if (arg == null)
                                {
                                    ReportUnrecognizedArgument(argument);
                                    hadError = true;
                                }
                                else
                                {
                                    hadError |= !arg.SetValue(optionArgument, destination);
                                }
                                break;
                            case '@':
                                string[] nestedArguments;
                                hadError |= LexFileArguments(argument.Substring(1), out nestedArguments);
                                hadError |= ParseArgumentList(nestedArguments, destination);
                                break;
                            default:
                                if (_defaultArgument != null)
                                {
                                    hadError |= !_defaultArgument.SetValue(argument, destination);
                                }
                                else
                                {
                                    ReportUnrecognizedArgument(argument);
                                    hadError = true;
                                }
                                break;
                        }
                    }
                }
            }

            return hadError;
        }

        private void ReportUnrecognizedArgument(string argument)
        {
            _reporter(string.Format("Unrecognized command line argument '{0}'", argument));
        }

        private static string ShortName(ArgumentAttribute attribute, FieldInfo field)
        {
            if (attribute is DefaultArgumentAttribute)
                return null;
            if (!ExplicitShortName(attribute))
                return LongName(attribute, field).Substring(0, 1);
            return attribute.ShortName;
        }

        #region Nested type: Argument

        [DebuggerDisplay("Name = {LongName}")]
        private class Argument
        {
            private readonly ArrayList _collectionValues;
            private readonly object _defaultValue;
            private readonly Type _elementType;
            private readonly bool _explicitShortName;
            private readonly FieldInfo _field;
            private readonly ArgumentType _flags;
            private readonly bool _hasHelpText;
            private readonly string _helpText;
            private readonly bool _isDefault;
            private readonly string _longName;
            private readonly ErrorReporter _reporter;
            private bool _seenValue;
            private string _shortName;

            public Argument(ArgumentAttribute attribute, FieldInfo field, ErrorReporter reporter)
            {
                _longName = CommandLineParser.LongName(attribute, field);
                _explicitShortName = CommandLineParser.ExplicitShortName(attribute);
                _shortName = CommandLineParser.ShortName(attribute, field);
                _hasHelpText = CommandLineParser.HasHelpText(attribute);
                _helpText = CommandLineParser.HelpText(attribute);
                _defaultValue = CommandLineParser.DefaultValue(attribute);
                _elementType = ElementType(field);
                _flags = Flags(attribute, field);
                _field = field;
                _seenValue = false;
                _reporter = reporter;
                _isDefault = attribute != null && attribute is DefaultArgumentAttribute;

                if (IsCollection)
                {
                    _collectionValues = new ArrayList();
                }

                Debug.Assert(!string.IsNullOrEmpty(_longName));
                Debug.Assert(!_isDefault || !ExplicitShortName);
                Debug.Assert(!IsCollection || AllowMultiple, "Collection arguments must have allow multiple");
                Debug.Assert(!Unique || IsCollection, "Unique only applicable to collection arguments");
                Debug.Assert(IsValidElementType(Type) || IsCollectionType(Type));
                Debug.Assert((IsCollection && IsValidElementType(_elementType)) ||
                             (!IsCollection && _elementType == null));
                Debug.Assert(!(IsRequired && HasDefaultValue), "Required arguments cannot have default value");
                Debug.Assert(!HasDefaultValue || (_defaultValue.GetType() == field.FieldType),
                             "Type of default value must match field type");
            }

            private Type ValueType
            {
                get { return IsCollection ? _elementType : Type; }
            }

            public string LongName
            {
                get { return _longName; }
            }

            public bool ExplicitShortName
            {
                get { return _explicitShortName; }
            }

            public string ShortName
            {
                get { return _shortName; }
            }

            private bool HasShortName
            {
                get { return _shortName != null; }
            }

            private bool HasHelpText
            {
                get { return _hasHelpText; }
            }

            private string HelpText
            {
                get { return _helpText; }
            }

            private object DefaultValue
            {
                get { return _defaultValue; }
            }

            private bool HasDefaultValue
            {
                get { return null != _defaultValue; }
            }

            public string FullHelpText
            {
                get
                {
                    StringBuilder builder = new StringBuilder();
                    if (HasHelpText)
                    {
                        builder.Append(HelpText);
                    }
                    if (HasDefaultValue)
                    {
                        if (builder.Length > 0)
                            builder.Append(" ");
                        builder.Append("Default value:'");
                        AppendValue(builder, DefaultValue);
                        builder.Append('\'');
                    }
                    if (HasShortName)
                    {
                        if (builder.Length > 0)
                            builder.Append(" ");
                        builder.Append("(short form /");
                        builder.Append(ShortName);
                        builder.Append(")");
                    }
                    return builder.ToString();
                }
            }

            public string SyntaxHelp
            {
                get
                {
                    StringBuilder builder = new StringBuilder();

                    if (IsDefault)
                    {
                        builder.Append("<");
                        builder.Append(LongName);
                        builder.Append(">");
                    }
                    else
                    {
                        builder.Append("/");
                        builder.Append(LongName);
                        Type valueType = ValueType;
                        if (valueType == typeof (int))
                        {
                            builder.Append(":<int>");
                        }
                        else if (valueType == typeof (uint))
                        {
                            builder.Append(":<uint>");
                        }
                        else if (valueType == typeof (bool))
                        {
                            builder.Append("[+|-]");
                        }
                        else if (valueType == typeof (string))
                        {
                            builder.Append(":<string>");
                        }
                        else
                        {
                            Debug.Assert(valueType.IsEnum);

                            builder.Append(":{");
                            bool first = true;
                            foreach (FieldInfo info in valueType.GetFields())
                            {
                                if (info.IsStatic)
                                {
                                    if (first)
                                        first = false;
                                    else
                                        builder.Append('|');
                                    builder.Append(info.Name);
                                }
                            }
                            builder.Append('}');
                        }
                    }

                    return builder.ToString();
                }
            }

            private bool IsRequired
            {
                get { return 0 != (_flags & ArgumentType.Required); }
            }

            private bool SeenValue
            {
                get { return _seenValue; }
            }

            private bool AllowMultiple
            {
                get { return 0 != (_flags & ArgumentType.Multiple); }
            }

            private bool Unique
            {
                get { return 0 != (_flags & ArgumentType.Unique); }
            }

            private Type Type
            {
                get { return _field.FieldType; }
            }

            private bool IsCollection
            {
                get { return IsCollectionType(Type); }
            }

            private bool IsDefault
            {
                get { return _isDefault; }
            }

            public bool Finish(object destination)
            {
                if (SeenValue)
                {
                    if (IsCollection)
                    {
                        _field.SetValue(destination, _collectionValues.ToArray(_elementType));
                    }
                }
                else
                {
                    if (HasDefaultValue)
                    {
                        _field.SetValue(destination, DefaultValue);
                    }
                }

                return ReportMissingRequiredArgument();
            }

            private bool ReportMissingRequiredArgument()
            {
                if (IsRequired && !SeenValue)
                {
                    if (IsDefault)
                        _reporter(string.Format("Missing required argument '<{0}>'.", LongName));
                    else
                        _reporter(string.Format("Missing required argument '/{0}'.", LongName));
                    return true;
                }
                return false;
            }

            private void ReportDuplicateArgumentValue(string value)
            {
                _reporter(string.Format("Duplicate '{0}' argument '{1}'", LongName, value));
            }

            public bool SetValue(string value, object destination)
            {
                if (SeenValue && !AllowMultiple)
                {
                    _reporter(string.Format("Duplicate '{0}' argument", LongName));
                    return false;
                }
                _seenValue = true;

                object newValue;

                if (!ParseValue(ValueType, value, out newValue))
                {
                    return false;
                }

                if (IsCollection)
                {
                    if (Unique && _collectionValues.Contains(newValue))
                    {
                        ReportDuplicateArgumentValue(value);
                        return false;
                    }
                    _collectionValues.Add(newValue);
                }
                else
                {
                    _field.SetValue(destination, newValue);
                }

                return true;
            }

            private void ReportBadArgumentValue(string value)
            {
                _reporter(string.Format("'{0}' is not a valid value for the '{1}' command line option", value, LongName));
            }

            private bool ParseValue(Type type, string stringData, out object value)
            {
                // null is only valid for bool variables
                // empty string is never valid
                if (!string.IsNullOrEmpty(stringData) || type == typeof (bool))
                {
                    try
                    {
                        do //omed loop
                        {
                            if (type == typeof (string))
                            {
                                value = stringData;
                                return true;
                            }

                            if (type == typeof (bool))
                            {
                                if (stringData == null || stringData == "+")
                                {
                                    value = true;
                                    return true;
                                }

                                if (stringData == "-")
                                {
                                    value = false;
                                    return true;
                                }
                                break;
                            }

                            // from this point on, a null or empty string is invalid
                            if (string.IsNullOrEmpty(stringData))
                            {
                                break;
                            }

                            if (type == typeof (int))
                            {
                                value = int.Parse(stringData);
                                return true;
                            }

                            if (type == typeof (uint))
                            {
                                value = int.Parse(stringData);
                                return true;
                            }

                            //SKY:12/25/09 - added ushort
                            if (type == typeof (ushort))
                            {
                                value = ushort.Parse(stringData);
                                return true;
                            }

                            Debug.Assert(type.IsEnum);

                            bool valid = false;

                            foreach (string name in Enum.GetNames(type))
                            {
                                if (string.Compare(name, stringData, StringComparison.InvariantCultureIgnoreCase) == 0)
                                {
                                    valid = true;
                                    break;
                                }
                            }
                            if (valid)
                            {
                                value = Enum.Parse(type, stringData, true);
                                return true;
                            }
                        } while (false);
                    }
                        // ReSharper disable EmptyGeneralCatchClause
                    catch
                        // ReSharper restore EmptyGeneralCatchClause
                    {
                        // catch parse errors
                    }
                }

                ReportBadArgumentValue(stringData);
                value = null;
                return false;
            }

            private static void AppendValue(StringBuilder builder, object value)
            {
                //SKY:12/25/09 - add ushort
                if (value is string || value is int || value is uint || value is ushort || value.GetType().IsEnum)
                {
                    builder.Append(value.ToString());
                }
                else if (value is bool)
                {
                    builder.Append((bool) value ? "+" : "-");
                }
                else
                {
                    bool first = true;
                    foreach (object o in (Array) value)
                    {
                        if (!first)
                        {
                            builder.Append(", ");
                        }
                        AppendValue(builder, o);
                        first = false;
                    }
                }
            }

            public void ClearShortName()
            {
                _shortName = null;
            }
        }

        #endregion

        #region Nested type: ArgumentHelpStrings

        private struct ArgumentHelpStrings
        {
            public readonly string help;
            public readonly string syntax;

            public ArgumentHelpStrings(string syntax, string help)
            {
                this.syntax = syntax;
                this.help = help;
            }
        }

        #endregion

        #region Nested type: HelpArgument

        private class HelpArgument
        {
            [Argument(ArgumentType.AtMostOnce, ShortName = "?")] public bool help;
        }

        #endregion
    }

    /// <summary>
    /// A delegate used in error reporting.
    /// </summary>
    public delegate void ErrorReporter(string message);

    /// <summary>
    /// Indicates that this argument is the default argument.
    /// '/' or '-' prefix only the argument value is specified.
    /// The ShortName property should not be set for DefaultArgumentAttribute
    /// instances. The LongName property is used for usage text only and
    /// does not affect the usage of the argument.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class DefaultArgumentAttribute : ArgumentAttribute
    {
        /// <summary>
        /// Indicates that this argument is the default argument.
        /// </summary>
        /// <param name="type"> Specifies the error checking to be done on the argument. </param>
        public DefaultArgumentAttribute(ArgumentType type)
            : base(type)
        {
        }
    }

    /// <summary>
    /// Used to control parsing of command line arguments.
    /// </summary>
    [Flags]
    public enum ArgumentType
    {
        /// <summary>
        /// Indicates that this field is required. An error will be displayed
        /// if it is not present when parsing arguments.
        /// </summary>
        Required = 0x01,
        /// <summary>
        /// Only valid in conjunction with Multiple.
        /// Duplicate values will result in an error.
        /// </summary>
        Unique = 0x02,
        /// <summary>
        /// Inidicates that the argument may be specified more than once.
        /// Only valid if the argument is a collection
        /// </summary>
        Multiple = 0x04,

        /// <summary>
        /// The default type for non-collection arguments.
        /// The argument is not required, but an error will be reported if it is specified more than once.
        /// </summary>
        AtMostOnce = 0x00,

        /// <summary>
        /// For non-collection arguments, when the argument is specified more than
        /// once no error is reported and the value of the argument is the last
        /// value which occurs in the argument list.
        /// </summary>
        LastOccurenceWins = Multiple,

        /// <summary>
        /// The default type for collection arguments.
        /// The argument is permitted to occur multiple times, but duplicate 
        /// values will cause an error to be reported.
        /// </summary>
        MultipleUnique = Multiple | Unique,
    }

    /// <summary>
    /// Allows attaching generic help text to arguments class
    /// 
    /// 12/29/09 sky: added 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ArgumentsAttribute : Attribute
    {
        private string _helpText;

        /// <summary>
        /// Returns true if the argument has help text specified.
        /// </summary>
        public bool HasHelpText
        {
            get { return null != _helpText; }
        }

        /// <summary>
        /// The help text for the argument.
        /// </summary>
        public string HelpText
        {
            get { return _helpText; }
            set { _helpText = value; }
        }
    }

    /// <summary>
    /// Allows control of command line parsing.
    /// Attach this attribute to instance fields of types used
    /// as the destination of command line argument parsing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ArgumentAttribute : Attribute
    {
        #region Fields

        private readonly ArgumentType _type;
        private object _defaultValue;

        private string _helpText;

        private string _longName;

        private string _shortName;

        #endregion

        #region Constructors

        /// <summary>
        /// Allows control of command line parsing.
        /// </summary>
        /// <param name="type"> Specifies the error checking to be done on the argument. </param>
        public ArgumentAttribute(ArgumentType type)
        {
            _type = type;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns true if the argument did not have an explicit long name specified.
        /// </summary>
        public bool DefaultLongName
        {
            get { return null == _longName; }
        }

        /// <summary>
        /// Returns true if the argument did not have an explicit short name specified.
        /// </summary>
        public bool DefaultShortName
        {
            get { return null == _shortName; }
        }

        /// <summary>
        /// The default value of the argument.
        /// </summary>
        public object DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }

        /// <summary>
        /// Returns true if the argument has a default value.
        /// </summary>
        public bool HasDefaultValue
        {
            get { return null != _defaultValue; }
        }

        /// <summary>
        /// Returns true if the argument has help text specified.
        /// </summary>
        public bool HasHelpText
        {
            get { return null != _helpText; }
        }

        /// <summary>
        /// The help text for the argument.
        /// </summary>
        public string HelpText
        {
            get { return _helpText; }
            set { _helpText = value; }
        }

        /// <summary>
        /// The long name of the argument.
        /// Set to null means use the default long name.
        /// The long name for every argument must be unique.
        /// It is an error to specify a long name of String.Empty.
        /// </summary>
        public string LongName
        {
            get
            {
                Debug.Assert(!DefaultLongName);
                return _longName;
            }
            set
            {
                Debug.Assert(value != "");
                _longName = value;
            }
        }

        /// <summary>
        /// The short name of the argument.
        /// Set to null means use the default short name if it does not
        /// conflict with any other parameter name.
        /// Set to String.Empty for no short name.
        /// This property should not be set for DefaultArgumentAttributes.
        /// </summary>
        public string ShortName
        {
            get { return _shortName; }
            set
            {
                Debug.Assert(value == null || !(this is DefaultArgumentAttribute));
                _shortName = value;
            }
        }

        /// <summary>
        /// The error checking to be done on the argument.
        /// </summary>
        public ArgumentType Type
        {
            get { return _type; }
        }

        #endregion
    }
}