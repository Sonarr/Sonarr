using System;
using System.Linq;
using System.Reflection;
using AutoMoq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using PetaPoco;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class ConfigProviderTest : TestBase
    {
        [Test]
        public void Add_new_value_to_database()
        {
            const string key = "MY_KEY";
            const string value = "MY_VALUE";

            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            //Act
            mocker.Resolve<ConfigProvider>().SetValue(key, value);

            //Assert
            mocker.Resolve<ConfigProvider>().GetValue(key, "").Should().Be(value);
        }

        [Test]
        public void Get_value_from_database()
        {
            const string key = "MY_KEY";
            const string value = "MY_VALUE";

            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            db.Insert(new Config { Key = key, Value = value });
            db.Insert(new Config { Key = "Other Key", Value = "OtherValue" });

            //Act
            var result = mocker.Resolve<ConfigProvider>().GetValue(key, "");

            //Assert
            result.Should().Be(value);
        }


        [Test]
        public void Get_value_should_return_default_when_no_value()
        {
            const string key = "MY_KEY";
            const string value = "MY_VALUE";

            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);


            //Act
            var result = mocker.Resolve<ConfigProvider>().GetValue(key, value);

            //Assert
            result.Should().Be(value);
        }

        [Test]
        public void New_value_should_update_old_value_new_value()
        {
            const string key = "MY_KEY";
            const string originalValue = "OLD_VALUE";
            const string newValue = "NEW_VALUE";

            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            db.Insert(new Config { Key = key, Value = originalValue });

            //Act
            mocker.Resolve<ConfigProvider>().SetValue(key, newValue);
            var result = mocker.Resolve<ConfigProvider>().GetValue(key, "");

            //Assert
            result.Should().Be(newValue);
            db.Fetch<Config>().Should().HaveCount(1);
        }

        [Test]
        public void New_value_should_update_old_value_same_value()
        {
            const string key = "MY_KEY";
            const string value = "OLD_VALUE";


            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            //Act
            mocker.Resolve<ConfigProvider>().SetValue(key, value);
            mocker.Resolve<ConfigProvider>().SetValue(key, value);
            var result = mocker.Resolve<ConfigProvider>().GetValue(key, "");

            //Assert
            result.Should().Be(value);
            db.Fetch<Config>().Should().HaveCount(1);
        }

        [Test]
        [Description("This test will use reflection to ensure each config property read/writes to a unique key")]
        public void config_properties_should_write_and_read_using_same_key()
        {

            var mocker = new AutoMoqer(MockBehavior.Strict);
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var configProvider = mocker.Resolve<ConfigProvider>();
            var allProperties = typeof(ConfigProvider).GetProperties();


            //Act
            foreach (var propertyInfo in allProperties)
            {
                object value = null;

                if (propertyInfo.PropertyType == typeof(string))
                {
                    value = new Guid().ToString();
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

            db.Fetch<Config>().Should()
                .HaveSameCount(allProperties, "two different properties are writing to the same key in db. Copy/Past fail.");
        }
    }
}