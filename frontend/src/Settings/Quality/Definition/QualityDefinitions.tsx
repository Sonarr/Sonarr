import { useQueryClient } from '@tanstack/react-query';
import React, { useEffect } from 'react';
import FieldSet from 'Components/FieldSet';
import PageSectionContent from 'Components/Page/PageSectionContent';
import usePrevious from 'Helpers/Hooks/usePrevious';
import {
  OnChildStateChange,
  SetChildSave,
} from 'typings/Settings/SettingsState';
import translate from 'Utilities/String/translate';
import QualityDefinition from './QualityDefinition';
import { useManageQualityDefinitions } from './useQualityDefinitions';
import styles from './QualityDefinitions.css';

interface QualityDefinitionsProps {
  isResettingQualityDefinitions: boolean;
  setChildSave: SetChildSave;
  onChildStateChange: OnChildStateChange;
}

function QualityDefinitions({
  isResettingQualityDefinitions,
  setChildSave,
  onChildStateChange,
}: QualityDefinitionsProps) {
  const queryClient = useQueryClient();
  const {
    items,
    isFetching,
    isFetched,
    isSaving,
    error,
    hasPendingChanges,
    updateDefinition,
    saveQualityDefinitions,
  } = useManageQualityDefinitions();

  const wasResettingQualityDefinitions = usePrevious(
    isResettingQualityDefinitions
  );

  useEffect(() => {
    setChildSave(() => {
      saveQualityDefinitions();
    });
  }, [saveQualityDefinitions, setChildSave]);

  useEffect(() => {
    onChildStateChange({
      isSaving,
      hasPendingChanges,
    });
  }, [hasPendingChanges, isSaving, onChildStateChange]);

  useEffect(() => {
    if (wasResettingQualityDefinitions && !isResettingQualityDefinitions) {
      queryClient.invalidateQueries({ queryKey: ['/qualitydefinition'] });
    }
  }, [
    isResettingQualityDefinitions,
    wasResettingQualityDefinitions,
    queryClient,
  ]);

  return (
    <FieldSet legend={translate('QualityDefinitions')}>
      <PageSectionContent
        errorMessage={translate('QualityDefinitionsLoadError')}
        isFetching={isFetching}
        isPopulated={isFetched}
        error={error}
      >
        <div className={styles.header}>
          <div className={styles.quality}>{translate('Quality')}</div>
          <div className={styles.title}>{translate('Title')}</div>
        </div>

        <div className={styles.definitions}>
          {items.map((item) => {
            return (
              <QualityDefinition
                key={item.id}
                {...item}
                updateDefinition={updateDefinition}
              />
            );
          })}
        </div>
      </PageSectionContent>
    </FieldSet>
  );
}

export default QualityDefinitions;
