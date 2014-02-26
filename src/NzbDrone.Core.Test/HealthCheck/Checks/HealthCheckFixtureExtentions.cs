using FluentAssertions;
using NzbDrone.Core.HealthCheck;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    public static class HealthCheckFixtureExtensions
    {
        public static void ShouldBeWarning(this Core.HealthCheck.HealthCheck result)
        {
            result.Type.Should().Be(HealthCheckResultType.Warning);
        }

        public static void ShouldBeError(this Core.HealthCheck.HealthCheck result)
        {
            result.Type.Should().Be(HealthCheckResultType.Error);
        }
    }
}
