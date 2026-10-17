import React, { FocusEvent, useCallback, useMemo, useState } from 'react';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputButton from 'Components/Form/FormInputButton';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import ClipboardButton from 'Components/Link/ClipboardButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { icons, inputTypes, kinds } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';

interface CalendarLinkModalContentProps {
  onModalClose: () => void;
}

function CalendarLinkModalContent({
  onModalClose,
}: CalendarLinkModalContentProps) {
  const [state, setState] = useState<{
    unmonitored: boolean;
    premieresOnly: boolean;
    asAllDay: boolean;
    tags: number[];
  }>({
    unmonitored: false,
    premieresOnly: false,
    asAllDay: false,
    tags: [],
  });

  const { unmonitored, premieresOnly, asAllDay, tags } = state;

  const handleInputChange = useCallback(({ name, value }: InputChanged) => {
    setState((prevState) => ({ ...prevState, [name]: value }));
  }, []);

  const handleLinkFocus = useCallback(
    (event: FocusEvent<HTMLInputElement, Element>) => {
      event.target.select();
    },
    []
  );

  const { iCalHttpUrl, iCalWebCalUrl } = useMemo(() => {
    let icalUrl = `${window.location.host}${window.Sonarr.urlBase}/feed/v5/calendar/Sonarr.ics?`;

    if (unmonitored) {
      icalUrl += 'unmonitored=true&';
    }

    if (premieresOnly) {
      icalUrl += 'premieresOnly=true&';
    }

    if (asAllDay) {
      icalUrl += 'asAllDay=true&';
    }

    if (tags.length) {
      icalUrl += `tags=${tags.toString()}&`;
    }

    icalUrl += `apikey=${encodeURIComponent(window.Sonarr.apiKey)}`;

    return {
      iCalHttpUrl: `${window.location.protocol}//${icalUrl}`,
      iCalWebCalUrl: `webcal://${icalUrl}`,
    };
  }, [unmonitored, premieresOnly, asAllDay, tags]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('CalendarFeed')}</ModalHeader>

      <ModalBody>
        <Form>
          <FormRow>
            <FormLabel>{translate('IncludeUnmonitored')}</FormLabel>
            <FormInputHelpText
              text={translate('ICalIncludeUnmonitoredEpisodesHelpText')}
            />
            <FormInput
              type={inputTypes.CHECK}
              name="unmonitored"
              value={unmonitored}
              onChange={handleInputChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('SeasonPremieresOnly')}</FormLabel>
            <FormInputHelpText
              text={translate('ICalSeasonPremieresOnlyHelpText')}
            />
            <FormInput
              type={inputTypes.CHECK}
              name="premieresOnly"
              value={premieresOnly}
              onChange={handleInputChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('ICalShowAsAllDayEvents')}</FormLabel>
            <FormInputHelpText
              text={translate('ICalShowAsAllDayEventsHelpText')}
            />
            <FormInput
              type={inputTypes.CHECK}
              name="asAllDay"
              value={asAllDay}
              onChange={handleInputChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('Tags')}</FormLabel>
            <FormInputHelpText text={translate('ICalTagsSeriesHelpText')} />
            <FormInput
              type={inputTypes.SERIES_TAG}
              name="tags"
              value={tags}
              onChange={handleInputChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('ICalFeed')}</FormLabel>
            <FormInputHelpText text={translate('ICalFeedHelpText')} />
            <FormInput
              type={inputTypes.TEXT}
              name="iCalHttpUrl"
              value={iCalHttpUrl}
              readOnly={true}
              buttons={[
                <ClipboardButton
                  key="copy"
                  value={iCalHttpUrl}
                  kind={kinds.DEFAULT}
                />,

                <FormInputButton
                  key="webcal"
                  kind={kinds.DEFAULT}
                  to={iCalWebCalUrl}
                  target="_blank"
                  noRouter={true}
                >
                  <Icon name={icons.CALENDAR_O} />
                </FormInputButton>,
              ]}
              onChange={handleInputChange}
              onFocus={handleLinkFocus}
            />
          </FormRow>
        </Form>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default CalendarLinkModalContent;
