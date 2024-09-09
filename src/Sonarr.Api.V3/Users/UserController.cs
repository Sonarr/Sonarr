using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.SignalR;
using Sonarr.Http;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V3.Users
{
    [V3ApiController]
    [Authorize(Roles = "Admin")]
    public class UserController : RestControllerWithSignalR<UserResource, User>, IHandle<UsersUpdatedEvent>
    {
        private readonly IUserService _userService;

        public UserController(IBroadcastSignalRMessage signalRBroadcaster,
        IUserService userService)
            : base(signalRBroadcaster)
        {
            _userService = userService;

            SharedValidator.RuleFor(u => u.Username).NotEmpty();
            SharedValidator.RuleFor(c => c.Username)
            .Must((v, c) => !_userService.All().Any(f => f.Username == c && f.Id != v.Id))
            .WithMessage("Username already exists.");

            PostValidator.RuleFor(c => c.Password)
            .NotNull().WithMessage("Password cannot be null");

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
        public ActionResult<UserResource> Create([FromBody] UserResource resource)
        {
            return Created(_userService.Add(resource.GetUserName(), resource.GetPassword(), resource.getRole()).Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        public ActionResult<UserResource> Update([FromBody] UserResource resource)
        {
            var user = _userService.UpdateByModel(resource.ToModel());
            if (resource.ResetApiKey)
            {
                _userService.ResetApiKey(user);
            }

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
