import React, { useCallback } from 'react';
import { CalendarView } from 'Calendar/calendarViews';
import Link, { LinkProps } from 'Components/Link/Link';
import titleCase from 'Utilities/String/titleCase';

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
      {titleCase(view)}
    </Link>
  );
}

export default CalendarHeaderViewButton;
