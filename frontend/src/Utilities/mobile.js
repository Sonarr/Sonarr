import MobileDetect from 'mobile-detect';

const mobileDetect = new MobileDetect(window.navigator.userAgent);

export function isMobile() {

  return mobileDetect.mobile() != null;
}

export function isIOS() {
  return mobileDetect.is('iOS');
}
