import React, { useCallback, useState } from 'react';
import PageSectionContent from 'Components/Page/PageSectionContent';
import AddCard from 'Components/SettingsCard/AddCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import { useTagList } from 'Tags/useTags';
import translate from 'Utilities/String/translate';
import AutoTagging from './AutoTagging';
import EditAutoTaggingModal from './EditAutoTaggingModal';
import { useSortedAutoTaggings } from './useAutoTaggings';

export default function AutoTaggings() {
  const {
    data: items,
    error,
    isFetching,
    isFetched: isPopulated,
  } = useSortedAutoTaggings();

  const tagList = useTagList();
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [cloneId, setCloneId] = useState<number>();

  const onClonePress = useCallback((id: number) => {
    setCloneId(id);
    setIsEditModalOpen(true);
  }, []);

  const onEditPress = useCallback(() => {
    setCloneId(undefined);
    setIsEditModalOpen(true);
  }, []);

  const onEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
    setCloneId(undefined);
  }, []);

  return (
    <PageSectionContent
      errorMessage={translate('AutoTaggingLoadError')}
      error={error}
      isFetching={isFetching}
      isPopulated={isPopulated}
    >
      <div className={settingsCardStyles.grid}>
        {items.map((item) => {
          return (
            <AutoTagging
              key={item.id}
              {...item}
              tagList={tagList}
              onCloneAutoTaggingPress={onClonePress}
            />
          );
        })}

        <AddCard label={translate('AddAutoTag')} onPress={onEditPress} />
      </div>

      <EditAutoTaggingModal
        isOpen={isEditModalOpen}
        cloneId={cloneId}
        onModalClose={onEditModalClose}
      />
    </PageSectionContent>
  );
}
