import { useQualityProfile } from 'Settings/Profiles/Quality/useQualityProfiles';
import Series from './Series';

const useSeriesQualityProfile = (series: Series | undefined) => {
  return useQualityProfile(series?.qualityProfileId);
};

export default useSeriesQualityProfile;
