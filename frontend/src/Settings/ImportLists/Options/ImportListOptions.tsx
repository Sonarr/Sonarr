import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import useShowAdvancedSettings from 'Helpers/Hooks/useShowAdvancedSettings';
import { inputTypes, kinds } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import {
  fetchImportListOptions,
  saveImportListOptions,
  setImportListOptionsValue,
} from 'Store/Actions/settingsActions';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import translate from 'Utilities/String/translate';

const SECTION = 'importListOptions';
const cleanLibraryLevelOptions = [
  { key: 'disabled', value: () => translate('Disabled') },
  { key: 'logOnly', value: () => translate('LogOnly') },
  { key: 'keepAndUnmonitor', value: () => translate('KeepAndUnmonitorSeries') },
  { key: 'keepAndTag', value: () => translate('KeepAndTagSeries') },
];

interface ImportListOptionsProps {
  setChildSave(saveCallback: () => void): void;
  onChildStateChange(payload: unknown): void;
}

function ImportListOptions({
  setChildSave,
  onChildStateChange,
}: ImportListOptionsProps) {
  const dispatch = useDispatch();
  const showAdvancedSettings = useShowAdvancedSettings();

  const {
    isSaving,
    hasPendingChanges,
    isFetching,
    error,
    settings,
    hasSettings,
  } = useSelector(createSettingsSectionSelector(SECTION));

  const { listSyncLevel, listSyncTag } = settings;

  const onInputChange = useCallback(
    ({ name, value }: { name: string; value: unknown }) => {
      // @ts-expect-error 'setImportListOptionsValue' isn't typed yet
      dispatch(setImportListOptionsValue({ name, value }));
    },
    [dispatch]
  );

  const onTagChange = useCallback(
    ({ name, value }: { name: string; value: number[] }) => {
      const id = value.length === 0 ? 0 : value.pop();
      // @ts-expect-error 'setImportListOptionsValue' isn't typed yet
      dispatch(setImportListOptionsValue({ name, value: id }));
    },
    [dispatch]
  );

  useEffect(() => {
    dispatch(fetchImportListOptions());
    setChildSave(() => dispatch(saveImportListOptions()));

    return () => {
      dispatch(clearPendingChanges({ section: `settings.${SECTION}` }));
    };
  }, [dispatch, setChildSave]);

  useEffect(() => {
    onChildStateChange({
      isSaving,
      hasPendingChanges,
    });
  }, [onChildStateChange, isSaving, hasPendingChanges]);

  if (!showAdvancedSettings) {
    return null;
  }

  return (
    <FieldSet legend={translate('Options')}>
      {isFetching ? <LoadingIndicator /> : null}

      {!isFetching && error ? (
        <Alert kind={kinds.DANGER}>{translate('ListOptionsLoadError')}</Alert>
      ) : null}

      {hasSettings && !isFetching && !error ? (
        <Form>
          <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('CleanLibraryLevel')}</FormLabel>
            <FormInputGroup
              type={inputTypes.SELECT}
              name="listSyncLevel"
              values={cleanLibraryLevelOptions}
              helpText={translate('ListSyncLevelHelpText')}
              onChange={onInputChange}
              {...listSyncLevel}
            />
          </FormGroup>
          {listSyncLevel.value === 'keepAndTag' ? (
            <FormGroup
              advancedSettings={showAdvancedSettings}
              isAdvanced={true}
            >
              <FormLabel>{translate('ListSyncTag')}</FormLabel>
              <FormInputGroup
                {...listSyncTag}
                type={inputTypes.TAG}
                name="listSyncTag"
                value={listSyncTag.value === 0 ? [] : [listSyncTag.value]}
                helpText={translate('ListSyncTagHelpText')}
                onChange={onTagChange}
              />
            </FormGroup>
          ) : null}
        </Form>
      ) : null}
    </FieldSet>
  );
}

export default ImportListOptions;
