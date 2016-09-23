using System;
using System.Collections.Generic;
using System.Linq;
using Octopus.Client;
using Octopus.Client.Model;

namespace Trurl.Plugins.OctopusDeploy
{
    public class OctopusDeployCommand : ICommand
    {
        private readonly OctopusRepository _octopus;
        private readonly string _server;
        private readonly string _apikey;

        public OctopusDeployCommand()
        {
            _server = Configuration.Configuration.GetConfigurationValue("OCTOPUS_SERVER");
            _apikey = Configuration.Configuration.GetConfigurationValue("OCTOPUS_APIKEY");

            _octopus = new OctopusRepository(new OctopusServerEndpoint(
                _server,
                _apikey
            ));
        }

        private ICommandResult CreateRelease(string project, string version)
        {
            try
            {
                var p = _octopus.Projects.FindByName(project);
                //var c = _octopus.Client.Get<ChannelResource>("/api/projects/" + p.Id + "/channels");
                var c = new ChannelResource();
                var d = _octopus.DeploymentProcesses.Get(p.DeploymentProcessId);
                var t = _octopus.DeploymentProcesses.GetTemplate(d, c);

                var r = new ReleaseResource()
                {
                    Version = version,
                    ProjectId = p.Id
                };

                foreach (var packages in t.Packages)
                {
                    r.SelectedPackages.Add(new SelectedPackage()
                    {
                        Version = version,
                        StepName = packages.StepName
                    });
                }

                var result = _octopus.Releases.Create(r, true);

                return new CommandResult()
                {
                    Status = Status.Ok,
                    Message = $"{ _server }{result.Link("Web")}\n{project}@{version} created!"
                };
            }
            catch (Exception e)
            {
                return new CommandResult()
                {
                    Status = Status.Error,
                    Message = $"Couldn't create a release. {e.Message}"
                };
            }
        }

        private ICommandResult DeployRelease(string project, string version, string environment)
        {
            try
            {
                var p = _octopus.Projects.FindByName(project);
                var r = _octopus.Releases.FindOne(x => x.ProjectId == p.Id && x.Version == version);
                var e = _octopus.Environments.FindByName(environment);
                var d = new DeploymentResource()
                {
                    ProjectId = p.Id,
                    ReleaseId = r.Id,
                    EnvironmentId = e.Id
                };

                var result = _octopus.Deployments.Create(d);

                return new CommandResult()
                {
                    Status = Status.Ok,
                    Message = $"{ _server }{ result.Link("Web") }\nCreated deployment of {p.Name}@{r.Version} to {e.Name}."
                };
            }
            catch (Exception e)
            {
                return new CommandResult()
                {
                    Status = Status.Error,
                    Message = $"Couldn't deploy {project}@{version} to {environment}. {e.Message}"
                };
            }
        }

        public List<string> Usage => new List<string>()
        {
            "`octo create [version] [project]` - creates a release",
            "`octo deploy [version] [project] [environment]` - deploys specific version to an environment"
        };

        public string Verb => "octo";

        public ICommandResult Execute()
        {
            return new CommandResult()
            {
                Status = Status.Warning,
                Message = "`octo` needs more information"
            };
        }

        public ICommandResult Execute(IEnumerable<string> args)
        {
            try
            {
                var parts = args.ToArray();
                if (parts.Any())
                {
                    switch (parts[0])
                    {
                        case "create":
                            
                            return CreateRelease(parts[2], parts[1]);
                        case "deploy":
                            return DeployRelease(parts[2], parts[1], parts[3]);
                    }
                }

                return new CommandResult()
                {
                    Status = Status.Warning,
                    Message = "`octo` needs more information"
                };
            }
            catch (Exception e)
            {
                return new CommandResult()
                {
                    Status = Status.Error,
                    Message = $"{e.Message}"
                };
            }
        }

        public ICommandResult Execute(string args) => Execute(args.Split());
    }
}
