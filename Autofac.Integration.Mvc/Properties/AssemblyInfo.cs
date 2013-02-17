using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Web;
using Autofac.Integration.Mvc;

[assembly: AssemblyTitle("Autofac.Integration.Mvc")]
[assembly: AssemblyDescription("Autofac ASP.NET MVC 4 Integration")]
[assembly: InternalsVisibleTo("Autofac.Tests.Integration.Mvc, PublicKey=00240000048000009400000006020000002400005253413100040000010001008728425885ef385e049261b18878327dfaaf0d666dea3bd2b0e4f18b33929ad4e5fbc9087e7eda3c1291d2de579206d9b4292456abffbe8be6c7060b36da0c33b883e3878eaf7c89fddf29e6e27d24588e81e86f3a22dd7b1a296b5f06fbfb500bbd7410faa7213ef4e2ce7622aefc03169b0324bcd30ccfe9ac8204e4960be6")]
[assembly: PreApplicationStartMethod(typeof(PreApplicationStartCode), "Start")]
[assembly: ComVisible(false)]
