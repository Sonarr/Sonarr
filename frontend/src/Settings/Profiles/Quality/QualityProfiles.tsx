import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { QualityProfilesAppState } from 'App/State/SettingsAppState';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import {
  cloneQualityProfile,
  fetchQualityProfiles,
} from 'Store/Actions/settingsActions';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import QualityProfileModel from 'typings/QualityProfile';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import EditQualityProfileModal from './EditQualityProfileModal';
import QualityProfile from './QualityProfile';
import styles from './QualityProfiles.css';

function QualityProfiles() {
  const dispatch = useDispatch();

  const { error, isFetching, isPopulated, isDeleting, items } = useSelector(
    createSortedSectionSelector<QualityProfileModel, QualityProfilesAppState>(
      'settings.qualityProfiles',
      sortByProp('name')
    )
  ) as QualityProfilesAppState;

  const [isQualityProfileModalOpen, setIsQualityProfileModalOpen] =
    useState(false);

  const handleEditQualityProfilePress = useCallback(() => {
    setIsQualityProfileModalOpen(true);
  }, []);

  const handleEditQualityProfileClosePress = useCallback(() => {
    setIsQualityProfileModalOpen(false);
  }, []);

  const handleCloneQualityProfilePress = useCallback(
    (id: number) => {
      dispatch(cloneQualityProfile({ id }));
      setIsQualityProfileModalOpen(true);
    },
    [dispatch]
  );

  useEffect(() => {
    dispatch(fetchQualityProfiles());
  }, [dispatch]);

  return (
    <FieldSet legend={translate('QualityProfiles')}>
      <PageSectionContent
        errorMessage={translate('QualityProfilesLoadError')}
        error={error}
        isFetching={isFetching}
        isPopulated={isPopulated}
      >
        <div className={styles.qualityProfiles}>
          {items.map((item) => {
            return (
              <QualityProfile
                key={item.id}
                {...item}
                isDeleting={isDeleting}
                onCloneQualityProfilePress={handleCloneQualityProfilePress}
              />
            );
          })}

          <Card
            className={styles.addQualityProfile}
            onPress={handleEditQualityProfilePress}
          >
            <div className={styles.center}>
              <Icon name={icons.ADD} size={45} />
            </div>
          </Card>
        </div>

        <EditQualityProfileModal
          isOpen={isQualityProfileModalOpen}
          onModalClose={handleEditQualityProfileClosePress}
        />
      </PageSectionContent>
    </FieldSet>
  );
}

export default QualityProfiles;
