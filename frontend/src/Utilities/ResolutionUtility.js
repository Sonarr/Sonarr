import $ from 'jquery';

module.exports = {
  resolutions: {
    desktopLarge: 1200,
    desktop: 992,
    tablet: 768,
    mobile: 480
  },

  isDesktopLarge() {
    return $(window).width() < this.resolutions.desktopLarge;
  },

  isDesktop() {
    return $(window).width() < this.resolutions.desktop;
  },

  isTablet() {
    return $(window).width() < this.resolutions.tablet;
  },

  isMobile() {
    return $(window).width() < this.resolutions.mobile;
  }
};
