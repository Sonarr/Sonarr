import PropTypes from 'prop-types';
import React from 'react';
import formatBytes from 'Utilities/Number/formatBytes';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import styles from './SeasonInfo.css';

function SeasonInfo(props) {
  const {
    totalEpisodeCount,
    monitoredEpisodeCount,
    episodeFileCount,
    sizeOnDisk
  } = props;

  return (
    <DescriptionList>
      <DescriptionListItem
        titleClassName={styles.title}
        descriptionClassName={styles.description}
        title="Total"
        data={totalEpisodeCount}
      />

      <DescriptionListItem
        titleClassName={styles.title}
        descriptionClassName={styles.description}
        title="Monitored"
        data={monitoredEpisodeCount}
      />

      <DescriptionListItem
        titleClassName={styles.title}
        descriptionClassName={styles.description}
        title="With Files"
        data={episodeFileCount}
      />

      <DescriptionListItem
        titleClassName={styles.title}
        descriptionClassName={styles.description}
        title="Size on Disk"
        data={formatBytes(sizeOnDisk)}
      />
    </DescriptionList>
  );
}

SeasonInfo.propTypes = {
  totalEpisodeCount: PropTypes.number.isRequired,
  monitoredEpisodeCount: PropTypes.number.isRequired,
  episodeFileCount: PropTypes.number.isRequired,
  sizeOnDisk: PropTypes.number.isRequired
};

export default SeasonInfo;
