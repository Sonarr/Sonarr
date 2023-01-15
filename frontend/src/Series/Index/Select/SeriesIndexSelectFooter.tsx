import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import { SelectActionType, useSelect } from 'App/SelectContext';
import { RENAME_SERIES } from 'Commands/commandNames';
import SpinnerButton from 'Components/Link/SpinnerButton';
import PageContentFooter from 'Components/Page/PageContentFooter';
import { kinds } from 'Helpers/Props';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import { saveSeriesEditor } from 'Store/Actions/seriesActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import DeleteSeriesModal from './Delete/DeleteSeriesModal';
import EditSeriesModal from './Edit/EditSeriesModal';
import OrganizeSeriesModal from './Organize/OrganizeSeriesModal';
import TagsModal from './Tags/TagsModal';
import styles from './SeriesIndexSelectFooter.css';

const seriesEditorSelector = createSelector(
  (state) => state.series,
  (series) => {
    const { isSaving, isDeleting, deleteError } = series;
    
    return {
      isSaving,
      isDeleting,
      deleteError
    }
  }
);

function SeriesIndexSelectFooter() {
  const { isSaving, isDeleting, deleteError } =
    useSelector(seriesEditorSelector);

  const isOrganizingSeries = useSelector(
    createCommandExecutingSelector(RENAME_SERIES)
  );

  const dispatch = useDispatch();

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isOrganizeModalOpen, setIsOrganizeModalOpen] = useState(false);
  const [isTagsModalOpen, setIsTagsModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isSavingSeries, setIsSavingSeries] = useState(false);
  const [isSavingTags, setIsSavingTags] = useState(false);

  const [selectState, selectDispatch] = useSelect();
  const { selectedState } = selectState;

  const seriesIds = useMemo(() => {
    return getSelectedIds(selectedState);
  }, [selectedState]);

  const selectedCount = seriesIds.length;

  const onEditPress = useCallback(() => {
    setIsEditModalOpen(true);
  }, [setIsEditModalOpen]);

  const onEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
  }, [setIsEditModalOpen]);

  const onSavePress = useCallback(
    (payload) => {
      setIsSavingSeries(true);
      setIsEditModalOpen(false);

      dispatch(
        saveSeriesEditor({
          ...payload,
          seriesIds,
        })
      );
    },
    [seriesIds, dispatch]
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
    (tags, applyTags) => {
      setIsSavingTags(true);
      setIsTagsModalOpen(false);

      dispatch(
        saveSeriesEditor({
          seriesIds,
          tags,
          applyTags,
        })
      );
    },
    [seriesIds, dispatch]
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
    }
  }, [isSaving]);

  useEffect(() => {
    if (!isDeleting && !deleteError) {
      selectDispatch({ type: SelectActionType.UnselectAll });
    }
  }, [isDeleting, deleteError, selectDispatch]);

  useEffect(() => {
    dispatch(fetchRootFolders());
  }, [dispatch]);

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
            {translate('Rename Files')}
          </SpinnerButton>

          <SpinnerButton
            isSpinning={isSaving && isSavingTags}
            isDisabled={!anySelected || isOrganizingSeries}
            onPress={onTagsPress}
          >
            {translate('Set Tags')}
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
        {translate('{count} series selected', { count: selectedCount })}
      </div>

      <EditSeriesModal
        isOpen={isEditModalOpen}
        seriesIds={seriesIds}
        onSavePress={onSavePress}
        onModalClose={onEditModalClose}
      />

      <TagsModal
        isOpen={isTagsModalOpen}
        seriesIds={seriesIds}
        onApplyTagsPress={onApplyTagsPress}
        onModalClose={onTagsModalClose}
      />

      <OrganizeSeriesModal
        isOpen={isOrganizeModalOpen}
        seriesIds={seriesIds}
        onModalClose={onOrganizeModalClose}
      />

      <DeleteSeriesModal
        isOpen={isDeleteModalOpen}
        seriesIds={seriesIds}
        onModalClose={onDeleteModalClose}
      />
    </PageContentFooter>
  );
}

export default SeriesIndexSelectFooter;
