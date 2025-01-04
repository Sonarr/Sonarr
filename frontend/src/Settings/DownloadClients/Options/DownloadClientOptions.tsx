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
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import {
  fetchDownloadClientOptions,
  saveDownloadClientOptions,
  setDownloadClientOptionsValue,
} from 'Store/Actions/settingsActions';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';

const SECTION = 'downloadClientOptions';

interface DownloadClientOptionsProps {
  setChildSave(saveCallback: () => void): void;
  onChildStateChange(payload: unknown): void;
}

function DownloadClientOptions({
  setChildSave,
  onChildStateChange,
}: DownloadClientOptionsProps) {
  const dispatch = useDispatch();
  const showAdvancedSettings = useShowAdvancedSettings();

  const {
    isFetching,
    isPopulated,
    isSaving,
    error,
    hasPendingChanges,
    hasSettings,
    settings,
  } = useSelector(createSettingsSectionSelector(SECTION));

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - actions aren't typed
      dispatch(setDownloadClientOptionsValue(change));
    },
    [dispatch]
  );

  useEffect(() => {
    dispatch(fetchDownloadClientOptions());
    setChildSave(() => dispatch(saveDownloadClientOptions()));

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

  return (
    <div>
      {isFetching ? <LoadingIndicator /> : null}

      {!isFetching && error ? (
        <Alert kind={kinds.DANGER}>
          {translate('DownloadClientOptionsLoadError')}
        </Alert>
      ) : null}

      {hasSettings && isPopulated && !error && showAdvancedSettings ? (
        <div>
          <FieldSet legend={translate('CompletedDownloadHandling')}>
            <Form>
              <FormGroup
                advancedSettings={showAdvancedSettings}
                isAdvanced={true}
                size={sizes.MEDIUM}
              >
                <FormLabel>{translate('Enable')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="enableCompletedDownloadHandling"
                  helpText={translate(
                    'EnableCompletedDownloadHandlingHelpText'
                  )}
                  onChange={handleInputChange}
                  {...settings.enableCompletedDownloadHandling}
                />
              </FormGroup>

              <FormGroup
                advancedSettings={showAdvancedSettings}
                isAdvanced={true}
                size={sizes.MEDIUM}
              >
                <FormLabel>{translate('AutoRedownloadFailed')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="autoRedownloadFailed"
                  helpText={translate('AutoRedownloadFailedHelpText')}
                  onChange={handleInputChange}
                  {...settings.autoRedownloadFailed}
                />
              </FormGroup>

              {settings.autoRedownloadFailed.value ? (
                <FormGroup
                  advancedSettings={showAdvancedSettings}
                  isAdvanced={true}
                  size={sizes.MEDIUM}
                >
                  <FormLabel>
                    {translate('AutoRedownloadFailedFromInteractiveSearch')}
                  </FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="autoRedownloadFailedFromInteractiveSearch"
                    helpText={translate(
                      'AutoRedownloadFailedFromInteractiveSearchHelpText'
                    )}
                    onChange={handleInputChange}
                    {...settings.autoRedownloadFailedFromInteractiveSearch}
                  />
                </FormGroup>
              ) : null}
            </Form>

            <Alert kind={kinds.INFO}>{translate('RemoveDownloadsAlert')}</Alert>
          </FieldSet>
        </div>
      ) : null}
    </div>
  );
}

export default DownloadClientOptions;
