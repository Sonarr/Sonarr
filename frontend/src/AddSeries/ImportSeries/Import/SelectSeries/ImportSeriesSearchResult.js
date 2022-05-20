import PropTypes from 'prop-types';
import React, { useCallback } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import ImportSeriesTitle from './ImportSeriesTitle';
import styles from './ImportSeriesSearchResult.css';

function ImportSeriesSearchResult(props) {
  const {
    tvdbId,
    title,
    year,
    network,
    isExistingSeries,
    onPress
  } = props;

  const onPressCallback = useCallback(() => onPress(tvdbId), [tvdbId, onPress]);

  return (
    <div className={styles.container}>
      <Link
        className={styles.series}
        onPress={onPressCallback}
      >
        <ImportSeriesTitle
          title={title}
          year={year}
          network={network}
          isExistingSeries={isExistingSeries}
        />
      </Link>

      <Link
        className={styles.tvdbLink}
        to={`http://www.thetvdb.com/?tab=series&id=${tvdbId}`}
      >
        <Icon
          className={styles.tvdbLinkIcon}
          name={icons.EXTERNAL_LINK}
          size={16}
        />
      </Link>
    </div>
  );
}

ImportSeriesSearchResult.propTypes = {
  tvdbId: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  network: PropTypes.string,
  isExistingSeries: PropTypes.bool.isRequired,
  onPress: PropTypes.func.isRequired
};

export default ImportSeriesSearchResult;
