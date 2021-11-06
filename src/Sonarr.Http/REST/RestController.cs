using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using NzbDrone.Core.Datastore;
using Sonarr.Http.REST.Attributes;
using Sonarr.Http.Validation;

namespace Sonarr.Http.REST
{
    public abstract class RestController<TResource> : Controller
        where TResource : RestResource, new()
    {
        private static readonly List<Type> VALIDATE_ID_ATTRIBUTES = new List<Type> { typeof(RestPutByIdAttribute), typeof(RestDeleteByIdAttribute) };

        protected ResourceValidator<TResource> PostValidator { get; private set; }
        protected ResourceValidator<TResource> PutValidator { get; private set; }
        protected ResourceValidator<TResource> SharedValidator { get; private set; }

        protected void ValidateId(int id)
        {
            if (id <= 0)
            {
                throw new BadRequestException(id + " is not a valid ID");
            }
        }

        protected RestController()
        {
            PostValidator = new ResourceValidator<TResource>();
            PutValidator = new ResourceValidator<TResource>();
            SharedValidator = new ResourceValidator<TResource>();

            PutValidator.RuleFor(r => r.Id).ValidId();
        }

        [RestGetById]
        public ActionResult<TResource> GetResourceByIdWithErrorHandler(int id)
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

        protected abstract TResource GetResourceById(int id);

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;

            var skipAttribute = (SkipValidationAttribute)Attribute.GetCustomAttribute(descriptor.MethodInfo, typeof(SkipValidationAttribute), true);
            var skipValidate = skipAttribute?.Skip ?? false;
            var skipShared = skipAttribute?.SkipShared ?? false;

            if (Request.Method == "POST" || Request.Method == "PUT")
            {
                var resourceArgs = context.ActionArguments.Values.Where(x => x.GetType() == typeof(TResource))
                    .Select(x => x as TResource)
                    .ToList();

                foreach (var resource in resourceArgs)
                {
                    ValidateResource(resource, skipValidate, skipShared);
                }
            }

            var attributes = descriptor.MethodInfo.CustomAttributes;
            if (attributes.Any(x => VALIDATE_ID_ATTRIBUTES.Contains(x.GetType())) && !skipValidate)
            {
                if (context.ActionArguments.TryGetValue("id", out var idObj))
                {
                    ValidateId((int)idObj);
                }
            }

            base.OnActionExecuting(context);
        }

        protected void ValidateResource(TResource resource, bool skipValidate = false, bool skipSharedValidate = false)
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
