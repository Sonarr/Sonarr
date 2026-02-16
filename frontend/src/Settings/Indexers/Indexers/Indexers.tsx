import React, { useCallback, useState } from 'react';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import { SelectedSchema } from 'Settings/useProviderSchema';
import translate from 'Utilities/String/translate';
import { useSortedIndexers } from '../useIndexers';
import AddIndexerModal from './AddIndexerModal';
import EditIndexerModal from './EditIndexerModal';
import Indexer from './Indexer';
import styles from './Indexers.css';

function Indexers() {
  const { isFetching, isFetched, data, error } = useSortedIndexers();

  const [isAddIndexerModalOpen, setIsAddIndexerModalOpen] = useState(false);
  const [isEditIndexerModalOpen, setIsEditIndexerModalOpen] = useState(false);
  const [cloneIndexerId, setCloneIndexerId] = useState<number | null>(null);

  const showPriority = data.some((index) => index.priority !== 25);

  const [selectedSchema, setSelectedSchema] = useState<
    SelectedSchema | undefined
  >(undefined);

  const handleAddIndexerPress = useCallback(() => {
    setCloneIndexerId(null);
    setIsAddIndexerModalOpen(true);
  }, []);

  const handleCloneIndexerPress = useCallback((id: number) => {
    setCloneIndexerId(id);
    setIsEditIndexerModalOpen(true);
  }, []);

  const handleIndexerSelect = useCallback((selected: SelectedSchema) => {
    setSelectedSchema(selected);
    setIsAddIndexerModalOpen(false);
    setIsEditIndexerModalOpen(true);
  }, []);

  const handleAddIndexerModalClose = useCallback(() => {
    setIsAddIndexerModalOpen(false);
  }, []);

  const handleEditIndexerModalClose = useCallback(() => {
    setCloneIndexerId(null);
    setIsEditIndexerModalOpen(false);
  }, []);

  return (
    <FieldSet legend={translate('Indexers')}>
      <PageSectionContent
        errorMessage={translate('IndexersLoadError')}
        error={error}
        isFetching={isFetching}
        isPopulated={isFetched}
      >
        <div className={styles.indexers}>
          {data.map((item) => {
            return (
              <Indexer
                key={item.id}
                {...item}
                showPriority={showPriority}
                onCloneIndexerPress={handleCloneIndexerPress}
              />
            );
          })}

          <Card className={styles.addIndexer} onPress={handleAddIndexerPress}>
            <div className={styles.center}>
              <Icon name={icons.ADD} size={45} />
            </div>
          </Card>
        </div>

        <AddIndexerModal
          isOpen={isAddIndexerModalOpen}
          onIndexerSelect={handleIndexerSelect}
          onModalClose={handleAddIndexerModalClose}
        />

        <EditIndexerModal
          isOpen={isEditIndexerModalOpen}
          cloneId={cloneIndexerId ?? undefined}
          selectedSchema={selectedSchema}
          onModalClose={handleEditIndexerModalClose}
        />
      </PageSectionContent>
    </FieldSet>
  );
}

export default Indexers;
