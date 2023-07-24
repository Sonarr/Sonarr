import PropTypes from 'prop-types';
import React, { Component } from 'react';
import SeriesMonitoringOptionsPopoverContent from 'AddSeries/SeriesMonitoringOptionsPopoverContent';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Popover from 'Components/Tooltip/Popover';
import { icons, inputTypes, tooltipPositions } from 'Helpers/Props';
import styles from './MonitoringOptionsModalContent.css';

const NO_CHANGE = 'noChange';

class MonitoringOptionsModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      monitor: NO_CHANGE
    };
  }

  componentDidUpdate(prevProps) {
    const {
      isSaving,
      saveError
    } = this.props;

    if (prevProps.isSaving && !isSaving && !saveError) {
      this.setState({
        monitor: NO_CHANGE
      });
    }
  }

  onInputChange = ({ name, value }) => {
    this.setState({ [name]: value });
  };

  //
  // Listeners

  onSavePress = () => {
    const {
      onSavePress
    } = this.props;
    const {
      monitor
    } = this.state;

    if (monitor !== NO_CHANGE) {
      onSavePress({ monitor });
    }
  };

  //
  // Render

  render() {
    const {
      isSaving,
      onInputChange,
      onModalClose,
      ...otherProps
    } = this.props;

    const {
      monitor
    } = this.state;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Monitor Series
        </ModalHeader>

        <ModalBody>
          <Form {...otherProps}>
            <FormGroup>
              <FormLabel>
                Monitoring

                <Popover
                  anchor={
                    <Icon
                      className={styles.labelIcon}
                      name={icons.INFO}
                    />
                  }
                  title="Monitoring Options"
                  body={<SeriesMonitoringOptionsPopoverContent />}
                  position={tooltipPositions.RIGHT}
                />
              </FormLabel>

              <FormInputGroup
                type={inputTypes.MONITOR_EPISODES_SELECT}
                name="monitor"
                value={monitor}
                includeNoChange={true}
                onChange={this.onInputChange}
              />
            </FormGroup>
          </Form>
        </ModalBody>

        <ModalFooter>
          <Button
            onPress={onModalClose}
          >
            Cancel
          </Button>

          <SpinnerButton
            isSpinning={isSaving}
            onPress={this.onSavePress}
          >
            Save
          </SpinnerButton>
        </ModalFooter>
      </ModalContent>
    );
  }
}

MonitoringOptionsModalContent.propTypes = {
  seriesId: PropTypes.number.isRequired,
  saveError: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

MonitoringOptionsModalContent.defaultProps = {
  isSaving: false
};

export default MonitoringOptionsModalContent;
