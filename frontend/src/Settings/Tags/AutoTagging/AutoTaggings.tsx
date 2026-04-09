import React, { useCallback, useState } from 'react';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import { useTagList } from 'Tags/useTags';
import translate from 'Utilities/String/translate';
import AutoTagging from './AutoTagging';
import EditAutoTaggingModal from './EditAutoTaggingModal';
import { useSortedAutoTaggings } from './useAutoTaggings';
import styles from './AutoTaggings.css';

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
    <FieldSet legend={translate('AutoTagging')}>
      <PageSectionContent
        errorMessage={translate('AutoTaggingLoadError')}
        error={error}
        isFetching={isFetching}
        isPopulated={isPopulated}
      >
        <div className={styles.autoTaggings}>
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

          <Card className={styles.addAutoTagging} onPress={onEditPress}>
            <div className={styles.center}>
              <Icon name={icons.ADD} size={45} />
            </div>
          </Card>
        </div>

        <EditAutoTaggingModal
          isOpen={isEditModalOpen}
          cloneId={cloneId}
          onModalClose={onEditModalClose}
        />
      </PageSectionContent>
    </FieldSet>
  );
}
