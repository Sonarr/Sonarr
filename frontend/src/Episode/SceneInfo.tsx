import React, { useMemo } from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import { AlternateTitle } from 'Series/Series';
import padNumber from 'Utilities/Number/padNumber';
import translate from 'Utilities/String/translate';
import styles from './SceneInfo.css';

interface SceneInfoProps {
  seasonNumber?: number;
  episodeNumber?: number;
  sceneSeasonNumber?: number;
  sceneEpisodeNumber?: number;
  sceneAbsoluteEpisodeNumber?: number;
  alternateTitles: AlternateTitle[];
  seriesType?: string;
}

function SceneInfo(props: SceneInfoProps) {
  const {
    seasonNumber,
    episodeNumber,
    sceneSeasonNumber,
    sceneEpisodeNumber,
    sceneAbsoluteEpisodeNumber,
    alternateTitles,
    seriesType,
  } = props;

  const groupedAlternateTitles = useMemo(() => {
    const reducedAlternateTitles = alternateTitles.map((alternateTitle) => {
      let suffix = '';

      const altSceneSeasonNumber =
        sceneSeasonNumber === undefined ? seasonNumber : sceneSeasonNumber;
      const altSceneEpisodeNumber =
        sceneEpisodeNumber === undefined ? episodeNumber : sceneEpisodeNumber;

      const mappingSeasonNumber =
        alternateTitle.sceneOrigin === 'tvdb'
          ? seasonNumber
          : altSceneSeasonNumber;
      const altSeasonNumber =
        alternateTitle.sceneSeasonNumber !== -1 &&
        alternateTitle.sceneSeasonNumber !== undefined
          ? alternateTitle.sceneSeasonNumber
          : mappingSeasonNumber;
      const altEpisodeNumber =
        alternateTitle.sceneOrigin === 'tvdb'
          ? episodeNumber
          : altSceneEpisodeNumber;

      if (altEpisodeNumber !== altSceneEpisodeNumber) {
        suffix = `S${padNumber(altSeasonNumber as number, 2)}E${padNumber(
          altEpisodeNumber as number,
          2
        )}`;
      } else if (altSeasonNumber !== altSceneSeasonNumber) {
        suffix = `S${padNumber(altSeasonNumber as number, 2)}`;
      }

      return {
        alternateTitle,
        title: alternateTitle.title,
        suffix,
        comment: alternateTitle.comment,
      };
    });

    return Object.values(
      reducedAlternateTitles.reduce(
        (
          acc: Record<
            string,
            { title: string; suffix: string; comment: string }
          >,
          alternateTitle
        ) => {
          const key = alternateTitle.suffix
            ? `${alternateTitle.title} ${alternateTitle.suffix}`
            : alternateTitle.title;
          const item = acc[key];

          if (item) {
            item.comment = alternateTitle.comment
              ? `${item.comment}/${alternateTitle.comment}`
              : item.comment;
          } else {
            acc[key] = {
              title: alternateTitle.title,
              suffix: alternateTitle.suffix,
              comment: alternateTitle.comment ?? '',
            };
          }

          return acc;
        },
        {}
      )
    );
  }, [
    alternateTitles,
    seasonNumber,
    episodeNumber,
    sceneSeasonNumber,
    sceneEpisodeNumber,
  ]);

  return (
    <DescriptionList className={styles.descriptionList}>
      {sceneSeasonNumber === undefined ? null : (
        <DescriptionListItem
          titleClassName={styles.title}
          descriptionClassName={styles.description}
          title={translate('Season')}
          data={sceneSeasonNumber}
        />
      )}

      {sceneEpisodeNumber === undefined ? null : (
        <DescriptionListItem
          titleClassName={styles.title}
          descriptionClassName={styles.description}
          title={translate('Episode')}
          data={sceneEpisodeNumber}
        />
      )}

      {seriesType === 'anime' && sceneAbsoluteEpisodeNumber !== undefined ? (
        <DescriptionListItem
          titleClassName={styles.title}
          descriptionClassName={styles.description}
          title={translate('Absolute')}
          data={sceneAbsoluteEpisodeNumber}
        />
      ) : null}

      {alternateTitles.length ? (
        <DescriptionListItem
          titleClassName={styles.title}
          descriptionClassName={styles.description}
          title={
            groupedAlternateTitles.length === 1
              ? translate('Title')
              : translate('Titles')
          }
          data={
            <div>
              {groupedAlternateTitles.map(({ title, suffix, comment }) => {
                return (
                  <div key={`${title} ${suffix}`}>
                    {title}
                    {suffix && <span> ({suffix})</span>}
                    {comment ? (
                      <span className={styles.comment}> {comment}</span>
                    ) : null}
                  </div>
                );
              })}
            </div>
          }
        />
      ) : null}
    </DescriptionList>
  );
}

export default SceneInfo;
