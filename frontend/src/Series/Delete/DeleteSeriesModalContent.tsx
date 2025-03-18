import React, { useCallback, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { icons, inputTypes, kinds } from 'Helpers/Props';
import { Statistics } from 'Series/Series';
import useSeries from 'Series/useSeries';
import { deleteSeries, setDeleteOption } from 'Store/Actions/seriesActions';
import { CheckInputChanged } from 'typings/inputs';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './DeleteSeriesModalContent.css';

export interface DeleteSeriesModalContentProps {
  seriesId: number;
  onModalClose: () => void;
}

function DeleteSeriesModalContent({
  seriesId,
  onModalClose,
}: DeleteSeriesModalContentProps) {
  const dispatch = useDispatch();
  const { title, path, statistics = {} as Statistics } = useSeries(seriesId)!;
  const { addImportListExclusion } = useSelector(
    (state: AppState) => state.series.deleteOptions
  );

  const { episodeFileCount = 0, sizeOnDisk = 0 } = statistics;

  const [deleteFiles, setDeleteFiles] = useState(false);

  const handleDeleteFilesChange = useCallback(
    ({ value }: CheckInputChanged) => {
      setDeleteFiles(value);
    },
    []
  );

  const handleDeleteSeriesConfirmed = useCallback(() => {
    dispatch(
      deleteSeries({ id: seriesId, deleteFiles, addImportListExclusion })
    );

    onModalClose();
  }, [seriesId, addImportListExclusion, deleteFiles, dispatch, onModalClose]);

  const handleDeleteOptionChange = useCallback(
    ({ name, value }: CheckInputChanged) => {
      dispatch(setDeleteOption({ [name]: value }));
    },
    [dispatch]
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('DeleteSeriesModalHeader', { title })}
      </ModalHeader>

      <ModalBody>
        <div className={styles.pathContainer}>
          <Icon className={styles.pathIcon} name={icons.FOLDER} />

          {path}
        </div>

        <FormGroup>
          <FormLabel>{translate('AddListExclusion')}</FormLabel>

          <FormInputGroup
            type={inputTypes.CHECK}
            name="addImportListExclusion"
            value={addImportListExclusion}
            helpText={translate('AddListExclusionSeriesHelpText')}
            onChange={handleDeleteOptionChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>
            {episodeFileCount === 0
              ? translate('DeleteSeriesFolder')
              : translate('DeleteEpisodesFiles', { episodeFileCount })}
          </FormLabel>

          <FormInputGroup
            type={inputTypes.CHECK}
            name="deleteFiles"
            value={deleteFiles}
            helpText={
              episodeFileCount === 0
                ? translate('DeleteSeriesFolderHelpText')
                : translate('DeleteEpisodesFilesHelpText')
            }
            kind={kinds.DANGER}
            onChange={handleDeleteFilesChange}
          />
        </FormGroup>

        {deleteFiles ? (
          <div className={styles.deleteFilesMessage}>
            <div>
              <InlineMarkdown
                data={translate('DeleteSeriesFolderConfirmation', { path })}
                blockClassName={styles.folderPath}
              />
            </div>

            {episodeFileCount ? (
              <div className={styles.deleteCount}>
                {translate('DeleteSeriesFolderEpisodeCount', {
                  episodeFileCount,
                  size: formatBytes(sizeOnDisk),
                })}
              </div>
            ) : null}
          </div>
        ) : null}
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>

        <Button kind={kinds.DANGER} onPress={handleDeleteSeriesConfirmed}>
          {translate('Delete')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default DeleteSeriesModalContent;
