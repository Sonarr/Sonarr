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
  saveReleaseProfile,
  setReleaseProfileValue,
} from 'Store/Actions/Settings/releaseProfiles';
import selectSettings from 'Store/Selectors/selectSettings';
import { InputChanged } from 'typings/inputs';
import ReleaseProfile from 'typings/Settings/ReleaseProfile';
import translate from 'Utilities/String/translate';
import styles from './EditReleaseProfileModalContent.css';

const tagInputDelimiters = ['Tab', 'Enter'];

const newReleaseProfile: ReleaseProfile = {
  id: 0,
  name: '',
  enabled: true,
  required: [],
  ignored: [],
  tags: [],
  excludedTags: [],
  indexerId: 0,
};

function createReleaseProfileSelector(id?: number) {
  return createSelector(
    (state: AppState) => state.settings.releaseProfiles,
    (releaseProfiles) => {
      const { items, isFetching, error, isSaving, saveError, pendingChanges } =
        releaseProfiles;

      const mapping = id ? items.find((i) => i.id === id)! : newReleaseProfile;
      const settings = selectSettings<ReleaseProfile>(
        mapping,
        pendingChanges,
        saveError
      );

      return {
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

interface EditReleaseProfileModalContentProps {
  id?: number;
  onModalClose: () => void;
  onDeleteReleaseProfilePress?: () => void;
}

function EditReleaseProfileModalContent({
  id,
  onModalClose,
  onDeleteReleaseProfilePress,
}: EditReleaseProfileModalContentProps) {
  const { item, isFetching, isSaving, error, saveError, ...otherProps } =
    useSelector(createReleaseProfileSelector(id));

  const { name, enabled, required, ignored, tags, excludedTags, indexerId } =
    item;

  const dispatch = useDispatch();
  const previousIsSaving = usePrevious(isSaving);

  useEffect(() => {
    if (!id) {
      Object.entries(newReleaseProfile).forEach(([name, value]) => {
        // @ts-expect-error 'setReleaseProfileValue' isn't typed yet
        dispatch(setReleaseProfileValue({ name, value }));
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
    dispatch(saveReleaseProfile({ id }));
  }, [dispatch, id]);

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error 'setReleaseProfileValue' isn't typed yet
      dispatch(setReleaseProfileValue(change));
    },
    [dispatch]
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? translate('EditReleaseProfile') : translate('AddReleaseProfile')}
      </ModalHeader>

      <ModalBody>
        <Form {...otherProps}>
          <FormGroup>
            <FormLabel>{translate('Name')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT}
              name="name"
              {...name}
              placeholder={translate('OptionalName')}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('EnableProfile')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="enabled"
              helpText={translate('EnableProfileHelpText')}
              {...enabled}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('MustContain')}</FormLabel>

            <FormInputGroup
              {...required}
              inputClassName={styles.tagInternalInput}
              type={inputTypes.TEXT_TAG}
              name="required"
              helpText={translate('MustContainHelpText')}
              kind={kinds.SUCCESS}
              placeholder={translate('AddNewRestriction')}
              delimiters={tagInputDelimiters}
              canEdit={true}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('MustNotContain')}</FormLabel>

            <FormInputGroup
              {...ignored}
              inputClassName={styles.tagInternalInput}
              type={inputTypes.TEXT_TAG}
              name="ignored"
              helpText={translate('MustNotContainHelpText')}
              kind={kinds.DANGER}
              placeholder={translate('AddNewRestriction')}
              delimiters={tagInputDelimiters}
              canEdit={true}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Indexer')}</FormLabel>

            <FormInputGroup
              type={inputTypes.INDEXER_SELECT}
              name="indexerId"
              helpText={translate('ReleaseProfileIndexerHelpText')}
              helpTextWarning={translate(
                'ReleaseProfileIndexerHelpTextWarning'
              )}
              {...indexerId}
              includeAny={true}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Tags')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TAG}
              name="tags"
              helpText={translate('ReleaseProfileTagSeriesHelpText')}
              {...tags}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('ExcludedTags')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TAG}
              name="excludedTags"
              helpText={translate('ReleaseProfileExcludedTagSeriesHelpText')}
              kind={kinds.DANGER}
              {...excludedTags}
              onChange={handleInputChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>
      <ModalFooter>
        {id ? (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeleteReleaseProfilePress}
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

export default EditReleaseProfileModalContent;
