import React, { useCallback, useState } from 'react';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import { useSaveSeries, useSingleSeries } from 'Series/useSeries';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import EpisodeRetentionPreviewModal from './EpisodeRetentionPreviewModal';
import styles from './EpisodeRetentionModalContent.module.css';

interface EpisodeRetentionModalContentProps {
  seriesId: number;
  onModalClose: () => void;
}

function EpisodeRetentionModalContent({
  seriesId,
  onModalClose,
}: EpisodeRetentionModalContentProps) {
  const series = useSingleSeries(seriesId)!;
  const { saveSeries, isSaving, saveError } = useSaveSeries();
  const [settings, setSettings] = useState({
    retentionEnabled: series.retentionEnabled,
    retentionKeepEpisodes: series.retentionKeepEpisodes,
    retentionIncludeSpecials: series.retentionIncludeSpecials,
  });
  const [isPreviewOpen, setIsPreviewOpen] = useState(false);
  const isDirty =
    settings.retentionEnabled !== series.retentionEnabled ||
    settings.retentionKeepEpisodes !== series.retentionKeepEpisodes ||
    settings.retentionIncludeSpecials !== series.retentionIncludeSpecials;
  const isValid =
    Number.isInteger(settings.retentionKeepEpisodes) &&
    settings.retentionKeepEpisodes > 0 &&
    settings.retentionKeepEpisodes <= 2147483647;

  const handleChange = useCallback(({ name, value }: InputChanged) => {
    setSettings((current) => ({ ...current, [name]: value }));
  }, []);

  const handleSave = useCallback(() => {
    saveSeries({ ...series, ...settings });
  }, [saveSeries, series, settings]);

  const handlePreview = useCallback(() => setIsPreviewOpen(true), []);
  const handlePreviewClose = useCallback(() => setIsPreviewOpen(false), []);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('EpisodeRetention')}</ModalHeader>

      <ModalBody>
        <p className={styles.intro}>{translate('EpisodeRetentionHelpText')}</p>
        <Form>
          <FormRow size={sizes.MEDIUM}>
            <FormLabel>{translate('RetentionAutomaticCleanup')}</FormLabel>
            <FormInputHelpText
              text={translate('RetentionAutomaticCleanupHelpText')}
            />
            <FormInput
              type={inputTypes.CHECK}
              name="retentionEnabled"
              value={settings.retentionEnabled}
              isDisabled={isSaving}
              onChange={handleChange}
            />
          </FormRow>

          <FormRow size={sizes.MEDIUM}>
            <FormLabel>{translate('RetentionKeepEpisodes')}</FormLabel>
            <FormInputHelpText
              text={translate('RetentionKeepEpisodesHelpText')}
            />
            <FormInput
              type={inputTypes.NUMBER}
              name="retentionKeepEpisodes"
              value={settings.retentionKeepEpisodes}
              min={1}
              max={2147483647}
              readOnly={isSaving}
              onChange={handleChange}
            />
          </FormRow>

          <FormRow size={sizes.MEDIUM}>
            <FormLabel>{translate('RetentionIncludeSpecials')}</FormLabel>
            <FormInputHelpText
              text={translate('RetentionIncludeSpecialsHelpText')}
            />
            <FormInput
              type={inputTypes.CHECK}
              name="retentionIncludeSpecials"
              value={settings.retentionIncludeSpecials}
              isDisabled={isSaving}
              onChange={handleChange}
            />
          </FormRow>
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

        {saveError ? (
          <Alert kind={kinds.DANGER}>
            {translate('RetentionRequestError')}
          </Alert>
        ) : null}
      </ModalBody>

      <ModalFooter className={styles.footer}>
        <Button className={styles.closeButton} onPress={onModalClose}>
          {translate('Close')}
        </Button>

        <SpinnerButton
          isSpinning={isSaving}
          isDisabled={isSaving || !isDirty || !isValid}
          onPress={handleSave}
        >
          {translate('Save')}
        </SpinnerButton>

        <Button
          isDisabled={
            isSaving || isDirty || !isValid || !series.retentionEnabled
          }
          onPress={handlePreview}
        >
          {translate('RetentionPreviewCleanup')}
        </Button>
      </ModalFooter>

      <EpisodeRetentionPreviewModal
        isOpen={isPreviewOpen}
        seriesId={seriesId}
        onModalClose={handlePreviewClose}
        onCleanupStart={onModalClose}
      />
    </ModalContent>
  );
}

export default EpisodeRetentionModalContent;
