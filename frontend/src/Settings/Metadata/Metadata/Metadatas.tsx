import React from 'react';
import PageSectionContent from 'Components/Page/PageSectionContent';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import translate from 'Utilities/String/translate';
import { useSortedMetadata } from '../useMetadata';
import Metadata from './Metadata';

function Metadatas() {
  const { data: items, isFetching, isFetched, error } = useSortedMetadata();

  return (
    <PageSectionContent
      error={error}
      errorMessage={translate('MetadataLoadError')}
      isFetching={isFetching}
      isPopulated={isFetched}
    >
      <div className={settingsCardStyles.grid}>
        {items.map((item) => {
          return <Metadata key={item.id} {...item} />;
        })}
      </div>
    </PageSectionContent>
  );
}

export default Metadatas;
