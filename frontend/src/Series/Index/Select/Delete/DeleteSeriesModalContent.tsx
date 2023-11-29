import { orderBy } from 'lodash';
import React, { useCallback, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import Series from 'Series/Series';
import { bulkDeleteSeries, setDeleteOption } from 'Store/Actions/seriesActions';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import { CheckInputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './DeleteSeriesModalContent.css';

interface DeleteSeriesModalContentProps {
  seriesIds: number[];
  onModalClose(): void;
}

const selectDeleteOptions = createSelector(
  (state: AppState) => state.series.deleteOptions,
  (deleteOptions) => deleteOptions
);

function DeleteSeriesModalContent(props: DeleteSeriesModalContentProps) {
  const { seriesIds, onModalClose } = props;

  const { addImportListExclusion } = useSelector(selectDeleteOptions);
  const allSeries: Series[] = useSelector(createAllSeriesSelector());
  const dispatch = useDispatch();

  const [deleteFiles, setDeleteFiles] = useState(false);

  const series = useMemo((): Series[] => {
    const seriesList = seriesIds.map((id) => {
      return allSeries.find((s) => s.id === id);
    }) as Series[];

    return orderBy(seriesList, ['sortTitle']);
  }, [seriesIds, allSeries]);

  const onDeleteFilesChange = useCallback(
    ({ value }: CheckInputChanged) => {
      setDeleteFiles(value);
    },
    [setDeleteFiles]
  );

  const onDeleteOptionChange = useCallback(
    ({ name, value }: { name: string; value: boolean }) => {
      dispatch(
        setDeleteOption({
          [name]: value,
        })
      );
    },
    [dispatch]
  );

  const onDeleteSeriesConfirmed = useCallback(() => {
    setDeleteFiles(false);

    dispatch(
      bulkDeleteSeries({
        seriesIds,
        deleteFiles,
        addImportListExclusion,
      })
    );

    onModalClose();
  }, [
    seriesIds,
    deleteFiles,
    addImportListExclusion,
    setDeleteFiles,
    dispatch,
    onModalClose,
  ]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('DeleteSelectedSeries')}</ModalHeader>

      <ModalBody>
        <div>
          <FormGroup>
            <FormLabel>{translate('AddListExclusion')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="addImportListExclusion"
              value={addImportListExclusion}
              helpText={translate('AddListExclusionSeriesHelpText')}
              onChange={onDeleteOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>
              {series.length > 1
                ? translate('DeleteSeriesFolders')
                : translate('DeleteSeriesFolder')}
            </FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="deleteFiles"
              value={deleteFiles}
              helpText={
                series.length > 1
                  ? translate('DeleteSeriesFoldersHelpText')
                  : translate('DeleteSeriesFolderHelpText')
              }
              kind={kinds.DANGER}
              onChange={onDeleteFilesChange}
            />
          </FormGroup>
        </div>

        <div className={styles.message}>
          {deleteFiles
            ? translate('DeleteSeriesFolderCountWithFilesConfirmation', {
                count: series.length,
              })
            : translate('DeleteSeriesFolderCountConfirmation', {
                count: series.length,
              })}
        </div>

        <ul>
          {series.map((s) => {
            return (
              <li key={s.title}>
                <span>{s.title}</span>

                {deleteFiles && (
                  <span className={styles.pathContainer}>
                    -<span className={styles.path}>{s.path}</span>
                  </span>
                )}
              </li>
            );
          })}
        </ul>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <Button kind={kinds.DANGER} onPress={onDeleteSeriesConfirmed}>
          {translate('Delete')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default DeleteSeriesModalContent;
