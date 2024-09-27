import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import { ReleaseProfilesAppState } from 'App/State/SettingsAppState';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons } from 'Helpers/Props';
import { fetchIndexers } from 'Store/Actions/Settings/indexers';
import { fetchReleaseProfiles } from 'Store/Actions/Settings/releaseProfiles';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import translate from 'Utilities/String/translate';
import EditReleaseProfileModal from './EditReleaseProfileModal';
import ReleaseProfileRow from './ReleaseProfileRow';
import styles from './ReleaseProfiles.css';

function ReleaseProfiles() {
  const { items, isFetching, isPopulated, error }: ReleaseProfilesAppState =
    useSelector(createClientSideCollectionSelector('settings.releaseProfiles'));

  const tagList = useSelector(createTagsSelector());
  const indexerList = useSelector(
    (state: AppState) => state.settings.indexers.items
  );

  const dispatch = useDispatch();

  const [
    isAddReleaseProfileModalOpen,
    setAddReleaseProfileModalOpen,
    setAddReleaseProfileModalClosed,
  ] = useModalOpenState(false);

  useEffect(() => {
    dispatch(fetchReleaseProfiles());
    dispatch(fetchIndexers());
  }, [dispatch]);

  return (
    <FieldSet legend={translate('ReleaseProfiles')}>
      <PageSectionContent
        errorMessage={translate('ReleaseProfilesLoadError')}
        isFetching={isFetching}
        isPopulated={isPopulated}
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

          {items.map((item) => {
            return (
              <ReleaseProfileRow
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
