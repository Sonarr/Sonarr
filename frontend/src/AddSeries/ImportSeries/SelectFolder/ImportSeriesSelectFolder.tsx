import React, { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Alert from 'Components/Alert';
import FileBrowserModal from 'Components/FileBrowser/FileBrowserModal';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import SectionHeading from 'Components/SectionHeading';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { icons, kinds } from 'Helpers/Props';
import RootFolders from 'RootFolder/RootFolders';
import useRootFolders, { useAddRootFolder } from 'RootFolder/useRootFolders';
import { useIsWindows } from 'System/Status/useSystemStatus';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './ImportSeriesSelectFolder.css';

function ImportSeriesSelectFolder() {
  const { isFetching, isFetched, error, data } = useRootFolders();
  const { addRootFolder, isAdding, addError } = useAddRootFolder();
  const navigate = useNavigate();

  const isWindows = useIsWindows();

  const [isAddNewRootFolderModalOpen, setIsAddNewRootFolderModalOpen] =
    useState(false);

  const wasAdding = usePrevious(isAdding);

  const hasRootFolders = data.length > 0;
  const goodFolderExample = isWindows ? 'C:\\tv shows' : '/tv shows';
  const badFolderExample = isWindows
    ? 'C:\\tv shows\\the simpsons'
    : '/tv shows/the simpsons';

  const handleAddNewRootFolderPress = useCallback(() => {
    setIsAddNewRootFolderModalOpen(true);
  }, []);

  const handleAddRootFolderModalClose = useCallback(() => {
    setIsAddNewRootFolderModalOpen(false);
  }, []);

  const handleNewRootFolderSelect = useCallback(
    ({ value }: InputChanged<string>) => {
      addRootFolder({ path: value });
    },
    [addRootFolder]
  );

  useEffect(() => {
    if (!isAdding && wasAdding && !addError) {
      const newFolderId = data.reduce((acc, item) => {
        return item.id > acc ? item.id : acc;
      }, 0);

      navigate(`/add/import/${newFolderId}`);
    }
  }, [isAdding, wasAdding, addError, data, navigate]);

  return (
    <PageContent title={translate('ImportSeries')}>
      <PageContentBody>
        <PageHeading
          scope={translate('Media')}
          title={translate('ImportSeries')}
        />

        {isFetching && !isFetched ? <LoadingIndicator /> : null}

        {!isFetching && error ? (
          <Alert kind={kinds.DANGER}>{translate('RootFoldersLoadError')}</Alert>
        ) : null}

        {!error && isFetched ? (
          <div>
            <SectionHeading
              title={translate('LibraryImportSeriesHeader')}
              description={translate('LibraryImportSeriesDescription')}
            />

            <details className={styles.tips}>
              <summary className={styles.tipsSummary}>
                <Icon
                  className={styles.tipsChevron}
                  name={icons.EXPAND_INDETERMINATE}
                  size={12}
                />
                {translate('LibraryImportTips')}
              </summary>

              <ul className={styles.tipsList}>
                <li>
                  <InlineMarkdown
                    data={translate(
                      'LibraryImportTipsQualityInEpisodeFilename'
                    )}
                  />
                </li>
                <li>
                  <InlineMarkdown
                    data={translate('LibraryImportTipsSeriesUseRootFolder', {
                      goodFolderExample,
                      badFolderExample,
                    })}
                  />
                </li>
                <li>{translate('LibraryImportTipsDontUseDownloadsFolder')}</li>
              </ul>
            </details>

            {hasRootFolders ? <RootFolders /> : null}

            {!isAdding && addError ? (
              <Alert className={styles.addErrorAlert} kind={kinds.DANGER}>
                {translate('AddRootFolderError')}

                <ul>
                  {Array.isArray(addError.statusBody) ? (
                    addError.statusBody.map((e, index) => (
                      <li key={index}>{e.errorMessage}</li>
                    ))
                  ) : (
                    <li>{JSON.stringify(addError.statusBody)}</li>
                  )}
                </ul>
              </Alert>
            ) : null}

            <div className={styles.chooseFolderButtonContainer}>
              <Button
                kind={hasRootFolders ? kinds.DEFAULT : kinds.PRIMARY}
                onPress={handleAddNewRootFolderPress}
              >
                <Icon className={styles.importButtonIcon} name={icons.DRIVE} />
                {hasRootFolders
                  ? translate('ChooseAnotherFolder')
                  : translate('StartImport')}
              </Button>
            </div>

            <FileBrowserModal
              isOpen={isAddNewRootFolderModalOpen}
              name="rootFolderPath"
              value=""
              onChange={handleNewRootFolderSelect}
              onModalClose={handleAddRootFolderModalClose}
            />
          </div>
        ) : null}
      </PageContentBody>
    </PageContent>
  );
}

export default ImportSeriesSelectFolder;
