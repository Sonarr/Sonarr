using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Test.ExtensionTests.IEnumerableExtensionTests
{
    [TestFixture]
    public class IntersectByFixture
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
        public void should_return_empty_when_no_intersections()
        {
            var first = new List<Object1>
                        {
                            new Object1 { Prop1 = "one" },
                            new Object1 { Prop1 = "two" }
                        };

            var second = new List<Object1>
                        {
                            new Object1 { Prop1 = "three" },
                            new Object1 { Prop1 = "four" }
                        };

            first.IntersectBy(o => o.Prop1, second, o => o.Prop1, StringComparer.InvariantCultureIgnoreCase).Should().BeEmpty();
        }

        [Test]
        public void should_return_objects_with_intersecting_values()
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

            var result = first.IntersectBy(o => o.Prop1, second, o => o.Prop1, StringComparer.InvariantCultureIgnoreCase).ToList();

            result.Should().HaveCount(1);
            result.First().Prop1.Should().Be("one");
        }
    }
}
