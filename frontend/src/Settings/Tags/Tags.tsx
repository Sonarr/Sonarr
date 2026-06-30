import { useQueryClient } from '@tanstack/react-query';
import React, { useEffect } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { kinds } from 'Helpers/Props';
import { useDownloadClients } from 'Settings/DownloadClients/DownloadClients/useDownloadClients';
import { useImportLists } from 'Settings/ImportLists/ImportLists/useImportLists';
import { useIndexers } from 'Settings/Indexers/useIndexers';
import { useConnections } from 'Settings/Notifications/useConnections';
import { useDelayProfiles } from 'Settings/Profiles/Delay/useDelayProfiles';
import { useReleaseProfiles } from 'Settings/Profiles/Release/useReleaseProfiles';
import useTagDetails from 'Tags/useTagDetails';
import useTags, { useSortedTagList } from 'Tags/useTags';
import translate from 'Utilities/String/translate';
import Tag from './Tag';
import styles from './Tags.css';

function Tags() {
  const queryClient = useQueryClient();

  const { isFetching, isFetched, error } = useTags();
  const items = useSortedTagList();
  const {
    isFetching: isDetailsFetching,
    isFetched: isDetailsFetched,
    error: detailsError,
  } = useTagDetails();

  useDelayProfiles();
  useReleaseProfiles();
  useConnections();
  useIndexers();
  useImportLists();
  useDownloadClients();

  useEffect(() => {
    queryClient.invalidateQueries({ queryKey: ['releaseprofile'] });
  }, [queryClient]);

  if (!items.length) {
    return (
      <Alert kind={kinds.INFO}>{translate('NoTagsHaveBeenAddedYet')}</Alert>
    );
  }

  return (
    <FieldSet legend={translate('Tags')}>
      <PageSectionContent
        errorMessage={translate('TagsLoadError')}
        error={error || detailsError}
        isFetching={isFetching || isDetailsFetching}
        isPopulated={isFetched && isDetailsFetched}
      >
        <div className={styles.tags}>
          {items.map((item) => {
            return <Tag key={item.id} {...item} />;
          })}
        </div>
      </PageSectionContent>
    </FieldSet>
  );
}

export default Tags;
