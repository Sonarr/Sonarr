using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Validation;
using Sonarr.Http;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V3.CustomFormats
{
    [V3ApiController]
    public class CustomFormatController : RestController<CustomFormatResource>
    {
        private readonly ICustomFormatService _formatService;
        private readonly List<ICustomFormatSpecification> _specifications;

        public CustomFormatController(ICustomFormatService formatService,
                                  List<ICustomFormatSpecification> specifications)
        {
            _formatService = formatService;
            _specifications = specifications;

            SharedValidator.RuleFor(c => c.Name).NotEmpty();
            SharedValidator.RuleFor(c => c.Name)
                .Must((v, c) => !_formatService.All().Any(f => f.Name == c && f.Id != v.Id)).WithMessage("Must be unique.");
            SharedValidator.RuleFor(c => c.Specifications).NotEmpty();
            SharedValidator.RuleFor(c => c).Custom((customFormat, context) =>
            {
                if (!customFormat.Specifications.Any())
                {
                    context.AddFailure("Must contain at least one Condition");
                }

                if (customFormat.Specifications.Any(s => s.Name.IsNullOrWhiteSpace()))
                {
                    context.AddFailure("Condition name(s) cannot be empty or consist of only spaces");
                }
            });
        }

        protected override CustomFormatResource GetResourceById(int id)
        {
            return _formatService.GetById(id).ToResource(true);
        }

        [RestPostById]
        [Consumes("application/json")]
        public ActionResult<CustomFormatResource> Create([FromBody] CustomFormatResource customFormatResource)
        {
            var model = customFormatResource.ToModel(_specifications);

            Validate(model);

            return Created(_formatService.Insert(model).Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        public ActionResult<CustomFormatResource> Update([FromBody] CustomFormatResource resource)
        {
            var model = resource.ToModel(_specifications);

            Validate(model);

            _formatService.Update(model);

            return Accepted(model.Id);
        }

        [HttpGet]
        [Produces("application/json")]
        public List<CustomFormatResource> GetAll()
        {
            return _formatService.All().ToResource(true);
        }

        [RestDeleteById]
        public void DeleteFormat(int id)
        {
            _formatService.Delete(id);
        }

        [HttpGet("schema")]
        public object GetTemplates()
        {
            var schema = _specifications.OrderBy(x => x.Order).Select(x => x.ToSchema()).ToList();

            var presets = GetPresets();

            foreach (var item in schema)
            {
                item.Presets = presets.Where(x => x.GetType().Name == item.Implementation).Select(x => x.ToSchema()).ToList();
            }

            return schema;
        }

        private void Validate(CustomFormat definition)
        {
            foreach (var validationResult in definition.Specifications.Select(spec => spec.Validate()))
            {
                VerifyValidationResult(validationResult);
            }
        }

        private void VerifyValidationResult(ValidationResult validationResult)
        {
            var result = new NzbDroneValidationResult(validationResult.Errors);

            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }

        private IEnumerable<ICustomFormatSpecification> GetPresets()
        {
            yield return new ReleaseTitleSpecification
            {
                Name = "x264",
                Value = @"(x|h)\.?264"
            };

            yield return new ReleaseTitleSpecification
            {
                Name = "x265",
                Value = @"(((x|h)\.?265)|(HEVC))"
            };

            yield return new ReleaseTitleSpecification
            {
                Name = "Simple Hardcoded Subs",
                Value = @"subs?"
            };

            yield return new ReleaseTitleSpecification
            {
                Name = "Hardcoded Subs",
                Value = @"\b(?<hcsub>(\w+SUBS?)\b)|(?<hc>(HC|SUBBED))\b"
            };

            yield return new ReleaseTitleSpecification
            {
                Name = "Surround Sound",
                Value = @"DTS.?(HD|ES|X(?!\D))|TRUEHD|ATMOS|DD(\+|P).?([5-9])|EAC3.?([5-9])"
            };

            yield return new ReleaseTitleSpecification
            {
                Name = "Preferred Words",
                Value = @"\b(SPARKS|Framestor)\b"
            };

            var formats = _formatService.All();
            foreach (var format in formats)
            {
                foreach (var condition in format.Specifications)
                {
                    var preset = condition.Clone();
                    preset.Name = $"{format.Name}: {preset.Name}";
                    yield return preset;
                }
            }
        }
    }
}
