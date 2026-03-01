import React, { useCallback } from 'react';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
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
          <FormGroup>
            <FormLabel>{translate('PosterSize')}</FormLabel>

            <FormInputGroup
              type={inputTypes.SELECT}
              name="size"
              value={size}
              values={posterSizeOptions}
              onChange={onPosterOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('DetailedProgressBar')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="detailedProgressBar"
              value={detailedProgressBar}
              helpText={translate('DetailedProgressBarHelpText')}
              onChange={onPosterOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('ShowTitle')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="showTitle"
              value={showTitle}
              helpText={translate('ShowSeriesTitleHelpText')}
              onChange={onPosterOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('ShowMonitored')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="showMonitored"
              value={showMonitored}
              helpText={translate('ShowMonitoredHelpText')}
              onChange={onPosterOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('ShowQualityProfile')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="showQualityProfile"
              value={showQualityProfile}
              helpText={translate('ShowQualityProfileHelpText')}
              onChange={onPosterOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('ShowTags')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="showTags"
              value={showTags}
              helpText={translate('ShowTagsHelpText')}
              onChange={onPosterOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('ShowSearch')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="showSearchAction"
              value={showSearchAction}
              helpText={translate('ShowSearchHelpText')}
              onChange={onPosterOptionChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default SeriesIndexPosterOptionsModalContent;
