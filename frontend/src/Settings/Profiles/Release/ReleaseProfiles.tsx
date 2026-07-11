import React from 'react';
import Card from 'Components/Card';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons } from 'Helpers/Props';
import { useIndexersData } from 'Settings/Indexers/useIndexers';
import { useTagList } from 'Tags/useTags';
import translate from 'Utilities/String/translate';
import EditReleaseProfileModal from './EditReleaseProfileModal';
import ReleaseProfileItem from './ReleaseProfileItem';
import { useReleaseProfiles } from './useReleaseProfiles';
import styles from './ReleaseProfiles.css';

function ReleaseProfiles() {
  const { data, isFetching, isFetched, error } = useReleaseProfiles();

  const tagList = useTagList();
  const indexerList = useIndexersData();

  const [
    isAddReleaseProfileModalOpen,
    setAddReleaseProfileModalOpen,
    setAddReleaseProfileModalClosed,
  ] = useModalOpenState(false);

  return (
    <PageSectionContent
      errorMessage={translate('ReleaseProfilesLoadError')}
      isFetching={isFetching}
      isPopulated={isFetched}
      error={error}
    >
      <div className={styles.releaseProfiles}>
        {data.map((item) => {
          return (
            <ReleaseProfileItem
              key={item.id}
              tagList={tagList}
              indexerList={indexerList}
              {...item}
            />
          );
        })}

        <Card
          className={styles.addReleaseProfile}
          aria-label={translate('AddReleaseProfile')}
          onPress={setAddReleaseProfileModalOpen}
        >
          <div className={styles.center}>
            <Icon name={icons.ADD} size={20} />
          </div>
          <div className={styles.addLabel}>
            {translate('AddReleaseProfile')}
          </div>
        </Card>
      </div>

      <EditReleaseProfileModal
        isOpen={isAddReleaseProfileModalOpen}
        onModalClose={setAddReleaseProfileModalClosed}
      />
    </PageSectionContent>
  );
}

export default ReleaseProfiles;
