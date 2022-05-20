import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FieldSet from 'Components/FieldSet';
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
import { firstDayOfWeekOptions, timeFormatOptions, weekColumnOptions } from 'Settings/UI/UISettings';

class CalendarOptionsModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const {
      firstDayOfWeek,
      calendarWeekColumnHeader,
      timeFormat,
      enableColorImpairedMode,
      fullColorEvents
    } = props;

    this.state = {
      firstDayOfWeek,
      calendarWeekColumnHeader,
      timeFormat,
      enableColorImpairedMode,
      fullColorEvents
    };
  }

  componentDidUpdate(prevProps) {
    const {
      firstDayOfWeek,
      calendarWeekColumnHeader,
      timeFormat,
      enableColorImpairedMode
    } = this.props;

    if (
      prevProps.firstDayOfWeek !== firstDayOfWeek ||
      prevProps.calendarWeekColumnHeader !== calendarWeekColumnHeader ||
      prevProps.timeFormat !== timeFormat ||
      prevProps.enableColorImpairedMode !== enableColorImpairedMode
    ) {
      this.setState({
        firstDayOfWeek,
        calendarWeekColumnHeader,
        timeFormat,
        enableColorImpairedMode
      });
    }
  }

  //
  // Listeners

  onOptionInputChange = ({ name, value }) => {
    const {
      dispatchSetCalendarOption
    } = this.props;

    dispatchSetCalendarOption({ [name]: value });
  };

  onGlobalInputChange = ({ name, value }) => {
    const {
      dispatchSaveUISettings
    } = this.props;

    const setting = { [name]: value };

    this.setState(setting, () => {
      dispatchSaveUISettings(setting);
    });
  };

  onLinkFocus = (event) => {
    event.target.select();
  };

  //
  // Render

  render() {
    const {
      collapseMultipleEpisodes,
      showEpisodeInformation,
      showFinaleIcon,
      showSpecialIcon,
      showCutoffUnmetIcon,
      fullColorEvents,
      onModalClose
    } = this.props;

    const {
      firstDayOfWeek,
      calendarWeekColumnHeader,
      timeFormat,
      enableColorImpairedMode
    } = this.state;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Calendar Options
        </ModalHeader>

        <ModalBody>
          <FieldSet legend="Local">
            <Form>
              <FormGroup>
                <FormLabel>Collapse Multiple Episodes</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="collapseMultipleEpisodes"
                  value={collapseMultipleEpisodes}
                  helpText="Collapse multiple episodes airing on the same day"
                  onChange={this.onOptionInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Show Episode Information</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="showEpisodeInformation"
                  value={showEpisodeInformation}
                  helpText="Show episode title and number"
                  onChange={this.onOptionInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Icon for Finales</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="showFinaleIcon"
                  value={showFinaleIcon}
                  helpText="Show icon for series/season finales based on available episode information"
                  onChange={this.onOptionInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Icon for Specials</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="showSpecialIcon"
                  value={showSpecialIcon}
                  helpText="Show icon for special episodes (season 0)"
                  onChange={this.onOptionInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Icon for Cutoff Unmet</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="showCutoffUnmetIcon"
                  value={showCutoffUnmetIcon}
                  helpText="Show icon for files when the cutoff hasn't been met"
                  onChange={this.onOptionInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Full Color Events</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="fullColorEvents"
                  value={fullColorEvents}
                  helpText="Altered style to color the entire event with the status color, instead of just the left edge. Does not apply to Agenda"
                  onChange={this.onOptionInputChange}
                />
              </FormGroup>
            </Form>
          </FieldSet>

          <FieldSet legend="Global">
            <Form>
              <FormGroup>
                <FormLabel>First Day of Week</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="firstDayOfWeek"
                  values={firstDayOfWeekOptions}
                  value={firstDayOfWeek}
                  onChange={this.onGlobalInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Week Column Header</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="calendarWeekColumnHeader"
                  values={weekColumnOptions}
                  value={calendarWeekColumnHeader}
                  onChange={this.onGlobalInputChange}
                  helpText="Shown above each column when week is the active view"
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Time Format</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="timeFormat"
                  values={timeFormatOptions}
                  value={timeFormat}
                  onChange={this.onGlobalInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Enable Color-Impaired Mode</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="enableColorImpairedMode"
                  value={enableColorImpairedMode}
                  helpText="Altered style to allow color-impaired users to better distinguish color coded information"
                  onChange={this.onGlobalInputChange}
                />
              </FormGroup>
            </Form>
          </FieldSet>
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            Close
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

CalendarOptionsModalContent.propTypes = {
  collapseMultipleEpisodes: PropTypes.bool.isRequired,
  showEpisodeInformation: PropTypes.bool.isRequired,
  showFinaleIcon: PropTypes.bool.isRequired,
  showSpecialIcon: PropTypes.bool.isRequired,
  showCutoffUnmetIcon: PropTypes.bool.isRequired,
  firstDayOfWeek: PropTypes.number.isRequired,
  calendarWeekColumnHeader: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  enableColorImpairedMode: PropTypes.bool.isRequired,
  fullColorEvents: PropTypes.bool.isRequired,
  dispatchSetCalendarOption: PropTypes.func.isRequired,
  dispatchSaveUISettings: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default CalendarOptionsModalContent;
