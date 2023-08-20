import PropTypes from 'prop-types';
import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
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
        title={translate('Total')}
        data={totalEpisodeCount}
      />

      <DescriptionListItem
        titleClassName={styles.title}
        descriptionClassName={styles.description}
        title={translate('Monitored')}
        data={monitoredEpisodeCount}
      />

      <DescriptionListItem
        titleClassName={styles.title}
        descriptionClassName={styles.description}
        title={translate('WithFiles')}
        data={episodeFileCount}
      />

      <DescriptionListItem
        titleClassName={styles.title}
        descriptionClassName={styles.description}
        title={translate('SizeOnDisk')}
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
