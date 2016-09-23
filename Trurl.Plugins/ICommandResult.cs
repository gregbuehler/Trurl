namespace Trurl.Plugins
{
    public enum Status
    {
        Ok,
        Warning,
        Error
    }
    public interface ICommandResult
    {
        Status Status { get; }
        string Message { get; }
    }
}
