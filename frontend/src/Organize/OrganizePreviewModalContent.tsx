import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { SelectProvider, useSelect } from 'App/Select/SelectContext';
import AppState from 'App/State/AppState';
import CommandNames from 'Commands/CommandNames';
import { useExecuteCommand } from 'Commands/useCommands';
import Alert from 'Components/Alert';
import CheckInput from 'Components/Form/CheckInput';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import formatSeason from 'Season/formatSeason';
import { useSingleSeries } from 'Series/useSeries';
import { fetchNamingSettings } from 'Store/Actions/settingsActions';
import { CheckInputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import OrganizePreviewRow from './OrganizePreviewRow';
import useOrganizePreview, { OrganizePreviewModel } from './useOrganizePreview';
import styles from './OrganizePreviewModalContent.css';

function getValue(allSelected: boolean, allUnselected: boolean) {
  if (allSelected) {
    return true;
  } else if (allUnselected) {
    return false;
  }

  return null;
}

export interface OrganizePreviewModalContentProps {
  seriesId: number;
  seasonNumber?: number;
  onModalClose: () => void;
}

function OrganizePreviewModalContentInner({
  seriesId,
  seasonNumber,
  onModalClose,
}: OrganizePreviewModalContentProps) {
  const dispatch = useDispatch();
  const executeCommand = useExecuteCommand();
  const {
    items,
    isFetching: isPreviewFetching,
    isFetched: isPreviewFetched,
    error: previewError,
  } = useOrganizePreview(seriesId, seasonNumber);

  const {
    isFetching: isNamingFetching,
    isPopulated: isNamingPopulated,
    error: namingError,
    item: naming,
  } = useSelector((state: AppState) => state.settings.naming);

  const series = useSingleSeries(seriesId)!;

  const { allSelected, allUnselected, getSelectedIds, selectAll, unselectAll } =
    useSelect<OrganizePreviewModel>();

  const isFetching = isPreviewFetching || isNamingFetching;
  const isPopulated = isPreviewFetched && isNamingPopulated;
  const error = previewError || namingError;
  const { renameEpisodes } = naming;
  const episodeFormat = naming[`${series.seriesType}EpisodeFormat`];

  const selectAllValue = getValue(allSelected, allUnselected);

  const handleSelectAllChange = useCallback(
    ({ value }: CheckInputChanged) => {
      if (value) {
        selectAll();
      } else {
        unselectAll();
      }
    },
    [selectAll, unselectAll]
  );

  const handleOrganizePress = useCallback(() => {
    const files = getSelectedIds();

    executeCommand({
      name: CommandNames.RenameFiles,
      files,
      seriesId,
    });

    onModalClose();
  }, [seriesId, getSelectedIds, executeCommand, onModalClose]);

  useEffect(() => {
    dispatch(fetchNamingSettings());
  }, [dispatch]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {seasonNumber == null
          ? translate('OrganizeModalHeader')
          : translate('OrganizeModalHeaderSeason', {
              season: formatSeason(seasonNumber) ?? '',
            })}
      </ModalHeader>

      <ModalBody>
        {isFetching ? <LoadingIndicator /> : null}

        {!isFetching && error ? (
          <Alert kind={kinds.DANGER}>{translate('OrganizeLoadError')}</Alert>
        ) : null}

        {!isFetching && isPopulated && !items.length ? (
          <div>
            {renameEpisodes ? (
              <div>{translate('OrganizeNothingToRename')}</div>
            ) : (
              <div>{translate('OrganizeRenamingDisabled')}</div>
            )}
          </div>
        ) : null}

        {!isFetching && isPopulated && items.length ? (
          <div>
            <Alert>
              <div>
                <InlineMarkdown
                  data={translate('OrganizeRelativePaths', {
                    path: series.path,
                  })}
                  blockClassName={styles.path}
                />
              </div>

              <div>
                <InlineMarkdown
                  data={translate('OrganizeNamingPattern', { episodeFormat })}
                  blockClassName={styles.episodeFormat}
                />
              </div>
            </Alert>

            <div className={styles.previews}>
              {items.map((item) => {
                return (
                  <OrganizePreviewRow
                    key={item.episodeFileId}
                    id={item.episodeFileId}
                    existingPath={item.existingPath}
                    newPath={item.newPath}
                  />
                );
              })}
            </div>
          </div>
        ) : null}
      </ModalBody>

      <ModalFooter>
        {isPopulated && items.length ? (
          <CheckInput
            className={styles.selectAllInput}
            containerClassName={styles.selectAllInputContainer}
            name="selectAll"
            value={selectAllValue}
            onChange={handleSelectAllChange}
          />
        ) : null}

        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <Button kind={kinds.PRIMARY} onPress={handleOrganizePress}>
          {translate('Organize')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

function OrganizePreviewModalContent({
  seriesId,
  seasonNumber,
  onModalClose,
}: OrganizePreviewModalContentProps) {
  const { items } = useOrganizePreview(seriesId, seasonNumber);

  return (
    <SelectProvider<OrganizePreviewModel> items={items}>
      <OrganizePreviewModalContentInner
        seriesId={seriesId}
        seasonNumber={seasonNumber}
        onModalClose={onModalClose}
      />
    </SelectProvider>
  );
}

export default OrganizePreviewModalContent;
