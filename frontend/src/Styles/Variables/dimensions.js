// Mirror values in Styles/Variables/variables.css so var(--foo) at runtime
// matches $foo at build time.

module.exports = {
  // Page
  pageContentBodyPadding: '32px',
  pageContentBodyPaddingSmallScreen: '16px',

  // Header
  headerHeight: '56px',

  // Sidebar
  sidebarWidth: '240px',

  // Toolbar
  toolbarHeight: '52px',
  toolbarButtonWidth: '100px',
  toolbarSeparatorMargin: '12px',

  // Break Points
  breakpointExtraSmall: '480px',
  breakpointSmall: '768px',
  breakpointMedium: '992px',
  breakpointLarge: '1200px',
  breakpointExtraLarge: '1450px',

  // Form
  formGroupExtraSmallWidth: '550px',
  formGroupSmallWidth: '650px',
  formGroupMediumWidth: '800px',
  formGroupLargeWidth: '1200px',
  formLabelSmallWidth: '150px',
  formLabelLargeWidth: '250px',
  formLabelRightMarginWidth: '20px',

  // Drag
  dragHandleWidth: '40px',
  qualityProfileItemHeight: '30px',
  qualityProfileItemDragSourcePadding: '4px',

  // Progress Bar
  progressBarSmallHeight: '5px',
  progressBarMediumHeight: '15px',
  progressBarLargeHeight: '20px',

  // Jump Bar
  jumpBarItemHeight: '25px',

  // Modal
  modalBodyPadding: '30px',

  // Series
  seriesIndexColumnPadding: '16px',
  seriesIndexColumnPaddingSmallScreen: '8px',
  seriesIndexOverviewInfoRowHeight: '21px'
};
