import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppUpdatedModal from 'App/AppUpdatedModal';
import ColorImpairedContext from 'App/ColorImpairedContext';
import ConnectionLostModal from 'App/ConnectionLostModal';
import AppState from 'App/State/AppState';
import SignalRConnector from 'Components/SignalRConnector';
import AuthenticationRequiredModal from 'FirstRun/AuthenticationRequiredModal';
import useAppPage from 'Helpers/Hooks/useAppPage';
import { saveDimensions } from 'Store/Actions/appActions';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createSystemStatusSelector from 'Store/Selectors/createSystemStatusSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import ErrorPage from './ErrorPage';
import PageHeader from './Header/PageHeader';
import LoadingPage from './LoadingPage';
import PageSidebar from './Sidebar/PageSidebar';
import styles from './Page.css';

interface PageProps {
  children: React.ReactNode;
}

function Page({ children }: PageProps) {
  const dispatch = useDispatch();
  const { hasError, errors, isPopulated, isLocalStorageSupported } =
    useAppPage();
  const [isUpdatedModalOpen, setIsUpdatedModalOpen] = useState(false);
  const [isConnectionLostModalOpen, setIsConnectionLostModalOpen] =
    useState(false);

  const { enableColorImpairedMode } = useSelector(createUISettingsSelector());
  const { isSmallScreen } = useSelector(createDimensionsSelector());
  const { authentication } = useSelector(createSystemStatusSelector());
  const authenticationEnabled = authentication !== 'none';
  const { isSidebarVisible, isUpdated, isDisconnected, version } = useSelector(
    (state: AppState) => state.app
  );

  const handleUpdatedModalClose = useCallback(() => {
    setIsUpdatedModalOpen(false);
  }, []);

  const handleResize = useCallback(() => {
    dispatch(
      saveDimensions({
        width: window.innerWidth,
        height: window.innerHeight,
      })
    );
  }, [dispatch]);

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
        <SignalRConnector />

        <PageHeader />

        <div className={styles.main}>
          <PageSidebar
            isSmallScreen={isSmallScreen}
            isSidebarVisible={isSidebarVisible}
          />

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
