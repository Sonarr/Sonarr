import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes } from 'Helpers/Props';
import { setSeriesOverviewOption } from 'Store/Actions/seriesIndexActions';
import selectOverviewOptions from '../selectOverviewOptions';

const posterSizeOptions = [
  { key: 'small', value: 'Small' },
  { key: 'medium', value: 'Medium' },
  { key: 'large', value: 'Large' },
];

interface SeriesIndexOverviewOptionsModalContentProps {
  onModalClose(...args: unknown[]): void;
}

function SeriesIndexOverviewOptionsModalContent(
  props: SeriesIndexOverviewOptionsModalContentProps
) {
  const { onModalClose } = props;

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
    showSearchAction,
  } = useSelector(selectOverviewOptions);

  const dispatch = useDispatch();

  const onOverviewOptionChange = useCallback(
    ({ name, value }) => {
      dispatch(setSeriesOverviewOption({ [name]: value }));
    },
    [dispatch]
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>Overview Options</ModalHeader>

      <ModalBody>
        <Form>
          <FormGroup>
            <FormLabel>Poster Size</FormLabel>

            <FormInputGroup
              type={inputTypes.SELECT}
              name="size"
              value={size}
              values={posterSizeOptions}
              onChange={onOverviewOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Detailed Progress Bar</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="detailedProgressBar"
              value={detailedProgressBar}
              helpText="Show text on progress bar"
              onChange={onOverviewOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Show Monitored</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="showMonitored"
              value={showMonitored}
              onChange={onOverviewOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Show Network</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="showNetwork"
              value={showNetwork}
              onChange={onOverviewOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Show Quality Profile</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="showQualityProfile"
              value={showQualityProfile}
              onChange={onOverviewOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Show Previous Airing</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="showPreviousAiring"
              value={showPreviousAiring}
              onChange={onOverviewOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Show Date Added</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="showAdded"
              value={showAdded}
              onChange={onOverviewOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Show Season Count</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="showSeasonCount"
              value={showSeasonCount}
              onChange={onOverviewOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Show Path</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="showPath"
              value={showPath}
              onChange={onOverviewOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Show Size on Disk</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="showSizeOnDisk"
              value={showSizeOnDisk}
              onChange={onOverviewOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Show Search</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="showSearchAction"
              value={showSearchAction}
              helpText="Show search button on hover"
              onChange={onOverviewOptionChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>Close</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default SeriesIndexOverviewOptionsModalContent;
