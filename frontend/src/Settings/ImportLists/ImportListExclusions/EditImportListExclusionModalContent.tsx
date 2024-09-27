import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { inputTypes, kinds } from 'Helpers/Props';
import {
  saveImportListExclusion,
  setImportListExclusionValue,
} from 'Store/Actions/settingsActions';
import selectSettings from 'Store/Selectors/selectSettings';
import ImportListExclusion from 'typings/ImportListExclusion';
import { PendingSection } from 'typings/pending';
import translate from 'Utilities/String/translate';
import styles from './EditImportListExclusionModalContent.css';

const newImportListExclusion = {
  title: '',
  tvdbId: 0,
};

function createImportListExclusionSelector(id?: number) {
  return createSelector(
    (state: AppState) => state.settings.importListExclusions,
    (importListExclusions) => {
      const { isFetching, error, isSaving, saveError, pendingChanges, items } =
        importListExclusions;

      const mapping = id
        ? items.find((i) => i.id === id)
        : newImportListExclusion;
      const settings = selectSettings(mapping, pendingChanges, saveError);

      return {
        id,
        isFetching,
        error,
        isSaving,
        saveError,
        item: settings.settings as PendingSection<ImportListExclusion>,
        ...settings,
      };
    }
  );
}

interface EditImportListExclusionModalContentProps {
  id?: number;
  onModalClose: () => void;
  onDeleteImportListExclusionPress?: () => void;
}

function EditImportListExclusionModalContent({
  id,
  onModalClose,
  onDeleteImportListExclusionPress,
}: EditImportListExclusionModalContentProps) {
  const { isFetching, isSaving, item, error, saveError, ...otherProps } =
    useSelector(createImportListExclusionSelector(id));

  const { title, tvdbId } = item;

  const dispatch = useDispatch();
  const previousIsSaving = usePrevious(isSaving);

  const dispatchSetImportListExclusionValue = (payload: {
    name: string;
    value: string | number;
  }) => {
    // @ts-expect-error 'setImportListExclusionValue' isn't typed yet
    dispatch(setImportListExclusionValue(payload));
  };

  useEffect(() => {
    if (!id) {
      Object.entries(newImportListExclusion).forEach(([name, value]) => {
        dispatchSetImportListExclusionValue({ name, value });
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (previousIsSaving && !isSaving && !saveError) {
      onModalClose();
    }
  }, [previousIsSaving, isSaving, saveError, onModalClose]);

  const onSavePress = useCallback(() => {
    dispatch(saveImportListExclusion({ id }));
  }, [dispatch, id]);

  const onInputChange = useCallback(
    (payload: { name: string; value: string | number }) => {
      // @ts-expect-error 'setImportListExclusionValue' isn't typed yet
      dispatch(setImportListExclusionValue(payload));
    },
    [dispatch]
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id
          ? translate('EditImportListExclusion')
          : translate('AddImportListExclusion')}
      </ModalHeader>

      <ModalBody className={styles.body}>
        {isFetching && <LoadingIndicator />}

        {!isFetching && !!error && (
          <Alert kind={kinds.DANGER}>
            {translate('AddImportListExclusionError')}
          </Alert>
        )}

        {!isFetching && !error && (
          <Form {...otherProps}>
            <FormGroup>
              <FormLabel>{translate('Title')}</FormLabel>

              <FormInputGroup
                type={inputTypes.TEXT}
                name="title"
                helpText={translate('SeriesTitleToExcludeHelpText')}
                {...title}
                onChange={onInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('TvdbId')}</FormLabel>

              <FormInputGroup
                type={inputTypes.NUMBER}
                name="tvdbId"
                helpText={translate('TvdbIdExcludeHelpText')}
                {...tvdbId}
                onChange={onInputChange}
              />
            </FormGroup>
          </Form>
        )}
      </ModalBody>

      <ModalFooter>
        {id && (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeleteImportListExclusionPress}
          >
            {translate('Delete')}
          </Button>
        )}

        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={onSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default EditImportListExclusionModalContent;
