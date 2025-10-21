import React, { useCallback, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import * as commandNames from 'Commands/commandNames';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import TextInput from 'Components/Form/TextInput';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import { icons, kinds, sizes } from 'Helpers/Props';
import InteractiveSearch from 'InteractiveSearch/InteractiveSearch';
import { executeCommand } from 'Store/Actions/commandActions';
import translate from 'Utilities/String/translate';
import styles from './EpisodeSearch.css';

interface EpisodeSearchProps {
  episodeId: number;
  startInteractiveSearch: boolean;
  onModalClose: () => void;
}

function EpisodeSearch({
  episodeId,
  startInteractiveSearch,
  onModalClose,
}: EpisodeSearchProps) {
  const dispatch = useDispatch();
  const { isPopulated } = useSelector((state: AppState) => state.releases);

  const [isInteractiveSearchOpen, setIsInteractiveSearchOpen] = useState(
    startInteractiveSearch || isPopulated
  );
  const [isManualSearchOpen, setIsManualSearchOpen] = useState(false);
  const [manualSearchQuery, setManualSearchQuery] = useState('');

  const handleQuickSearchPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: commandNames.EPISODE_SEARCH,
        episodeIds: [episodeId],
      })
    );

    onModalClose();
  }, [episodeId, dispatch, onModalClose]);

  const handleInteractiveSearchPress = useCallback(() => {
    setIsInteractiveSearchOpen(true);
  }, []);

  const handleManualSearchPress = useCallback(() => {
    setIsManualSearchOpen(true);
  }, []);

  const handleManualSearchQueryChange = useCallback(
    ({ value }: { value: string }) => {
      setManualSearchQuery(value);
    },
    []
  );

  const handleManualSearchExecute = useCallback(() => {
    if (manualSearchQuery.trim()) {
      setIsInteractiveSearchOpen(true);
    }
  }, [manualSearchQuery]);

  if (isInteractiveSearchOpen) {
    const searchPayload = manualSearchQuery.trim()
      ? { episodeId, searchQuery: manualSearchQuery }
      : { episodeId };
    
    return <InteractiveSearch type="episode" searchPayload={searchPayload} />;
  }

  if (isManualSearchOpen) {
    return (
      <div>
        <div className={styles.manualSearchContainer}>
          <FormGroup>
            <FormLabel>{translate('SearchQuery')}</FormLabel>
            <TextInput
              name="manualSearchQuery"
              value={manualSearchQuery}
              placeholder={translate('EnterSearchQuery')}
              autoFocus={true}
              onChange={handleManualSearchQueryChange}
            />
          </FormGroup>
          <div className={styles.manualSearchHelp}>
            {translate('ManualSearchHelpText')}
          </div>
        </div>

        <div className={styles.buttonContainer}>
          <Button
            className={styles.button}
            kind={kinds.PRIMARY}
            size={sizes.LARGE}
            isDisabled={!manualSearchQuery.trim()}
            onPress={handleManualSearchExecute}
          >
            <Icon className={styles.buttonIcon} name={icons.SEARCH} />

            {translate('Search')}
          </Button>
        </div>
      </div>
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
          onPress={handleInteractiveSearchPress}
        >
          <Icon className={styles.buttonIcon} name={icons.INTERACTIVE} />

          {translate('InteractiveSearch')}
        </Button>
      </div>

      <div className={styles.buttonContainer}>
        <Button
          className={styles.button}
          size={sizes.LARGE}
          onPress={handleManualSearchPress}
        >
          <Icon className={styles.buttonIcon} name={icons.EDIT} />

          {translate('ManualSearch')}
        </Button>
      </div>
    </div>
  );
}

export default EpisodeSearch;
