import PropTypes from 'prop-types';
import React, { Component } from 'react';
import titleCase from 'Utilities/String/titleCase';
import FieldSet from 'Components/FieldSet';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import StartTime from './StartTime';
import styles from './About.css';

class About extends Component {

  //
  // Render

  render() {
    const {
      version,
      isMonoRuntime,
      runtimeVersion,
      appData,
      startupPath,
      mode,
      startTime,
      timeFormat,
      longDateFormat
    } = this.props;

    return (
      <FieldSet legend="About">
        <DescriptionList className={styles.descriptionList}>
          <DescriptionListItem
            title="Version"
            data={version}
          />

          {
            isMonoRuntime &&
              <DescriptionListItem
                title="Mono Version"
                data={runtimeVersion}
              />
          }

          <DescriptionListItem
            title="AppData directory"
            data={appData}
          />

          <DescriptionListItem
            title="Startup directory"
            data={startupPath}
          />

          <DescriptionListItem
            title="Mode"
            data={titleCase(mode)}
          />

          <DescriptionListItem
            title="Uptime"
            data={
              <StartTime
                startTime={startTime}
                timeFormat={timeFormat}
                longDateFormat={longDateFormat}
              />
            }
          />
        </DescriptionList>
      </FieldSet>
    );
  }

}

About.propTypes = {
  version: PropTypes.string.isRequired,
  isMonoRuntime: PropTypes.bool.isRequired,
  runtimeVersion: PropTypes.string.isRequired,
  appData: PropTypes.string.isRequired,
  startupPath: PropTypes.string.isRequired,
  mode: PropTypes.string.isRequired,
  startTime: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  longDateFormat: PropTypes.string.isRequired
};

export default About;
