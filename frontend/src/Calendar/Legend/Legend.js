import PropTypes from 'prop-types';
import React from 'react';
import LegendItem from './LegendItem';
import styles from './Legend.css';

function Legend({ colorImpairedMode }) {
  return (
    <div className={styles.legend}>
      <div>
        <LegendItem
          name="Unaired Premiere"
          status="premiere"
          tooltip="Premiere episode hasn't aired yet"
          colorImpairedMode={colorImpairedMode}
        />

        <LegendItem
          status="unaired"
          tooltip="Episode hasn't aired yet"
          colorImpairedMode={colorImpairedMode}
        />
      </div>

      <div>
        <LegendItem
          status="downloading"
          tooltip="Episode is currently downloading"
          colorImpairedMode={colorImpairedMode}
        />

        <LegendItem
          status="downloaded"
          tooltip="Episode was downloaded and sorted"
          colorImpairedMode={colorImpairedMode}
        />
      </div>

      <div>
        <LegendItem
          name="On Air"
          status="onAir"
          tooltip="Episode is currently airing"
          colorImpairedMode={colorImpairedMode}
        />

        <LegendItem
          status="missing"
          tooltip="Episode file has not been found"
          colorImpairedMode={colorImpairedMode}
        />
      </div>

      <div>
        <LegendItem
          status="unmonitored"
          tooltip="Episode is unmonitored"
          colorImpairedMode={colorImpairedMode}
        />
      </div>
    </div>
  );
}

Legend.propTypes = {
  colorImpairedMode: PropTypes.bool.isRequired
};

export default Legend;
