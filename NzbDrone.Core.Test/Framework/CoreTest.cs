using System.IO;
using NUnit.Framework;
using Ninject;
using NzbDrone.Test.Common;
using PetaPoco;

namespace NzbDrone.Core.Test.Framework
{
    public class CoreTest : TestBase
    // ReSharper disable InconsistentNaming
    {
        static CoreTest()
        {
            var oldDbFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.sdf", SearchOption.AllDirectories);
            foreach (var file in oldDbFiles)
            {
                try
                {
                    File.Delete(file);
                }
                catch { }
            }

            MockLib.CreateDataBaseTemplate();
        }

        protected StandardKernel LiveKernel = null;
        protected IDatabase Db = null;


        [SetUp]
        public virtual void SetupBase()
        {
            LiveKernel = new StandardKernel();
        }

        protected override void WithStrictMocker()
        {
            base.WithStrictMocker();

            if (Db != null)
            {
                Mocker.SetConstant(Db);
            }
        }

        protected void WithRealDb()
        {
            Db = MockLib.GetEmptyDatabase();
            Mocker.SetConstant(Db);
        }
    }
}
