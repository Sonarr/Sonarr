﻿using System.Collections.Generic;
using FluentValidation.Results;
using System;

namespace NzbDrone.Core.ThingiProvider
{
    public interface IProviderFactory<TProvider, TProviderDefinition>
        where TProviderDefinition : ProviderDefinition, new()
        where TProvider : IProvider
    {
        List<TProviderDefinition> All();
        List<TProvider> GetAvailableProviders();
        TProviderDefinition Get(int id);
        TProviderDefinition Create(TProviderDefinition definition);
        void Update(TProviderDefinition definition);
        void Delete(int id);
        IEnumerable<TProviderDefinition> GetDefaultDefinitions();
        IEnumerable<TProviderDefinition> GetPresetDefinitions(TProviderDefinition providerDefinition);
        TProviderDefinition GetProviderCharacteristics(TProvider provider, TProviderDefinition definition);
        TProvider GetInstance(TProviderDefinition definition);
        ValidationResult Test(TProviderDefinition definition);
        object ConnectData(TProviderDefinition definition, string stage);
    }
}