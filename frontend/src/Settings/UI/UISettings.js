import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { inputTypes } from 'Helpers/Props';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import themes from 'Styles/Themes';
import titleCase from 'Utilities/String/titleCase';

export const firstDayOfWeekOptions = [
  { key: 0, value: 'Sunday' },
  { key: 1, value: 'Monday' }
];

export const weekColumnOptions = [
  { key: 'ddd M/D', value: 'Tue 3/25' },
  { key: 'ddd MM/DD', value: 'Tue 03/25' },
  { key: 'ddd D/M', value: 'Tue 25/3' },
  { key: 'ddd DD/MM', value: 'Tue 25/03' }
];

const shortDateFormatOptions = [
  { key: 'MMM D YYYY', value: 'Mar 25 2014' },
  { key: 'DD MMM YYYY', value: '25 Mar 2014' },
  { key: 'MM/D/YYYY', value: '03/25/2014' },
  { key: 'MM/DD/YYYY', value: '03/25/2014' },
  { key: 'DD/MM/YYYY', value: '25/03/2014' },
  { key: 'YYYY-MM-DD', value: '2014-03-25' }
];

const longDateFormatOptions = [
  { key: 'dddd, MMMM D YYYY', value: 'Tuesday, March 25, 2014' },
  { key: 'dddd, D MMMM YYYY', value: 'Tuesday, 25 March, 2014' }
];

export const timeFormatOptions = [
  { key: 'h(:mm)a', value: '5pm/5:30pm' },
  { key: 'HH:mm', value: '17:00/17:30' }
];

class UISettings extends Component {

  //
  // Render

  render() {
    const {
      isFetching,
      error,
      settings,
      hasSettings,
      onInputChange,
      onSavePress,
      ...otherProps
    } = this.props;

    const themeOptions = Object.keys(themes)
      .map((theme) => ({ key: theme, value: titleCase(theme) }));

    return (
      <PageContent title="UI Settings">
        <SettingsToolbarConnector
          {...otherProps}
          onSavePress={onSavePress}
        />

        <PageContentBody>
          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            !isFetching && error &&
              <div>Unable to load UI settings</div>
          }

          {
            hasSettings && !isFetching && !error &&
              <Form
                id="uiSettings"
                {...otherProps}
              >
                <FieldSet legend="Calendar">
                  <FormGroup>
                    <FormLabel>First Day of Week</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="firstDayOfWeek"
                      values={firstDayOfWeekOptions}
                      onChange={onInputChange}
                      {...settings.firstDayOfWeek}
                    />
                  </FormGroup>

                  <FormGroup>
                    <FormLabel>Week Column Header</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="calendarWeekColumnHeader"
                      values={weekColumnOptions}
                      onChange={onInputChange}
                      helpText="Shown above each column when week is the active view"
                      {...settings.calendarWeekColumnHeader}
                    />
                  </FormGroup>
                </FieldSet>

                <FieldSet
                  legend="Dates"
                >
                  <FormGroup>
                    <FormLabel>Short Date Format</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="shortDateFormat"
                      values={shortDateFormatOptions}
                      onChange={onInputChange}
                      {...settings.shortDateFormat}
                    />
                  </FormGroup>

                  <FormGroup>
                    <FormLabel>Long Date Format</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="longDateFormat"
                      values={longDateFormatOptions}
                      onChange={onInputChange}
                      {...settings.longDateFormat}
                    />
                  </FormGroup>

                  <FormGroup>
                    <FormLabel>Time Format</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="timeFormat"
                      values={timeFormatOptions}
                      onChange={onInputChange}
                      {...settings.timeFormat}
                    />
                  </FormGroup>

                  <FormGroup>
                    <FormLabel>Show Relative Dates</FormLabel>
                    <FormInputGroup
                      type={inputTypes.CHECK}
                      name="showRelativeDates"
                      helpText="Show relative (Today/Yesterday/etc) or absolute dates"
                      onChange={onInputChange}
                      {...settings.showRelativeDates}
                    />
                  </FormGroup>
                </FieldSet>

                <FieldSet
                  legend="Style"
                >
                  <FormGroup>
                    <FormLabel>Theme</FormLabel>
                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="theme"
                      helpText="Change Application UI Theme, Inspired by Theme.Park"
                      values={themeOptions}
                      onChange={onInputChange}
                      {...settings.theme}
                    />
                  </FormGroup>

                  <FormGroup>
                    <FormLabel>Enable Color-Impaired Mode</FormLabel>
                    <FormInputGroup
                      type={inputTypes.CHECK}
                      name="enableColorImpairedMode"
                      helpText="Altered style to allow color-impaired users to better distinguish color coded information"
                      onChange={onInputChange}
                      {...settings.enableColorImpairedMode}
                    />
                  </FormGroup>
                </FieldSet>
              </Form>
          }
        </PageContentBody>
      </PageContent>
    );
  }

}

UISettings.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  settings: PropTypes.object.isRequired,
  hasSettings: PropTypes.bool.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default UISettings;
