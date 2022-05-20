import _ from 'lodash';
import PropTypes from 'prop-types';
import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import padNumber from 'Utilities/Number/padNumber';
import styles from './SceneInfo.css';

function SceneInfo(props) {
  const {
    seasonNumber,
    episodeNumber,
    sceneSeasonNumber,
    sceneEpisodeNumber,
    sceneAbsoluteEpisodeNumber,
    alternateTitles,
    seriesType
  } = props;

  const reducedAlternateTitles = alternateTitles.map((alternateTitle) => {
    let suffix = '';

    const altSceneSeasonNumber = sceneSeasonNumber === undefined ? seasonNumber : sceneSeasonNumber;
    const altSceneEpisodeNumber = sceneEpisodeNumber === undefined ? episodeNumber : sceneEpisodeNumber;

    const mappingSeasonNumber = alternateTitle.sceneOrigin === 'tvdb' ? seasonNumber : altSceneSeasonNumber;
    const altSeasonNumber = (alternateTitle.sceneSeasonNumber !== -1 && alternateTitle.sceneSeasonNumber !== undefined) ? alternateTitle.sceneSeasonNumber : mappingSeasonNumber;
    const altEpisodeNumber = alternateTitle.sceneOrigin === 'tvdb' ? episodeNumber : altSceneEpisodeNumber;

    if (altEpisodeNumber !== altSceneEpisodeNumber) {
      suffix = `S${padNumber(altSeasonNumber, 2)}E${padNumber(altEpisodeNumber, 2)}`;
    } else if (altSeasonNumber !== altSceneSeasonNumber) {
      suffix = `S${padNumber(altSeasonNumber, 2)}`;
    }

    return {
      alternateTitle,
      title: alternateTitle.title,
      suffix,
      comment: alternateTitle.comment
    };
  });

  const groupedAlternateTitles = _.map(_.groupBy(reducedAlternateTitles, (item) => `${item.title} ${item.suffix}`), (group) => {
    return {
      title: group[0].title,
      suffix: group[0].suffix,
      comment: _.uniq(group.map((item) => item.comment)).join('/')
    };
  });

  return (
    <DescriptionList className={styles.descriptionList}>
      {
        sceneSeasonNumber !== undefined &&
          <DescriptionListItem
            titleClassName={styles.title}
            descriptionClassName={styles.description}
            title="Season"
            data={sceneSeasonNumber}
          />
      }

      {
        sceneEpisodeNumber !== undefined &&
          <DescriptionListItem
            titleClassName={styles.title}
            descriptionClassName={styles.description}
            title="Episode"
            data={sceneEpisodeNumber}
          />
      }

      {
        seriesType === 'anime' && sceneAbsoluteEpisodeNumber !== undefined &&
          <DescriptionListItem
            titleClassName={styles.title}
            descriptionClassName={styles.description}
            title="Absolute"
            data={sceneAbsoluteEpisodeNumber}
          />
      }

      {
        !!alternateTitles.length &&
          <DescriptionListItem
            titleClassName={styles.title}
            descriptionClassName={styles.description}
            title={groupedAlternateTitles.length === 1 ? 'Title' : 'Titles'}
            data={
              <div>
                {
                  groupedAlternateTitles.map(({ title, suffix, comment }) => {
                    return (
                      <div
                        key={`${title} ${suffix}`}
                      >
                        {title}
                        {
                          suffix &&
                            <span> ({suffix})</span>
                        }
                        {
                          comment &&
                            <span className={styles.comment}> {comment}</span>
                        }
                      </div>
                    );
                  })
                }
              </div>
            }
          />
      }
    </DescriptionList>
  );
}

SceneInfo.propTypes = {
  seasonNumber: PropTypes.number,
  episodeNumber: PropTypes.number,
  sceneSeasonNumber: PropTypes.number,
  sceneEpisodeNumber: PropTypes.number,
  sceneAbsoluteEpisodeNumber: PropTypes.number,
  alternateTitles: PropTypes.arrayOf(PropTypes.object).isRequired,
  seriesType: PropTypes.string
};

export default SceneInfo;
