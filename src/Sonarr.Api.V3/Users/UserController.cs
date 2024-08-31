using System.Collections.Generic;
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

        public UserController(IBroadcastSignalRMessage signalRBroadcaster, IUserService userService)
            : base(signalRBroadcaster)
        {
            _userService = userService;
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
    }
}
