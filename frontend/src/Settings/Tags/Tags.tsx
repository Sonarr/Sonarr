import { useQueryClient } from '@tanstack/react-query';
import React, { useEffect } from 'react';
import { useDispatch } from 'react-redux';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { kinds } from 'Helpers/Props';
import {
  fetchDelayProfiles,
  fetchDownloadClients,
  fetchImportLists,
  fetchIndexers,
  fetchNotifications,
} from 'Store/Actions/settingsActions';
import useTagDetails from 'Tags/useTagDetails';
import useTags, { useSortedTagList } from 'Tags/useTags';
import translate from 'Utilities/String/translate';
import Tag from './Tag';
import styles from './Tags.css';

function Tags() {
  const dispatch = useDispatch();
  const queryClient = useQueryClient();

  const { isFetching, isFetched, error } = useTags();
  const items = useSortedTagList();
  const {
    isFetching: isDetailsFetching,
    isFetched: isDetailsFetched,
    error: detailsError,
  } = useTagDetails();

  useEffect(() => {
    dispatch(fetchDelayProfiles());
    dispatch(fetchImportLists());
    dispatch(fetchNotifications());
    dispatch(fetchIndexers());
    dispatch(fetchDownloadClients());

    queryClient.invalidateQueries({ queryKey: ['releaseprofile'] });
  }, [dispatch, queryClient]);

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
