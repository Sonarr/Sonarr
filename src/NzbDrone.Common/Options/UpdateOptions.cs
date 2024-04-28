namespace NzbDrone.Common.Options;

public class UpdateOptions
{
    public string Mechanism { get; set; }
    public bool? Automatically { get; set; }
    public string ScriptPath { get; set; }
    public string Branch { get; set; }
}
