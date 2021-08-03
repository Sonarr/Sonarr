using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Test.ExtensionTests.IEnumerableExtensionTests
{
    [TestFixture]
    public class ExceptByFixture
    {
        public class Object1
        {
            public string Prop1 { get; set; }
        }

        public class Object2
        {
            public string Prop1 { get; set; }
        }

        [Test]
        public void should_return_empty_when_object_with_property_exists_in_both_lists()
        {
            var first = new List<Object1>
                        {
                            new Object1 { Prop1 = "one" },
                            new Object1 { Prop1 = "two" }
                        };

            var second = new List<Object1>
                        {
                            new Object1 { Prop1 = "two" },
                            new Object1 { Prop1 = "one" }
                        };

            first.ExceptBy(o => o.Prop1, second, o => o.Prop1, StringComparer.InvariantCultureIgnoreCase).Should().BeEmpty();
        }

        [Test]
        public void should_return_objects_that_do_not_have_a_match_in_the_second_list()
        {
            var first = new List<Object1>
                        {
                            new Object1 { Prop1 = "one" },
                            new Object1 { Prop1 = "two" }
                        };

            var second = new List<Object1>
                        {
                            new Object1 { Prop1 = "one" },
                            new Object1 { Prop1 = "four" }
                        };

            var result = first.ExceptBy(o => o.Prop1, second, o => o.Prop1, StringComparer.InvariantCultureIgnoreCase).ToList();

            result.Should().HaveCount(1);
            result.First().GetType().Should().Be(typeof(Object1));
            result.First().Prop1.Should().Be("two");
        }
    }
}
