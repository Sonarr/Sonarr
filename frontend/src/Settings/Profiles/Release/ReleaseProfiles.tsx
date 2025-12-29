import React from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons } from 'Helpers/Props';
import { useTagList } from 'Tags/useTags';
import translate from 'Utilities/String/translate';
import EditReleaseProfileModal from './EditReleaseProfileModal';
import ReleaseProfileItem from './ReleaseProfileItem';
import { useReleaseProfiles } from './useReleaseProfiles';
import styles from './ReleaseProfiles.css';

function ReleaseProfiles() {
  const { data, isFetching, isFetched, error } = useReleaseProfiles();

  const tagList = useTagList();
  const indexerList = useSelector(
    (state: AppState) => state.settings.indexers.items
  );

  const [
    isAddReleaseProfileModalOpen,
    setAddReleaseProfileModalOpen,
    setAddReleaseProfileModalClosed,
  ] = useModalOpenState(false);

  return (
    <FieldSet legend={translate('ReleaseProfiles')}>
      <PageSectionContent
        errorMessage={translate('ReleaseProfilesLoadError')}
        isFetching={isFetching}
        isPopulated={isFetched}
        error={error}
      >
        <div className={styles.releaseProfiles}>
          <Card
            className={styles.addReleaseProfile}
            onPress={setAddReleaseProfileModalOpen}
          >
            <div className={styles.center}>
              <Icon name={icons.ADD} size={45} />
            </div>
          </Card>

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
        </div>

        <EditReleaseProfileModal
          isOpen={isAddReleaseProfileModalOpen}
          onModalClose={setAddReleaseProfileModalClosed}
        />
      </PageSectionContent>
    </FieldSet>
  );
}

export default ReleaseProfiles;
