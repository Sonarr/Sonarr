using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.SignalR;
using Sonarr.Http;
using Sonarr.Http.Authentication;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V3.Users
{
    [V3ApiController]
    [Authorize(Roles = "Admin")]
    public class UserController : RestControllerWithSignalR<UserResource, User>, IHandle<UsersUpdatedEvent>
    {
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authService;

        private readonly IConfigFileProvider _configFileProvider;

        public UserController(IBroadcastSignalRMessage signalRBroadcaster,
        IUserService userService,
        IAuthenticationService authService,
        IConfigFileProvider configFileProvider)
            : base(signalRBroadcaster)
        {
            _userService = userService;
            _authService = authService;
            _configFileProvider = configFileProvider;

            SharedValidator.RuleFor(c => c.PasswordConfirmation)
            .Must((resource, p) => IsMatchingPassword(resource)).WithMessage("Must match Password");
        }

        protected override UserResource GetResourceById(int id)
        {
            return _userService.FindUser(id).ToResource();
        }

        [HttpGet]
        [Produces("application/json")]
        public List<UserResource> GetAll()
        {
            return _userService.All().ToResource();
        }

        [HttpPost]
        [Consumes("application/json")]
        public ActionResult Create([FromBody] UserResource resource)
        {
            var hasUsers = _userService.hasUsers();
            var role = hasUsers ? resource.getRole() : UserRole.Admin;
            var user = _userService.Add(resource.GetUserName(), resource.GetPassword(), role);

            if (!hasUsers)
            {
                _authService.SignInUser(HttpContext, user, true);
            }

            // Redirect to www.google.com.au after user creation
            return Redirect("https://www.google.com.au");
        }

        [RestPutById]
        [Consumes("application/json")]
        public ActionResult<UserResource> Update([FromBody] UserResource resource)
        {
            _userService.UpdateByModel(resource.ToModel());
            return Accepted(resource.Id);
        }

        [RestDeleteById]
        public void DeleteUser(int id)
        {
            _userService.Delete(id);
        }

        [NonAction]
        public void Handle(UsersUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Sync);
        }

        private bool IsMatchingPassword(UserResource resource)
        {
            if (resource.Password == resource.PasswordConfirmation)
            {
                return true;
            }

            return false;
        }
    }
}
