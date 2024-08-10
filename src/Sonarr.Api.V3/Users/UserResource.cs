using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Authentication;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Users
{
    public class UserResource : RestResource
    {
        public Guid Identifier { get; set; }
        public string Username { get; set; }

        public string Password { get; set; }
    }

    public static class UserResourceMapper
    {
        public static UserResource ToResource(this User model)
        {
            if (model == null)
            {
                return null;
            }

            return new UserResource
            {
                Id = model.Id,
                Identifier = model.Identifier,
                Username = model.Username
            };
        }

        public static User ToModel(this UserResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new User
            {
                Id = resource.Id,
                Identifier = resource.Identifier
            };
        }

        public static string GetUserName(this UserResource resource)
        {
            return resource.Username;
        }

        public static string GetPassword(this UserResource resource)
        {
            return resource.Password;
        }

        public static List<UserResource> ToResource(this IEnumerable<User> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
