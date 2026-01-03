import React from 'react';
import FieldSet from 'Components/FieldSet';
import PageSectionContent from 'Components/Page/PageSectionContent';
import translate from 'Utilities/String/translate';
import { useSortedMetadata } from '../useMetadata';
import Metadata from './Metadata';
import styles from './Metadatas.css';

function Metadatas() {
  const { data: items, isFetching, isFetched, error } = useSortedMetadata();

  return (
    <FieldSet legend={translate('Metadata')}>
      <PageSectionContent
        error={error}
        errorMessage={translate('MetadataLoadError')}
        isFetching={isFetching}
        isPopulated={isFetched}
      >
        <div className={styles.metadatas}>
          {items.map((item) => {
            return <Metadata key={item.id} {...item} />;
          })}
        </div>
      </PageSectionContent>
    </FieldSet>
  );
}

export default Metadatas;
