using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class add_custom_formatsFixture : MigrationTest<add_custom_formats>
    {
        [Test]
        public void should_add_cf_from_named_release_profile()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("ReleaseProfiles").Row(new
                {
                    Name = "Profile",
                    Preferred = new[]
                    {
                        new
                        {
                            Key = "x264",
                            Value = 2
                        }
                    }.ToJson(),
                    Required = "[]",
                    Ignored = "[]",
                    Tags = "[]",
                    IncludePreferredWhenRenaming = false,
                    Enabled = true,
                    IndexerId = 0
                });
            });

            var customFormats = db.Query<CustomFormat171>("SELECT Id, Name, IncludeCustomFormatWhenRenaming, Specifications FROM CustomFormats");

            customFormats.Should().HaveCount(1);
            customFormats.First().Name.Should().Be("Profile_1");
            customFormats.First().IncludeCustomFormatWhenRenaming.Should().BeFalse();
            customFormats.First().Specifications.Should().HaveCount(1);
        }

        [Test]
        public void should_not_migrate_if_bad_regex_in_release_profile()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("ReleaseProfiles").Row(new
                {
                    Name = "Profile",
                    Preferred = new[]
                    {
                        new
                        {
                            Key = "[somestring[",
                            Value = 2
                        }
                    }.ToJson(),
                    Required = "[]",
                    Ignored = "[]",
                    Tags = "[]",
                    IncludePreferredWhenRenaming = true,
                    Enabled = true,
                    IndexerId = 0
                });
            });

            var customFormats = db.Query<CustomFormat171>("SELECT Id, Name, IncludeCustomFormatWhenRenaming, Specifications FROM CustomFormats");

            customFormats.Should().HaveCount(0);
        }

        [Test]
        public void should_set_cf_naming_token_if_set_in_release_profile()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("ReleaseProfiles").Row(new
                {
                    Name = "Profile",
                    Preferred = new[]
                    {
                        new
                        {
                            Key = "x264",
                            Value = 2
                        }
                    }.ToJson(),
                    Required = "[]",
                    Ignored = "[]",
                    Tags = "[]",
                    IncludePreferredWhenRenaming = true,
                    Enabled = true,
                    IndexerId = 0
                });
            });

            var customFormats = db.Query<CustomFormat171>("SELECT Id, Name, IncludeCustomFormatWhenRenaming, Specifications FROM CustomFormats");

            customFormats.Should().HaveCount(1);
            customFormats.First().Name.Should().Be("Profile_1");
            customFormats.First().IncludeCustomFormatWhenRenaming.Should().BeTrue();
            customFormats.First().Specifications.Should().HaveCount(1);
        }

        [Test]
        public void should_not_remove_release_profile_if_ignored_or_required()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("ReleaseProfiles").Row(new
                {
                    Name = "Profile",
                    Preferred = new[]
                    {
                        new
                        {
                            Key = "x264",
                            Value = 2
                        }
                    }.ToJson(),
                    Required = new[]
                    {
                        "some",
                        "words"
                    }.ToJson(),
                    Ignored = "[]",
                    Tags = "[]",
                    IncludePreferredWhenRenaming = true,
                    Enabled = true,
                    IndexerId = 0
                });
            });

            var releaseProfiles = db.Query<ReleaseProfile171>("SELECT Id, Name FROM ReleaseProfiles");

            releaseProfiles.Should().HaveCount(1);
            releaseProfiles.First().Name.Should().Be("Profile");
        }

        [Test]
        public void should_remove_release_profile_if_no_ignored_or_required()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("ReleaseProfiles").Row(new
                {
                    Name = "Profile",
                    Preferred = new[]
                    {
                        new
                        {
                            Key = "x264",
                            Value = 2
                        }
                    }.ToJson(),
                    Required = "[]",
                    Ignored = "[]",
                    Tags = "[]",
                    IncludePreferredWhenRenaming = true,
                    Enabled = true,
                    IndexerId = 0
                });
            });

            var releaseProfiles = db.Query<ReleaseProfile171>("SELECT Id, Name FROM ReleaseProfiles");

            releaseProfiles.Should().HaveCount(0);
        }

        [Test]
        public void should_add_cf_from_unnamed_release_profile()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("ReleaseProfiles").Row(new
                {
                    Preferred = new[]
                    {
                        new
                        {
                            Key = "x264",
                            Value = 2
                        }
                    }.ToJson(),
                    Required = "[]",
                    Ignored = "[]",
                    Tags = "[]",
                    IncludePreferredWhenRenaming = false,
                    Enabled = true,
                    IndexerId = 0
                });
            });

            var customFormats = db.Query<CustomFormat171>("SELECT Id, Name, IncludeCustomFormatWhenRenaming, Specifications FROM CustomFormats");

            customFormats.Should().HaveCount(1);
            customFormats.First().Name.Should().Be("Unnamed_1");
            customFormats.First().IncludeCustomFormatWhenRenaming.Should().BeFalse();
            customFormats.First().Specifications.Should().HaveCount(1);
        }

        [Test]
        public void should_add_cfs_from_multiple_unnamed_release_profile()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("ReleaseProfiles").Row(new
                {
                    Preferred = new[]
                    {
                        new
                        {
                            Key = "x264",
                            Value = 2
                        }
                    }.ToJson(),
                    Required = "[]",
                    Ignored = "[]",
                    Tags = "[]",
                    IncludePreferredWhenRenaming = false,
                    Enabled = true,
                    IndexerId = 0
                });

                c.Insert.IntoTable("ReleaseProfiles").Row(new
                {
                    Preferred = new[]
                    {
                        new
                        {
                            Key = "x265",
                            Value = 2
                        }
                    }.ToJson(),
                    Required = "[]",
                    Ignored = "[]",
                    Tags = "[]",
                    IncludePreferredWhenRenaming = false,
                    Enabled = true,
                    IndexerId = 0
                });
            });

            var customFormats = db.Query<CustomFormat171>("SELECT Id, Name, IncludeCustomFormatWhenRenaming, Specifications FROM CustomFormats");

            customFormats.Should().HaveCount(2);
            customFormats.First().Name.Should().Be("Unnamed_1");
            customFormats.Last().Name.Should().Be("Unnamed_2");
            customFormats.First().IncludeCustomFormatWhenRenaming.Should().BeFalse();
            customFormats.First().Specifications.Should().HaveCount(1);
        }

        [Test]
        public void should_add_cfs_same_named_release_profiles()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("ReleaseProfiles").Row(new
                {
                    Name = "Some - Profile",
                    Preferred = new[]
                    {
                        new
                        {
                            Key = "x264",
                            Value = 2
                        },
                        new
                        {
                            Key = "x265",
                            Value = 3
                        }
                    }.ToJson(),
                    Required = "[]",
                    Ignored = "[]",
                    Tags = "[]",
                    IncludePreferredWhenRenaming = false,
                    Enabled = true,
                    IndexerId = 0
                });

                c.Insert.IntoTable("ReleaseProfiles").Row(new
                {
                    Name = "Some - Profile",
                    Preferred = new[]
                    {
                        new
                        {
                            Key = "x264",
                            Value = 2
                        },
                        new
                        {
                            Key = "x265",
                            Value = 3
                        }
                    }.ToJson(),
                    Required = "[]",
                    Ignored = "[]",
                    Tags = "[]",
                    IncludePreferredWhenRenaming = false,
                    Enabled = true,
                    IndexerId = 0
                });

                c.Insert.IntoTable("ReleaseProfiles").Row(new
                {
                    Name = "Some - Profile",
                    Preferred = new[]
                    {
                        new
                        {
                            Key = "x264",
                            Value = 2
                        },
                        new
                        {
                            Key = "x265",
                            Value = 3
                        }
                    }.ToJson(),
                    Required = "[]",
                    Ignored = "[]",
                    Tags = "[]",
                    IncludePreferredWhenRenaming = false,
                    Enabled = true,
                    IndexerId = 0
                });
            });

            var customFormats = db.Query<CustomFormat171>("SELECT Id, Name, IncludeCustomFormatWhenRenaming, Specifications FROM CustomFormats");

            customFormats.Should().HaveCount(6);
            customFormats.First().Name.Should().Be("Some - Profile_1_0");
            customFormats.Last().Name.Should().Be("Some - Profile_3_1");
            customFormats.First().IncludeCustomFormatWhenRenaming.Should().BeFalse();
            customFormats.First().Specifications.Should().HaveCount(1);
        }

        [Test]
        public void should_add_two_cfs_if_release_profile_has_multiple_terms()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("ReleaseProfiles").Row(new
                {
                    Name = "Profile",
                    Preferred = new[]
                    {
                        new
                        {
                            Key = "x264",
                            Value = 2
                        },
                        new
                        {
                            Key = "x265",
                            Value = 5
                        }
                    }.ToJson(),
                    Required = "[]",
                    Ignored = "[]",
                    Tags = "[]",
                    IncludePreferredWhenRenaming = false,
                    Enabled = true,
                    IndexerId = 0
                });
            });

            var customFormats = db.Query<CustomFormat171>("SELECT Id, Name, IncludeCustomFormatWhenRenaming, Specifications FROM CustomFormats");

            customFormats.Should().HaveCount(2);
            customFormats.First().Name.Should().Be("Profile_1_0");
            customFormats.Last().Name.Should().Be("Profile_1_1");
            customFormats.First().IncludeCustomFormatWhenRenaming.Should().BeFalse();
            customFormats.First().Specifications.Should().HaveCount(1);
        }

        [Test]
        public void should_set_scores_for_enabled_release_profiles()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("ReleaseProfiles").Row(new
                {
                    Name = "Profile",
                    Preferred = new[]
                    {
                        new
                        {
                            Key = "x264",
                            Value = 2
                        }
                    }.ToJson(),
                    Required = "[]",
                    Ignored = "[]",
                    Tags = "[]",
                    IncludePreferredWhenRenaming = false,
                    Enabled = true,
                    IndexerId = 0
                });

                c.Insert.IntoTable("QualityProfiles").Row(new
                {
                    Name = "SDTV",
                    Cutoff = 1,
                    Items = "[ { \"quality\": 1, \"allowed\": true } ]"
                });
            });

            var customFormats = db.Query<QualityProfile171>("SELECT Id, Name, FormatItems FROM QualityProfiles");

            customFormats.Should().HaveCount(1);
            customFormats.First().FormatItems.Should().HaveCount(1);
            customFormats.First().FormatItems.First().Score.Should().Be(2);
        }

        [Test]
        public void should_set_zero_scores_for_disabled_release_profiles()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("ReleaseProfiles").Row(new
                {
                    Name = "Profile",
                    Preferred = new[]
                    {
                        new
                        {
                            Key = "x264",
                            Value = 2
                        }
                    }.ToJson(),
                    Required = "[]",
                    Ignored = "[]",
                    Tags = "[]",
                    IncludePreferredWhenRenaming = false,
                    Enabled = false,
                    IndexerId = 0
                });

                c.Insert.IntoTable("QualityProfiles").Row(new
                {
                    Name = "SDTV",
                    Cutoff = 1,
                    Items = "[ { \"quality\": 1, \"allowed\": true } ]"
                });
            });

            var customFormats = db.Query<QualityProfile171>("SELECT Id, Name, FormatItems FROM QualityProfiles");

            customFormats.Should().HaveCount(1);
            customFormats.First().FormatItems.Should().HaveCount(1);
            customFormats.First().FormatItems.First().Score.Should().Be(0);
        }

        [Test]
        public void should_migrate_naming_configs()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("NamingConfig").Row(new
                {
                    MultiEpisodeStyle = false,
                    StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title} {Preferred Words } {Quality Full}",
                    DailyEpisodeFormat = "{Series Title} - {Air-Date} - {Episode Title} {Preferred.Words } {Quality Full}",
                    AnimeEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Preferred_Words} {Quality Full}",
                });
            });

            var customFormats = db.Query<NamingConfig171>("SELECT StandardEpisodeFormat, DailyEpisodeFormat, AnimeEpisodeFormat FROM NamingConfig");

            customFormats.Should().HaveCount(1);
            customFormats.First().StandardEpisodeFormat.Should().Be("{Series Title} - S{season:00}E{episode:00} - {Episode Title} {Custom Formats } {Quality Full}");
            customFormats.First().DailyEpisodeFormat.Should().Be("{Series Title} - {Air-Date} - {Episode Title} {Custom.Formats } {Quality Full}");
            customFormats.First().AnimeEpisodeFormat.Should().Be("{Series Title} - S{season:00}E{episode:00} - {Custom_Formats} {Quality Full}");
        }

        private class NamingConfig171
        {
            public string StandardEpisodeFormat { get; set; }
            public string DailyEpisodeFormat { get; set; }
            public string AnimeEpisodeFormat { get; set; }
        }

        private class ReleaseProfile171
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class QualityProfile171
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<FormatItem171> FormatItems { get; set; }
        }

        private class FormatItem171
        {
            public int Format { get; set; }
            public int Score { get; set; }
        }

        private class CustomFormat171
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IncludeCustomFormatWhenRenaming { get; set; }
            public List<CustomFormatSpec171> Specifications { get; set; }
        }

        private class CustomFormatSpec171
        {
            public string Type { get; set; }
            public CustomFormatReleaseTitleSpec171 Body { get; set; }
        }

        private class CustomFormatReleaseTitleSpec171
        {
            public int Order { get; set; }
            public string ImplementationName { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public bool Required { get; set; }
            public bool Negate { get; set; }
        }
    }
}
