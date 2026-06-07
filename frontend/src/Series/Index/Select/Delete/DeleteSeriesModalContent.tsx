import React, { useCallback, useState } from 'react';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import {
  setSeriesDeleteOptions,
  useSeriesDeleteOptions,
} from 'Series/seriesOptionsStore';
import { useBulkDeleteSeries } from 'Series/useSeries';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import SeriesDeleteList from './SeriesDeleteList';
import useSelectedSeriesStats from './useSelectedSeriesStats';
import styles from './DeleteSeriesModalContent.css';

export interface DeleteSeriesModalContentProps {
  onModalClose(): void;
}

function DeleteSeriesModalContent({
  onModalClose,
}: DeleteSeriesModalContentProps) {
  const { addImportListExclusion } = useSeriesDeleteOptions();
  const { bulkDeleteSeries } = useBulkDeleteSeries();
  const [deleteFiles, setDeleteFiles] = useState(false);
  const { series, seriesIds, totalEpisodeFileCount, totalSizeOnDisk } =
    useSelectedSeriesStats();

  const onDeleteFilesChange = useCallback(
    ({ value }: InputChanged<boolean>) => {
      setDeleteFiles(value);
    },
    [setDeleteFiles]
  );

  const onDeleteOptionChange = useCallback(
    ({ name, value }: { name: string; value: boolean }) => {
      setSeriesDeleteOptions({
        [name]: value,
      });
    },
    []
  );

  const onDeleteSeriesConfirmed = useCallback(() => {
    setDeleteFiles(false);

    bulkDeleteSeries({
      seriesIds,
      deleteFiles,
      addImportListExclusion,
    });

    onModalClose();
  }, [
    deleteFiles,
    addImportListExclusion,
    setDeleteFiles,
    seriesIds,
    bulkDeleteSeries,
    onModalClose,
  ]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('DeleteSelectedSeries')}</ModalHeader>
      <ModalBody>
        <div>
          <FormRow>
            <FormLabel>{translate('AddListExclusion')}</FormLabel>
            <FormInputHelpText
              text={translate('AddListExclusionSeriesHelpText')}
            />
            <FormInput
              type={inputTypes.CHECK}
              name="addImportListExclusion"
              value={addImportListExclusion}
              onChange={onDeleteOptionChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>
              {series.length > 1
                ? translate('DeleteSeriesFolders')
                : translate('DeleteSeriesFolder')}
            </FormLabel>
            <FormInputHelpText
              text={
                series.length > 1
                  ? translate('DeleteSeriesFoldersHelpText')
                  : translate('DeleteSeriesFolderHelpText')
              }
            />
            <FormInput
              type={inputTypes.CHECK}
              name="deleteFiles"
              value={deleteFiles}
              kind="danger"
              onChange={onDeleteFilesChange}
            />
          </FormRow>
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

        <SeriesDeleteList
          series={series}
          showFileDetails={deleteFiles}
          totalEpisodeFileCount={totalEpisodeFileCount}
          totalSizeOnDisk={totalSizeOnDisk}
          styles={styles}
        />
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
