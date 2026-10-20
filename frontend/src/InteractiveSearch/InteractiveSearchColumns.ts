import { CSSProperties } from 'react';

const COLUMN_WIDTHS = {
  release: 400,
  age: 92,
  size: 84,
  peers: 130,
  quality: 170,
  score: 64,
  actions: 76,
};

const COLUMN_GAP = 16;
const ROW_PADDING = 15;

const widths = Object.values(COLUMN_WIDTHS);

export const ROW_LAYOUT_MIN_WIDTH =
  widths.reduce((total, width) => total + width, 0) +
  COLUMN_GAP * (widths.length - 1) +
  ROW_PADDING * 2;

export const COLUMN_STYLE = {
  '--columnReleaseMinWidth': `${COLUMN_WIDTHS.release}px`,
  '--columnAgeWidth': `${COLUMN_WIDTHS.age}px`,
  '--columnSizeWidth': `${COLUMN_WIDTHS.size}px`,
  '--columnPeersWidth': `${COLUMN_WIDTHS.peers}px`,
  '--columnQualityWidth': `${COLUMN_WIDTHS.quality}px`,
  '--columnScoreWidth': `${COLUMN_WIDTHS.score}px`,
  '--columnActionsWidth': `${COLUMN_WIDTHS.actions}px`,
  '--columnGap': `${COLUMN_GAP}px`,
  '--columnRowPadding': `${ROW_PADDING}px`,
} as CSSProperties;
