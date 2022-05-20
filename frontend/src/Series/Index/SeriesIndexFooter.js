import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { PureComponent } from 'react';
import { ColorImpairedConsumer } from 'App/ColorImpairedContext';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import formatBytes from 'Utilities/Number/formatBytes';
import styles from './SeriesIndexFooter.css';

class SeriesIndexFooter extends PureComponent {

  //
  // Render

  render() {
    const { series } = this.props;
    const count = series.length;
    let episodes = 0;
    let episodeFiles = 0;
    let ended = 0;
    let continuing = 0;
    let monitored = 0;
    let totalFileSize = 0;

    series.forEach((s) => {
      const { statistics = {} } = s;

      const {
        episodeCount = 0,
        episodeFileCount = 0,
        sizeOnDisk = 0
      } = statistics;

      episodes += episodeCount;
      episodeFiles += episodeFileCount;

      if (s.status === 'ended') {
        ended++;
      } else {
        continuing++;
      }

      if (s.monitored) {
        monitored++;
      }

      totalFileSize += sizeOnDisk;
    });

    return (
      <ColorImpairedConsumer>
        {(enableColorImpairedMode) => {
          return (
            <div className={styles.footer}>
              <div>
                <div className={styles.legendItem}>
                  <div
                    className={classNames(
                      styles.continuing,
                      enableColorImpairedMode && 'colorImpaired'
                    )}
                  />
                  <div>Continuing (All episodes downloaded)</div>
                </div>

                <div className={styles.legendItem}>
                  <div
                    className={classNames(
                      styles.ended,
                      enableColorImpairedMode && 'colorImpaired'
                    )}
                  />
                  <div>Ended (All episodes downloaded)</div>
                </div>

                <div className={styles.legendItem}>
                  <div
                    className={classNames(
                      styles.missingMonitored,
                      enableColorImpairedMode && 'colorImpaired'
                    )}
                  />
                  <div>Missing Episodes (Series monitored)</div>
                </div>

                <div className={styles.legendItem}>
                  <div
                    className={classNames(
                      styles.missingUnmonitored,
                      enableColorImpairedMode && 'colorImpaired'
                    )}
                  />
                  <div>Missing Episodes (Series not monitored)</div>
                </div>
              </div>

              <div className={styles.statistics}>
                <DescriptionList>
                  <DescriptionListItem
                    title="Series"
                    data={count}
                  />

                  <DescriptionListItem
                    title="Ended"
                    data={ended}
                  />

                  <DescriptionListItem
                    title="Continuing"
                    data={continuing}
                  />
                </DescriptionList>

                <DescriptionList>
                  <DescriptionListItem
                    title="Monitored"
                    data={monitored}
                  />

                  <DescriptionListItem
                    title="Unmonitored"
                    data={count - monitored}
                  />
                </DescriptionList>

                <DescriptionList>
                  <DescriptionListItem
                    title="Episodes"
                    data={episodes}
                  />

                  <DescriptionListItem
                    title="Files"
                    data={episodeFiles}
                  />
                </DescriptionList>

                <DescriptionList>
                  <DescriptionListItem
                    title="Total File Size"
                    data={formatBytes(totalFileSize)}
                  />
                </DescriptionList>
              </div>
            </div>
          );
        }}
      </ColorImpairedConsumer>
    );
  }
}

SeriesIndexFooter.propTypes = {
  series: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default SeriesIndexFooter;
