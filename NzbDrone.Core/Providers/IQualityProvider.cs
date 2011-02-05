using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Core.Providers
{
    public interface IQualityProvider
    {
        void Add(QualityProfile profile);
        void Update(QualityProfile profile);
        void Delete(int profileId);
        List<QualityProfile> GetAllProfiles();
        QualityProfile Find(int profileId);
    }
}
