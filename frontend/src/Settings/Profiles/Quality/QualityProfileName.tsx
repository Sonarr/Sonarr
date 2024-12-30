import React from 'react';
import { useSelector } from 'react-redux';
import { createQualityProfileSelectorForHook } from 'Store/Selectors/createQualityProfileSelector';
import translate from 'Utilities/String/translate';

interface QualityProfileNameProps {
  qualityProfileId: number;
}

function QualityProfileName({ qualityProfileId }: QualityProfileNameProps) {
  const qualityProfile = useSelector(
    createQualityProfileSelectorForHook(qualityProfileId)
  );

  return <span>{qualityProfile?.name ?? translate('Unknown')}</span>;
}

export default QualityProfileName;
