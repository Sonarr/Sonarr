import React, { useCallback, useEffect, useMemo } from 'react';
import { useDispatch, useSelector } from 'react-redux';
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
import SettingsToolbar from 'Settings/SettingsToolbar';
import {
  fetchUISettings,
  saveUISettings,
  setUISettingsValue,
} from 'Store/Actions/settingsActions';
import createLanguagesSelector from 'Store/Selectors/createLanguagesSelector';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import themes from 'Styles/Themes';
import { InputChanged } from 'typings/inputs';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';

const SECTION = 'ui';

export const firstDayOfWeekOptions = [
  {
    key: 0,
    get value() {
      return translate('Sunday');
    },
  },
  {
    key: 1,
    get value() {
      return translate('Monday');
    },
  },
];

export const weekColumnOptions = [
  { key: 'ddd M/D', value: 'Tue 3/25', hint: 'ddd M/D' },
  { key: 'ddd MM/DD', value: 'Tue 03/25', hint: 'ddd MM/DD' },
  { key: 'ddd D/M', value: 'Tue 25/3', hint: 'ddd D/M' },
  { key: 'ddd DD/MM', value: 'Tue 25/03', hint: 'ddd DD/MM' },
];

const shortDateFormatOptions = [
  { key: 'MMM D YYYY', value: 'Mar 25 2014', hint: 'MMM D YYYY' },
  { key: 'DD MMM YYYY', value: '25 Mar 2014', hint: 'DD MMM YYYY' },
  { key: 'MM/D/YYYY', value: '03/25/2014', hint: 'MM/D/YYYY' },
  { key: 'MM/DD/YYYY', value: '03/25/2014', hint: 'MM/DD/YYYY' },
  { key: 'DD/MM/YYYY', value: '25/03/2014', hint: 'DD/MM/YYYY' },
  { key: 'YYYY-MM-DD', value: '2014-03-25', hint: 'YYYY-MM-DD' },
];

const longDateFormatOptions = [
  { key: 'dddd, MMMM D YYYY', value: 'Tuesday, March 25, 2014' },
  { key: 'dddd, D MMMM YYYY', value: 'Tuesday, 25 March, 2014' },
];

export const timeFormatOptions = [
  { key: 'h(:mm)a', value: '5pm/5:30pm' },
  { key: 'HH:mm', value: '17:00/17:30' },
];

function UISettings() {
  const dispatch = useDispatch();

  const {
    items,
    isFetching: isLanguagesFetching,
    isPopulated: isLanguagesPopulated,
    error: languagesError,
  } = useSelector(
    createLanguagesSelector({
      Any: true,
      Original: true,
      Unknown: true,
    })
  );

  const {
    isFetching: isSettingsFetching,
    isPopulated: isSettingsPopulated,
    error: settingsError,
    hasSettings,
    settings,
    hasPendingChanges,
    isSaving,
    validationErrors,
    validationWarnings,
  } = useSelector(createSettingsSectionSelector(SECTION));

  const isFetching = isLanguagesFetching || isSettingsFetching;
  const isPopulated = isLanguagesPopulated && isSettingsPopulated;
  const error = languagesError || settingsError;

  const languages = useMemo(() => {
    return items.map((item) => {
      return {
        key: item.id,
        value: item.name,
      };
    });
  }, [items]);

  const themeOptions = Object.keys(themes).map((theme) => ({
    key: theme,
    value: titleCase(theme),
  }));

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - actions aren't typed
      dispatch(setUISettingsValue(change));
    },
    [dispatch]
  );
  const handleSavePress = useCallback(() => {
    dispatch(saveUISettings());
  }, [dispatch]);

  useEffect(() => {
    dispatch(fetchUISettings());

    return () => {
      // @ts-expect-error - actions aren't typed
      dispatch(setUISettingsValue({ section: `settings.${SECTION}` }));
    };
  }, [dispatch]);

  return (
    <PageContent title={translate('UiSettings')}>
      <SettingsToolbar
        hasPendingChanges={hasPendingChanges}
        isSaving={isSaving}
        onSavePress={handleSavePress}
      />

      <PageContentBody>
        {isFetching && isPopulated ? <LoadingIndicator /> : null}

        {!isFetching && error ? (
          <Alert kind={kinds.DANGER}>{translate('UiSettingsLoadError')}</Alert>
        ) : null}

        {hasSettings && isPopulated && !error ? (
          <Form
            id="uiSettings"
            validationErrors={validationErrors}
            validationWarnings={validationWarnings}
          >
            <FieldSet legend={translate('Calendar')}>
              <FormGroup>
                <FormLabel>{translate('FirstDayOfWeek')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="firstDayOfWeek"
                  values={firstDayOfWeekOptions}
                  onChange={handleInputChange}
                  {...settings.firstDayOfWeek}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('WeekColumnHeader')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="calendarWeekColumnHeader"
                  values={weekColumnOptions}
                  helpText={translate('WeekColumnHeaderHelpText')}
                  onChange={handleInputChange}
                  {...settings.calendarWeekColumnHeader}
                />
              </FormGroup>
            </FieldSet>

            <FieldSet legend={translate('Dates')}>
              <FormGroup>
                <FormLabel>{translate('ShortDateFormat')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="shortDateFormat"
                  values={shortDateFormatOptions}
                  onChange={handleInputChange}
                  {...settings.shortDateFormat}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('LongDateFormat')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="longDateFormat"
                  values={longDateFormatOptions}
                  onChange={handleInputChange}
                  {...settings.longDateFormat}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('TimeFormat')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="timeFormat"
                  values={timeFormatOptions}
                  onChange={handleInputChange}
                  {...settings.timeFormat}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('ShowRelativeDates')}</FormLabel>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="showRelativeDates"
                  helpText={translate('ShowRelativeDatesHelpText')}
                  onChange={handleInputChange}
                  {...settings.showRelativeDates}
                />
              </FormGroup>
            </FieldSet>

            <FieldSet legend={translate('Style')}>
              <FormGroup>
                <FormLabel>{translate('Theme')}</FormLabel>
                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="theme"
                  helpText={translate('ThemeHelpText')}
                  values={themeOptions}
                  onChange={handleInputChange}
                  {...settings.theme}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('EnableColorImpairedMode')}</FormLabel>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="enableColorImpairedMode"
                  helpText={translate('EnableColorImpairedModeHelpText')}
                  onChange={handleInputChange}
                  {...settings.enableColorImpairedMode}
                />
              </FormGroup>
            </FieldSet>

            <FieldSet legend={translate('Language')}>
              <FormGroup>
                <FormLabel>{translate('UiLanguage')}</FormLabel>
                <FormInputGroup
                  type={inputTypes.LANGUAGE_SELECT}
                  name="uiLanguage"
                  helpText={translate('UiLanguageHelpText')}
                  helpTextWarning={translate('BrowserReloadRequired')}
                  onChange={handleInputChange}
                  {...settings.uiLanguage}
                  errors={
                    languages.some(
                      (language) => language.key === settings.uiLanguage.value
                    )
                      ? settings.uiLanguage.errors
                      : [
                          ...settings.uiLanguage.errors,
                          { message: translate('InvalidUILanguage') },
                        ]
                  }
                />
              </FormGroup>
            </FieldSet>
          </Form>
        ) : null}
      </PageContentBody>
    </PageContent>
  );
}

export default UISettings;
