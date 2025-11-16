using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Profiles.Qualities;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.Release;

public abstract class ReleaseControllerBase : RestController<ReleaseResource>
{
    private readonly QualityProfile _qualityProfile;

    public ReleaseControllerBase(IQualityProfileService qualityProfileService)
    {
        _qualityProfile = qualityProfileService.GetDefaultProfile(string.Empty);
    }

    [NonAction]
    public override ActionResult<ReleaseResource> GetResourceByIdWithErrorHandler(int id)
    {
        return base.GetResourceByIdWithErrorHandler(id);
    }

    protected override ReleaseResource GetResourceById(int id)
    {
        throw new NotImplementedException();
    }

    protected virtual List<ReleaseResource> MapDecisions(IEnumerable<DownloadDecision> decisions)
    {
        var result = new List<ReleaseResource>();

        foreach (var downloadDecision in decisions)
        {
            var release = MapDecision(downloadDecision, result.Count);

            result.Add(release);
        }

        return result;
    }

    protected virtual ReleaseResource MapDecision(DownloadDecision decision, int initialWeight)
    {
        var release = decision.ToResource();

        release.ReleaseWeight = initialWeight;

        if (release.ParsedInfo?.Quality == null)
        {
            release.QualityWeight = 0;
        }
        else
        {
            release.QualityWeight = _qualityProfile.GetIndex(release.ParsedInfo.Quality.Quality).Index * 100;
            release.QualityWeight += release.ParsedInfo.Quality.Revision.Real * 10;
            release.QualityWeight += release.ParsedInfo.Quality.Revision.Version;
        }

        return release;
    }
}
