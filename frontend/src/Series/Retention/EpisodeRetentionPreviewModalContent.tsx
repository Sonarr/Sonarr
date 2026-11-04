import React, { useCallback } from 'react';
import CommandNames from 'Commands/CommandNames';
import { useExecuteCommand } from 'Commands/useCommands';
import Alert from 'Components/Alert';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './EpisodeRetentionModalContent.module.css';

interface RetentionEpisode {
  id: number;
  seasonNumber: number;
  episodeNumber: number;
  title: string;
}

interface RetentionPreview {
  files: {
    id: number;
    relativePath: string;
    episodes: RetentionEpisode[];
  }[];
  unmonitorEpisodes: RetentionEpisode[];
}

interface EpisodeRetentionPreviewModalContentProps {
  seriesId: number;
  onModalClose: () => void;
  onCleanupStart: () => void;
}

function episodeLabel(episode: RetentionEpisode) {
  return `S${String(episode.seasonNumber).padStart(2, '0')}E${String(
    episode.episodeNumber
  ).padStart(2, '0')} ${episode.title}`;
}

function EpisodeRetentionPreviewModalContent({
  seriesId,
  onModalClose,
  onCleanupStart,
}: EpisodeRetentionPreviewModalContentProps) {
  const executeCommand = useExecuteCommand();
  const {
    data: preview,
    isFetching,
    error,
  } = useApiQuery<RetentionPreview>({
    path: `/series/${seriesId}/retention`,
  });
  const missingEpisodes = preview?.unmonitorEpisodes ?? [];
  const hasChanges =
    !!preview && (preview.files.length > 0 || missingEpisodes.length > 0);

  const handleTrim = useCallback(() => {
    if (!preview || isFetching || error || !hasChanges) {
      return;
    }

    executeCommand({
      name: CommandNames.EpisodeRetention,
      seriesId,
      files: preview.files.map((file) => file.id),
      episodeIds: preview.unmonitorEpisodes.map((episode) => episode.id),
    });

    onCleanupStart();
  }, [
    preview,
    isFetching,
    error,
    hasChanges,
    executeCommand,
    seriesId,
    onCleanupStart,
  ]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('RetentionCleanupPreview')}</ModalHeader>

      <ModalBody>
        {isFetching ? <LoadingIndicator /> : null}

        {!isFetching && error ? (
          <Alert kind={kinds.DANGER}>
            {translate('RetentionRequestError')}
          </Alert>
        ) : null}

        {!isFetching && !error && preview ? (
          <>
            {preview.files.length > 0 ? (
              <>
                <h3 className={styles.previewHeading}>
                  {translate('RetentionFilesToDelete', {
                    count: preview.files.length,
                  })}
                </h3>
                <ul className={styles.previewList}>
                  {preview.files.map((file) => (
                    <li key={file.id}>
                      <div className={styles.filePath}>{file.relativePath}</div>
                      <div className={styles.helpText}>
                        {file.episodes.map(episodeLabel).join(', ')}
                      </div>
                    </li>
                  ))}
                </ul>
              </>
            ) : null}

            {missingEpisodes.length > 0 ? (
              <>
                <h3 className={styles.previewHeading}>
                  {translate('RetentionEpisodesToUnmonitor', {
                    count: missingEpisodes.length,
                  })}
                </h3>
                <ul className={styles.previewList}>
                  {missingEpisodes.map((episode) => (
                    <li key={episode.id}>{episodeLabel(episode)}</li>
                  ))}
                </ul>
              </>
            ) : null}

            <Alert kind={hasChanges ? kinds.WARNING : kinds.INFO}>
              {translate(
                hasChanges ? 'RetentionTrimWarning' : 'RetentionNothingToDo'
              )}
            </Alert>
          </>
        ) : null}
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>
        <Button
          kind={kinds.DANGER}
          isDisabled={isFetching || !!error || !hasChanges}
          onPress={handleTrim}
        >
          {translate('RetentionTrimNow')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default EpisodeRetentionPreviewModalContent;
