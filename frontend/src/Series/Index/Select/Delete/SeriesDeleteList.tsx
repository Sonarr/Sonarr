import React from 'react';
import Series from 'Series/Series';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';

interface SeriesDeleteListStyles {
  pathContainer: string;
  path: string;
  statistics: string;
  deleteFilesMessage: string;
}

interface SeriesDeleteListProps {
  series: Series[];
  showFileDetails: boolean;
  totalEpisodeFileCount: number;
  totalSizeOnDisk: number;
  styles: SeriesDeleteListStyles;
}

function SeriesDeleteList({
  series,
  showFileDetails,
  totalEpisodeFileCount,
  totalSizeOnDisk,
  styles,
}: SeriesDeleteListProps) {
  return (
    <>
      <ul>
        {series.map(({ title, path, statistics = {} }) => {
          const { episodeFileCount = 0, sizeOnDisk = 0 } = statistics;

          return (
            <li key={title}>
              <span>{title}</span>

              {showFileDetails ? (
                <span>
                  <span className={styles.pathContainer}>
                    -<span className={styles.path}>{path}</span>
                  </span>

                  {episodeFileCount ? (
                    <span className={styles.statistics}>
                      (
                      {translate('DeleteSeriesFolderEpisodeCount', {
                        episodeFileCount,
                        size: formatBytes(sizeOnDisk),
                      })}
                      )
                    </span>
                  ) : null}
                </span>
              ) : null}
            </li>
          );
        })}
      </ul>

      {showFileDetails && totalEpisodeFileCount ? (
        <div className={styles.deleteFilesMessage}>
          {translate('DeleteSeriesFolderEpisodeCount', {
            episodeFileCount: totalEpisodeFileCount,
            size: formatBytes(totalSizeOnDisk),
          })}
        </div>
      ) : null}
    </>
  );
}

export default SeriesDeleteList;
