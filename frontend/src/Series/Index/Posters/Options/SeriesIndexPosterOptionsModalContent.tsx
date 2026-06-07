import React, { useCallback } from 'react';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes } from 'Helpers/Props';
import {
  setSeriesPosterOptions,
  useSeriesPosterOptions,
} from 'Series/seriesOptionsStore';
import translate from 'Utilities/String/translate';

const posterSizeOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: 'small',
    get value() {
      return translate('Small');
    },
  },
  {
    key: 'medium',
    get value() {
      return translate('Medium');
    },
  },
  {
    key: 'large',
    get value() {
      return translate('Large');
    },
  },
];

const showStatusOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: 'none',
    get value() {
      return translate('None');
    },
  },
  {
    key: 'deleted',
    get value() {
      return translate('DeletedOnly');
    },
  },
  {
    key: 'active',
    get value() {
      return translate('Active');
    },
  },
  {
    key: 'all',
    get value() {
      return translate('All');
    },
  },
];

interface SeriesIndexPosterOptionsModalContentProps {
  onModalClose(...args: unknown[]): unknown;
}

function SeriesIndexPosterOptionsModalContent({
  onModalClose,
}: SeriesIndexPosterOptionsModalContentProps) {
  const {
    detailedProgressBar,
    size,
    showTitle,
    showMonitored,
    showQualityProfile,
    showTags,
    showSearchAction,
    showStatus,
  } = useSeriesPosterOptions();

  const onPosterOptionChange = useCallback(
    ({ name, value }: { name: string; value: unknown }) => {
      setSeriesPosterOptions({ [name]: value });
    },
    []
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('PosterOptions')}</ModalHeader>
      <ModalBody>
        <Form>
          <FormRow>
            <FormLabel>{translate('PosterSize')}</FormLabel>

            <FormInput
              type={inputTypes.SELECT}
              name="size"
              value={size}
              values={posterSizeOptions}
              onChange={onPosterOptionChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('DetailedProgressBar')}</FormLabel>
            <FormInputHelpText
              text={translate('DetailedProgressBarHelpText')}
            />
            <FormInput
              type={inputTypes.CHECK}
              name="detailedProgressBar"
              value={detailedProgressBar}
              onChange={onPosterOptionChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('ShowTitle')}</FormLabel>
            <FormInputHelpText text={translate('ShowSeriesTitleHelpText')} />
            <FormInput
              type={inputTypes.CHECK}
              name="showTitle"
              value={showTitle}
              onChange={onPosterOptionChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('ShowMonitored')}</FormLabel>
            <FormInputHelpText text={translate('ShowMonitoredHelpText')} />
            <FormInput
              type={inputTypes.CHECK}
              name="showMonitored"
              value={showMonitored}
              onChange={onPosterOptionChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('ShowQualityProfile')}</FormLabel>
            <FormInputHelpText text={translate('ShowQualityProfileHelpText')} />
            <FormInput
              type={inputTypes.CHECK}
              name="showQualityProfile"
              value={showQualityProfile}
              onChange={onPosterOptionChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('ShowTags')}</FormLabel>
            <FormInputHelpText text={translate('ShowTagsHelpText')} />
            <FormInput
              type={inputTypes.CHECK}
              name="showTags"
              value={showTags}
              onChange={onPosterOptionChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('ShowSearch')}</FormLabel>
            <FormInputHelpText text={translate('ShowSearchHelpText')} />
            <FormInput
              type={inputTypes.CHECK}
              name="showSearchAction"
              value={showSearchAction}
              onChange={onPosterOptionChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('ShowStatus')}</FormLabel>
            <FormInputHelpText text={translate('ShowStatusHelpText')} />
            <FormInput
              type={inputTypes.SELECT}
              name="showStatus"
              value={showStatus ?? 'deleted'}
              values={showStatusOptions}
              onChange={onPosterOptionChange}
            />
          </FormRow>
        </Form>
      </ModalBody>
      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default SeriesIndexPosterOptionsModalContent;
