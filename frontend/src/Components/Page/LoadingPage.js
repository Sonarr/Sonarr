import React from 'react';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import LoadingMessage from 'Components/Loading/LoadingMessage';
import styles from './LoadingPage.css';

const sonarrLogo = 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAMAAABg3Am1AAAAsVBMVEUAAADu7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u46P1Hu7u7u7u7u7u46P1E6P1GChZAnbos6P1FRVWUuW3QsYn0paYU6P1Hu7u4AzP9FSlskdJLMzdE2SFwvWXIWl74PqdR+gYyrrLMZjrMLst6Ul6DBwsdna3hRVWXj4+QEw/QSoMkdhqiJjJa2t70sYn0oa4cHuukhfZ1ydoLY2NpcYG6foakzUWcmib9jAAAAGnRSTlMAr4DPn4/vQL8gEDDfYGBwUICf72BwgJ+An/dgjJ0AAAIcSURBVHjapZTZdqJAEEAb2UWTmcxeBYjirnFNZvv/D0svdFvQtHnIfeOcew9dJS2ziAeejxLfG8TsHeIgwhZRkN3TE+whcb1m6KEDb9jnf4rQSRTb/ucT3mHxres/Aczv+ABPbf/XDGhh+7D80ToPlKpw+1MYk3kLoIXtV4cpQGEmT6OJKfaWP+d+LfzJSG/XQzTFboEtNjvjI4bKjxF1UYnktNlr+8R1uDQ+J5VBgro45iUothxQzPJS+xioFyj2BVQrWSiMPzO+ekVgjgu0oD5sUKKmiMg+RHEFQpnXALBGzUicyPAHeCEHLw91PZPveuWPBdl1ygZ44y9Ipi+5ZDUFyZwoD4xegrXyn/OGZ1WciRKqpZqxOdVLbqhBsCFKwpAwUXMSLsBBZ4DAOdDg2A3QDmoa1B8PPngka2i51z0d2l4rrO6v1aOXsfvDVSBYECVsfRonUEXzjrrq+zRivLETIwDnuqzr40U8iceCKCn9vBfN/SKI+0HP5NMLhDt1XyiVLHZmT488yMz90ffFLua3E3ESc6D/+aoCuwBTBEwQax+Wr8IvtuuG34UorktTpEwSaF/8rtszEjZbsWZdDJhiGBm/+Icdzrf/XZ9pYuNP0GKiiy8ZM4xdvimWAGNG+K59Z1Fyn/JT+e7iK+swpr5djJlFNkIno4z1MAzRQThk/aQB9hCkzE0ajjqHGWjdSfYYJj5y/CR8sO03cenGuuEsM7AAAAAASUVORK5CYII=';

function LoadingPage() {
  return (
    <div className={styles.page}>
      <img
        className={styles.logoFull}
        src={sonarrLogo}
      />
      <LoadingMessage />
      <LoadingIndicator />
    </div>
  );
}

export default LoadingPage;
