const dark = require('./dark');

const trueBlack = '#000000';
const elevatedBlack = '#080808';
const interactiveBlack = '#101214';
const borderBlack = '#2d3338';
const textPrimary = '#e6e8eb';
const textSecondary = '#b4bac0';
const textMuted = '#929aa2';

module.exports = {
  ...dark,

  // OLED foundation
  textColor: textPrimary,
  defaultColor: textPrimary,
  disabledColor: '#7f878e',
  dimColor: '#626970',
  offWhite: '#f4f6f8',
  helpTextColor: textMuted,
  darkGray: '#8e959c',
  gray: '#abb1b7',
  lightGray: '#d4d8dc',
  mediumGray: '#9ba2a9',

  themeDarkColor: elevatedBlack,
  themeLightColor: interactiveBlack,
  pageBackground: trueBlack,
  pageFooterBackground: trueBlack,

  inverseLabelColor: '#e0e3e6',
  disabledLabelColor: '#717980',

  defaultLinkHoverColor: '#ffffff',
  linkColor: '#70adf5',
  linkHoverColor: '#9ac6fa',

  pageHeaderBackgroundColor: '#050505',

  sidebarColor: textPrimary,
  sidebarBackgroundColor: '#050505',
  sidebarActiveBackgroundColor: '#121518',

  toolbarColor: textPrimary,
  toolbarBackgroundColor: elevatedBlack,
  toolbarMenuItemBackgroundColor: interactiveBlack,
  toolbarMenuItemHoverBackgroundColor: '#181c20',
  toolbarLabelColor: textPrimary,

  borderColor: borderBlack,
  inputBorderColor: '#454c53',
  inputBoxShadowColor: 'rgba(0, 0, 0, 0.55)',
  inputFocusBorderColor: '#66afe9',
  inputFocusBoxShadowColor: 'rgba(102, 175, 233, 0.52)',

  defaultButtonTextColor: textPrimary,
  defaultBackgroundColor: interactiveBlack,
  defaultBorderColor: '#343a40',
  defaultHoverBackgroundColor: '#1a1f23',
  defaultHoverBorderColor: '#525b63',

  iconButtonDisabledColor: '#60676d',
  iconButtonHoverColor: '#c5cbd0',
  iconButtonHoverLightColor: '#ffffff',

  modalBackdropBackgroundColor: 'rgba(0, 0, 0, 0.82)',
  modalBackgroundColor: elevatedBlack,
  modalCloseButtonHoverColor: '#c7ccd1',

  menuItemColor: textPrimary,
  menuItemHoverBackgroundColor: '#181c20',

  scrollbarBackgroundColor: '#3b4248',
  scrollbarHoverBackgroundColor: '#596169',

  cardBackgroundColor: elevatedBlack,
  cardShadowColor: 'rgba(0, 0, 0, 0.72)',
  cardAlternateBackgroundColor: '#0b0d0f',
  cardCenterBackgroundColor: '#050505',

  alertDangerBorderColor: '#a83f49',
  alertDangerBackgroundColor: 'rgba(240, 80, 80, 0.14)',
  alertDangerColor: '#f1d9dc',
  alertInfoBorderColor: '#3c789b',
  alertInfoBackgroundColor: 'rgba(93, 156, 236, 0.14)',
  alertInfoColor: '#dce9f8',
  alertSuccessBorderColor: '#397a4b',
  alertSuccessBackgroundColor: 'rgba(39, 194, 76, 0.13)',
  alertSuccessColor: '#d9efdf',
  alertWarningBorderColor: '#967033',
  alertWarningBackgroundColor: 'rgba(255, 165, 0, 0.13)',
  alertWarningColor: '#f5e7cf',

  inputBackgroundColor: interactiveBlack,
  inputReadOnlyBackgroundColor: '#070809',
  inputHoverBackgroundColor: '#171b1f',
  inputSelectedBackgroundColor: '#1d2227',
  advancedFormLabelColor: textSecondary,
  disabledCheckInputColor: '#3d444a',
  disabledInputColor: '#747b82',

  popoverTitleBackgroundColor: '#0c0e10',
  popoverTitleBorderColor: borderBlack,
  popoverBodyBackgroundColor: elevatedBlack,
  popoverShadowColor: 'rgba(0, 0, 0, 0.85)',
  popoverArrowBorderColor: borderBlack,
  popoverTitleBackgroundInverseColor: '#f4f6f8',
  popoverTitleBorderInverseColor: '#d4d8dc',
  popoverShadowInverseColor: 'rgba(0, 0, 0, 0.4)',
  popoverArrowBorderInverseColor: '#d4d8dc',

  calendarTodayBackgroundColor: '#11171c',
  calendarBackgroundColor: trueBlack,
  calendarBorderColor: borderBlack,
  calendarTextDim: '#777f86',
  calendarTextDimAlternate: '#8d959c',

  tableRowHoverBackgroundColor: '#11161a',
  addSeriesBackgroundColor: trueBlack,
  seriesBackgroundColor: trueBlack,
  searchIconContainerBackgroundColor: '#0d1012',
  collapseButtonBackgroundColor: '#0d1012',
  seasonBackgroundColor: elevatedBlack,
  episodesBackgroundColor: trueBlack,
  progressBarFrontTextColor: '#ffffff',
  progressBarBackTextColor: textPrimary,
  progressBarBackgroundColor: '#252a2f',
  logEventsBackgroundColor: '#050505'
};
