import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { inputTypes, kinds } from 'Helpers/Props';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import themes from 'Styles/Themes';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';

export const firstDayOfWeekOptions = [
  {
    key: 0,
    get value() {
      return translate('Sunday');
    }
  },
  {
    key: 1,
    get value() {
      return translate('Monday');
    }
  }
];

export const weekColumnOptions = [
  { key: 'ddd M/D', value: 'Tue 3/25', hint: 'ddd M/D' },
  { key: 'ddd MM/DD', value: 'Tue 03/25', hint: 'ddd MM/DD' },
  { key: 'ddd D/M', value: 'Tue 25/3', hint: 'ddd D/M' },
  { key: 'ddd DD/MM', value: 'Tue 25/03', hint: 'ddd DD/MM' }
];

const shortDateFormatOptions = [
  { key: 'MMM D YYYY', value: 'Mar 25 2014', hint: 'MMM D YYYY' },
  { key: 'DD MMM YYYY', value: '25 Mar 2014', hint: 'DD MMM YYYY' },
  { key: 'MM/D/YYYY', value: '03/25/2014', hint: 'MM/D/YYYY' },
  { key: 'MM/DD/YYYY', value: '03/25/2014', hint: 'MM/DD/YYYY' },
  { key: 'DD/MM/YYYY', value: '25/03/2014', hint: 'DD/MM/YYYY' },
  { key: 'YYYY-MM-DD', value: '2014-03-25', hint: 'YYYY-MM-DD' }
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
      languages,
      ...otherProps
    } = this.props;

    const themeOptions = Object.keys(themes)
      .map((theme) => ({ key: theme, value: titleCase(theme) }));

    return (
      <PageContent title={translate('UiSettings')}>
        <SettingsToolbarConnector
          {...otherProps}
          onSavePress={onSavePress}
        />

        <PageContentBody>
          {
            isFetching ?
              <LoadingIndicator /> :
              null
          }

          {
            !isFetching && error ?
              <Alert kind={kinds.DANGER}>
                {translate('UiSettingsLoadError')}
              </Alert> :
              null
          }

          {
            hasSettings && !isFetching && !error ?
              <Form
                id="uiSettings"
                {...otherProps}
              >
                <FieldSet legend={translate('Calendar')}>
                  <FormGroup>
                    <FormLabel>{translate('FirstDayOfWeek')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="firstDayOfWeek"
                      values={firstDayOfWeekOptions}
                      onChange={onInputChange}
                      {...settings.firstDayOfWeek}
                    />
                  </FormGroup>

                  <FormGroup>
                    <FormLabel>{translate('WeekColumnHeader')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="calendarWeekColumnHeader"
                      values={weekColumnOptions}
                      onChange={onInputChange}
                      helpText={translate('WeekColumnHeaderHelpText')}
                      {...settings.calendarWeekColumnHeader}
                    />
                  </FormGroup>
                </FieldSet>

                <FieldSet
                  legend={translate('Dates')}
                >
                  <FormGroup>
                    <FormLabel>{translate('ShortDateFormat')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="shortDateFormat"
                      values={shortDateFormatOptions}
                      onChange={onInputChange}
                      {...settings.shortDateFormat}
                    />
                  </FormGroup>

                  <FormGroup>
                    <FormLabel>{translate('LongDateFormat')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="longDateFormat"
                      values={longDateFormatOptions}
                      onChange={onInputChange}
                      {...settings.longDateFormat}
                    />
                  </FormGroup>

                  <FormGroup>
                    <FormLabel>{translate('TimeFormat')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="timeFormat"
                      values={timeFormatOptions}
                      onChange={onInputChange}
                      {...settings.timeFormat}
                    />
                  </FormGroup>

                  <FormGroup>
                    <FormLabel>{translate('ShowRelativeDates')}</FormLabel>
                    <FormInputGroup
                      type={inputTypes.CHECK}
                      name="showRelativeDates"
                      helpText={translate('ShowRelativeDatesHelpText')}
                      onChange={onInputChange}
                      {...settings.showRelativeDates}
                    />
                  </FormGroup>
                </FieldSet>

                <FieldSet
                  legend={translate('Style')}
                >
                  <FormGroup>
                    <FormLabel>{translate('Theme')}</FormLabel>
                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="theme"
                      helpText={translate('ThemeHelpText')}
                      values={themeOptions}
                      onChange={onInputChange}
                      {...settings.theme}
                    />
                  </FormGroup>

                  <FormGroup>
                    <FormLabel>{translate('EnableColorImpairedMode')}</FormLabel>
                    <FormInputGroup
                      type={inputTypes.CHECK}
                      name="enableColorImpairedMode"
                      helpText={translate('EnableColorImpairedModeHelpText')}
                      onChange={onInputChange}
                      {...settings.enableColorImpairedMode}
                    />
                  </FormGroup>
                </FieldSet>

                <FieldSet legend={translate('Language')}>
                  <FormGroup>
                    <FormLabel>{translate('UiLanguage')}</FormLabel>
                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="uiLanguage"
                      values={languages}
                      helpText={translate('UiLanguageHelpText')}
                      helpTextWarning={translate('BrowserReloadRequired')}
                      onChange={onInputChange}
                      {...settings.uiLanguage}
                      errors={
                        languages.some((language) => language.key === settings.uiLanguage.value) ?
                          settings.uiLanguage.errors :
                          [
                            ...settings.uiLanguage.errors,
                            { message: translate('InvalidUILanguage') }
                          ]}
                    />
                  </FormGroup>
                </FieldSet>
              </Form> :
              null
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
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  onSavePress: PropTypes.func.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default UISettings;
