import React, { useCallback } from 'react';
import { CalendarView } from 'Calendar/calendarViews';
import Link, { LinkProps } from 'Components/Link/Link';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';

const getTitle = (view: CalendarView) => {
  switch (view) {
    case 'agenda':
      return translate('Agenda');
    case 'day':
      return translate('Day');
    case 'forecast':
      return translate('Forecast');
    case 'month':
      return translate('Month');
    case 'week':
      return translate('Week');
    default:
      return titleCase(view);
  }
};

interface CalendarHeaderViewButtonProps
  extends Omit<LinkProps, 'children' | 'onPress'> {
  view: CalendarView;
  selectedView: CalendarView;
  onPress: (view: CalendarView) => void;
}

function CalendarHeaderViewButton({
  view,
  selectedView,
  onPress,
  ...otherProps
}: CalendarHeaderViewButtonProps) {
  const handlePress = useCallback(() => {
    onPress(view);
  }, [view, onPress]);

  return (
    <Link
      isDisabled={selectedView === view}
      {...otherProps}
      onPress={handlePress}
    >
      {getTitle(view)}
    </Link>
  );
}

export default CalendarHeaderViewButton;
