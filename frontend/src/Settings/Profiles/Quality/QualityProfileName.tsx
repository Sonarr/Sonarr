import React from 'react';
import translate from 'Utilities/String/translate';
import { useQualityProfile } from './useQualityProfiles';

interface QualityProfileNameProps {
  qualityProfileId: number;
}

function QualityProfileName({ qualityProfileId }: QualityProfileNameProps) {
  const qualityProfile = useQualityProfile(qualityProfileId);

  return <span>{qualityProfile?.name ?? translate('Unknown')}</span>;
}

export default QualityProfileName;
