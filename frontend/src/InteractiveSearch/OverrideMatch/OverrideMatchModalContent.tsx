import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import DownloadProtocol from 'DownloadClient/DownloadProtocol';
import EpisodeLanguages from 'Episode/EpisodeLanguages';
import EpisodeQuality from 'Episode/EpisodeQuality';
import usePrevious from 'Helpers/Hooks/usePrevious';
import SelectEpisodeModal from 'InteractiveImport/Episode/SelectEpisodeModal';
import { SelectedEpisode } from 'InteractiveImport/Episode/SelectEpisodeModalContent';
import SelectLanguageModal from 'InteractiveImport/Language/SelectLanguageModal';
import SelectQualityModal from 'InteractiveImport/Quality/SelectQualityModal';
import SelectSeasonModal from 'InteractiveImport/Season/SelectSeasonModal';
import SelectSeriesModal from 'InteractiveImport/Series/SelectSeriesModal';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import Series from 'Series/Series';
import { grabRelease } from 'Store/Actions/releaseActions';
import { fetchDownloadClients } from 'Store/Actions/settingsActions';
import createEnabledDownloadClientsSelector from 'Store/Selectors/createEnabledDownloadClientsSelector';
import { createSeriesSelectorForHook } from 'Store/Selectors/createSeriesSelector';
import { ReleaseEpisode } from 'typings/Release';
import translate from 'Utilities/String/translate';
import SelectDownloadClientModal from './DownloadClient/SelectDownloadClientModal';
import OverrideMatchData from './OverrideMatchData';
import styles from './OverrideMatchModalContent.css';

type SelectType =
  | 'select'
  | 'series'
  | 'season'
  | 'episode'
  | 'quality'
  | 'language'
  | 'downloadClient';

interface OverrideMatchModalContentProps {
  indexerId: number;
  title: string;
  guid: string;
  seriesId?: number;
  seasonNumber?: number;
  episodes: ReleaseEpisode[];
  languages: Language[];
  quality: QualityModel;
  protocol: DownloadProtocol;
  isGrabbing: boolean;
  grabError?: string;
  onModalClose(): void;
}

