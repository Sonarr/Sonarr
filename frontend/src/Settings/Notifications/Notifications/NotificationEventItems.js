import PropTypes from 'prop-types';
import React from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import { inputTypes } from 'Helpers/Props';
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
    onRename,
    onSeriesDelete,
    onEpisodeFileDelete,
    onEpisodeFileDeleteForUpgrade,
    onHealthIssue,
    onApplicationUpdate,
    supportsOnGrab,
    supportsOnDownload,
    supportsOnUpgrade,
    supportsOnRename,
    supportsOnSeriesDelete,
    supportsOnEpisodeFileDelete,
    supportsOnEpisodeFileDeleteForUpgrade,
    supportsOnApplicationUpdate,
    supportsOnHealthIssue,
    includeHealthWarnings
  } = item;

  return (
    <FormGroup>
      <FormLabel>Notification Triggers</FormLabel>
      <div>
        <FormInputHelpText
          text="Select which events should trigger this notification"
          link="https://wiki.servarr.com/sonarr/settings#connections"
        />
        <div className={styles.events}>
          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onGrab"
              helpText="On Grab"
              isDisabled={!supportsOnGrab.value}
              {...onGrab}
              onChange={onInputChange}
            />
          </div>

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onDownload"
              helpText="On Import"
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
                  helpText="On Upgrade"
                  isDisabled={!supportsOnUpgrade.value}
                  {...onUpgrade}
                  onChange={onInputChange}
                />
              </div>
          }

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onRename"
              helpText="On Rename"
              isDisabled={!supportsOnRename.value}
              {...onRename}
              onChange={onInputChange}
            />
          </div>

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onSeriesDelete"
              helpText="On Series Delete"
              isDisabled={!supportsOnSeriesDelete.value}
              {...onSeriesDelete}
              onChange={onInputChange}
            />
          </div>

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onEpisodeFileDelete"
              helpText="On Episode File Delete"
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
                  helpText="On Episode File Delete For Upgrade"
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
              helpText="On Health Issue"
              isDisabled={!supportsOnHealthIssue.value}
              {...onHealthIssue}
              onChange={onInputChange}
            />
          </div>

          {
            onHealthIssue.value &&
              <div>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="includeHealthWarnings"
                  helpText="Include Health Warnings"
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
              helpText="On Application Update"
              isDisabled={!supportsOnApplicationUpdate.value}
              {...onApplicationUpdate}
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
