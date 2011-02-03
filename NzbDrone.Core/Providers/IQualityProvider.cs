using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Core.Providers
{
    public interface IQualityProvider
    {
        void AddProfile(QualityProfile profile, List<AllowedQuality> allowedQualities);
        void UpdateProfile(QualityProfile profile, List<AllowedQuality> allowedQualities);
        void RemoveProfile(int profileId);
        List<QualityProfile> GetProfiles();
    }
}
