import MobileDetect from 'mobile-detect';

export default function isMobile() {
  const mobileDetect = new MobileDetect(window.navigator.userAgent);

  return mobileDetect.mobile() != null;
}
