using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Atlas;
using Quartz;

namespace CspScheduledTasks
{
    /// <summary>
    /// Represents an entity for event handling while a job is running
    /// </summary>
    public class CspSchedulerJobListener : IJobListener
    {
        private readonly IContainerProvider _provider;
        private IUnitOfWorkContainer _container;

        /// <summary>
        /// Initializes a new instance of CspSchedulerJobListener class
        /// </summary>
        /// <param name="provider">Container provider instance</param>
        public CspSchedulerJobListener(IContainerProvider provider)
        {
            _provider = provider;
        }

        public void JobToBeExecuted(IJobExecutionContext context)
        {
            _container = _provider.CreateUnitOfWork();
            _container.InjectUnsetProperties(context.JobInstance);
        }

        public void JobExecutionVetoed(IJobExecutionContext context)
        {
            
        }

        public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            _container.Dispose();
        }

        public string Name
        {
            get { return "CspSchedulerJobListener"; }
        }
    }
}
