import React, { useCallback, useEffect, useState } from 'react';
import { saveDimensions, useAppValue } from 'App/appStore';
import AppUpdatedModal from 'App/AppUpdatedModal';
import ColorImpairedContext from 'App/ColorImpairedContext';
import ConnectionLostModal from 'App/ConnectionLostModal';
import SignalRListener from 'Components/SignalRListener';
import AuthenticationRequiredModal from 'FirstRun/AuthenticationRequiredModal';
import useAppPage from 'Helpers/Hooks/useAppPage';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';
import { useSystemStatusData } from 'System/Status/useSystemStatus';
import ErrorPage from './ErrorPage';
import PageHeader from './Header/PageHeader';
import LoadingPage from './LoadingPage';
import PageSidebar from './Sidebar/PageSidebar';
import styles from './Page.css';

interface PageProps {
  children: React.ReactNode;
}

function Page({ children }: PageProps) {
  const isUpdated = useAppValue('isUpdated');
  const isDisconnected = useAppValue('isDisconnected');
  const version = useAppValue('version');
  const { hasError, errors, isPopulated, isLocalStorageSupported } =
    useAppPage();
  const [isUpdatedModalOpen, setIsUpdatedModalOpen] = useState(false);
  const [isConnectionLostModalOpen, setIsConnectionLostModalOpen] =
    useState(false);

  const { enableColorImpairedMode } = useUiSettingsValues();
  const { authentication } = useSystemStatusData();

  const authenticationEnabled = authentication !== 'none';

  const handleUpdatedModalClose = useCallback(() => {
    setIsUpdatedModalOpen(false);
  }, []);

  const handleResize = useCallback(() => {
    saveDimensions({
      width: window.innerWidth,
      height: window.innerHeight,
    });
  }, []);

  useEffect(() => {
    window.addEventListener('resize', handleResize);

    return () => {
      window.removeEventListener('resize', handleResize);
    };
  }, [handleResize]);

  useEffect(() => {
    if (isDisconnected) {
      setIsConnectionLostModalOpen(true);
    }
  }, [isDisconnected]);

  useEffect(() => {
    if (isUpdated) {
      setIsUpdatedModalOpen(true);
    }
  }, [isUpdated]);

  if (hasError || !isLocalStorageSupported) {
    return (
      <ErrorPage
        {...errors}
        version={version}
        isLocalStorageSupported={isLocalStorageSupported}
      />
    );
  }

  if (!isPopulated) {
    return <LoadingPage />;
  }

  return (
    <ColorImpairedContext.Provider value={enableColorImpairedMode}>
      <div className={styles.page}>
        <SignalRListener />

        <PageHeader />

        <div className={styles.main}>
          <PageSidebar />

          {children}
        </div>

        <AppUpdatedModal
          isOpen={isUpdatedModalOpen}
          onModalClose={handleUpdatedModalClose}
        />

        <ConnectionLostModal isOpen={isConnectionLostModalOpen} />

        <AuthenticationRequiredModal isOpen={!authenticationEnabled} />
      </div>
    </ColorImpairedContext.Provider>
  );
}

export default Page;
