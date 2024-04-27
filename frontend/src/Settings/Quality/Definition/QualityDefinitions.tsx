import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import {
  fetchQualityDefinitions,
  saveQualityDefinitions,
} from 'Store/Actions/settingsActions';
import translate from 'Utilities/String/translate';
import QualityDefinition from './QualityDefinition';
import styles from './QualityDefinitions.css';

function qualityDefinitionsSelector() {
  return createSelector(
    (state: AppState) => state.settings.qualityDefinitions,
    (qualityDefinitions) => {
      const items = qualityDefinitions.items.map((item) => {
        const pendingChanges = qualityDefinitions.pendingChanges[item.id] || {};

        return Object.assign({}, item, pendingChanges);
      });

      return {
        ...qualityDefinitions,
        items,
        hasPendingChanges: !!Object.keys(qualityDefinitions.pendingChanges)
          .length,
      };
    }
  );
}

interface QualityDefinitionsProps {
  isSaving: boolean;
  onChildMounted: (saveCallback: () => void) => void;
  onChildStateChange: ({
    isSaving,
    hasPendingChanges,
  }: {
    isSaving: boolean;
    hasPendingChanges: boolean;
  }) => void;
}

function QualityDefinitions(props: QualityDefinitionsProps) {
  const { isSaving, onChildMounted, onChildStateChange } = props;
  const { isFetching, isPopulated, error, items, hasPendingChanges } =
    useSelector(qualityDefinitionsSelector());
  const dispatch = useDispatch();

  const handleSavePress = useCallback(() => {
    dispatch(saveQualityDefinitions());
  }, [dispatch]);

  useEffect(() => {
    dispatch(fetchQualityDefinitions());

    return () => {
      dispatch(
        clearPendingChanges({
          section: 'settings.qualityDefinitions',
        })
      );
    };
  }, [dispatch]);

  useEffect(() => {
    onChildMounted(handleSavePress);
  }, [handleSavePress, onChildMounted]);

  useEffect(() => {
    onChildStateChange({ isSaving, hasPendingChanges });
  }, [isSaving, hasPendingChanges, onChildStateChange]);

  return (
    <FieldSet legend={translate('QualityDefinitions')}>
      <Alert className={styles.notice}>
        {translate('QualityDefinitionsSizeNotice')}
      </Alert>
      <PageSectionContent
        error={error}
        errorMessage={translate('QualityDefinitionsLoadError')}
        isFetching={isFetching}
        isPopulated={isPopulated}
      >
        <div className={styles.header}>
          <div className={styles.quality}>{translate('Quality')}</div>

          <div className={styles.title}>{translate('Title')}</div>
        </div>

        <div className={styles.definitions}>
          {items.map((item) => {
            return <QualityDefinition key={item.id} {...item} />;
          })}
        </div>
      </PageSectionContent>
    </FieldSet>
  );
}

export default QualityDefinitions;
