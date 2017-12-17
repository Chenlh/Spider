﻿using Autofac;
using D.Spider.Core;
using D.Spider.Core.Interface;
using D.Util.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D.Spider.Example
{
    /// <summary>
    /// IStartup 样例
    /// </summary>
    class Startup : IStartup
    {
        public Startup()
        {

        }

        public void CollectConfig(IConfigCollector configCollector)
        {
            Console.WriteLine("Startup 收集配置");
        }

        public void ConfigService(ContainerBuilder builder)
        {
            Console.WriteLine("Startup 配置服务（依赖注入）");
        }

        public void ManualCollectPlugin(IPluginCollecter pluginCollecter)
        {
            Console.WriteLine("Startup 手动添加插件");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            new SpiderBuilder(args)
                .UseStartup<Startup>()
                .Build()
                .Run();
        }
    }
}
