using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Processes;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class CommandLineArgumentsFixture
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase(" \t ")]
        public void should_return_no_arguments_for_empty_input(string commandLine)
        {
            CommandLineArguments.Split(commandLine).Should().BeEmpty();
        }

        [TestCase("arg1", new[] { "arg1" })]
        [TestCase("arg1 arg2", new[] { "arg1", "arg2" })]
        [TestCase("arg1 arg2 arg3", new[] { "arg1", "arg2", "arg3" })]
        [TestCase("  arg1   arg2    arg3  ", new[] { "arg1", "arg2", "arg3" })]
        [TestCase("arg1\targ2", new[] { "arg1", "arg2" })]
        public void should_split_on_spaces_and_tabs(string commandLine, string[] expected)
        {
            CommandLineArguments.Split(commandLine).Should().Equal(expected);
        }

        [TestCase("\"arg1\"", new[] { "arg1" })]
        [TestCase("\"arg1\" arg2", new[] { "arg1", "arg2" })]
        [TestCase("arg1 arg2 \"arg 3\"", new[] { "arg1", "arg2", "arg 3" })]
        [TestCase("\"   \"", new[] { "   " })]
        [TestCase("\"c:\\path with spaces\"", new[] { "c:\\path with spaces" })]
        [TestCase("/data=\"/path/with spaces\"", new[] { "/data=/path/with spaces" })]
        [TestCase("a\"b c\"d", new[] { "ab cd" })]
        public void should_group_quoted_sections(string commandLine, string[] expected)
        {
            CommandLineArguments.Split(commandLine).Should().Equal(expected);
        }

        [TestCase("\"arg 1", new[] { "arg 1" })]
        [TestCase("arg1 arg2 \"arg 3", new[] { "arg1", "arg2", "arg 3" })]
        public void should_treat_unclosed_quote_as_running_to_end(string commandLine, string[] expected)
        {
            CommandLineArguments.Split(commandLine).Should().Equal(expected);
        }

        [TestCase("\"\"", new[] { "" })]
        [TestCase("arg1 \"\" arg2", new[] { "arg1", "", "arg2" })]
        public void should_preserve_empty_quoted_arguments(string commandLine, string[] expected)
        {
            CommandLineArguments.Split(commandLine).Should().Equal(expected);
        }

        [TestCase("\"a\"\"b\"", new[] { "a\"b" })]
        [TestCase("\"a \"\" b\"", new[] { "a \" b" })]
        public void should_treat_doubled_quote_inside_quotes_as_literal(string commandLine, string[] expected)
        {
            CommandLineArguments.Split(commandLine).Should().Equal(expected);
        }

        [TestCase("c:\\users\\test", new[] { "c:\\users\\test" })]
        [TestCase("\\\\server\\share", new[] { "\\\\server\\share" })]
        [TestCase("a\\\\ b", new[] { "a\\\\", "b" })]
        public void should_keep_backslashes_not_followed_by_quote(string commandLine, string[] expected)
        {
            CommandLineArguments.Split(commandLine).Should().Equal(expected);
        }

        [TestCase("a\\\"b", new[] { "a\"b" })]
        [TestCase("a\\\\\"b c\"", new[] { "a\\b c" })]
        [TestCase("a\\\\\\\"b", new[] { "a\\\"b" })]
        [TestCase("\"c:\\users\\test\\\\\" next", new[] { "c:\\users\\test\\", "next" })]
        public void should_apply_backslash_rules_before_quotes(string commandLine, string[] expected)
        {
            CommandLineArguments.Split(commandLine).Should().Equal(expected);
        }

        [Test]
        public void should_treat_escaped_trailing_quote_as_literal()
        {
            var commandLine = "-data=\"c:\\users\\test\\\" -nobrowser";

            CommandLineArguments.Split(commandLine).Should().Equal("-data=c:\\users\\test\" -nobrowser");
        }

        [TestCase("SHOW.s01e01.\"; whoami; \".mkv", new[] { "SHOW.s01e01.; whoami; .mkv" })]
        [TestCase("a;b", new[] { "a;b" })]
        [TestCase("$(whoami) `id` | & > <", new[] { "$(whoami)", "`id`", "|", "&", ">", "<" })]
        [TestCase("'single quoted'", new[] { "'single", "quoted'" })]
        public void should_not_interpret_shell_metacharacters(string commandLine, string[] expected)
        {
            CommandLineArguments.Split(commandLine).Should().Equal(expected);
        }
    }
}
