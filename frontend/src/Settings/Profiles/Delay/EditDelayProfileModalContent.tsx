import React, { useCallback, useEffect, useMemo } from 'react';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalSection from 'Components/ModalSection';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { inputTypes, kinds } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import { DelayProfile, useManageDelayProfile } from './useDelayProfiles';
import styles from './EditDelayProfileModalContent.css';

const protocolOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: 'preferUsenet',
    get value() {
      return translate('PreferUsenet');
    },
  },
  {
    key: 'preferTorrent',
    get value() {
      return translate('PreferTorrent');
    },
  },
  {
    key: 'onlyUsenet',
    get value() {
      return translate('OnlyUsenet');
    },
  },
  {
    key: 'onlyTorrent',
    get value() {
      return translate('OnlyTorrent');
    },
  },
];

export interface EditDelayProfileModalContentProps {
  id?: number;
  onDeleteDelayProfilePress?: () => void;
  onModalClose: () => void;
}

function EditDelayProfileModalContent({
  id,
  onModalClose,
  onDeleteDelayProfilePress,
  ...otherProps
}: EditDelayProfileModalContentProps) {
  const {
    item,
    validationErrors,
    validationWarnings,
    updateValue,
    saveProvider,
    isSaving,
    saveError,
  } = useManageDelayProfile(id);

  const {
    enableUsenet,
    enableTorrent,
    preferredProtocol,
    usenetDelay,
    torrentDelay,
    bypassIfHighestQuality,
    bypassIfAboveCustomFormatScore,
    minimumCustomFormatScore,
    tags,
  } = item;

  const protocol = useMemo(() => {
    if (!enableUsenet.value) {
      return 'onlyTorrent';
    } else if (!enableTorrent.value) {
      return 'onlyUsenet';
    }

    return preferredProtocol.value === 'usenet'
      ? 'preferUsenet'
      : 'preferTorrent';
  }, [enableUsenet, enableTorrent, preferredProtocol]);

  const wasSaving = usePrevious(isSaving);

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      updateValue(
        name as keyof DelayProfile,
        value as DelayProfile[keyof DelayProfile]
      );
    },
    [updateValue]
  );

  const handleProtocolChange = useCallback(
    ({ value }: InputChanged) => {
      switch (value) {
        case 'preferUsenet':
          updateValue('enableUsenet', true);
          updateValue('enableTorrent', true);
          updateValue('preferredProtocol', 'usenet');
          break;
        case 'preferTorrent':
          updateValue('enableUsenet', true);
          updateValue('enableTorrent', true);
          updateValue('preferredProtocol', 'torrent');
          break;
        case 'onlyUsenet':
          updateValue('enableUsenet', true);
          updateValue('enableTorrent', false);
          updateValue('preferredProtocol', 'usenet');
          break;
        case 'onlyTorrent':
          updateValue('enableUsenet', false);
          updateValue('enableTorrent', true);
          updateValue('preferredProtocol', 'torrent');
          break;
        default:
          throw Error(`Unknown protocol option: ${value}`);
      }
    },
    [updateValue]
  );

  const handleSavePress = useCallback(() => {
    saveProvider();
  }, [saveProvider]);

  useEffect(() => {
    if (wasSaving && !isSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? translate('EditDelayProfile') : translate('AddDelayProfile')}
      </ModalHeader>
      <ModalBody>
        <Form
          {...otherProps}
          validationErrors={validationErrors}
          validationWarnings={validationWarnings}
        >
          <ModalSection title={translate('Protocol')}>
            <FormRow>
              <FormLabel>{translate('PreferredProtocol')}</FormLabel>
              <FormInputHelpText text={translate('ProtocolHelpText')} />
              <FormInput
                type={inputTypes.SELECT}
                name="protocol"
                value={protocol}
                values={protocolOptions}
                onChange={handleProtocolChange}
              />
            </FormRow>

            {enableUsenet.value ? (
              <FormRow>
                <FormLabel>{translate('UsenetDelay')}</FormLabel>
                <FormInputHelpText text={translate('UsenetDelayHelpText')} />
                <FormInput
                  type={inputTypes.NUMBER}
                  name="usenetDelay"
                  unit="minutes"
                  {...usenetDelay}
                  onChange={handleInputChange}
                />
              </FormRow>
            ) : null}

            {enableTorrent.value ? (
              <FormRow>
                <FormLabel>{translate('TorrentDelay')}</FormLabel>
                <FormInputHelpText text={translate('TorrentDelayHelpText')} />
                <FormInput
                  type={inputTypes.NUMBER}
                  name="torrentDelay"
                  unit="minutes"
                  {...torrentDelay}
                  onChange={handleInputChange}
                />
              </FormRow>
            ) : null}
          </ModalSection>

          <ModalSection title={translate('ProfileSectionBypass')}>
            <FormRow>
              <FormLabel>{translate('BypassDelayIfHighestQuality')}</FormLabel>
              <FormInputHelpText
                text={translate('BypassDelayIfHighestQualityHelpText')}
              />
              <FormInput
                type={inputTypes.CHECK}
                name="bypassIfHighestQuality"
                {...bypassIfHighestQuality}
                onChange={handleInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>
                {translate('BypassDelayIfAboveCustomFormatScore')}
              </FormLabel>
              <FormInputHelpText
                text={translate('BypassDelayIfAboveCustomFormatScoreHelpText')}
              />
              <FormInput
                type={inputTypes.CHECK}
                name="bypassIfAboveCustomFormatScore"
                {...bypassIfAboveCustomFormatScore}
                onChange={handleInputChange}
              />
            </FormRow>

            {bypassIfAboveCustomFormatScore.value ? (
              <FormRow>
                <FormLabel>
                  {translate('BypassDelayIfAboveCustomFormatScoreMinimumScore')}
                </FormLabel>
                <FormInputHelpText
                  text={translate(
                    'BypassDelayIfAboveCustomFormatScoreMinimumScoreHelpText'
                  )}
                />
                <FormInput
                  type={inputTypes.NUMBER}
                  name="minimumCustomFormatScore"
                  {...minimumCustomFormatScore}
                  onChange={handleInputChange}
                />
              </FormRow>
            ) : null}
          </ModalSection>

          {id === 1 ? (
            <p className={styles.intro}>
              {translate('DefaultDelayProfileSeries')}
            </p>
          ) : (
            <FormRow>
              <FormLabel>{translate('Tags')}</FormLabel>
              <FormInputHelpText
                text={translate('DelayProfileSeriesTagsHelpText')}
              />
              <FormInput
                type={inputTypes.TAG}
                name="tags"
                {...tags}
                onChange={handleInputChange}
              />
            </FormRow>
          )}
        </Form>
      </ModalBody>
      <ModalFooter>
        {id && id > 1 ? (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeleteDelayProfilePress}
          >
            {translate('Delete')}
          </Button>
        ) : null}

        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={handleSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default EditDelayProfileModalContent;
