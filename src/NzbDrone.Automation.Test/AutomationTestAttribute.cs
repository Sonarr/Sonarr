using NUnit.Framework;

namespace NzbDrone.Automation.Test
{
    public class AutomationTestAttribute : CategoryAttribute
    {
        public AutomationTestAttribute()
            : base("AutomationTest")
        {
        }
    }
}
