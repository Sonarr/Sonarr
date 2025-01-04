import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons, kinds } from 'Helpers/Props';
import { fetchRemotePathMappings } from 'Store/Actions/settingsActions';
import translate from 'Utilities/String/translate';
import EditRemotePathMappingModal from './EditRemotePathMappingModal';
import RemotePathMapping from './RemotePathMapping';
import styles from './RemotePathMappings.css';

function RemotePathMappings() {
  const dispatch = useDispatch();

  const { isFetching, isPopulated, error, items } = useSelector(
    (state: AppState) => state.settings.remotePathMappings
  );

  const [isAddRemotePathMappingModalOpen, setIsAddRemotePathMappingModalOpen] =
    useState(false);

  const handleAddRemotePathMappingPress = useCallback(() => {
    setIsAddRemotePathMappingModalOpen(true);
  }, []);

  const handleAddRemotePathMappingModalClose = useCallback(() => {
    setIsAddRemotePathMappingModalOpen(false);
  }, []);

  useEffect(() => {
    dispatch(fetchRemotePathMappings());
  }, [dispatch]);

  return (
    <FieldSet legend={translate('RemotePathMappings')}>
      <PageSectionContent
        errorMessage={translate('RemotePathMappingsLoadError')}
        error={error}
        isFetching={isFetching}
        isPopulated={isPopulated}
      >
        <Alert kind={kinds.INFO}>
          <InlineMarkdown
            data={translate('RemotePathMappingsInfo', {
              wikiLink:
                'https://wiki.servarr.com/sonarr/settings#remote-path-mappings',
            })}
          />
        </Alert>

        <div className={styles.remotePathMappingsHeader}>
          <div className={styles.host}>{translate('Host')}</div>
          <div className={styles.path}>{translate('RemotePath')}</div>
          <div className={styles.path}>{translate('LocalPath')}</div>
        </div>

        <div>
          {items.map((item) => {
            return <RemotePathMapping key={item.id} {...item} />;
          })}
        </div>

        <div className={styles.addRemotePathMapping}>
          <Link
            className={styles.addButton}
            onPress={handleAddRemotePathMappingPress}
          >
            <Icon name={icons.ADD} />
          </Link>
        </div>

        <EditRemotePathMappingModal
          isOpen={isAddRemotePathMappingModalOpen}
          onModalClose={handleAddRemotePathMappingModalClose}
        />
      </PageSectionContent>
    </FieldSet>
  );
}

export default RemotePathMappings;
