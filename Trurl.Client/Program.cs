using System.Collections.Generic;
using StructureMap;
using System.Linq;
using StructureMap.Query;
using Topshelf;
using Trurl.Plugins;

namespace Trurl.Client
{
    class Program
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger("Trurl");

        static void Main(string[] args)
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scan =>
                {
                    scan.AssembliesFromApplicationBaseDirectory();
                    scan.AddAllTypesOf<ICommand>();
                });
            });

            var instances = container.Model.InstancesOf<ICommand>();
            var instanceRefs = instances as IList<InstanceRef> ?? instances.ToList();
            foreach (var instance in instanceRefs)
            {
                Log.Debug($"Using { instance.Name }, { instance.Description }");
            }
            

            HostFactory.Run(x =>
            {
                x.Service<TrurlClient>(s =>
                {
                    s.ConstructUsing( name => new TrurlClient(container) );
                    s.WhenStarted(ts => ts.Start());
                    s.WhenStopped(ts => ts.Stop());
                });

                x.UseLog4Net();

                x.RunAsPrompt();

                //x.RunAsLocalService();
                //x.SetDescription("Trurl Client Service");
                //x.SetDisplayName("Trurl Client");
                //x.SetServiceName("TrurlClient");
            });
        }
    }
}
