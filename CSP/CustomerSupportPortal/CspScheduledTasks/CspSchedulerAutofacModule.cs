using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Atlas;
using Autofac;
using Quartz;
using Quartz.Impl;

namespace CspScheduledTasks
{
    public class CspSchedulerAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            LoadQuartz(builder);
            LoadServices(builder);
        }

        private static void LoadQuartz(ContainerBuilder builder)
        {
            builder.Register(c => new StdSchedulerFactory()
                .GetScheduler())
                .As<IScheduler>()
                .InstancePerLifetimeScope();

            builder.Register(c => new CspSchedulerJobListener(ContainerProvider.Instance))
                .As<IJobListener>();
        }

        private static void LoadServices(ContainerBuilder builder)
        {
            builder.RegisterType<CspSchedulerService>()
                .As<IAmAHostedProcess>()
                .PropertiesAutowired();
        }
    }
}
