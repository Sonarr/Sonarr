import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { DownloadClientAppState } from 'App/State/SettingsAppState';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import { fetchDownloadClients } from 'Store/Actions/settingsActions';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import DownloadClientModel from 'typings/DownloadClient';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import AddDownloadClientModal from './AddDownloadClientModal';
import DownloadClient from './DownloadClient';
import EditDownloadClientModal from './EditDownloadClientModal';
import styles from './DownloadClients.css';

function DownloadClients() {
  const dispatch = useDispatch();

  const { error, isFetching, isPopulated, items } = useSelector(
    createSortedSectionSelector<DownloadClientModel, DownloadClientAppState>(
      'settings.downloadClients',
      sortByProp('name')
    )
  );

  const [isAddDownloadClientModalOpen, setIsAddDownloadClientModalOpen] =
    useState(false);

  const [isEditDownloadClientModalOpen, setIsEditDownloadClientModalOpen] =
    useState(false);

  const handleAddDownloadClientPress = useCallback(() => {
    setIsAddDownloadClientModalOpen(true);
  }, []);

  const handleDownloadClientSelect = useCallback(() => {
    setIsAddDownloadClientModalOpen(false);
    setIsEditDownloadClientModalOpen(true);
  }, []);

  const handleAddDownloadClientModalClose = useCallback(() => {
    setIsAddDownloadClientModalOpen(false);
  }, []);

  const handleEditDownloadClientModalClose = useCallback(() => {
    setIsEditDownloadClientModalOpen(false);
  }, []);

  useEffect(() => {
    dispatch(fetchDownloadClients());
  }, [dispatch]);

  return (
    <FieldSet legend={translate('DownloadClients')}>
      <PageSectionContent
        errorMessage={translate('DownloadClientsLoadError')}
        error={error}
        isFetching={isFetching}
        isPopulated={isPopulated}
      >
        <div className={styles.downloadClients}>
          {items.map((item) => {
            return <DownloadClient key={item.id} {...item} />;
          })}

          <Card
            className={styles.addDownloadClient}
            onPress={handleAddDownloadClientPress}
          >
            <div className={styles.center}>
              <Icon name={icons.ADD} size={45} />
            </div>
          </Card>
        </div>

        <AddDownloadClientModal
          isOpen={isAddDownloadClientModalOpen}
          onDownloadClientSelect={handleDownloadClientSelect}
          onModalClose={handleAddDownloadClientModalClose}
        />

        <EditDownloadClientModal
          isOpen={isEditDownloadClientModalOpen}
          onModalClose={handleEditDownloadClientModalClose}
        />
      </PageSectionContent>
    </FieldSet>
  );
}

export default DownloadClients;
