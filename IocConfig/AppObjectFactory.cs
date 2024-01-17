using StructureMap;
using System;
using System.Threading;

namespace TES.IocConfig
{
    public static class AppObjectFactory
    {
        private static readonly Lazy<Container> ContainerBuilder = new Lazy<Container>(DefaultContainer, LazyThreadSafetyMode.ExecutionAndPublication);

        public static IContainer Container => ContainerBuilder.Value;

        private static Container DefaultContainer() => new Container();
    }
}