function OverrideMatchModalContent(props: OverrideMatchModalContentProps) {
  const modalTitle = translate('ManualGrab');
  const {
    indexerId,
    title,
    guid,
    protocol,
    isGrabbing,
    grabError,
    onModalClose,
  } = props;

  const [seriesId, setSeriesId] = useState(props.seriesId);
  const [seasonNumber, setSeasonNumber] = useState(props.seasonNumber);
  const [episodes, setEpisodes] = useState(props.episodes);
  const [languages, setLanguages] = useState(props.languages);
  const [quality, setQuality] = useState(props.quality);
  const [downloadClientId, setDownloadClientId] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [selectModalOpen, setSelectModalOpen] = useState<SelectType | null>(
    null
  );
  const previousIsGrabbing = usePrevious(isGrabbing);

  const dispatch = useDispatch();
  const series: Series | undefined = useSelector(
    createSeriesSelectorForHook(seriesId)
  );
  const { items: downloadClients } = useSelector(
    createEnabledDownloadClientsSelector(protocol)
  );

  const episodeInfo = useMemo(() => {
    return episodes.map((episode) => {
      return (
        <div key={episode.id}>
          {episode.episodeNumber}

          {series?.seriesType === 'anime' &&
          episode.absoluteEpisodeNumber != null
            ? ` (${episode.absoluteEpisodeNumber})`
            : ''}

          {` - ${episode.title}`}
        </div>
      );
    });
  }, [series, episodes]);

  const onSelectModalClose = useCallback(() => {
    setSelectModalOpen(null);
  }, [setSelectModalOpen]);

  const onSelectSeriesPress = useCallback(() => {
    setSelectModalOpen('series');
  }, [setSelectModalOpen]);

  const onSeriesSelect = useCallback(
    (s: Series) => {
      setSeriesId(s.id);
      setSeasonNumber(undefined);
      setEpisodes([]);
      setSelectModalOpen(null);
    },
    [setSeriesId, setSeasonNumber, setEpisodes, setSelectModalOpen]
  );

  const onSelectSeasonPress = useCallback(() => {
    setSelectModalOpen('season');
  }, [setSelectModalOpen]);

  const onSeasonSelect = useCallback(
    (s: number) => {
      setSeasonNumber(s);
      setEpisodes([]);
      setSelectModalOpen(null);
    },
    [setSeasonNumber, setEpisodes, setSelectModalOpen]
  );

  const onSelectEpisodePress = useCallback(() => {
    setSelectModalOpen('episode');
  }, [setSelectModalOpen]);

  const onEpisodesSelect = useCallback(
    (episodeMap: SelectedEpisode[]) => {
      setEpisodes(episodeMap[0].episodes);
      setSelectModalOpen(null);
    },
    [setEpisodes, setSelectModalOpen]
  );

  const onSelectQualityPress = useCallback(() => {
    setSelectModalOpen('quality');
  }, [setSelectModalOpen]);

  const onQualitySelect = useCallback(
    (quality: QualityModel) => {
      setQuality(quality);
      setSelectModalOpen(null);
    },
    [setQuality, setSelectModalOpen]
  );

  const onSelectLanguagesPress = useCallback(() => {
    setSelectModalOpen('language');
  }, [setSelectModalOpen]);

  const onLanguagesSelect = useCallback(
    (languages: Language[]) => {
      setLanguages(languages);
      setSelectModalOpen(null);
    },
    [setLanguages, setSelectModalOpen]
  );

  const onSelectDownloadClientPress = useCallback(() => {
    setSelectModalOpen('downloadClient');
  }, [setSelectModalOpen]);

  const onDownloadClientSelect = useCallback(
    (downloadClientId: number) => {
      setDownloadClientId(downloadClientId);
      setSelectModalOpen(null);
    },
    [setDownloadClientId, setSelectModalOpen]
  );

  const onGrabPress = useCallback(() => {
    if (!seriesId) {
      setError(translate('OverrideGrabNoSeries'));
      return;
    } else if (!episodes.length) {
      setError(translate('OverrideGrabNoEpisode'));
      return;
    } else if (!quality) {
      setError(translate('OverrideGrabNoQuality'));
      return;
    } else if (!languages.length) {
      setError(translate('OverrideGrabNoLanguage'));
      return;
    }

    dispatch(
      grabRelease({
        indexerId,
        guid,
        seriesId,
        episodeIds: episodes.map((e) => e.id),
        quality,
        languages,
        downloadClientId,
        shouldOverride: true,
      })
    );
  }, [
    indexerId,
    guid,
    seriesId,
    episodes,
    quality,
    languages,
    downloadClientId,
    setError,
    dispatch,
  ]);

  useEffect(() => {
    if (!isGrabbing && previousIsGrabbing) {
      onModalClose();
    }
  }, [isGrabbing, previousIsGrabbing, onModalClose]);

  useEffect(
    () => {
      dispatch(fetchDownloadClients());
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('OverrideGrabModalTitle', { title })}
      </ModalHeader>

      <ModalBody>
        <DescriptionList>
          <DescriptionListItem
            className={styles.item}
            title={translate('Series')}
            data={
              <OverrideMatchData
                value={series?.title}
                onPress={onSelectSeriesPress}
              />
            }
          />

          <DescriptionListItem
            className={styles.item}
            title={translate('SeasonNumber')}
            data={
              <OverrideMatchData
                value={seasonNumber}
                isDisabled={!series}
                onPress={onSelectSeasonPress}
              />
            }
          />

          <DescriptionListItem
            className={styles.item}
            title={translate('Episodes')}
            data={
              <OverrideMatchData
                value={episodeInfo}
                isDisabled={!series || isNaN(Number(seasonNumber))}
                onPress={onSelectEpisodePress}
              />
            }
          />

          <DescriptionListItem
            className={styles.item}
            title={translate('Quality')}
            data={
              <OverrideMatchData
                value={
                  <EpisodeQuality className={styles.label} quality={quality} />
                }
                onPress={onSelectQualityPress}
              />
            }
          />

          <DescriptionListItem
            className={styles.item}
            title={translate('Languages')}
            data={
              <OverrideMatchData
                value={
                  <EpisodeLanguages
                    className={styles.label}
                    languages={languages}
                  />
                }
                onPress={onSelectLanguagesPress}
              />
            }
          />

          {downloadClients.length > 1 ? (
            <DescriptionListItem
              className={styles.item}
              title={translate('DownloadClient')}
              data={
                <OverrideMatchData
                  value={
                    downloadClients.find(
                      (downloadClient) => downloadClient.id === downloadClientId
                    )?.name ?? translate('Default')
                  }
                  onPress={onSelectDownloadClientPress}
                />
              }
            />
          ) : null}
        </DescriptionList>
      </ModalBody>

      <ModalFooter className={styles.footer}>
        <div className={styles.error}>{error || grabError}</div>

        <div className={styles.buttons}>
          <Button onPress={onModalClose}>{translate('Cancel')}</Button>

          <SpinnerErrorButton
            isSpinning={isGrabbing}
            error={grabError}
            onPress={onGrabPress}
          >
            {translate('GrabRelease')}
          </SpinnerErrorButton>
        </div>
      </ModalFooter>

      <SelectSeriesModal
        isOpen={selectModalOpen === 'series'}
        modalTitle={modalTitle}
        onSeriesSelect={onSeriesSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectSeasonModal
        isOpen={selectModalOpen === 'season'}
        modalTitle={modalTitle}
        seriesId={seriesId}
        onSeasonSelect={onSeasonSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectEpisodeModal
        isOpen={selectModalOpen === 'episode'}
        selectedIds={[guid]}
        seriesId={seriesId}
        isAnime={series?.seriesType === 'anime'}
        seasonNumber={seasonNumber}
        selectedDetails={title}
        modalTitle={modalTitle}
        onEpisodesSelect={onEpisodesSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectQualityModal
        isOpen={selectModalOpen === 'quality'}
        qualityId={quality ? quality.quality.id : 0}
        proper={quality ? quality.revision.version > 1 : false}
        real={quality ? quality.revision.real > 0 : false}
        modalTitle={modalTitle}
        onQualitySelect={onQualitySelect}
        onModalClose={onSelectModalClose}
      />

      <SelectLanguageModal
        isOpen={selectModalOpen === 'language'}
        languageIds={languages ? languages.map((l) => l.id) : []}
        modalTitle={modalTitle}
        onLanguagesSelect={onLanguagesSelect}
        onModalClose={onSelectModalClose}
      />

      <SelectDownloadClientModal
        isOpen={selectModalOpen === 'downloadClient'}
        protocol={protocol}
        modalTitle={modalTitle}
        onDownloadClientSelect={onDownloadClientSelect}
        onModalClose={onSelectModalClose}
      />
    </ModalContent>
  );
}

export default OverrideMatchModalContent;
