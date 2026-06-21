import React, { useCallback, useEffect, useState } from 'react';
import {
  CalendarOptions,
  setCalendarOption,
  useCalendarOptions,
} from 'Calendar/calendarOptionsStore';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { OptionChanged } from 'Helpers/Hooks/useOptionsStore';
import { inputTypes } from 'Helpers/Props';
import {
  firstDayOfWeekOptions,
  timeFormatOptions,
  weekColumnOptions,
} from 'Settings/UI/UISettings';
import {
  UiSettingsModel,
  useSaveUiSettings,
  useUiSettingsValues,
} from 'Settings/UI/useUiSettings';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';

interface CalendarOptionsModalContentProps {
  onModalClose: () => void;
}

function CalendarOptionsModalContent({
  onModalClose,
}: CalendarOptionsModalContentProps) {
  const {
    collapseMultipleEpisodes,
    showCoverArt,
    showEpisodeInformation,
    showFinaleIcon,
    showSpecialIcon,
    showCutoffUnmetIcon,
    fullColorEvents,
  } = useCalendarOptions();

  const uiSettings = useUiSettingsValues();
  const saveUiSettings = useSaveUiSettings();

  const [state, setState] = useState<Partial<UiSettingsModel>>({
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
    ({ name, value }: OptionChanged<CalendarOptions>) => {
      setCalendarOption(name, value);
    },
    []
  );

  const handleGlobalInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      setState((prevState) => ({ ...prevState, [name]: value }));

      saveUiSettings({ [name]: value });
    },
    [saveUiSettings]
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
            <FormRow>
              <FormLabel>{translate('CollapseMultipleEpisodes')}</FormLabel>

              <FormInputHelpText
                text={translate('CollapseMultipleEpisodesHelpText')}
              />
              <FormInput
                type={inputTypes.CHECK}
                name="collapseMultipleEpisodes"
                value={collapseMultipleEpisodes}
                // @ts-expect-error - The typing for inputs needs more work
                onChange={handleOptionInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('ShowCoverArt')}</FormLabel>

              <FormInputHelpText text={translate('ShowCoverArtHelpText')} />
              <FormInput
                type={inputTypes.CHECK}
                name="showCoverArt"
                value={showCoverArt}
                // @ts-expect-error - The typing for inputs needs more work
                onChange={handleOptionInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('ShowEpisodeInformation')}</FormLabel>

              <FormInputHelpText
                text={translate('ShowEpisodeInformationHelpText')}
              />
              <FormInput
                type={inputTypes.CHECK}
                name="showEpisodeInformation"
                value={showEpisodeInformation}
                // @ts-expect-error - The typing for inputs needs more work
                onChange={handleOptionInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('IconForFinales')}</FormLabel>

              <FormInputHelpText text={translate('IconForFinalesHelpText')} />
              <FormInput
                type={inputTypes.CHECK}
                name="showFinaleIcon"
                value={showFinaleIcon}
                // @ts-expect-error - The typing for inputs needs more work
                onChange={handleOptionInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('IconForSpecials')}</FormLabel>

              <FormInputHelpText text={translate('IconForSpecialsHelpText')} />
              <FormInput
                type={inputTypes.CHECK}
                name="showSpecialIcon"
                value={showSpecialIcon}
                // @ts-expect-error - The typing for inputs needs more work
                onChange={handleOptionInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('IconForCutoffUnmet')}</FormLabel>

              <FormInputHelpText
                text={translate('IconForCutoffUnmetHelpText')}
              />
              <FormInput
                type={inputTypes.CHECK}
                name="showCutoffUnmetIcon"
                value={showCutoffUnmetIcon}
                // @ts-expect-error - The typing for inputs needs more work
                onChange={handleOptionInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('FullColorEvents')}</FormLabel>

              <FormInputHelpText text={translate('FullColorEventsHelpText')} />
              <FormInput
                type={inputTypes.CHECK}
                name="fullColorEvents"
                value={fullColorEvents}
                // @ts-expect-error - The typing for inputs needs more work
                onChange={handleOptionInputChange}
              />
            </FormRow>
          </Form>
        </FieldSet>

        <FieldSet legend={translate('Global')}>
          <Form>
            <FormRow>
              <FormLabel>{translate('FirstDayOfWeek')}</FormLabel>

              <FormInput
                type={inputTypes.SELECT}
                name="firstDayOfWeek"
                values={firstDayOfWeekOptions}
                value={firstDayOfWeek}
                onChange={handleGlobalInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('WeekColumnHeader')}</FormLabel>

              <FormInputHelpText text={translate('WeekColumnHeaderHelpText')} />
              <FormInput
                type={inputTypes.SELECT}
                name="calendarWeekColumnHeader"
                values={weekColumnOptions}
                value={calendarWeekColumnHeader}
                onChange={handleGlobalInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('TimeFormat')}</FormLabel>

              <FormInput
                type={inputTypes.SELECT}
                name="timeFormat"
                values={timeFormatOptions}
                value={timeFormat}
                onChange={handleGlobalInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('EnableColorImpairedMode')}</FormLabel>

              <FormInputHelpText
                text={translate('EnableColorImpairedModeHelpText')}
              />
              <FormInput
                type={inputTypes.CHECK}
                name="enableColorImpairedMode"
                value={enableColorImpairedMode}
                onChange={handleGlobalInputChange}
              />
            </FormRow>
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
