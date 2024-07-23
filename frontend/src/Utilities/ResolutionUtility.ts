module.exports = {
  resolutions: {
    desktopLarge: 1200,
    desktop: 992,
    tablet: 768,
    mobile: 480,
  },

  isDesktopLarge() {
    return window.innerWidth < this.resolutions.desktopLarge;
  },

  isDesktop() {
    return window.innerWidth < this.resolutions.desktop;
  },

  isTablet() {
    return window.innerWidth < this.resolutions.tablet;
  },

  isMobile() {
    return window.innerWidth < this.resolutions.mobile;
  },
};
