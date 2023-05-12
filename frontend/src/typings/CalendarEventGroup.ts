import Episode from 'Episode/Episode';

export interface CalendarEvent extends Episode {
  isGroup: false;
}

interface CalendarEventGroup {
  isGroup: true;
  seriesId: number;
  seasonNumber: number;
  episodeIds: number[];
  events: Episode[];
}

export default CalendarEventGroup;
