import { useQueryClient } from '@tanstack/react-query';
import React, { useEffect } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import PageSectionContent from 'Components/Page/PageSectionContent';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { kinds } from 'Helpers/Props';
import {
  OnChildStateChange,
  SetChildSave,
} from 'typings/Settings/SettingsState';
import translate from 'Utilities/String/translate';
import QualityDefinition from './QualityDefinition';
import { useManageQualityDefinitions } from './useQualityDefinitions';
import styles from './QualityDefinitions.module.css';

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
    <FieldSet
      legend={translate('QualityDefinitions')}
      caption={translate('QualityDefinitionsCaption')}
      notice={
        <Alert kind={kinds.INFO}>
          <InlineMarkdown data={translate('QualityDefinitionsSizeNotice')} />
        </Alert>
      }
    >
      <PageSectionContent
        errorMessage={translate('QualityDefinitionsLoadError')}
        isFetching={isFetching}
        isPopulated={isFetched}
        error={error}
      >
        <div className={styles.header}>
          <div className={styles.headerQuality}>{translate('Quality')}</div>
          <div className={styles.headerTitle}>{translate('Title')}</div>
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
