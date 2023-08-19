import PropTypes from 'prop-types';
import React, { useCallback, useState } from 'react';
import Link from 'Components/Link/Link';
import EpisodeDetailsModal from 'Episode/EpisodeDetailsModal';
import FinaleType from './FinaleType';
import styles from './EpisodeTitleLink.css';

interface EpisodeTitleLinkProps {
  episodeTitle: string;
  finaleType?: string;
}

function EpisodeTitleLink(props: EpisodeTitleLinkProps) {
  const { episodeTitle, finaleType, ...otherProps } = props;
  const [isDetailsModalOpen, setIsDetailsModalOpen] = useState(false);
  const handleLinkPress = useCallback(() => {
    setIsDetailsModalOpen(true);
  }, [setIsDetailsModalOpen]);
  const handleModalClose = useCallback(() => {
    setIsDetailsModalOpen(false);
  }, [setIsDetailsModalOpen]);

  return (
    <div className={styles.container}>
      <Link className={styles.link} onPress={handleLinkPress}>
        {episodeTitle}
      </Link>

      {finaleType ? <FinaleType finaleType={finaleType} /> : null}

      <EpisodeDetailsModal
        isOpen={isDetailsModalOpen}
        episodeTitle={episodeTitle}
        {...otherProps}
        onModalClose={handleModalClose}
      />
    </div>
  );
}

EpisodeTitleLink.propTypes = {
  episodeTitle: PropTypes.string.isRequired,
  finaleType: PropTypes.string,
};

export default EpisodeTitleLink;
