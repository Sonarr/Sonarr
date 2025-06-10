import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { inputTypes, kinds } from 'Helpers/Props';
import {
  saveNotificationTemplate,
  setNotificationTemplateValue,
} from 'Store/Actions/Settings/notificationTemplates';
import selectSettings from 'Store/Selectors/selectSettings';
import NotificationTemplate from 'typings/Settings/NotificationTemplate';
import translate from 'Utilities/String/translate';
import styles from './EditNotificationTemplateModalContent.css';

const newNotificationTemplate: NotificationTemplate = {
  id: 0,
  name: '',
  title: '',
  body: '',
  onGrab: true,
  onDownload: true,
  onUpgrade: true,
  onImportComplete: true,
  onRename: false,
  onSeriesAdd: true,
  onSeriesDelete: false,
  onEpisodeFileDelete: false,
  onEpisodeFileDeleteForUpgrade: false,
  onHealthIssue: false,
  onHealthRestored: false,
  onApplicationUpdate: false,
  onManualInteractionRequired: false
};

function createNotificationTemplateSelector(id?: number) {
  return createSelector(
    (state: AppState) => state.settings.notificationTemplates,
    (notificationTemplates) => {
      const { items, isFetching, error, isSaving, saveError, pendingChanges } =
        notificationTemplates;

      const mapping = id ? items.find((i) => i.id === id)! : newNotificationTemplate;
      const settings = selectSettings<NotificationTemplate>(
        mapping,
        pendingChanges,
        saveError
      );

      return {
        id,
        isFetching,
        error,
        isSaving,
        saveError,
        item: settings.settings,
        ...settings,
      };
    }
  );
}

interface EditNotificationTemplateModalContentProps {
  id?: number;
  onModalClose: () => void;
  onDeleteNotificationTemplatePress?: () => void;
}

function EditNotificationTemplateModalContent({
  id,
  onModalClose,
  onDeleteNotificationTemplatePress,
}: EditNotificationTemplateModalContentProps) {
  const { item, isFetching, isSaving, error, saveError, ...otherProps } =
    useSelector(createNotificationTemplateSelector(id));

  const {
    name,
    title,
    body,
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
    onManualInteractionRequired
  } = item;

  const dispatch = useDispatch();
  const previousIsSaving = usePrevious(isSaving);

  useEffect(() => {
    if (!id) {
      Object.entries(newNotificationTemplate).forEach(([name, value]) => {
      // @ts-expect-error 'setNotificationTemplateValue' isn't typed yet
        dispatch(setNotificationTemplateValue({ name, value }));
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (previousIsSaving && !isSaving && !saveError) {
      onModalClose();
    }
  }, [previousIsSaving, isSaving, saveError, onModalClose]);

  const handleSavePress = useCallback(() => {
    dispatch(saveNotificationTemplate({ id }));
  }, [dispatch, id]);

  const onInputChange = useCallback(
    (payload: { name: string; value: string | number | boolean }) => {
      // @ts-expect-error 'setNotificationTemplateValue' isn't typed yet
      dispatch(setNotificationTemplateValue(payload));
    },
    [dispatch]
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? translate('EditNotificationTemplate') : translate('AddNotificationTemplate')}
      </ModalHeader>

      <ModalBody>
        <Form {...otherProps}>
          <FormGroup>
            <FormLabel>{translate('Name')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT}
              name="name"
              {...name}
              canEdit={true}
              onChange={onInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Title')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT_AREA}
              name="title"
              helpText={translate('NotificationTemplateTitleHelpText')}
              {...title}
              onChange={onInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Body')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT_AREA}
              name="body"
              helpText={translate('NotificationTemplateBodyHelpText')}
              {...body}
              onChange={onInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('NotificationTriggers')}</FormLabel>
            <div>
              <div>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onGrab"
                  helpText={translate('OnGrab')}
                  {...onGrab}
                  onChange={onInputChange}
                />
              </div>

              <div>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onDownload"
                  helpText={translate('OnFileImport')}
                  {...onDownload}
                  onChange={onInputChange}
                />
              </div>

              <div>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onUpgrade"
                  helpText={translate('OnFileUpgrade')}
                  {...onUpgrade}
                  onChange={onInputChange}
                />
              </div>

              <div>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onImportComplete"
                  helpText={translate('OnImportComplete')}
                  {...onImportComplete}
                  onChange={onInputChange}
                />
              </div>

              <div>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onRename"
                  helpText={translate('OnRename')}
                  {...onRename}
                  onChange={onInputChange}
                />
              </div>

              <div>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onSeriesAdd"
                  helpText={translate('OnSeriesAdd')}
                  {...onSeriesAdd}
                  onChange={onInputChange}
                />
              </div>

              <div>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onSeriesDelete"
                  helpText={translate('OnSeriesDelete')}
                  {...onSeriesDelete}
                  onChange={onInputChange}
                />
              </div>

              <div>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onEpisodeFileDelete"
                  helpText={translate('OnEpisodeFileDelete')}
                  {...onEpisodeFileDelete}
                  onChange={onInputChange}
                />
              </div>

              <div>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onEpisodeFileDeleteForUpgrade"
                  helpText={translate('OnEpisodeFileDeleteForUpgrade')}
                  {...onEpisodeFileDeleteForUpgrade}
                  onChange={onInputChange}
                />
              </div>

              <div>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onHealthIssue"
                  helpText={translate('OnHealthIssue')}
                  {...onHealthIssue}
                  onChange={onInputChange}
                />
              </div>

              <div>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onHealthRestored"
                  helpText={translate('OnHealthRestored')}
                  {...onHealthRestored}
                  onChange={onInputChange}
                />
              </div>

              <div>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onApplicationUpdate"
                  helpText={translate('OnApplicationUpdate')}
                  {...onApplicationUpdate}
                  onChange={onInputChange}
                />
              </div>

              <div>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onManualInteractionRequired"
                  helpText={translate('OnManualInteractionRequired')}
                  {...onManualInteractionRequired}
                  onChange={onInputChange}
                />
              </div>
            </div>
          </FormGroup>
        </Form>
      </ModalBody>
      <ModalFooter>
        {id ? (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeleteNotificationTemplatePress}
          >
            {translate('Delete')}
          </Button>
        ) : null}

        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={handleSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default EditNotificationTemplateModalContent;
