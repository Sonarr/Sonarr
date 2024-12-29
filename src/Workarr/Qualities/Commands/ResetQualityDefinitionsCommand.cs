using Workarr.Messaging.Commands;

namespace Workarr.Qualities.Commands
{
    public class ResetQualityDefinitionsCommand : Command
    {
        public bool ResetTitles { get; set; }

        public ResetQualityDefinitionsCommand(bool resetTitles = false)
        {
            ResetTitles = resetTitles;
        }
    }
}
