using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Configuration
{
    [TestFixture]
    public class ConfigServiceFixture : DbTest<ConfigService, Config>
    {
        [SetUp]
        public void SetUp()
        {
            Mocker.Resolve<IConfigRepository, ConfigRepository>();
        }

        [Test]
        public void Add_new_value_to_database()
        {
            const string key = "MY_KEY";
            const string value = "MY_VALUE";

            Subject.SetValue(key, value);
            Subject.GetValue(key, "").Should().Be(value);
        }

        [Test]
        public void Get_value_from_database()
        {
            const string key = "MY_KEY";
            const string value = "MY_VALUE";


            Db.Insert(new Config { Key = key, Value = value });
            Db.Insert(new Config { Key = "Other Key", Value = "OtherValue" });

            var result = Subject.GetValue(key, "");

            result.Should().Be(value);
        }


        [Test]
        public void Get_value_should_return_default_when_no_value()
        {
            const string key = "MY_KEY";
            const string value = "MY_VALUE";

            var result = Subject.GetValue(key, value);

            result.Should().Be(value);
        }

        [Test]
        public void New_value_should_update_old_value_new_value()
        {
            const string key = "MY_KEY";
            const string originalValue = "OLD_VALUE";
            const string newValue = "NEW_VALUE";

            Db.Insert(new Config { Key = key, Value = originalValue });

            Subject.SetValue(key, newValue);
            var result = Subject.GetValue(key, "");

            //Assert
            result.Should().Be(newValue);
            AllStoredModels.Should().HaveCount(1);
        }

        [Test]
        public void New_value_should_update_old_value_same_value()
        {
            const string key = "MY_KEY";
            const string value = "OLD_VALUE";

            Subject.SetValue(key, value);
            Subject.SetValue(key, value);
            var result = Subject.GetValue(key, "");

            result.Should().Be(value);
            AllStoredModels.Should().HaveCount(1);
        }

        [Test]
        public void get_value_with_persist_should_store_default_value()
        {
            const string key = "MY_KEY";
            string value = Guid.NewGuid().ToString();

            Subject.GetValue(key, value, persist: true).Should().Be(value);
            Subject.GetValue(key, string.Empty).Should().Be(value);
        }

        [Test]
        public void get_value_with_out_persist_should_not_store_default_value()
        {
            const string key = "MY_KEY";
            string value1 = Guid.NewGuid().ToString();
            string value2 = Guid.NewGuid().ToString();

            Subject.GetValue(key, value1).Should().Be(value1);
            Subject.GetValue(key, value2).Should().Be(value2);
        }



        [Test]
        public void uguid_should_only_be_set_once()
        {
            var guid1 = Subject.UGuid;
            var guid2 = Subject.UGuid;

            guid1.Should().Be(guid2);
        }

        [Test]
        public void uguid_should_return_valid_result_on_first_call()
        {
            var guid = Subject.UGuid;
            guid.Should().NotBeEmpty();
        }

        [Test]
        public void updating_a_vakye_should_update_its_value()
        {
            Subject.SabHost = "Test";
            Subject.SabHost.Should().Be("Test");

            Subject.SabHost = "Test2";
            Subject.SabHost.Should().Be("Test2");
        }

        [Test]
        [Description("This test will use reflection to ensure each config property read/writes to a unique key")]
        public void config_properties_should_write_and_read_using_same_key()
        {
            var configProvider = Subject;
            var allProperties = typeof(ConfigService).GetProperties().Where(p => p.GetSetMethod() != null).ToList();


            //Act
            foreach (var propertyInfo in allProperties)
            {
                object value = null;

                if (propertyInfo.PropertyType == typeof(string))
                {
                    value = Guid.NewGuid().ToString();
                }
                else if (propertyInfo.PropertyType == typeof(int))
                {
                    value = DateTime.Now.Millisecond;
                }
                else if (propertyInfo.PropertyType == typeof(bool))
                {
                    value = true;
                }
                else if (propertyInfo.PropertyType.BaseType == typeof(Enum))
                {
                    value = 0;
                }

                propertyInfo.GetSetMethod().Invoke(configProvider, new[] { value });
                var returnValue = propertyInfo.GetGetMethod().Invoke(configProvider, null);

                if (propertyInfo.PropertyType.BaseType == typeof(Enum))
                {
                    returnValue = (int)returnValue;
                }

                returnValue.Should().Be(value, propertyInfo.Name);
            }

            AllStoredModels.Should()
                .HaveSameCount(allProperties, "two different properties are writing to the same key in db. Copy/Past fail.");
        }
    }
}