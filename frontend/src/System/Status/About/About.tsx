import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import FieldSet from 'Components/FieldSet';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import { fetchStatus } from 'Store/Actions/systemActions';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';
import StartTime from './StartTime';
import styles from './About.css';

function About() {
  const dispatch = useDispatch();
  const { item } = useSelector((state: AppState) => state.system.status);

  const {
    version,
    packageVersion,
    packageAuthor,
    isNetCore,
    isDocker,
    runtimeVersion,
    databaseVersion,
    databaseType,
    migrationVersion,
    appData,
    startupPath,
    mode,
    startTime,
  } = item;

  useEffect(() => {
    dispatch(fetchStatus());
  }, [dispatch]);

  return (
    <FieldSet legend={translate('About')}>
      <DescriptionList className={styles.descriptionList}>
        <DescriptionListItem title={translate('Version')} data={version} />

        {packageVersion && (
          <DescriptionListItem
            title={translate('PackageVersion')}
            data={
              packageAuthor ? (
                <InlineMarkdown
                  data={translate('PackageVersionInfo', {
                    packageVersion,
                    packageAuthor,
                  })}
                />
              ) : (
                packageVersion
              )
            }
          />
        )}

        {isNetCore ? (
          <DescriptionListItem
            title={translate('DotNetVersion')}
            data={`Yes (${runtimeVersion})`}
          />
        ) : null}

        {isDocker ? (
          <DescriptionListItem title={translate('Docker')} data="Yes" />
        ) : null}

        <DescriptionListItem
          title={translate('Database')}
          data={`${titleCase(databaseType)} ${databaseVersion}`}
        />

        <DescriptionListItem
          title={translate('DatabaseMigration')}
          data={migrationVersion}
        />

        <DescriptionListItem
          title={translate('AppDataDirectory')}
          data={appData}
        />

        <DescriptionListItem
          title={translate('StartupDirectory')}
          data={startupPath}
        />

        <DescriptionListItem title={translate('Mode')} data={titleCase(mode)} />

        <DescriptionListItem
          title={translate('Uptime')}
          data={<StartTime startTime={startTime} />}
        />
      </DescriptionList>
    </FieldSet>
  );
}

export default About;
