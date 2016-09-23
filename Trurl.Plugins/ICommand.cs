using System.Collections.Generic;

namespace Trurl.Plugins
{
    public interface ICommand
    {
        string Verb { get; }
        List<string> Usage { get; }
        ICommandResult Execute();
        ICommandResult Execute(string args);
        ICommandResult Execute(IEnumerable<string> args);
    }
}
