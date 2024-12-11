import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
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
import {
  firstDayOfWeekOptions,
  timeFormatOptions,
  weekColumnOptions,
} from 'Settings/UI/UISettings';
import { setCalendarOption } from 'Store/Actions/calendarActions';
import { saveUISettings } from 'Store/Actions/settingsActions';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import { InputChanged } from 'typings/inputs';
import UiSettings from 'typings/Settings/UiSettings';
import translate from 'Utilities/String/translate';

interface CalendarOptionsModalContentProps {
  onModalClose: () => void;
}

function CalendarOptionsModalContent({
  onModalClose,
}: CalendarOptionsModalContentProps) {
  const dispatch = useDispatch();

  const {
    collapseMultipleEpisodes,
    showEpisodeInformation,
    showFinaleIcon,
    showSpecialIcon,
    showCutoffUnmetIcon,
    fullColorEvents,
  } = useSelector((state: AppState) => state.calendar.options);

  const uiSettings = useSelector(createUISettingsSelector());

  const [state, setState] = useState<Partial<UiSettings>>({
    firstDayOfWeek: uiSettings.firstDayOfWeek,
    calendarWeekColumnHeader: uiSettings.calendarWeekColumnHeader,
    timeFormat: uiSettings.timeFormat,
    enableColorImpairedMode: uiSettings.enableColorImpairedMode,
  });

  const {
    firstDayOfWeek,
    calendarWeekColumnHeader,
    timeFormat,
    enableColorImpairedMode,
  } = state;

  const handleOptionInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      dispatch(setCalendarOption({ [name]: value }));
    },
    [dispatch]
  );

  const handleGlobalInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      setState((prevState) => ({ ...prevState, [name]: value }));

      dispatch(saveUISettings({ [name]: value }));
    },
    [dispatch]
  );

  useEffect(() => {
    setState({
      firstDayOfWeek: uiSettings.firstDayOfWeek,
      calendarWeekColumnHeader: uiSettings.calendarWeekColumnHeader,
      timeFormat: uiSettings.timeFormat,
      enableColorImpairedMode: uiSettings.enableColorImpairedMode,
    });
  }, [uiSettings]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('CalendarOptions')}</ModalHeader>

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
                onChange={handleOptionInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('ShowEpisodeInformation')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showEpisodeInformation"
                value={showEpisodeInformation}
                helpText={translate('ShowEpisodeInformationHelpText')}
                onChange={handleOptionInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('IconForFinales')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showFinaleIcon"
                value={showFinaleIcon}
                helpText={translate('IconForFinalesHelpText')}
                onChange={handleOptionInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('IconForSpecials')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showSpecialIcon"
                value={showSpecialIcon}
                helpText={translate('IconForSpecialsHelpText')}
                onChange={handleOptionInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('IconForCutoffUnmet')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showCutoffUnmetIcon"
                value={showCutoffUnmetIcon}
                helpText={translate('IconForCutoffUnmetHelpText')}
                onChange={handleOptionInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('FullColorEvents')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="fullColorEvents"
                value={fullColorEvents}
                helpText={translate('FullColorEventsHelpText')}
                onChange={handleOptionInputChange}
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
                onChange={handleGlobalInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('WeekColumnHeader')}</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="calendarWeekColumnHeader"
                values={weekColumnOptions}
                value={calendarWeekColumnHeader}
                helpText={translate('WeekColumnHeaderHelpText')}
                onChange={handleGlobalInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('TimeFormat')}</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="timeFormat"
                values={timeFormatOptions}
                value={timeFormat}
                onChange={handleGlobalInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('EnableColorImpairedMode')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="enableColorImpairedMode"
                value={enableColorImpairedMode}
                helpText={translate('EnableColorImpairedModeHelpText')}
                onChange={handleGlobalInputChange}
              />
            </FormGroup>
          </Form>
        </FieldSet>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default CalendarOptionsModalContent;
