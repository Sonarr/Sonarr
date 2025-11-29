import React, { useCallback, useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import { useSelect } from 'App/Select/SelectContext';
import { RENAME_SERIES } from 'Commands/commandNames';
import SpinnerButton from 'Components/Link/SpinnerButton';
import PageContentFooter from 'Components/Page/PageContentFooter';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { kinds } from 'Helpers/Props';
import Series from 'Series/Series';
import {
  useBulkDeleteSeries,
  useSaveSeriesEditor,
  useUpdateSeriesMonitor,
} from 'Series/useSeries';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import translate from 'Utilities/String/translate';
import DeleteSeriesModal from './Delete/DeleteSeriesModal';
import EditSeriesModal from './Edit/EditSeriesModal';
import OrganizeSeriesModal from './Organize/OrganizeSeriesModal';
import ChangeMonitoringModal from './SeasonPass/ChangeMonitoringModal';
import TagsModal from './Tags/TagsModal';
import styles from './SeriesIndexSelectFooter.css';

interface SavePayload {
  monitored?: boolean;
  qualityProfileId?: number;
  seriesType?: string;
  seasonFolder?: boolean;
  rootFolderPath?: string;
  moveFiles?: boolean;
}

function SeriesIndexSelectFooter() {
  const { saveSeriesEditor, isSavingSeriesEditor } = useSaveSeriesEditor();
  const { updateSeriesMonitor, isUpdatingSeriesMonitor } =
    useUpdateSeriesMonitor();
  const { isBulkDeleting, bulkDeleteError } = useBulkDeleteSeries();

  const isOrganizingSeries = useSelector(
    createCommandExecutingSelector(RENAME_SERIES)
  );

  const isSaving = isSavingSeriesEditor || isUpdatingSeriesMonitor;
  const isDeleting = isBulkDeleting;
  const deleteError = bulkDeleteError;

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isOrganizeModalOpen, setIsOrganizeModalOpen] = useState(false);
  const [isTagsModalOpen, setIsTagsModalOpen] = useState(false);
  const [isMonitoringModalOpen, setIsMonitoringModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isSavingSeries, setIsSavingSeries] = useState(false);
  const [isSavingTags, setIsSavingTags] = useState(false);
  const [isSavingMonitoring, setIsSavingMonitoring] = useState(false);
  const previousIsDeleting = usePrevious(isDeleting);
  const { selectedCount, unselectAll, useSelectedIds } = useSelect<Series>();
  const seriesIds = useSelectedIds();

  const onEditPress = useCallback(() => {
    setIsEditModalOpen(true);
  }, [setIsEditModalOpen]);

  const onEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
  }, [setIsEditModalOpen]);

  const onSavePress = useCallback(
    (payload: SavePayload) => {
      setIsSavingSeries(true);
      setIsEditModalOpen(false);

      saveSeriesEditor({
        ...payload,
        seriesIds,
      });
    },
    [seriesIds, saveSeriesEditor]
  );

  const onOrganizePress = useCallback(() => {
    setIsOrganizeModalOpen(true);
  }, [setIsOrganizeModalOpen]);

  const onOrganizeModalClose = useCallback(() => {
    setIsOrganizeModalOpen(false);
  }, [setIsOrganizeModalOpen]);

  const onTagsPress = useCallback(() => {
    setIsTagsModalOpen(true);
  }, [setIsTagsModalOpen]);

  const onTagsModalClose = useCallback(() => {
    setIsTagsModalOpen(false);
  }, [setIsTagsModalOpen]);

  const onApplyTagsPress = useCallback(
    (tags: number[], _applyTags: string) => {
      setIsSavingTags(true);
      setIsTagsModalOpen(false);

      saveSeriesEditor({
        seriesIds,
        tags,
      });
    },
    [seriesIds, saveSeriesEditor]
  );

  const onMonitoringPress = useCallback(() => {
    setIsMonitoringModalOpen(true);
  }, [setIsMonitoringModalOpen]);

  const onMonitoringClose = useCallback(() => {
    setIsMonitoringModalOpen(false);
  }, [setIsMonitoringModalOpen]);

  const onMonitoringSavePress = useCallback(
    (monitor: string) => {
      setIsSavingMonitoring(true);
      setIsMonitoringModalOpen(false);

      updateSeriesMonitor({
        series: seriesIds.map((id) => ({ id })),
        monitoringOptions: { monitor },
      });
    },
    [seriesIds, updateSeriesMonitor]
  );

  const onDeletePress = useCallback(() => {
    setIsDeleteModalOpen(true);
  }, [setIsDeleteModalOpen]);

  const onDeleteModalClose = useCallback(() => {
    setIsDeleteModalOpen(false);
  }, []);

  useEffect(() => {
    if (!isSaving) {
      setIsSavingSeries(false);
      setIsSavingTags(false);
      setIsSavingMonitoring(false);
    }
  }, [isSaving]);

  useEffect(() => {
    if (previousIsDeleting && !isDeleting && !deleteError) {
      unselectAll();
    }
  }, [previousIsDeleting, isDeleting, deleteError, unselectAll]);

  const anySelected = selectedCount > 0;

  return (
    <PageContentFooter className={styles.footer}>
      <div className={styles.buttons}>
        <div className={styles.actionButtons}>
          <SpinnerButton
            isSpinning={isSaving && isSavingSeries}
            isDisabled={!anySelected || isOrganizingSeries}
            onPress={onEditPress}
          >
            {translate('Edit')}
          </SpinnerButton>

          <SpinnerButton
            kind={kinds.WARNING}
            isSpinning={isOrganizingSeries}
            isDisabled={!anySelected || isOrganizingSeries}
            onPress={onOrganizePress}
          >
            {translate('RenameFiles')}
          </SpinnerButton>

          <SpinnerButton
            isSpinning={isSaving && isSavingTags}
            isDisabled={!anySelected || isOrganizingSeries}
            onPress={onTagsPress}
          >
            {translate('SetTags')}
          </SpinnerButton>

          <SpinnerButton
            isSpinning={isSaving && isSavingMonitoring}
            isDisabled={!anySelected || isOrganizingSeries}
            onPress={onMonitoringPress}
          >
            {translate('UpdateMonitoring')}
          </SpinnerButton>
        </div>

        <div className={styles.deleteButtons}>
          <SpinnerButton
            kind={kinds.DANGER}
            isSpinning={isDeleting}
            isDisabled={!anySelected || isDeleting}
            onPress={onDeletePress}
          >
            {translate('Delete')}
          </SpinnerButton>
        </div>
      </div>

      <div className={styles.selected}>
        {translate('CountSeriesSelected', { count: selectedCount })}
      </div>

      <EditSeriesModal
        isOpen={isEditModalOpen}
        onSavePress={onSavePress}
        onModalClose={onEditModalClose}
      />

      <TagsModal
        isOpen={isTagsModalOpen}
        onApplyTagsPress={onApplyTagsPress}
        onModalClose={onTagsModalClose}
      />

      <ChangeMonitoringModal
        isOpen={isMonitoringModalOpen}
        onSavePress={onMonitoringSavePress}
        onModalClose={onMonitoringClose}
      />

      <OrganizeSeriesModal
        isOpen={isOrganizeModalOpen}
        onModalClose={onOrganizeModalClose}
      />

      <DeleteSeriesModal
        isOpen={isDeleteModalOpen}
        onModalClose={onDeleteModalClose}
      />
    </PageContentFooter>
  );
}

export default SeriesIndexSelectFooter;
