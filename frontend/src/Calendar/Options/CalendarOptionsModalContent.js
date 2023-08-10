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
import translate from 'Utilities/String/translate';

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
          {translate('CalendarOptions')}
        </ModalHeader>

        <ModalBody>
          <FieldSet legend={translate('Local')}>
            <Form>
              <FormGroup>
                <FormLabel>{translate('CollapseMultipleEpisodes')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="collapseMultipleEpisodes"
                  value={collapseMultipleEpisodes}
                  helpText={translate('CollapseMultipleEpisodesHelpText')}
                  onChange={this.onOptionInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('ShowEpisodeInformation')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="showEpisodeInformation"
                  value={showEpisodeInformation}
                  helpText={translate('ShowEpisodeInformationHelpText')}
                  onChange={this.onOptionInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('IconForFinales')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="showFinaleIcon"
                  value={showFinaleIcon}
                  helpText={translate('IconForFinalesHelpText')}
                  onChange={this.onOptionInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('IconForSpecials')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="showSpecialIcon"
                  value={showSpecialIcon}
                  helpText={translate('IconForSpecialsHelpText')}
                  onChange={this.onOptionInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('IconForCutoffUnmet')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="showCutoffUnmetIcon"
                  value={showCutoffUnmetIcon}
                  helpText={translate('IconForCutoffUnmetHelpText')}
                  onChange={this.onOptionInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('FullColorEvents')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="fullColorEvents"
                  value={fullColorEvents}
                  helpText={translate('FullColorEventsHelpText')}
                  onChange={this.onOptionInputChange}
                />
              </FormGroup>
            </Form>
          </FieldSet>

          <FieldSet legend={translate('Global')}>
            <Form>
              <FormGroup>
                <FormLabel>{translate('FirstDayOfWeek')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="firstDayOfWeek"
                  values={firstDayOfWeekOptions}
                  value={firstDayOfWeek}
                  onChange={this.onGlobalInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('WeekColumnHeader')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="calendarWeekColumnHeader"
                  values={weekColumnOptions}
                  value={calendarWeekColumnHeader}
                  onChange={this.onGlobalInputChange}
                  helpText={translate('WeekColumnHeaderHelpText')}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('TimeFormat')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="timeFormat"
                  values={timeFormatOptions}
                  value={timeFormat}
                  onChange={this.onGlobalInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('EnableColorImpairedMode')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="enableColorImpairedMode"
                  value={enableColorImpairedMode}
                  helpText={translate('EnableColorImpairedModeHelpText')}
                  onChange={this.onGlobalInputChange}
                />
              </FormGroup>
            </Form>
          </FieldSet>
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            {translate('Close')}
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
