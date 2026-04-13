using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Profiles.Releases;
using NzbDrone.Core.Tags;
using Sonarr.Http;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V5.Profiles.Release;

[V5ApiController]
public class ReleaseProfileController : RestController<ReleaseProfileResource>
{
    private readonly IReleaseProfileService _releaseProfileService;

    public ReleaseProfileController(IReleaseProfileService releaseProfileService, IIndexerFactory indexerFactory, ITagService tagService)
    {
        _releaseProfileService = releaseProfileService;

        SharedValidator.RuleFor(d => d).Custom((restriction, context) =>
        {
            if (restriction.Required.Empty() && restriction.Ignored.Empty() && !restriction.AirDateRestriction)
            {
                context.AddFailure(nameof(ReleaseProfileResource.Required), "'Must contain' or 'Must not contain' is required");
            }

            if (restriction.Required.Any(t => t.IsNullOrWhiteSpace()))
            {
                context.AddFailure(nameof(ReleaseProfileResource.Required), "'Must contain' should not contain whitespaces or an empty string");
            }

            if (restriction.Ignored.Any(t => t.IsNullOrWhiteSpace()))
            {
                context.AddFailure(nameof(ReleaseProfileResource.Ignored), "'Must not contain' should not contain whitespaces or an empty string");
            }

            if (restriction is { Enabled: true, IndexerIds.Count: > 0 })
            {
                foreach (var indexerId in restriction.IndexerIds.Where(indexerId => !indexerFactory.Exists(indexerId)))
                {
                    context.AddFailure(nameof(ReleaseProfileResource.IndexerIds), $"Indexer does not exist: {indexerId}");
                }
            }
        });

        SharedValidator.RuleFor(d => d.Tags.Intersect(d.ExcludedTags))
            .Empty()
            .WithName("ExcludedTags")
            .WithMessage(d => $"'{string.Join(", ", tagService.GetTags(d.Tags.Intersect(d.ExcludedTags)).Select(t => t.Label))}' cannot be in both 'Tags' and 'Excluded Tags'");
    }

    [RestPostById]
    public Results<Created<ReleaseProfileResource>, NotFound> Create([FromBody] ReleaseProfileResource resource)
    {
        var model = resource.ToModel();
        model = _releaseProfileService.Add(model);
        return TypedCreated(model.Id);
    }

    [RestDeleteById]
    public NoContent DeleteProfile(int id)
    {
        _releaseProfileService.Delete(id);

        return TypedResults.NoContent();
    }

    [RestPutById]
    public Results<Accepted<ReleaseProfileResource>, NotFound> Update([FromBody] ReleaseProfileResource resource)
    {
        var model = resource.ToModel();

        _releaseProfileService.Update(model);

        return TypedAccepted(model.Id);
    }

    protected override ReleaseProfileResource GetResourceById(int id)
    {
        return _releaseProfileService.Get(id).ToResource();
    }

    [HttpGet]
    public Ok<List<ReleaseProfileResource>> GetAll()
    {
        return TypedResults.Ok(_releaseProfileService.All().ToResource());
    }
}
