import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import MetadataAppState from 'App/State/MetadataAppState';
import FieldSet from 'Components/FieldSet';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { fetchMetadata } from 'Store/Actions/settingsActions';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import MetadataType from 'typings/Metadata';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import Metadata from './Metadata';
import styles from './Metadatas.css';

function createMetadatasSelector() {
  return createSelector(
    createSortedSectionSelector<MetadataType, MetadataAppState>(
      'settings.metadata',
      sortByProp('name')
    ),
    (metadata: MetadataAppState) => metadata
  );
}

function Metadatas() {
  const dispatch = useDispatch();
  const { isFetching, error, items, ...otherProps } = useSelector(
    createMetadatasSelector()
  );

  useEffect(() => {
    dispatch(fetchMetadata());
  }, [dispatch]);

  return (
    <FieldSet legend={translate('Metadata')}>
      <PageSectionContent
        isFetching={isFetching}
        errorMessage={translate('MetadataLoadError')}
        {...otherProps}
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
