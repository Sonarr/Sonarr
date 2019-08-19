namespace NzbDrone.Core.Diagnostics
{
    public class ScriptRequest
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string StateId { get; set; }
        public bool Debug { get; set; }
        public bool StoreState { get; set; }
    }
}