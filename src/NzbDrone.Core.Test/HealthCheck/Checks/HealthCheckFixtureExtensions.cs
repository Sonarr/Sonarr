using FluentAssertions;
using Workarr.Extensions;
using Workarr.HealthCheck;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    public static class HealthCheckFixtureExtensions
    {
        public static void ShouldBeOk(this Workarr.HealthCheck.HealthCheck result)
        {
            result.Type.Should().Be(HealthCheckResult.Ok);
        }

        public static void ShouldBeNotice(this Workarr.HealthCheck.HealthCheck result, string message = null)
        {
            result.Type.Should().Be(HealthCheckResult.Notice);

            if (message.IsNotNullOrWhiteSpace())
            {
                result.Message.Should().Contain(message);
            }
        }

        public static void ShouldBeWarning(this Workarr.HealthCheck.HealthCheck result, string message = null, string wikiFragment = null)
        {
            result.Type.Should().Be(HealthCheckResult.Warning);

            if (message.IsNotNullOrWhiteSpace())
            {
                result.Message.Should().Contain(message);
            }

            if (wikiFragment.IsNotNullOrWhiteSpace())
            {
                result.WikiUrl.Fragment.Should().Be(wikiFragment);
            }
        }

        public static void ShouldBeError(this Workarr.HealthCheck.HealthCheck result, string message = null, string wikiFragment = null)
        {
            result.Type.Should().Be(HealthCheckResult.Error);

            if (message.IsNotNullOrWhiteSpace())
            {
                result.Message.Should().Contain(message);
            }

            if (wikiFragment.IsNotNullOrWhiteSpace())
            {
                result.WikiUrl.Fragment.Should().Be(wikiFragment);
            }
        }
    }
}
