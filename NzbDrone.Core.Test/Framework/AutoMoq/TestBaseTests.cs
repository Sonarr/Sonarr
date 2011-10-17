using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NLog;
using NUnit.Framework;

namespace NzbDrone.Core.Test.Framework.AutoMoq
{
    [TestFixture]
    class TestBaseTests : TestBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Test]
        public void Test_should_pass_when_no_exceptions_are_logged()
        {
            Logger.Info("Everything is fine and dandy!");
        }

        [Test]
        public void Test_should_pass_when_errors_are_excpected()
        {
            Logger.Error("I knew this would happer");
            ExceptionVerification.ExcpectedErrors(1);
        }

        [Test]
        public void Test_should_pass_when_warns_are_excpected()
        {
            Logger.Warn("I knew this would happer");
            ExceptionVerification.ExcpectedWarns(1);
        }

        [Test]
        public void Test_should_pass_when_warns_are_ignored()
        {
            Logger.Warn("I knew this would happer");
            Logger.Warn("I knew this would happer");
            Logger.Warn("I knew this would happer");
            ExceptionVerification.IgnoreWarns();
        }

        [Test]
        public void Test_should_pass_when_errors_are_ignored()
        {
            Logger.Error("I knew this would happer");
            Logger.Error("I knew this would happer");
            Logger.Error("I knew this would happer");
            ExceptionVerification.IgnoreErrors();
        }

        [Test]
        public void Test_should_pass_when_exception_type_is_ignored()
        {
            Logger.ErrorException("bad exception", new WebException("Test"));
            ExceptionVerification.MarkInconclusive(typeof(WebException));
        }
    }
}
