import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { inputTypes } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';

const posterSizeOptions = [
  { key: 'small', value: 'Small' },
  { key: 'medium', value: 'Medium' },
  { key: 'large', value: 'Large' }
];

class SeriesIndexPosterOptionsModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      detailedProgressBar: props.detailedProgressBar,
      size: props.size,
      showTitle: props.showTitle,
      showMonitored: props.showMonitored,
      showQualityProfile: props.showQualityProfile
    };
  }

  componentDidUpdate(prevProps) {
    const {
      detailedProgressBar,
      size,
      showTitle,
      showMonitored,
      showQualityProfile
    } = this.props;

    const state = {};

    if (detailedProgressBar !== prevProps.detailedProgressBar) {
      state.detailedProgressBar = detailedProgressBar;
    }

    if (size !== prevProps.size) {
      state.size = size;
    }

    if (showTitle !== prevProps.showTitle) {
      state.showTitle = showTitle;
    }

    if (showMonitored !== prevProps.showMonitored) {
      state.showMonitored = showMonitored;
    }

    if (showQualityProfile !== prevProps.showQualityProfile) {
      state.showQualityProfile = showQualityProfile;
    }

    if (!_.isEmpty(state)) {
      this.setState(state);
    }
  }

  //
  // Listeners

  onChangePosterOption = ({ name, value }) => {
    this.setState({
      [name]: value
    }, () => {
      this.props.onChangePosterOption({ [name]: value });
    });
  }

  //
  // Render

  render() {
    const {
      onModalClose
    } = this.props;

    const {
      detailedProgressBar,
      size,
      showTitle,
      showMonitored,
      showQualityProfile
    } = this.state;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Poster Options
        </ModalHeader>

        <ModalBody>
          <Form>
            <FormGroup>
              <FormLabel>Poster Size</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="size"
                value={size}
                values={posterSizeOptions}
                onChange={this.onChangePosterOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>Detailed Progress Bar</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="detailedProgressBar"
                value={detailedProgressBar}
                helpText="Show text on progess bar"
                onChange={this.onChangePosterOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>Show Title</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showTitle"
                value={showTitle}
                helpText="Show series title under poster"
                onChange={this.onChangePosterOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>Show Monitored</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showMonitored"
                value={showMonitored}
                helpText="Show monitored status under poster"
                onChange={this.onChangePosterOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>Show Quality Profile</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showQualityProfile"
                value={showQualityProfile}
                helpText="Show quality profile under poster"
                onChange={this.onChangePosterOption}
              />
            </FormGroup>
          </Form>
        </ModalBody>

        <ModalFooter>
          <Button
            onPress={onModalClose}
          >
            Close
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

SeriesIndexPosterOptionsModalContent.propTypes = {
  size: PropTypes.string.isRequired,
  showTitle: PropTypes.bool.isRequired,
  showMonitored: PropTypes.bool.isRequired,
  showQualityProfile: PropTypes.bool.isRequired,
  detailedProgressBar: PropTypes.bool.isRequired,
  onChangePosterOption: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SeriesIndexPosterOptionsModalContent;
