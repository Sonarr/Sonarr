import React, { useMemo } from 'react';
import ModelBase from 'App/ModelBase';
import Label from 'Components/Label';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import useSeries from 'Series/useSeries';
import { useDownloadClientsWithIds } from 'Settings/DownloadClients/DownloadClients/useDownloadClients';
import { useImportListsWithIds } from 'Settings/ImportLists/ImportLists/useImportLists';
import { useIndexersWithIds } from 'Settings/Indexers/useIndexers';
import { useConnectionsWithIds } from 'Settings/Notifications/useConnections';
import { useDelayProfilesWithIds } from 'Settings/Profiles/Delay/useDelayProfiles';
import { useReleaseProfilesWithIds } from 'Settings/Profiles/Release/useReleaseProfiles';
import { useAutoTaggingsWithIds } from 'Settings/Tags/AutoTagging/useAutoTaggings';
import translate from 'Utilities/String/translate';
import TagDetailsDelayProfile from './TagDetailsDelayProfile';
import styles from './TagDetailsModalContent.css';

function findMatchingItems<T extends ModelBase>(
  ids: number[],
  items: ReadonlyArray<T>
) {
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

  const delayProfiles = useDelayProfilesWithIds(delayProfileIds);

  const importLists = useImportListsWithIds(importListIds);

  const releaseProfiles = useReleaseProfilesWithIds(releaseProfileIds);
  const notifications = useConnectionsWithIds(notificationIds);
  const indexers = useIndexersWithIds(indexerIds);
  const downloadClients = useDownloadClientsWithIds(downloadClientIds);

  const autoTags = useAutoTaggingsWithIds(autoTagIds);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('TagDetails', { label })}</ModalHeader>

      <ModalBody>
        {isTagUsed ? null : (
          <div>{translate('TagIsNotUsedAndCanBeDeleted')}</div>
        )}

        {series.length ? (
          <section className={styles.section}>
            <h3 className={styles.sectionHeading}>{translate('Series')}</h3>

            {series.map((item) => {
              return <div key={item.id}>{item.title}</div>;
            })}
          </section>
        ) : null}

        {delayProfiles.length ? (
          <section className={styles.section}>
            <h3 className={styles.sectionHeading}>
              {translate('DelayProfile')}
            </h3>

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
          </section>
        ) : null}

        {notifications.length ? (
          <section className={styles.section}>
            <h3 className={styles.sectionHeading}>
              {translate('Connections')}
            </h3>

            {notifications.map((item) => {
              return <div key={item.id}>{item.name}</div>;
            })}
          </section>
        ) : null}

        {importLists.length ? (
          <section className={styles.section}>
            <h3 className={styles.sectionHeading}>
              {translate('ImportLists')}
            </h3>

            {importLists.map((item) => {
              return <div key={item.id}>{item.name}</div>;
            })}
          </section>
        ) : null}

        {releaseProfiles.length ? (
          <section className={styles.section}>
            <h3 className={styles.sectionHeading}>
              {translate('ReleaseProfiles')}
            </h3>

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
          </section>
        ) : null}

        {indexers.length ? (
          <section className={styles.section}>
            <h3 className={styles.sectionHeading}>{translate('Indexers')}</h3>

            {indexers.map((item) => {
              return <div key={item.id}>{item.name}</div>;
            })}
          </section>
        ) : null}

        {downloadClients.length ? (
          <section className={styles.section}>
            <h3 className={styles.sectionHeading}>
              {translate('DownloadClients')}
            </h3>

            {downloadClients.map((item) => {
              return <div key={item.id}>{item.name}</div>;
            })}
          </section>
        ) : null}

        {autoTags.length ? (
          <section className={styles.section}>
            <h3 className={styles.sectionHeading}>
              {translate('AutoTagging')}
            </h3>

            {autoTags.map((item) => {
              return <div key={item.id}>{item.name}</div>;
            })}
          </section>
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
