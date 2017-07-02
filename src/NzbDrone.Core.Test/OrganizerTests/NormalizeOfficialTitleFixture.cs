using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.OrganizerTests
{
    [TestFixture]
    public class NormalizeOfficialTitleFixture : CoreTest
    {
        [TestCase("$#*! My Dad Says", "S#* My Dad Says")]
        //[TestCase("", "")]
        public void should_scenify_special_cases(string title, string expected)
        {
            // These need special handling on a case by case basis.
            NormalizeOfficialTitle.ScenifyTitle(title).Should().Be(expected);
        }

        [TestCase("@midnight", "At midnight")]
        [TestCase("Murder @ 9", "Murder at 9")]
        [TestCase("T@gged", "Tagged")]
        [TestCase("PUCHIM@S", "PUCHIMAS")]
        [TestCase("extr@", "extra")]
        [TestCase("Live@Much", "Live at Much")]
        //[TestCase("", "")]
        public void should_scenify_at_char(string title, string expected)
        {
            NormalizeOfficialTitle.ScenifyTitle(title).Should().Be(expected);
        }

        [TestCase("3%", "3 Percent")]
        //[TestCase("", "")]
        public void should_scenify_percent_char(string title, string expected)
        {
            NormalizeOfficialTitle.ScenifyTitle(title).Should().Be(expected);
        }

        [TestCase("Law & Order (UK)", "Law and Order UK")]
        [TestCase("Sun, Sea and A&E", "Sun Sea and A and E")]
        //[TestCase("", "")]
        public void should_scenify_and_char(string title, string expected)
        {
            NormalizeOfficialTitle.ScenifyTitle(title).Should().Be(expected);
        }

        [TestCase("Code:Breaker", "Code Breaker")]
        [TestCase("Transformers: Prime", "Transformers Prime")]
        [TestCase("Mobile Suit Gundam UC RE:0096", "Mobile Suit Gundam UC RE 0096")]
        [TestCase("What the Bleep!?: Down the Rabbit Hole", "What the Bleep Down the Rabbit Hole")]
        //[TestCase("", "")]
        public void should_scenify_colon_char(string title, string expected)
        {
            NormalizeOfficialTitle.ScenifyTitle(title).Should().Be(expected);
        }

        [TestCase("Sun, Sea and A&E", "Sun Sea and A and E")]
        [TestCase("The $25,000 Pyramid", "The 25000 Pyramid")]
        //[TestCase("", "")]
        public void should_scenify_comma_char(string title, string expected)
        {
            NormalizeOfficialTitle.ScenifyTitle(title).Should().Be(expected);
        }

        //[TestCase("The $100,000 Pyramid", "The 100000 Dollar Pyramid")]
        [TestCase("$25 Million Dollar Hoax", "25 Million Dollar Hoax")]
        [TestCase("Arli$$", "Arliss")]
        [TestCase("Country Buck$", "Country Bucks")]
        [TestCase("Tamara Ecclestone: Billion $$ Girl", "Tamara Ecclestone Billion Dollar Girl")]
        [TestCase("$#*! My Dad Says", "S#* My Dad Says")]
        //[TestCase("", "")]
        public void should_scenify_dollar_char(string title, string expected)
        {
            NormalizeOfficialTitle.ScenifyTitle(title).Should().Be(expected);
        }

        [TestCase("Separation?!", "Separation")]
        [TestCase("Snog Marry Avoid?", "Snog Marry Avoid")]
        [TestCase("What the Bleep!?: Down the Rabbit Hole", "What the Bleep Down the Rabbit Hole")]
        //[TestCase("", "")]
        public void should_scenify_question_char(string title, string expected)
        {
            NormalizeOfficialTitle.ScenifyTitle(title).Should().Be(expected);
        }

        [TestCase("Separation?!", "Separation")]
        [TestCase("What the Bleep!?: Down the Rabbit Hole", "What the Bleep Down the Rabbit Hole")]
        [TestCase("What's Happening!!", "Whats Happening")]
        //[TestCase("", "")]
        public void should_scenify_exclamation_char(string title, string expected)
        {
            NormalizeOfficialTitle.ScenifyTitle(title).Should().Be(expected);
        }

        [TestCase("Bro'Town", "Bro Town")]
        [TestCase("'Til Death", "Til Death")]
        [TestCase("Those Who Can't", "Those Who Cant")]
        [TestCase("Paul O'Grady: For the Love of Dogs", "Paul O Grady For the Love of Dogs")]
        [TestCase("Bitchin' Rides", "Bitchin Rides")]
        [TestCase("Trust Me, I'm a Vet", "Trust Me Im a Vet")]
        [TestCase("You're the Worst", "Youre the Worst")]
        //[TestCase("", "")]
        public void should_scenify_quote_char(string title, string expected)
        {
            NormalizeOfficialTitle.ScenifyTitle(title).Should().Be(expected);
        }

        [TestCase("Robotics;Notes", "Robotics Notes")]
        [TestCase("Myself; Yourself", "Myself Yourself")]
        //[TestCase("", "")]
        public void should_scenify_semicolon_char(string title, string expected)
        {
            NormalizeOfficialTitle.ScenifyTitle(title).Should().Be(expected);
        }

        [TestCase("Acquisitions Incorporated: The \"C\" Team", "Acquisitions Incorporated The C Team")]
        //[TestCase("", "")]
        public void should_scenify_doublequote_char(string title, string expected)
        {
            NormalizeOfficialTitle.ScenifyTitle(title).Should().Be(expected);
        }
    }
}
