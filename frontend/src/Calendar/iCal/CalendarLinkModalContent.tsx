import React, { FocusEvent, useCallback, useMemo, useState } from 'react';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputButton from 'Components/Form/FormInputButton';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import ClipboardButton from 'Components/Link/ClipboardButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { icons, inputTypes, kinds, sizes } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';

interface CalendarLinkModalContentProps {
  onModalClose: () => void;
}

function CalendarLinkModalContent({
  onModalClose,
}: CalendarLinkModalContentProps) {
  const [state, setState] = useState({
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
    let icalUrl = `${window.location.host}${window.Sonarr.urlBase}/feed/v3/calendar/Sonarr.ics?`;

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
          <FormGroup>
            <FormLabel>{translate('IncludeUnmonitored')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="unmonitored"
              value={unmonitored}
              helpText={translate('ICalIncludeUnmonitoredEpisodesHelpText')}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('SeasonPremieresOnly')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="premieresOnly"
              value={premieresOnly}
              helpText={translate('ICalSeasonPremieresOnlyHelpText')}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('ICalShowAsAllDayEvents')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="asAllDay"
              value={asAllDay}
              helpText={translate('ICalShowAsAllDayEventsHelpText')}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Tags')}</FormLabel>

            <FormInputGroup
              type={inputTypes.SERIES_TAG}
              name="tags"
              value={tags}
              helpText={translate('ICalTagsSeriesHelpText')}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup size={sizes.LARGE}>
            <FormLabel>{translate('ICalFeed')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT}
              name="iCalHttpUrl"
              value={iCalHttpUrl}
              readOnly={true}
              helpText={translate('ICalFeedHelpText')}
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
          </FormGroup>
        </Form>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default CalendarLinkModalContent;
