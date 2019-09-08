using System;
using System.Collections.Generic;
using Unity.Builder;
using Unity.Extension;

namespace NzbDrone.Test.Common.AutoMoq.Unity
{
    public class AutoMockingContainerExtension : UnityContainerExtension
    {
        private readonly IList<Type> registeredTypes = new List<Type>();

        protected override void Initialize()
        {
            SetEventsOnContainerToTrackAllRegisteredTypes();
            SetBuildingStrategyForBuildingUnregisteredTypes();
        }

        #region private methods

        private void SetEventsOnContainerToTrackAllRegisteredTypes()
        {
            Context.Registering += ((sender, e) => RegisterType(e.TypeFrom));
            Context.RegisteringInstance += ((sender, e) => RegisterType(e.RegisteredType));
        }

        private void RegisterType(Type typeToRegister)
        {
            registeredTypes.Add(typeToRegister);
        }

        private void SetBuildingStrategyForBuildingUnregisteredTypes()
        {
            var strategy = new AutoMockingBuilderStrategy(registeredTypes, Container);
            Context.Strategies.Add(strategy, UnityBuildStage.PreCreation);
        }

        #endregion
    }
}
