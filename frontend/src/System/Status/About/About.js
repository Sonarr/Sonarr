import PropTypes from 'prop-types';
import React, { Component } from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import FieldSet from 'Components/FieldSet';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';
import StartTime from './StartTime';
import styles from './About.css';

class About extends Component {

  //
  // Render

  render() {
    const {
      version,
      packageVersion,
      packageAuthor,
      isNetCore,
      isDocker,
      runtimeVersion,
      databaseVersion,
      databaseType,
      appData,
      startupPath,
      mode,
      startTime,
      timeFormat,
      longDateFormat
    } = this.props;

    return (
      <FieldSet legend={translate('About')}>
        <DescriptionList className={styles.descriptionList}>
          <DescriptionListItem
            title={translate('Version')}
            data={version}
          />

          {
            packageVersion &&
              <DescriptionListItem
                title={translate('PackageVersion')}
                data={(packageAuthor ?
                  <InlineMarkdown data={translate('PackageVersionInfo', {
                    packageVersion,
                    packageAuthor
                  })}
                  /> :
                  packageVersion
                )}
              />
          }

          {
            isNetCore &&
              <DescriptionListItem
                title={translate('DotNetVersion')}
                data={`Yes (${runtimeVersion})`}
              />
          }

          {
            isDocker &&
              <DescriptionListItem
                title={translate('Docker')}
                data={'Yes'}
              />
          }

          <DescriptionListItem
            title={translate('Database')}
            data={`${titleCase(databaseType)} ${databaseVersion}`}
          />

          <DescriptionListItem
            title={translate('AppDataDirectory')}
            data={appData}
          />

          <DescriptionListItem
            title={translate('StartupDirectory')}
            data={startupPath}
          />

          <DescriptionListItem
            title={translate('Mode')}
            data={titleCase(mode)}
          />

          <DescriptionListItem
            title={translate('Uptime')}
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
  packageVersion: PropTypes.string,
  packageAuthor: PropTypes.string,
  isNetCore: PropTypes.bool.isRequired,
  runtimeVersion: PropTypes.string.isRequired,
  isDocker: PropTypes.bool.isRequired,
  databaseType: PropTypes.string.isRequired,
  databaseVersion: PropTypes.string.isRequired,
  appData: PropTypes.string.isRequired,
  startupPath: PropTypes.string.isRequired,
  mode: PropTypes.string.isRequired,
  startTime: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  longDateFormat: PropTypes.string.isRequired
};

export default About;
