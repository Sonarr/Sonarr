import { useQueryClient } from '@tanstack/react-query';
import React, { useCallback, useEffect, useState } from 'react';
import { useCommands } from 'Commands/useCommands';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { inputTypes, kinds } from 'Helpers/Props';
import { useSaveSeries, useSingleSeries } from 'Series/useSeries';
import { InputChanged } from 'typings/inputs';
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
  previewToken: string;
}

interface EpisodeRetentionModalContentProps {
  seriesId: number;
  onModalClose: () => void;
}

function episodeLabel(episode: RetentionEpisode) {
  return `S${String(episode.seasonNumber).padStart(2, '0')}E${String(
    episode.episodeNumber
  ).padStart(2, '0')} ${episode.title}`;
}

function EpisodeRetentionModalContent({
  seriesId,
  onModalClose,
}: EpisodeRetentionModalContentProps) {
  const series = useSingleSeries(seriesId)!;
  const queryClient = useQueryClient();
  const { saveSeries, isSaving, saveError } = useSaveSeries();
  const savedSettings = {
    retentionEnabled: series.retentionEnabled ?? false,
    retentionKeepEpisodes: series.retentionKeepEpisodes ?? 5,
    retentionIncludeSpecials: series.retentionIncludeSpecials ?? false,
  };
  const [settings, setSettings] = useState(savedSettings);
  const [preview, setPreview] = useState<Readonly<RetentionPreview> | null>(
    null
  );
  const [dismissedCommandId, setDismissedCommandId] = useState<number | null>(
    null
  );
  const {
    data: commands,
    error: commandError,
    isPending: isLoadingCommands,
    isFetching: isFetchingCommands,
    refetch: refetchCommands,
  } = useCommands();
  const retentionCommands = commands.filter(
    (command) =>
      command.name === 'EpisodeRetention' &&
      'seriesId' in command.body &&
      command.body.seriesId === seriesId
  );
  const runningCommands = retentionCommands.filter(
    (command) => command.status === 'queued' || command.status === 'started'
  );
  const command = (
    runningCommands.length ? runningCommands : retentionCommands
  ).reduce<(typeof commands)[number] | undefined>(
    (latest, item) => (!latest || item.id > latest.id ? item : latest),
    undefined
  );
  const isCommandRunning = runningCommands.length > 0;
  const isComplete =
    !commandError &&
    command?.status === 'completed' &&
    command.id !== dismissedCommandId;
  const commandFailed =
    !commandError &&
    !!command &&
    command.id !== dismissedCommandId &&
    !isCommandRunning &&
    command.status !== 'completed';
  const savedSettingsKey = JSON.stringify(savedSettings);
  const isDirty = JSON.stringify(settings) !== savedSettingsKey;
  const isValid =
    Number.isInteger(settings.retentionKeepEpisodes) &&
    settings.retentionKeepEpisodes > 0 &&
    settings.retentionKeepEpisodes <= 2147483647;
  const {
    refetch,
    isFetching,
    error: previewError,
  } = useApiQuery<RetentionPreview>({
    path: `/series/${seriesId}/retention`,
    queryOptions: { enabled: false, retry: false },
  });
  const {
    mutate: trim,
    isPending: isTrimming,
    error: trimError,
    reset: resetTrim,
  } = useApiMutation<{ commandId: number }, { previewToken: string }>({
    path: `/series/${seriesId}/retention`,
    method: 'POST',
    mutationOptions: {
      onSuccess: async () => {
        setPreview(null);
        await queryClient.invalidateQueries({ queryKey: ['/command'] });
      },
      onError: () => setPreview(null),
    },
  });
  useEffect(() => {
    if (commandError) {
      setPreview(null);
    }
  }, [commandError]);

  const isBusy =
    isSaving ||
    isFetching ||
    isTrimming ||
    isCommandRunning ||
    isLoadingCommands;

  const canPreview =
    !isBusy &&
    !isDirty &&
    isValid &&
    settings.retentionEnabled &&
    !saveError &&
    !commandError;
  const deletedEpisodeIds = new Set(
    preview?.files.flatMap((file) => file.episodes.map((episode) => episode.id))
  );
  const missingEpisodesToUnmonitor =
    preview?.unmonitorEpisodes.filter(
      (episode) => !deletedEpisodeIds.has(episode.id)
    ) ?? [];
  const hasChanges =
    !!preview &&
    (preview.files.length > 0 || preview.unmonitorEpisodes.length > 0);

  // A preview is valid only for the saved settings that produced it.
  useEffect(() => {
    setPreview(null);
  }, [savedSettingsKey]);

  const handleChange = useCallback(
    ({ name, value }: InputChanged) => {
      setSettings((current) => ({ ...current, [name]: value }));
      setPreview(null);
      setDismissedCommandId(command?.id ?? null);
    },
    [command?.id]
  );

  const handleSave = useCallback(() => {
    setPreview(null);
    setDismissedCommandId(command?.id ?? null);
    saveSeries({ ...series, ...settings });
  }, [command?.id, saveSeries, series, settings]);

  const handlePreview = useCallback(async () => {
    setPreview(null);
    setDismissedCommandId(command?.id ?? null);
    resetTrim();
    const result = await refetch();

    if (result.isSuccess) {
      setPreview(result.data);
    }
  }, [command?.id, refetch, resetTrim]);

  const handleRefreshCommands = useCallback(() => {
    refetchCommands();
  }, [refetchCommands]);

  const handleTrim = useCallback(() => {
    if (canPreview && preview && !previewError && !trimError && !commandError) {
      trim({ previewToken: preview.previewToken });
    }
  }, [canPreview, preview, previewError, trimError, commandError, trim]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('EpisodeRetention')}</ModalHeader>
      <ModalBody innerClassName={styles.body}>
        <p className={styles.intro}>{translate('EpisodeRetentionHelpText')}</p>
        <Form>
          <div className={styles.settingRow}>
            <div>
              <FormLabel
                className={styles.settingLabel}
                name="retentionEnabled"
              >
                {translate('RetentionAutomaticCleanup')}
              </FormLabel>
              <p className={styles.helpText}>
                {translate('RetentionAutomaticCleanupHelpText')}
              </p>
            </div>
            <FormInput
              type={inputTypes.CHECK}
              id="retentionEnabled"
              name="retentionEnabled"
              ariaLabel={translate('RetentionAutomaticCleanup')}
              value={settings.retentionEnabled}
              isDisabled={isBusy}
              onChange={handleChange}
            />
          </div>
          <div className={styles.settingRow}>
            <div>
              <FormLabel
                className={styles.settingLabel}
                name="retentionKeepEpisodes"
              >
                {translate('RetentionKeepEpisodes')}
              </FormLabel>
              <p className={styles.helpText}>
                {translate('RetentionKeepEpisodesHelpText')}
              </p>
            </div>
            <FormInput
              type={inputTypes.NUMBER}
              id="retentionKeepEpisodes"
              name="retentionKeepEpisodes"
              ariaLabel={translate('RetentionKeepEpisodes')}
              value={settings.retentionKeepEpisodes}
              min={1}
              max={2147483647}
              readOnly={isBusy}
              onChange={handleChange}
            />
          </div>
          <div className={styles.settingRow}>
            <div>
              <FormLabel
                className={styles.settingLabel}
                name="retentionIncludeSpecials"
              >
                {translate('RetentionIncludeSpecials')}
              </FormLabel>
              <p className={styles.helpText}>
                {translate('RetentionIncludeSpecialsHelpText')}
              </p>
            </div>
            <FormInput
              type={inputTypes.CHECK}
              id="retentionIncludeSpecials"
              name="retentionIncludeSpecials"
              ariaLabel={translate('RetentionIncludeSpecials')}
              value={settings.retentionIncludeSpecials}
              isDisabled={isBusy}
              onChange={handleChange}
            />
          </div>
        </Form>
        <p className={styles.policyNote}>{translate('RetentionPolicyNote')}</p>
        {isDirty ? (
          <Alert kind={kinds.WARNING}>{translate('RetentionSaveFirst')}</Alert>
        ) : null}
        {isValid ? null : (
          <Alert kind={kinds.DANGER}>
            {translate('RetentionKeepEpisodesInvalid')}
          </Alert>
        )}
        {saveError || previewError || trimError || commandError ? (
          <Alert kind={kinds.DANGER}>
            {translate(
              trimError?.statusCode === 409
                ? 'RetentionPlanChanged'
                : 'RetentionRequestError'
            )}
          </Alert>
        ) : null}
        {commandFailed ? (
          <Alert kind={kinds.DANGER}>
            {translate('RetentionCommandFailed')}
          </Alert>
        ) : null}
        {isCommandRunning ? (
          <Alert kind={kinds.INFO}>
            {translate('RetentionCommandRunning')}
          </Alert>
        ) : null}
        {isComplete ? (
          <Alert kind={kinds.SUCCESS}>{translate('RetentionComplete')}</Alert>
        ) : null}
        {preview && !isDirty ? (
          <section
            className={styles.preview}
            aria-label={translate('RetentionCleanupPreview')}
          >
            <h3 className={styles.sectionTitle}>
              {translate('RetentionCleanupPreview')}
            </h3>
            {preview.files.length > 0 ? (
              <>
                <h4 className={styles.previewHeading}>
                  {translate('RetentionFilesToDelete', {
                    count: preview.files.length,
                  })}
                </h4>
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
            {missingEpisodesToUnmonitor.length > 0 ? (
              <>
                <h4 className={styles.previewHeading}>
                  {translate('RetentionEpisodesToUnmonitor', {
                    count: missingEpisodesToUnmonitor.length,
                  })}
                </h4>
                <ul className={styles.previewList}>
                  {missingEpisodesToUnmonitor.map((episode) => (
                    <li key={episode.id}>{episodeLabel(episode)}</li>
                  ))}
                </ul>
              </>
            ) : null}
            {hasChanges ? (
              <Alert kind={kinds.WARNING}>
                {translate('RetentionTrimWarning')}
              </Alert>
            ) : (
              <Alert kind={kinds.INFO}>
                {translate('RetentionNothingToDo')}
              </Alert>
            )}
          </section>
        ) : null}
      </ModalBody>
      <ModalFooter className={styles.footer}>
        <Button className={styles.closeButton} onPress={onModalClose}>
          {translate('Close')}
        </Button>
        {commandError ? (
          <SpinnerButton
            isSpinning={isFetchingCommands}
            isDisabled={isFetchingCommands}
            onPress={handleRefreshCommands}
          >
            {translate('Refresh')}
          </SpinnerButton>
        ) : null}
        <SpinnerButton
          isSpinning={isSaving}
          isDisabled={isBusy || !isDirty || !isValid}
          onPress={handleSave}
        >
          {translate('Save')}
        </SpinnerButton>
        <SpinnerButton
          isSpinning={isFetching}
          isDisabled={!canPreview}
          onPress={handlePreview}
        >
          {translate('RetentionPreviewCleanup')}
        </SpinnerButton>
        <SpinnerButton
          kind={kinds.DANGER}
          isSpinning={isTrimming || isCommandRunning}
          isDisabled={
            !canPreview ||
            !hasChanges ||
            !!previewError ||
            !!trimError ||
            !!commandError
          }
          onPress={handleTrim}
        >
          {translate('RetentionTrimNow')}
        </SpinnerButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default EpisodeRetentionModalContent;
