import PropTypes from 'prop-types';
import React from 'react';
import PageContent from 'Components/Page/PageContent';
import translate from 'Utilities/String/translate';
import styles from './NotFound.css';

function NotFound(props) {
  const { message = translate('DefaultNotFoundMessage') } = props;

  return (
    <PageContent title="MIA">
      <div className={styles.container}>
        <div className={styles.message}>
          {message}
        </div>

        <img
          className={styles.image}
          src={`${window.Sonarr.urlBase}/Content/Images/404.png`}
        />
      </div>
    </PageContent>
  );
}

NotFound.propTypes = {
  message: PropTypes.string
};

export default NotFound;
