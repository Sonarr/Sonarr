using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Test.Common
{
    public static class MockerExtensions
    {
        public static TInterface Resolve<TInterface, TService>(this AutoMoqer mocker)
                where TService : TInterface
        {
            var service = mocker.Resolve<TService>();
            mocker.SetConstant<TInterface>(service);
            return service;
        }
    }
}
