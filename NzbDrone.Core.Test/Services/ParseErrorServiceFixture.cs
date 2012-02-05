using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Services
{
    [TestFixture]
    public class ParseErrorServiceFixture : CoreTest
    {

        public ParseErrorServiceFixture()
        {
            AppDomain.CurrentDomain.AssemblyResolve +=
                new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }


        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);
            if (name.Name == "Newtonsoft.Json")
            {
                return typeof(Newtonsoft.Json.JsonSerializer).Assembly;
            }
            return null;
        }
    }
}
