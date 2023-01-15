import { orderBy } from 'lodash';
import React, { useCallback, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import { bulkDeleteSeries, setDeleteOption } from 'Store/Actions/seriesActions';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import styles from './DeleteSeriesModalContent.css';

interface DeleteSeriesModalContentProps {
  seriesIds: number[];
  onModalClose(): void;
}

const selectDeleteOptions = createSelector(
  (state) => state.series.deleteOptions,
  (deleteOptions) => deleteOptions
);

function DeleteSeriesModalContent(props: DeleteSeriesModalContentProps) {
  const { seriesIds, onModalClose } = props;

  const { addImportListExclusion } = useSelector(selectDeleteOptions);
  const allSeries = useSelector(createAllSeriesSelector());
  const dispatch = useDispatch();

  const [deleteFiles, setDeleteFiles] = useState(false);

  const series = useMemo(() => {
    const series = seriesIds.map((id) => {
      return allSeries.find((s) => s.id === id);
    });

    return orderBy(series, ['sortTitle']);
  }, [seriesIds, allSeries]);

  const onDeleteFilesChange = useCallback(
    ({ value }) => {
      setDeleteFiles(value);
    },
    [setDeleteFiles]
  );

  const onDeleteOptionChange = useCallback(
    ({ name, value }) => {
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
      <ModalHeader>Delete Selected Series</ModalHeader>

      <ModalBody>
        <div>
          <FormGroup>
            <FormLabel>Add List Exclusion</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="addImportListExclusion"
              value={addImportListExclusion}
              helpText="Prevent series from being added to Sonarr by lists"
              onChange={onDeleteOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{`Delete Series Folder${
              series.length > 1 ? 's' : ''
            }`}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="deleteFiles"
              value={deleteFiles}
              helpText={`Delete Series Folder${
                series.length > 1 ? 's' : ''
              } and all contents`}
              kind={kinds.DANGER}
              onChange={onDeleteFilesChange}
            />
          </FormGroup>
        </div>

        <div className={styles.message}>
          {`Are you sure you want to delete ${series.length} selected series${
            deleteFiles ? ' and all contents' : ''
          }?`}
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
        <Button onPress={onModalClose}>Cancel</Button>

        <Button kind={kinds.DANGER} onPress={onDeleteSeriesConfirmed}>
          Delete
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default DeleteSeriesModalContent;
