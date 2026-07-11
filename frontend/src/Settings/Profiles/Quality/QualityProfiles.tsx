import React, { useCallback, useState } from 'react';
import Card from 'Components/Card';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import EditQualityProfileModal from './EditQualityProfileModal';
import QualityProfile from './QualityProfile';
import { useQualityProfiles } from './useQualityProfiles';
import styles from './QualityProfiles.css';

function QualityProfiles() {
  const { data, error, isFetching, isFetched } = useQualityProfiles();

  // Sort the data by name
  const sortedItems = data ? [...data].sort(sortByProp('name')) : [];

  const [isQualityProfileModalOpen, setIsQualityProfileModalOpen] =
    useState(false);
  const [cloneProfileId, setCloneProfileId] = useState<number | null>(null);

  const handleAddQualityProfilePress = useCallback(() => {
    setCloneProfileId(null);
    setIsQualityProfileModalOpen(true);
  }, []);

  const handleAddQualityProfileClosePress = useCallback(() => {
    setCloneProfileId(null);
    setIsQualityProfileModalOpen(false);
  }, []);

  const handleCloneQualityProfilePress = useCallback((id: number) => {
    setCloneProfileId(id);
    setIsQualityProfileModalOpen(true);
  }, []);

  return (
    <PageSectionContent
      errorMessage={translate('QualityProfilesLoadError')}
      error={error}
      isFetching={isFetching}
      isPopulated={isFetched}
    >
      <div className={styles.qualityProfiles}>
        {sortedItems.map((item) => {
          return (
            <QualityProfile
              key={item.id}
              {...item}
              onCloneQualityProfilePress={handleCloneQualityProfilePress}
            />
          );
        })}

        <Card
          className={styles.addQualityProfile}
          aria-label={translate('AddQualityProfile')}
          onPress={handleAddQualityProfilePress}
        >
          <div className={styles.center}>
            <Icon name={icons.ADD} size={20} />
          </div>
          <div className={styles.addLabel}>
            {translate('AddQualityProfile')}
          </div>
        </Card>
      </div>

      <EditQualityProfileModal
        isOpen={isQualityProfileModalOpen}
        cloneId={cloneProfileId ?? undefined}
        onModalClose={handleAddQualityProfileClosePress}
      />
    </PageSectionContent>
  );
}

export default QualityProfiles;
