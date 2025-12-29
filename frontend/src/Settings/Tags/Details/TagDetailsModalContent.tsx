import React, { useMemo } from 'react';
import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import ModelBase from 'App/ModelBase';
import AppState from 'App/State/AppState';
import FieldSet from 'Components/FieldSet';
import Label from 'Components/Label';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import useSeries from 'Series/useSeries';
import { useReleaseProfilesWithIds } from 'Settings/Profiles/Release/useReleaseProfiles';
import translate from 'Utilities/String/translate';
import TagDetailsDelayProfile from './TagDetailsDelayProfile';
import styles from './TagDetailsModalContent.css';

function findMatchingItems<T extends ModelBase>(ids: number[], items: T[]) {
  return items.filter((s) => {
    return ids.includes(s.id);
  });
}

function useMatchingSeries(seriesIds: number[]) {
  const { data: allSeries = [] } = useSeries();

  return useMemo(() => {
    const matchingSeries = findMatchingItems(seriesIds, allSeries);

    return matchingSeries.sort((seriesA, seriesB) => {
      const sortTitleA = seriesA.sortTitle;
      const sortTitleB = seriesB.sortTitle;

      if (sortTitleA > sortTitleB) {
        return 1;
      } else if (sortTitleA < sortTitleB) {
        return -1;
      }

      return 0;
    });
  }, [seriesIds, allSeries]);
}

function createMatchingItemSelector<T extends ModelBase>(
  ids: number[],
  selector: (state: AppState) => T[]
) {
  return createSelector(selector, (items) => findMatchingItems<T>(ids, items));
}

export interface TagDetailsModalContentProps {
  label: string;
  isTagUsed: boolean;
  delayProfileIds: number[];
  importListIds: number[];
  notificationIds: number[];
  releaseProfileIds: number[];
  indexerIds: number[];
  downloadClientIds: number[];
  autoTagIds: number[];
  seriesIds: number[];
  onModalClose: () => void;
  onDeleteTagPress: () => void;
}

function TagDetailsModalContent({
  label,
  isTagUsed,
  delayProfileIds = [],
  importListIds = [],
  notificationIds = [],
  releaseProfileIds = [],
  indexerIds = [],
  downloadClientIds = [],
  autoTagIds = [],
  seriesIds = [],
  onModalClose,
  onDeleteTagPress,
}: TagDetailsModalContentProps) {
  const series = useMatchingSeries(seriesIds);

  const delayProfiles = useSelector(
    createMatchingItemSelector(
      delayProfileIds,
      (state: AppState) => state.settings.delayProfiles.items
    )
  );

  const importLists = useSelector(
    createMatchingItemSelector(
      importListIds,
      (state: AppState) => state.settings.importLists.items
    )
  );

  const notifications = useSelector(
    createMatchingItemSelector(
      notificationIds,
      (state: AppState) => state.settings.notifications.items
    )
  );

  const releaseProfiles = useReleaseProfilesWithIds(releaseProfileIds);

  const indexers = useSelector(
    createMatchingItemSelector(
      indexerIds,
      (state: AppState) => state.settings.indexers.items
    )
  );

  const downloadClients = useSelector(
    createMatchingItemSelector(
      downloadClientIds,
      (state: AppState) => state.settings.downloadClients.items
    )
  );

  const autoTags = useSelector(
    createMatchingItemSelector(
      autoTagIds,
      (state: AppState) => state.settings.autoTaggings.items
    )
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('TagDetails', { label })}</ModalHeader>

      <ModalBody>
        {!isTagUsed && <div>{translate('TagIsNotUsedAndCanBeDeleted')}</div>}

        {series.length ? (
          <FieldSet legend={translate('Series')}>
            {series.map((item) => {
              return <div key={item.id}>{item.title}</div>;
            })}
          </FieldSet>
        ) : null}

        {delayProfiles.length ? (
          <FieldSet legend={translate('DelayProfile')}>
            {delayProfiles.map((item) => {
              const {
                id,
                preferredProtocol,
                enableUsenet,
                enableTorrent,
                usenetDelay,
                torrentDelay,
              } = item;

              return (
                <TagDetailsDelayProfile
                  key={id}
                  preferredProtocol={preferredProtocol}
                  enableUsenet={enableUsenet}
                  enableTorrent={enableTorrent}
                  usenetDelay={usenetDelay}
                  torrentDelay={torrentDelay}
                />
              );
            })}
          </FieldSet>
        ) : null}

        {notifications.length ? (
          <FieldSet legend={translate('Connections')}>
            {notifications.map((item) => {
              return <div key={item.id}>{item.name}</div>;
            })}
          </FieldSet>
        ) : null}

        {importLists.length ? (
          <FieldSet legend={translate('ImportLists')}>
            {importLists.map((item) => {
              return <div key={item.id}>{item.name}</div>;
            })}
          </FieldSet>
        ) : null}

        {releaseProfiles.length ? (
          <FieldSet legend={translate('ReleaseProfiles')}>
            {releaseProfiles.map((item) => {
              return (
                <div key={item.id} className={styles.restriction}>
                  <div>
                    {item.required.map((r) => {
                      return (
                        <Label key={r} kind={kinds.SUCCESS}>
                          {r}
                        </Label>
                      );
                    })}
                  </div>

                  <div>
                    {item.ignored.map((i) => {
                      return (
                        <Label key={i} kind={kinds.DANGER}>
                          {i}
                        </Label>
                      );
                    })}
                  </div>
                </div>
              );
            })}
          </FieldSet>
        ) : null}

        {indexers.length ? (
          <FieldSet legend={translate('Indexers')}>
            {indexers.map((item) => {
              return <div key={item.id}>{item.name}</div>;
            })}
          </FieldSet>
        ) : null}

        {downloadClients.length ? (
          <FieldSet legend={translate('DownloadClients')}>
            {downloadClients.map((item) => {
              return <div key={item.id}>{item.name}</div>;
            })}
          </FieldSet>
        ) : null}

        {autoTags.length ? (
          <FieldSet legend={translate('AutoTagging')}>
            {autoTags.map((item) => {
              return <div key={item.id}>{item.name}</div>;
            })}
          </FieldSet>
        ) : null}
      </ModalBody>

      <ModalFooter>
        <Button
          className={styles.deleteButton}
          kind={kinds.DANGER}
          title={
            isTagUsed ? translate('TagCannotBeDeletedWhileInUse') : undefined
          }
          isDisabled={isTagUsed}
          onPress={onDeleteTagPress}
        >
          {translate('Delete')}
        </Button>

        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default TagDetailsModalContent;
