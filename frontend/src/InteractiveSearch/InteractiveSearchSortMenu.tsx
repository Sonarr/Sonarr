import React, { useCallback } from 'react';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import PageMenuButton from 'Components/Menu/PageMenuButton';
import SortMenuItem from 'Components/Menu/SortMenuItem';
import { align, icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import {
  FIXED_SORT_DIRECTIONS,
  setReleaseSort,
  useReleaseSort,
} from './useReleases';

const SORT_OPTIONS = [
  { name: 'releaseWeight', label: () => translate('Preferred') },
  { name: 'title', label: () => translate('Title') },
  { name: 'qualityWeight', label: () => translate('Quality') },
  { name: 'customFormatScore', label: () => translate('CustomFormatScore') },
  { name: 'languages', label: () => translate('Languages') },
  { name: 'size', label: () => translate('Size') },
  { name: 'age', label: () => translate('Age') },
  { name: 'peers', label: () => translate('Peers') },
  { name: 'protocol', label: () => translate('Source') },
  { name: 'indexer', label: () => translate('Indexer') },
  { name: 'indexerFlags', label: () => translate('IndexerFlags') },
  { name: 'history', label: () => translate('History') },
  { name: 'rejections', label: () => translate('Rejections') },
];

function InteractiveSearchSortMenu() {
  const { sortKey, sortDirection } = useReleaseSort();

  const handleSortSelect = useCallback((key: string) => {
    setReleaseSort(key, FIXED_SORT_DIRECTIONS[key]);
  }, []);

  return (
    <Menu alignMenu={align.RIGHT}>
      <PageMenuButton
        iconName={icons.SORT}
        showIndicator={sortKey !== 'releaseWeight'}
        text={translate('Sort')}
      />

      <MenuContent>
        {SORT_OPTIONS.map(({ name, label }) => (
          <SortMenuItem
            key={name}
            name={name}
            sortKey={sortKey}
            sortDirection={sortDirection}
            onPress={handleSortSelect}
          >
            {label()}
          </SortMenuItem>
        ))}
      </MenuContent>
    </Menu>
  );
}

export default InteractiveSearchSortMenu;
