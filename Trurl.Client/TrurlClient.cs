using SlackAPI;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Trurl.Plugins;

namespace Trurl.Client
{
    class TrurlClient
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IContainer _container;
        private Dictionary<string, ICommand> _commands;

        private Timer _timer;
        private readonly string _token;
        private readonly string _channelName;
        private readonly SlackSocketClient _client;
        

        public TrurlClient(IContainer container)
        {
            _container = container;
            Log.Debug("Initializing Trurl");
            LoadPlugins();

            _token = Configuration.Configuration.GetConfigurationValue("SLACK_TOKEN");
            _channelName = Configuration.Configuration.GetConfigurationValue("SLACK_CHANNEL");
            _client = new SlackSocketClient(_token);
            _client.Connect(connecting =>
            {
                // Client is connecting
                Log.Info($"client op: { connecting.url }!");
            },
            () =>
            {
                // Client is connected
                Log.Info("client op: connected!");
            });

            _client.OnMessageReceived += (message) =>
            {
                Log.Info($"recv: {message.text}");
                var channelId = _client.Channels.Find(c => c.name == _channelName).id;
                if (message.channel.Equals(channelId))
                {
                    try
                    {
                        var parts = message.text.Split(' ');
                        if (parts.Length < 2) return;
                        Log.Info($"Attempting to detect on '{ message.text }'");
                        switch (parts[0])
                        {
                            case "!trurl":
                                var args = new List<string>();
                                if (parts.Length >= 3)
                                {
                                    args = parts.ToList().GetRange(2, parts.Length - 2);
                                }

                                var command = parts.ElementAtOrDefault(1);
                                if (command == null)
                                {
                                    SendWarning(message.channel, "Huh?");
                                    return;
                                }
                                if (_commands.Keys.Contains(command.ToLower()))
                                {
                                    var response = _commands[command].Execute(args);
                                    switch (response.Status)
                                    {
                                        case Status.Ok:
                                            SendSuccess(message.channel, response.Message);
                                            break;
                                        case Status.Warning:
                                            SendWarning(message.channel, response.Message);
                                            break;
                                        case Status.Error:
                                            SendError(message.channel, response.Message);
                                            break;
                                        default:
                                            SendMessage(message.channel, response.Message);
                                            break;
                                    }
                                }
                                else if (command.ToLower() == "help")
                                {
                                    var response = "Sure thing! Here's what I can do:\n";
                                    foreach (var c in _commands)
                                    {
                                        response += $"{string.Join(Environment.NewLine, c.Value.Usage)} \n";
                                    }

                                    SendMessage(message.channel, response);
                                }
                                else
                                {
                                    SendError(message.channel, $"Sorry, but I don't know how to `{command}`");
                                }
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Info($"error: { e.ToString() }");
                        SendError(message.channel, e.ToString());
                    }
                }
            };
        }

        private void LoadPlugins()
        {
            _commands = new Dictionary<string, ICommand>();
            foreach (var instance in _container.GetAllInstances<ICommand>())
            {
                _commands.Add(instance.Verb, instance);
                Log.Debug($"Spawned instance of `{ instance.Verb }` handler");
            }
        }

        public void SendMessage(string channel, string message)
        {
            _client.SendMessage((m) => { }, channel, message);
        }

        public void SendMessageWithColor(string channel, string message, string color)
        {
            _client.PostMessage((m) => { }, channel, "", attachments: new[]
            {
                new Attachment(){
                    text = message,
                    color = color
                }
            }, as_user: true);
        }

        public void SendSuccess(string channel, string message)
        {
            SendMessageWithColor(channel, message, "good");
        }

        public void SendWarning(string channel, string message)
        {
            SendMessageWithColor(channel, message, "warning");
        }

        public void SendError(string channel, string message)
        {
            SendMessageWithColor(channel, message, "danger");
        }

        public void Start()
        {
            _timer = new Timer(1000) { AutoReset = true };
            _timer.Elapsed += (sender, eventArgs) => Tick();
            _timer.Start();           
        }
        public void Stop()
        {
            _timer.Stop();
            _timer.Dispose();
        }

        private void Tick()
        {
            // No-op
        }
    }
}
