import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './SeasonInfo.css';

interface SeasonInfoProps {
  totalEpisodeCount: number;
  monitoredEpisodeCount: number;
  episodeFileCount: number;
  sizeOnDisk: number;
}

function SeasonInfo({
  totalEpisodeCount,
  monitoredEpisodeCount,
  episodeFileCount,
  sizeOnDisk,
}: SeasonInfoProps) {
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

export default SeasonInfo;
