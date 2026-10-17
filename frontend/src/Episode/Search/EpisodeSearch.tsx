import React, { RefObject, useCallback, useMemo } from 'react';
import CommandNames from 'Commands/CommandNames';
import { useExecuteCommand } from 'Commands/useCommands';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import { icons, kinds, sizes } from 'Helpers/Props';
import InteractiveSearch from 'InteractiveSearch/InteractiveSearch';
import translate from 'Utilities/String/translate';
import styles from './EpisodeSearch.module.css';

interface EpisodeSearchProps {
  episodeId: number;
  isInteractiveSearchOpen: boolean;
  filter: string;
  scrollerRef: RefObject<HTMLDivElement>;
  onInteractiveSearchPress: () => void;
  onModalClose: () => void;
}

function EpisodeSearch({
  episodeId,
  isInteractiveSearchOpen,
  filter,
  scrollerRef,
  onInteractiveSearchPress,
  onModalClose,
}: EpisodeSearchProps) {
  const executeCommand = useExecuteCommand();
  const searchPayload = useMemo(() => ({ episodeId }), [episodeId]);

  const handleQuickSearchPress = useCallback(() => {
    executeCommand({
      name: CommandNames.EpisodeSearch,
      episodeIds: [episodeId],
    });

    onModalClose();
  }, [episodeId, executeCommand, onModalClose]);

  if (isInteractiveSearchOpen) {
    return (
      <InteractiveSearch
        searchPayload={searchPayload}
        scrollerRef={scrollerRef}
        filter={filter}
      />
    );
  }

  return (
    <div>
      <div className={styles.buttonContainer}>
        <Button
          className={styles.button}
          size={sizes.LARGE}
          onPress={handleQuickSearchPress}
        >
          <Icon className={styles.buttonIcon} name={icons.QUICK} />

          {translate('QuickSearch')}
        </Button>
      </div>

      <div className={styles.buttonContainer}>
        <Button
          className={styles.button}
          kind={kinds.PRIMARY}
          size={sizes.LARGE}
          onPress={onInteractiveSearchPress}
        >
          <Icon className={styles.buttonIcon} name={icons.INTERACTIVE} />

          {translate('InteractiveSearch')}
        </Button>
      </div>
    </div>
  );
}

export default EpisodeSearch;
