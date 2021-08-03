using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Profiles.Delay
{
    [TestFixture]
    public class DelayProfileServiceFixture : CoreTest<DelayProfileService>
    {
        private List<DelayProfile> _delayProfiles;
        private DelayProfile _first;
        private DelayProfile _last;

        [SetUp]
        public void Setup()
        {
            _delayProfiles = Builder<DelayProfile>.CreateListOfSize(4)
                                                  .TheFirst(1)
                                                  .With(d => d.Order = int.MaxValue)
                                                  .TheNext(1)
                                                  .With(d => d.Order = 1)
                                                  .TheNext(1)
                                                  .With(d => d.Order = 2)
                                                  .TheNext(1)
                                                  .With(d => d.Order = 3)
                                                  .Build()
                                                  .ToList();

            _first = _delayProfiles[1];
            _last = _delayProfiles.Last();

            Mocker.GetMock<IDelayProfileRepository>()
                  .Setup(s => s.All())
                  .Returns(_delayProfiles);
        }

        [Test]
        public void should_move_to_first_if_afterId_is_null()
        {
            var moving = _last;
            var result = Subject.Reorder(moving.Id, null).OrderBy(d => d.Order).ToList();
            var moved = result.First();

            moved.Id.Should().Be(moving.Id);
            moved.Order.Should().Be(1);
        }

        [Test]
        public void should_move_after_if_afterId_is_not_null()
        {
            var after = _first;
            var moving = _last;
            var result = Subject.Reorder(moving.Id, _first.Id).OrderBy(d => d.Order).ToList();
            var moved = result[1];

            moved.Id.Should().Be(moving.Id);
            moved.Order.Should().Be(after.Order + 1);
        }

        [Test]
        public void should_reorder_delay_profiles_that_are_after_moved()
        {
            var moving = _last;
            var result = Subject.Reorder(moving.Id, null).OrderBy(d => d.Order).ToList();

            for (int i = 1; i < result.Count; i++)
            {
                var delayProfile = result[i];

                if (delayProfile.Id == 1)
                {
                    delayProfile.Order.Should().Be(int.MaxValue);
                }
                else
                {
                    delayProfile.Order.Should().Be(i + 1);
                }
            }
        }

        [Test]
        public void should_not_change_afters_order_if_moving_was_after()
        {
            var after = _first;
            var afterOrder = after.Order;
            var moving = _last;
            var result = Subject.Reorder(moving.Id, _first.Id).OrderBy(d => d.Order).ToList();
            var afterMove = result.First();

            afterMove.Id.Should().Be(after.Id);
            afterMove.Order.Should().Be(afterOrder);
        }

        [Test]
        public void should_change_afters_order_if_moving_was_before()
        {
            var after = _last;
            var afterOrder = after.Order;
            var moving = _first;

            var result = Subject.Reorder(moving.Id, after.Id).OrderBy(d => d.Order).ToList();
            var afterMove = result.Single(d => d.Id == after.Id);

            afterMove.Order.Should().BeLessThan(afterOrder);
        }
    }
}
