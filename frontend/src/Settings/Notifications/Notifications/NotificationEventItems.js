import PropTypes from 'prop-types';
import React from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import { inputTypes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './NotificationEventItems.css';

function NotificationEventItems(props) {
  const {
    item,
    onInputChange
  } = props;

  const {
    onGrab,
    onDownload,
    onUpgrade,
    onImportComplete,
    onRename,
    onSeriesAdd,
    onSeriesDelete,
    onEpisodeFileDelete,
    onEpisodeFileDeleteForUpgrade,
    onHealthIssue,
    onHealthRestored,
    onApplicationUpdate,
    onManualInteractionRequired,
    supportsOnGrab,
    supportsOnDownload,
    supportsOnUpgrade,
    supportsOnImportComplete,
    supportsOnRename,
    supportsOnSeriesAdd,
    supportsOnSeriesDelete,
    supportsOnEpisodeFileDelete,
    supportsOnEpisodeFileDeleteForUpgrade,
    supportsOnApplicationUpdate,
    supportsOnManualInteractionRequired,
    supportsOnHealthIssue,
    supportsOnHealthRestored,
    includeHealthWarnings
  } = item;

  return (
    <FormGroup>
      <FormLabel>{translate('NotificationTriggers')}</FormLabel>
      <div>
        <FormInputHelpText
          text={translate('NotificationTriggersHelpText')}
          link="https://wiki.servarr.com/sonarr/settings#connections"
        />
        <div className={styles.events}>
          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onGrab"
              helpText={translate('OnGrab')}
              isDisabled={!supportsOnGrab.value}
              {...onGrab}
              onChange={onInputChange}
            />
          </div>

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onDownload"
              helpText={translate('OnFileImport')}
              isDisabled={!supportsOnDownload.value}
              {...onDownload}
              onChange={onInputChange}
            />
          </div>

          {
            onDownload.value &&
              <div>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onUpgrade"
                  helpText={translate('OnUpgrade')}
                  isDisabled={!supportsOnUpgrade.value}
                  {...onUpgrade}
                  onChange={onInputChange}
                />
              </div>
          }

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onImportComplete"
              helpText={translate('OnImportComplete')}
              isDisabled={!supportsOnImportComplete.value}
              {...onImportComplete}
              onChange={onInputChange}
            />
          </div>

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onRename"
              helpText={translate('OnRename')}
              isDisabled={!supportsOnRename.value}
              {...onRename}
              onChange={onInputChange}
            />
          </div>

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onSeriesAdd"
              helpText={translate('OnSeriesAdd')}
              isDisabled={!supportsOnSeriesAdd.value}
              {...onSeriesAdd}
              onChange={onInputChange}
            />
          </div>

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onSeriesDelete"
              helpText={translate('OnSeriesDelete')}
              isDisabled={!supportsOnSeriesDelete.value}
              {...onSeriesDelete}
              onChange={onInputChange}
            />
          </div>

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onEpisodeFileDelete"
              helpText={translate('OnEpisodeFileDelete')}
              isDisabled={!supportsOnEpisodeFileDelete.value}
              {...onEpisodeFileDelete}
              onChange={onInputChange}
            />
          </div>

          {
            onEpisodeFileDelete.value &&
              <div>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onEpisodeFileDeleteForUpgrade"
                  helpText={translate('OnEpisodeFileDeleteForUpgrade')}
                  isDisabled={!supportsOnEpisodeFileDeleteForUpgrade.value}
                  {...onEpisodeFileDeleteForUpgrade}
                  onChange={onInputChange}
                />
              </div>
          }

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onHealthIssue"
              helpText={translate('OnHealthIssue')}
              isDisabled={!supportsOnHealthIssue.value}
              {...onHealthIssue}
              onChange={onInputChange}
            />
          </div>

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onHealthRestored"
              helpText={translate('OnHealthRestored')}
              isDisabled={!supportsOnHealthRestored.value}
              {...onHealthRestored}
              onChange={onInputChange}
            />
          </div>

          {
            (onHealthIssue.value || onHealthRestored.value) &&
              <div>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="includeHealthWarnings"
                  helpText={translate('IncludeHealthWarnings')}
                  isDisabled={!supportsOnHealthIssue.value}
                  {...includeHealthWarnings}
                  onChange={onInputChange}
                />
              </div>
          }

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onApplicationUpdate"
              helpText={translate('OnApplicationUpdate')}
              isDisabled={!supportsOnApplicationUpdate.value}
              {...onApplicationUpdate}
              onChange={onInputChange}
            />
          </div>

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onManualInteractionRequired"
              helpText={translate('OnManualInteractionRequired')}
              isDisabled={!supportsOnManualInteractionRequired.value}
              {...onManualInteractionRequired}
              onChange={onInputChange}
            />
          </div>
        </div>
      </div>
    </FormGroup>
  );
}

NotificationEventItems.propTypes = {
  item: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default NotificationEventItems;
