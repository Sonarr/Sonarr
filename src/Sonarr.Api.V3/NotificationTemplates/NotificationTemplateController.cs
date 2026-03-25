using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Notifications.NotificationTemplates;
using NzbDrone.Core.Validation;
using Sonarr.Http;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V3.NotificationTemplates
{
    [V3ApiController]
    public class NotificationTemplateController : RestController<NotificationTemplateResource>
    {
        private readonly INotificationTemplateService _templateService;

        public NotificationTemplateController(INotificationTemplateService templateService)
        {
            _templateService = templateService;

            SharedValidator.RuleFor(c => c.Name).NotEmpty();
            SharedValidator.RuleFor(c => c.Name)
                .Must((v, c) => !_templateService.All().Any(f => f.Name == c && f.Id != v.Id)).WithMessage("Must be unique.");
        }

        protected override NotificationTemplateResource GetResourceById(int id)
        {
            return _templateService.GetById(id).ToResource();
        }

        [HttpGet]
        [Produces("application/json")]
        public List<NotificationTemplateResource> GetAll()
        {
            return _templateService.All().ToResource();
        }

        [RestPostById]
        [Consumes("application/json")]
        public ActionResult<NotificationTemplateResource> Create([FromBody] NotificationTemplateResource notificationTemplateResource)
        {
            var model = notificationTemplateResource.ToModel();

            Validate(model);

            return Created(_templateService.Insert(model).Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        public ActionResult<NotificationTemplateResource> Update([FromBody] NotificationTemplateResource resource)
        {
            var model = resource.ToModel();

            Validate(model);

            _templateService.Update(model);

            return Accepted(model.Id);
        }

        [RestDeleteById]
        public void DeleteFormat(int id)
        {
            _templateService.Delete(id);
        }

        private void Validate(NotificationTemplate notificationTemplate)
        {
            // TODO
        }

        private void VerifyValidationResult(ValidationResult validationResult)
        {
            var result = new NzbDroneValidationResult(validationResult.Errors);

            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }
    }
}
