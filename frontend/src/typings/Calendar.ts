import Episode from 'Episode/Episode';

export interface CalendarItem extends Omit<Episode, 'airDateUtc'> {
  airDateUtc: string;
}

export interface CalendarEvent extends CalendarItem {
  isGroup: false;
}

export interface CalendarEventGroup {
  isGroup: true;
  seriesId: number;
  seasonNumber: number;
  episodeIds: number[];
  events: CalendarItem[];
}

export type CalendarStatus =
  | 'downloaded'
  | 'downloading'
  | 'unmonitored'
  | 'onAir'
  | 'missing'
  | 'unaired';
