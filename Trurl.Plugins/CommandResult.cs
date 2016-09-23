namespace Trurl.Plugins
{
    public class CommandResult : ICommandResult
    {
        public Status Status { get; set; }
        public string Message { get; set; }
    }
}
