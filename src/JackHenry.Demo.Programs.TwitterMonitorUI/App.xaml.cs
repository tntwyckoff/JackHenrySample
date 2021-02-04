using Microsoft.Extensions.Configuration;
using mh = Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using JackHenry.Demo.Clients.Statistics;
using JackHenry.Demo.Clients.Statistics.Extensions;
using Prism.Unity;
using Prism.Ioc;
using JackHenry.Demo.Programs.TwitterMonitorUI.Views;

namespace JackHenry.Demo.Programs.TwitterMonitorUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<StatisticsServiceClientConfiguration>(() =>
            {
                return new StatisticsServiceClientConfiguration()
                {
                    Host = "localhost",
                    Port = 5000,
                    UseSSL= false
                };
            });
            containerRegistry.RegisterSingleton<StatisticsServiceClient>();
        }

    }
}
