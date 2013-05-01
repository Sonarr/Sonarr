using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Api.ClientSchema;
using NzbDrone.Test.Common;

namespace NzbDrone.Api.Test.ClientSchemaTests
{
    [TestFixture]
    public class SchemaBuilderFixture : TestBase
    {
        [Test]
        public void should_return_field_for_every_property()
        {
            var schema = SchemaBuilder.GenerateSchema(new TestModel());
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

            var schema = SchemaBuilder.GenerateSchema(model);

            schema.Should().Contain(c => c.Order == 1 && c.Name == "LastName" && c.Label == "Last Name" && c.HelpText == "Your Last Name" && c.Value == "Poop");
            schema.Should().Contain(c => c.Order == 0 && c.Name == "FirstName" && c.Label == "First Name" && c.HelpText == "Your First Name" && c.Value == "Bob");
        }

    }


    public class TestModel
    {
        [FieldDefinition(0, Label = "First Name", HelpText = "Your First Name")]
        public string FirstName { get; set; }

        [FieldDefinition(1, Label = "Last Name", HelpText = "Your Last Name")]
        public string LastName { get; set; }
    }
}