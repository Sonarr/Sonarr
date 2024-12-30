import { isEmpty } from 'lodash';
import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import FieldSet from 'Components/FieldSet';
import PageSectionContent from 'Components/Page/PageSectionContent';
import usePrevious from 'Helpers/Hooks/usePrevious';
import {
  fetchQualityDefinitions,
  saveQualityDefinitions,
} from 'Store/Actions/settingsActions';
import {
  OnChildStateChange,
  SetChildSave,
} from 'typings/Settings/SettingsState';
import translate from 'Utilities/String/translate';
import QualityDefinition from './QualityDefinition';
import styles from './QualityDefinitions.css';

function createQualityDefinitionsSelector() {
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
        hasPendingChanges: !isEmpty(qualityDefinitions.pendingChanges),
      };
    }
  );
}

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
  const dispatch = useDispatch();
  const { items, isFetching, isPopulated, isSaving, error, hasPendingChanges } =
    useSelector(createQualityDefinitionsSelector());

  const wasResettingQualityDefinitions = usePrevious(
    isResettingQualityDefinitions
  );

  useEffect(() => {
    dispatch(fetchQualityDefinitions());

    setChildSave(() => {
      dispatch(saveQualityDefinitions());
    });
  }, [dispatch, setChildSave]);

  useEffect(() => {
    onChildStateChange({
      isSaving,
      hasPendingChanges,
    });
  }, [hasPendingChanges, isSaving, onChildStateChange]);

  useEffect(() => {
    if (wasResettingQualityDefinitions && !isResettingQualityDefinitions) {
      dispatch(fetchQualityDefinitions());
    }
  }, [isResettingQualityDefinitions, wasResettingQualityDefinitions, dispatch]);

  return (
    <FieldSet legend={translate('QualityDefinitions')}>
      <PageSectionContent
        errorMessage={translate('QualityDefinitionsLoadError')}
        isFetching={isFetching}
        isPopulated={isPopulated}
        error={error}
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
