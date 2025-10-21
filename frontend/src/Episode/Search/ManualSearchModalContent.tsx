import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import TextInput from 'Components/Form/TextInput';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import Episode from 'Episode/Episode';
import episodeEntities from 'Episode/episodeEntities';
import useEpisode from 'Episode/useEpisode';
import InteractiveSearch from 'InteractiveSearch/InteractiveSearch';
import Series from 'Series/Series';
import useSeries from 'Series/useSeries';
import { kinds } from 'Helpers/Props';
import {
  cancelFetchReleases,
  clearReleases,
} from 'Store/Actions/releaseActions';
import translate from 'Utilities/String/translate';
import styles from './ManualSearchModalContent.css';

interface ManualSearchModalContentProps {
  episodeId: number;
  episodeTitle: string;
  onModalClose: () => void;
}

function ManualSearchModalContent({
  episodeId,
  episodeTitle: _episodeTitle,
  onModalClose,
}: ManualSearchModalContentProps) {
  const dispatch = useDispatch();
  const [searchQuery, setSearchQuery] = useState('');
  const [isSearching, setIsSearching] = useState(false);

  const episode = useEpisode(episodeId, episodeEntities.EPISODES) as Episode;
  const series = useSeries(episode?.seriesId) as Series;

  const { seasonNumber, episodeNumber } = episode || {};
  const { title: seriesTitle } = series || {};

  const releases = useSelector((state: any) => state.releases.items);

  useEffect(() => {
    return () => {
      dispatch(cancelFetchReleases());
      dispatch(clearReleases());
    };
  }, [dispatch]);

  // Close modal when a grab succeeds (isGrabbed becomes true)
  useEffect(() => {
    if (isSearching && releases.some((release: any) => release.isGrabbed)) {
      onModalClose();
    }
  }, [releases, isSearching, onModalClose]);

  const handleSearchChange = useCallback(
    ({ value }: { value: string }) => {
      setSearchQuery(value);
    },
    []
  );

  const handleSearchPress = useCallback(() => {
    if (searchQuery.trim()) {
      setIsSearching(true);
    }
  }, [searchQuery]);

  const handleSubmit = useCallback(
    (event: React.FormEvent) => {
      event.preventDefault();
      if (searchQuery.trim()) {
        setIsSearching(true);
      }
    },
    [searchQuery]
  );

  if (isSearching) {
    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {translate('ManualSearch')} - {seriesTitle} - S{seasonNumber?.toString().padStart(2, '0')}E{episodeNumber?.toString().padStart(2, '0')}
        </ModalHeader>
        <ModalBody>
          <InteractiveSearch
            type="episode"
            searchPayload={{ episodeId, searchQuery }}
          />
        </ModalBody>
        <ModalFooter>
          <Button onPress={onModalClose}>{translate('Close')}</Button>
        </ModalFooter>
      </ModalContent>
    );
  }

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('ManualSearch')} - {seriesTitle} - S{seasonNumber?.toString().padStart(2, '0')}E{episodeNumber?.toString().padStart(2, '0')}
      </ModalHeader>
      <ModalBody>
        <form onSubmit={handleSubmit}>
          <div className={styles.searchInputContainer}>
            <FormGroup>
              <FormLabel>{translate('SearchQuery')}</FormLabel>
              <TextInput
                name="searchQuery"
                value={searchQuery}
                placeholder={translate('EnterSearchQuery')}
                autoFocus={true}
                onChange={handleSearchChange}
              />
            </FormGroup>
            <div className={styles.helpText}>
              {translate('ManualSearchHelpText')}
            </div>
          </div>
        </form>
      </ModalBody>
      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>
        <Button
          kind={kinds.PRIMARY}
          isDisabled={!searchQuery.trim()}
          onPress={handleSearchPress}
        >
          {translate('Search')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default ManualSearchModalContent;
