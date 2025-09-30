using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using NLog;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Datastore;
using Sonarr.Http.REST.Attributes;
using Sonarr.Http.Validation;

namespace Sonarr.Http.REST
{
    public abstract class RestController<TResource> : Controller
        where TResource : RestResource, new()
    {
        private static readonly List<Type> VALIDATE_ID_ATTRIBUTES = new List<Type> { typeof(RestPutByIdAttribute), typeof(RestDeleteByIdAttribute) };
        private static readonly Type DEPRECATED_ATTRIBUTE = typeof(ObsoleteAttribute);

        private readonly Logger _logger;

        protected ResourceValidator<TResource> PostValidator { get; private set; }
        protected ResourceValidator<TResource> PutValidator { get; private set; }
        protected ResourceValidator<TResource> SharedValidator { get; private set; }
        private ResourceValidator<TResource> IdValidator { get; set; }

        protected void ValidateId(int id)
        {
            if (id <= 0)
            {
                throw new BadRequestException(id + " is not a valid ID");
            }
        }

        protected RestController()
        {
            _logger = NzbDroneLogger.GetLogger(this);

            PostValidator = new ResourceValidator<TResource>();
            PutValidator = new ResourceValidator<TResource>();
            SharedValidator = new ResourceValidator<TResource>();
            IdValidator = new ResourceValidator<TResource>();

            IdValidator.RuleFor(r => r.Id).ValidId();
        }

        [RestGetById]
        [Produces("application/json")]
        public virtual ActionResult<TResource> GetResourceByIdWithErrorHandler(int id)
        {
            try
            {
                return GetResourceById(id);
            }
            catch (ModelNotFoundException)
            {
                return NotFound();
            }
        }

        #nullable enable
        protected virtual TResource? GetResourceById(int id)
        {
            throw new NotImplementedException();
        }
        #nullable disable

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;

            var skipAttribute = (SkipValidationAttribute)Attribute.GetCustomAttribute(descriptor.MethodInfo, typeof(SkipValidationAttribute), true);
            var skipValidate = skipAttribute?.Skip ?? false;
            var skipShared = skipAttribute?.SkipShared ?? false;

            var attributes = descriptor.MethodInfo.CustomAttributes as IReadOnlyCollection<CustomAttributeData> ??
                             descriptor.MethodInfo.CustomAttributes.ToArray();

            var validateId = attributes.Any(x => VALIDATE_ID_ATTRIBUTES.Contains(x.AttributeType));

            if (Request.Method is "POST" or "PUT")
            {
                var resourceArgs = context.ActionArguments.Values
                    .SelectMany(x => x switch
                    {
                        TResource single => new[] { single },
                        IEnumerable<TResource> multiple => multiple,
                        _ => Enumerable.Empty<TResource>()
                    });

                foreach (var resource in resourceArgs)
                {
                    // Map route Id to body resource if not set in request
                    if (Request.Method == "PUT" && resource.Id == 0 && context.RouteData.Values.TryGetValue("id", out var routeId))
                    {
                        resource.Id = Convert.ToInt32(routeId);
                    }

                    ValidateResource(resource, validateId, skipValidate, skipShared);
                }
            }

            if (validateId && !skipValidate)
            {
                if (context.ActionArguments.TryGetValue("id", out var idObj))
                {
                    ValidateId((int)idObj);
                }
            }

            var controllerAttributes = descriptor.ControllerTypeInfo.CustomAttributes;
            if (controllerAttributes.Any(x => x.AttributeType == DEPRECATED_ATTRIBUTE) || attributes.Any(x => x.AttributeType == DEPRECATED_ATTRIBUTE))
            {
                _logger.Warn("API call made to deprecated endpoint from {0}", Request.Headers.UserAgent.ToString());
                Response.Headers["Deprecation"] = "true";
            }

            base.OnActionExecuting(context);
        }

        protected void ValidateResource(TResource resource, bool validateId = false, bool skipValidate = false, bool skipSharedValidate = false)
        {
            if (resource == null)
            {
                throw new BadRequestException("Request body can't be empty");
            }

            var errors = new List<ValidationFailure>();

            if (!skipSharedValidate)
            {
                errors.AddRange(SharedValidator.Validate(resource).Errors);
            }

            if (Request.Method.Equals("POST", StringComparison.InvariantCultureIgnoreCase) && !skipValidate && !Request.Path.ToString().EndsWith("/test", StringComparison.InvariantCultureIgnoreCase))
            {
                errors.AddRange(PostValidator.Validate(resource).Errors);
            }
            else if (Request.Method.Equals("PUT", StringComparison.InvariantCultureIgnoreCase))
            {
                errors.AddRange(PutValidator.Validate(resource).Errors);

                if (validateId)
                {
                    errors.AddRange(IdValidator.Validate(resource).Errors);
                }
            }

            if (errors.Any())
            {
                throw new ValidationException(errors);
            }
        }

        protected ActionResult<TResource> Accepted(int id)
        {
            var result = GetResourceById(id);
            return AcceptedAtAction(nameof(GetResourceByIdWithErrorHandler), new { id = id }, result);
        }

        protected ActionResult<TResource> Created(int id)
        {
            var result = GetResourceById(id);
            return CreatedAtAction(nameof(GetResourceByIdWithErrorHandler), new { id = id }, result);
        }
    }
}
