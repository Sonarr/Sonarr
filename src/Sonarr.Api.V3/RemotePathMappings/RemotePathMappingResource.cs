using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.RemotePathMappings;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.RemotePathMappings
{
    public class RemotePathMappingResource : RestResource
    {
        public string Host { get; set; }
        public string RemotePath { get; set; }
        public string LocalPath { get; set; }
    }

    public static class RemotePathMappingResourceMapper
    {
        public static RemotePathMappingResource ToResource(this RemotePathMapping model)
        {
            if (model == null)
            {
                return null;
            }

            return new RemotePathMappingResource
            {
                Id = model.Id,

                Host = model.Host,
                RemotePath = model.RemotePath,
                LocalPath = model.LocalPath
            };
        }

        public static RemotePathMapping ToModel(this RemotePathMappingResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new RemotePathMapping
            {
                Id = resource.Id,

                Host = resource.Host,
                RemotePath = resource.RemotePath,
                LocalPath = resource.LocalPath
            };
        }

        public static List<RemotePathMappingResource> ToResource(this IEnumerable<RemotePathMapping> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
