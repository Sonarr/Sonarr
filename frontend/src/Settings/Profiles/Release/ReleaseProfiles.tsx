import React from 'react';
import PageSectionContent from 'Components/Page/PageSectionContent';
import AddCard from 'Components/SettingsCard/AddCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { useIndexersData } from 'Settings/Indexers/useIndexers';
import { useTagList } from 'Tags/useTags';
import translate from 'Utilities/String/translate';
import EditReleaseProfileModal from './EditReleaseProfileModal';
import ReleaseProfileItem from './ReleaseProfileItem';
import { useReleaseProfiles } from './useReleaseProfiles';

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
      <div className={settingsCardStyles.grid}>
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

        <AddCard
          label={translate('AddReleaseProfile')}
          onPress={setAddReleaseProfileModalOpen}
        />
      </div>

      <EditReleaseProfileModal
        isOpen={isAddReleaseProfileModalOpen}
        onModalClose={setAddReleaseProfileModalClosed}
      />
    </PageSectionContent>
  );
}

export default ReleaseProfiles;
