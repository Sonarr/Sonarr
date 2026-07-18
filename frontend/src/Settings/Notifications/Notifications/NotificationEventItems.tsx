import React from 'react';
import CheckInput from 'Components/Form/CheckInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import { CheckInputChanged } from 'typings/inputs';
import { PendingSection } from 'typings/pending';
import translate from 'Utilities/String/translate';
import { NotificationModel } from '../useConnections';
import styles from './NotificationEventItems.css';

interface NotificationEventItemsProps {
  item: PendingSection<NotificationModel>;
  onInputChange: (change: CheckInputChanged) => void;
}

function NotificationEventItems({
  item,
  onInputChange,
}: NotificationEventItemsProps) {
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
    includeHealthWarnings,
  } = item;

  return (
    <FormRow>
      <FormLabel>{translate('NotificationTriggers')}</FormLabel>
      <div>
        <FormInputHelpText
          text={translate('NotificationTriggersHelpText')}
          link="https://wiki.servarr.com/sonarr/settings#connections"
        />
        <div className={styles.events}>
          <div>
            <CheckInput
              name="onGrab"
              helpText={translate('OnGrab')}
              isDisabled={!supportsOnGrab.value}
              {...onGrab}
              onChange={onInputChange}
            />
          </div>

          <div>
            <CheckInput
              name="onDownload"
              helpText={translate('OnFileImport')}
              isDisabled={!supportsOnDownload.value}
              {...onDownload}
              onChange={onInputChange}
            />
          </div>

          {onDownload.value && (
            <div>
              <CheckInput
                name="onUpgrade"
                helpText={translate('OnFileUpgrade')}
                isDisabled={!supportsOnUpgrade.value}
                {...onUpgrade}
                onChange={onInputChange}
              />
            </div>
          )}

          <div>
            <CheckInput
              name="onImportComplete"
              helpText={translate('OnImportComplete')}
              isDisabled={!supportsOnImportComplete.value}
              {...onImportComplete}
              onChange={onInputChange}
            />
          </div>

          <div>
            <CheckInput
              name="onRename"
              helpText={translate('OnRename')}
              isDisabled={!supportsOnRename.value}
              {...onRename}
              onChange={onInputChange}
            />
          </div>

          <div>
            <CheckInput
              name="onSeriesAdd"
              helpText={translate('OnSeriesAdd')}
              isDisabled={!supportsOnSeriesAdd.value}
              {...onSeriesAdd}
              onChange={onInputChange}
            />
          </div>

          <div>
            <CheckInput
              name="onSeriesDelete"
              helpText={translate('OnSeriesDelete')}
              isDisabled={!supportsOnSeriesDelete.value}
              {...onSeriesDelete}
              onChange={onInputChange}
            />
          </div>

          <div>
            <CheckInput
              name="onEpisodeFileDelete"
              helpText={translate('OnEpisodeFileDelete')}
              isDisabled={!supportsOnEpisodeFileDelete.value}
              {...onEpisodeFileDelete}
              onChange={onInputChange}
            />
          </div>

          {onEpisodeFileDelete.value && (
            <div>
              <CheckInput
                name="onEpisodeFileDeleteForUpgrade"
                helpText={translate('OnEpisodeFileDeleteForUpgrade')}
                isDisabled={!supportsOnEpisodeFileDeleteForUpgrade.value}
                {...onEpisodeFileDeleteForUpgrade}
                onChange={onInputChange}
              />
            </div>
          )}

          <div>
            <CheckInput
              name="onHealthIssue"
              helpText={translate('OnHealthIssue')}
              isDisabled={!supportsOnHealthIssue.value}
              {...onHealthIssue}
              onChange={onInputChange}
            />
          </div>

          <div>
            <CheckInput
              name="onHealthRestored"
              helpText={translate('OnHealthRestored')}
              isDisabled={!supportsOnHealthRestored.value}
              {...onHealthRestored}
              onChange={onInputChange}
            />
          </div>

          {(onHealthIssue.value || onHealthRestored.value) && (
            <div>
              <CheckInput
                name="includeHealthWarnings"
                helpText={translate('IncludeHealthWarnings')}
                isDisabled={!supportsOnHealthIssue.value}
                {...includeHealthWarnings}
                onChange={onInputChange}
              />
            </div>
          )}

          <div>
            <CheckInput
              name="onApplicationUpdate"
              helpText={translate('OnApplicationUpdate')}
              isDisabled={!supportsOnApplicationUpdate.value}
              {...onApplicationUpdate}
              onChange={onInputChange}
            />
          </div>

          <div>
            <CheckInput
              name="onManualInteractionRequired"
              helpText={translate('OnManualInteractionRequired')}
              isDisabled={!supportsOnManualInteractionRequired.value}
              {...onManualInteractionRequired}
              onChange={onInputChange}
            />
          </div>
        </div>
      </div>
    </FormRow>
  );
}

export default NotificationEventItems;
