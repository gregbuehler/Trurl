using System;
using System.Collections.Generic;

namespace Trurl.Plugins.Ping
{
    public class PingCommand : ICommand
    {   
        public string Verb => "ping";

        public List<string> Usage => new List<string>()
        {
            "`ping` - takes no arguments, just pongs!"
        };

        public ICommandResult Execute() => new CommandResult
        {
            Status = Status.Ok,
            Message = $"PONG! `{ Environment.MachineName }` handling `{ Configuration.Configuration.GetConfigurationValue("TRURL_SYSTEMS") }` reporting in!"
        };

        public ICommandResult Execute(string args) => Execute();

        public ICommandResult Execute(IEnumerable<string> args) => Execute();
    }
}
