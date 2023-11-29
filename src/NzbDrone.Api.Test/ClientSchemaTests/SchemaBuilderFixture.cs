using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Localization;
using NzbDrone.Test.Common;
using Sonarr.Http.ClientSchema;

namespace NzbDrone.Api.Test.ClientSchemaTests
{
    [TestFixture]
    public class SchemaBuilderFixture : TestBase
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ILocalizationService>()
                .Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
                .Returns<string, Dictionary<string, object>>((s, d) => s);

            SchemaBuilder.Initialize(Mocker.Container);
        }

        [Test]
        public void should_return_field_for_every_property()
        {
            var schema = SchemaBuilder.ToSchema(new TestModel());
            schema.Should().HaveCount(2);
        }

        [Test]
        public void schema_should_have_proper_fields()
        {
            var model = new TestModel
            {
                FirstName = "Bob",
                LastName = "Poop"
            };

            var schema = SchemaBuilder.ToSchema(model);

            schema.Should().Contain(c => c.Order == 1 && c.Name == "lastName" && c.Label == "Last Name" && c.HelpText == "Your Last Name" && c.HelpTextWarning == "Mandatory Last Name" && (string)c.Value == "Poop");
            schema.Should().Contain(c => c.Order == 0 && c.Name == "firstName" && c.Label == "First Name" && c.HelpText == "Your First Name" && c.HelpTextWarning == "Mandatory First Name" && (string)c.Value == "Bob");
        }

        [Test]
        public void schema_should_have_nested_fields()
        {
            var model = new NestedTestModel
            {
                Name =
                {
                    FirstName = "Bob",
                    LastName = "Poop"
                }
            };

            var schema = SchemaBuilder.ToSchema(model);

            schema.Should().Contain(c => c.Order == 0 && c.Name == "name.firstName" && c.Label == "First Name" && c.HelpText == "Your First Name" && c.HelpTextWarning == "Mandatory First Name" && (string)c.Value == "Bob");
            schema.Should().Contain(c => c.Order == 1 && c.Name == "name.lastName" && c.Label == "Last Name" && c.HelpText == "Your Last Name" && c.HelpTextWarning == "Mandatory Last Name" && (string)c.Value == "Poop");
            schema.Should().Contain(c => c.Order == 2 && c.Name == "quote" && c.Label == "Quote" && c.HelpText == "Your Favorite Quote");
        }
    }

    public class TestModel
    {
        [FieldDefinition(0, Label = "First Name", HelpText = "Your First Name", HelpTextWarning = "Mandatory First Name")]
        public string FirstName { get; set; }

        [FieldDefinition(1, Label = "Last Name", HelpText = "Your Last Name", HelpTextWarning = "Mandatory Last Name")]
        public string LastName { get; set; }

        public string Other { get; set; }
    }

    public class NestedTestModel
    {
        [FieldDefinition(0)]
        public TestModel Name { get; set; } = new TestModel();

        [FieldDefinition(1, Label = "Quote", HelpText = "Your Favorite Quote")]
        public string Quote { get; set; }
    }
}
