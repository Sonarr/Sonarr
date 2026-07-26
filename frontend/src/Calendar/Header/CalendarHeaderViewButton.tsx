import React, { useCallback } from 'react';
import { CalendarView } from 'Calendar/calendarViews';
import getCalendarViewLabel from 'Calendar/getCalendarViewLabel';
import Button, { ButtonProps } from 'Components/Link/Button';

interface CalendarHeaderViewButtonProps
  extends Omit<ButtonProps, 'children' | 'onPress'> {
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
    <Button
      aria-pressed={selectedView === view}
      {...otherProps}
      onPress={handlePress}
    >
      {getCalendarViewLabel(view)}
    </Button>
  );
}

export default CalendarHeaderViewButton;
