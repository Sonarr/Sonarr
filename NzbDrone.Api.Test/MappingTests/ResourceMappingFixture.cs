using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Marr.Data;
using NUnit.Framework;
using NzbDrone.Api.Config;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.History;
using NzbDrone.Api.Indexers;
using NzbDrone.Api.Logs;
using NzbDrone.Api.Mapping;
using NzbDrone.Api.Qualities;
using NzbDrone.Api.RootFolders;
using NzbDrone.Api.Series;
using NzbDrone.Api.Update;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Update;
using NzbDrone.Test.Common;
using System.Linq;

namespace NzbDrone.Api.Test.MappingTests
{
    [TestFixture]
    public class ResourceMappingFixture : TestBase
    {
        [TestCase(typeof(Core.Tv.Series), typeof(SeriesResource))]
        [TestCase(typeof(Core.Tv.Episode), typeof(EpisodeResource))]
        [TestCase(typeof(RootFolder), typeof(RootFolderResource))]
        [TestCase(typeof(NamingConfig), typeof(NamingConfigResource))]
        [TestCase(typeof(Indexer), typeof(IndexerResource))]
        [TestCase(typeof(ReportInfo), typeof(ReleaseResource))]
        [TestCase(typeof(ParsedEpisodeInfo), typeof(ReleaseResource))]
        [TestCase(typeof(DownloadDecision), typeof(ReleaseResource))]
        [TestCase(typeof(Core.History.History), typeof(HistoryResource))]
        [TestCase(typeof(UpdatePackage), typeof(UpdateResource))]
        [TestCase(typeof(QualityProfile), typeof(QualityProfileResource))]
        [TestCase(typeof(Quality), typeof(QualityResource))]
        [TestCase(typeof(Log), typeof(LogResource))]
        public void matching_fields(Type modelType, Type resourceType)
        {
            MappingValidation.ValidateMapping(modelType, resourceType);
        }


        [Test]
        public void should_map_lay_loaded_values_should_not_be_inject_if_not_loaded()
        {
            var modelWithLazy = new ModelWithLazy()
                {
                    Guid = new TestLazyLoaded<Guid>()
                };

            modelWithLazy.InjectTo<ModelWithNoLazy>().Guid.Should().BeEmpty();

            modelWithLazy.Guid.IsLoaded.Should().BeFalse();
        }


        [Test]
        public void should_map_lay_loaded_values_should_be_inject_if_loaded()
        {

            var guid = Guid.NewGuid();

            var modelWithLazy = new ModelWithLazy()
            {
                Guid = new LazyLoaded<Guid>(guid)
            };

            modelWithLazy.InjectTo<ModelWithNoLazy>().Guid.Should().Be(guid);

            modelWithLazy.Guid.IsLoaded.Should().BeTrue();
        }

        [Test]
        public void should_be_able_to_map_lists()
        {
            var modelList = Builder<TestModel>.CreateListOfSize(10).Build();

            var resourceList = modelList.InjectTo<List<TestResource>>();

            resourceList.Should().HaveSameCount(modelList);
        }

        [Test]
        public void should_map_wrapped_models()
        {
            var modelList = Builder<TestModel>.CreateListOfSize(10).Build().ToList();

            var wrapper = new TestModelWrapper
                {
                    TestlList = modelList
                };

            wrapper.InjectTo<TestResourceWrapper>();
        }


        [Test]
        public void should_map_qualityprofile()
        {

            var profileResource = new QualityProfileResource
                {
                    Allowed = Builder<QualityResource>.CreateListOfSize(1).Build().ToList(),
                };


            profileResource.InjectTo<QualityProfile>();

        }
    }

    public class ModelWithLazy
    {
        public LazyLoaded<Guid> Guid { get; set; }
    }

    public class ModelWithNoLazy
    {
        public Guid Guid { get; set; }
    }

    public class TestLazyLoaded<T> : LazyLoaded<T>
    {
        public override void Prepare(Func<IDataMapper> dataMapperFactory, object parent)
        {
            throw new InvalidOperationException();
        }
    }


    public class TestModelWrapper
    {
        public List<TestModel> TestlList { get; set; }
    }

    public class TestResourceWrapper
    {
        public List<TestResource> TestList { get; set; }
    }

    public class TestModel
    {
        public string Field1 { get; set; }
        public string Field2 { get; set; }
    }

    public class TestResource
    {
        public string Field1 { get; set; }
        public string Field2 { get; set; }
    }
}