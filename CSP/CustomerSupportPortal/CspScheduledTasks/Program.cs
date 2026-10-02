using System;
using System.Linq;
using System.Net.Mail;
using Atlas;
using Autofac;
using Moq;
using System.Configuration;
using CspScheduledTasks.HelperClasses;
using System.IO;

namespace CspScheduledTasks {
	class Program {

		static void Main(string[] args)
		{
          
            try
            {        

                var configuration = Host.UseAppConfig<CspSchedulerService>()
                                        .AllowMultipleInstances()
                                        .WithRegistrations(p => p.RegisterModule(new CspSchedulerAutofacModule()));
                if (args != null && args.Any())
                    configuration = configuration.WithArguments(args);

                Host.Start(configuration);                             
            }
            catch (Exception ex)
            {
                Directory.CreateDirectory(@"C:\logs\");
                var logFile = @"C:\logs\" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".txt";
                File.AppendAllText(logFile, "Error during CSPScheduledTasks startup"+ "Error: " + ex.Message + " Stack: " + ex.StackTrace);              
                Console.ReadLine();
            }
		}
	}
}
