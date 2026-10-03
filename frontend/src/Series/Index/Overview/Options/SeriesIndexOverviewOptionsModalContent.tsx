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
  setSeriesOverviewOptions,
  useSeriesOverviewOptions,
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

interface SeriesIndexOverviewOptionsModalContentProps {
  onModalClose(...args: unknown[]): void;
}

function SeriesIndexOverviewOptionsModalContent({
  onModalClose,
}: SeriesIndexOverviewOptionsModalContentProps) {
  const {
    detailedProgressBar,
    size,
    showMonitored,
    showNetwork,
    showQualityProfile,
    showPreviousAiring,
    showAdded,
    showSeasonCount,
    showPath,
    showSizeOnDisk,
    showTags,
    showSearchAction,
  } = useSeriesOverviewOptions();

  const onOverviewOptionChange = useCallback(
    ({ name, value }: { name: string; value: unknown }) => {
      setSeriesOverviewOptions({ [name]: value });
    },
    []
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('OverviewOptions')}</ModalHeader>
      <ModalBody>
        <Form>
          <FormRow>
            <FormLabel>{translate('PosterSize')}</FormLabel>

            <FormInput
              type={inputTypes.SELECT}
              name="size"
              value={size}
              values={posterSizeOptions}
              onChange={onOverviewOptionChange}
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
              onChange={onOverviewOptionChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('ShowMonitored')}</FormLabel>
            <FormInputHelpText text={translate('ShowMonitoredHelpText')} />
            <FormInput
              type={inputTypes.CHECK}
              name="showMonitored"
              value={showMonitored}
              onChange={onOverviewOptionChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('ShowNetwork')}</FormLabel>
            <FormInputHelpText text={translate('ShowNetworkHelpText')} />
            <FormInput
              type={inputTypes.CHECK}
              name="showNetwork"
              value={showNetwork}
              onChange={onOverviewOptionChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('ShowQualityProfile')}</FormLabel>
            <FormInputHelpText text={translate('ShowQualityProfileHelpText')} />
            <FormInput
              type={inputTypes.CHECK}
              name="showQualityProfile"
              value={showQualityProfile}
              onChange={onOverviewOptionChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('ShowPreviousAiring')}</FormLabel>
            <FormInputHelpText text={translate('ShowPreviousAiringHelpText')} />
            <FormInput
              type={inputTypes.CHECK}
              name="showPreviousAiring"
              value={showPreviousAiring}
              onChange={onOverviewOptionChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('ShowDateAdded')}</FormLabel>
            <FormInputHelpText text={translate('ShowDateAddedHelpText')} />
            <FormInput
              type={inputTypes.CHECK}
              name="showAdded"
              value={showAdded}
              onChange={onOverviewOptionChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('ShowSeasonCount')}</FormLabel>
            <FormInputHelpText text={translate('ShowSeasonCountHelpText')} />
            <FormInput
              type={inputTypes.CHECK}
              name="showSeasonCount"
              value={showSeasonCount}
              onChange={onOverviewOptionChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('ShowPath')}</FormLabel>
            <FormInputHelpText text={translate('ShowPathHelpText')} />
            <FormInput
              type={inputTypes.CHECK}
              name="showPath"
              value={showPath}
              onChange={onOverviewOptionChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('ShowSizeOnDisk')}</FormLabel>
            <FormInputHelpText text={translate('ShowSizeOnDiskHelpText')} />
            <FormInput
              type={inputTypes.CHECK}
              name="showSizeOnDisk"
              value={showSizeOnDisk}
              onChange={onOverviewOptionChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('ShowTags')}</FormLabel>
            <FormInputHelpText text={translate('ShowTagsHelpText')} />
            <FormInput
              type={inputTypes.CHECK}
              name="showTags"
              value={showTags}
              onChange={onOverviewOptionChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('ShowSearch')}</FormLabel>
            <FormInputHelpText text={translate('ShowSearchHelpText')} />
            <FormInput
              type={inputTypes.CHECK}
              name="showSearchAction"
              value={showSearchAction}
              onChange={onOverviewOptionChange}
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

export default SeriesIndexOverviewOptionsModalContent;
