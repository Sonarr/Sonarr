import React, { useCallback, useEffect, useState } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import FileBrowserModal from 'Components/FileBrowser/FileBrowserModal';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { icons, kinds, sizes } from 'Helpers/Props';
import RootFolders from 'RootFolder/RootFolders';
import useRootFolders, { useAddRootFolder } from 'RootFolder/useRootFolders';
import { useIsWindows } from 'System/Status/useSystemStatus';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './ImportSeriesSelectFolder.css';

function ImportSeriesSelectFolder() {
  const { isFetching, isFetched, error, data } = useRootFolders();
  const { addRootFolder, isAdding, addError } = useAddRootFolder();

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
      data.reduce((acc, item) => {
        if (item.id > acc) {
          return item.id;
        }

        return acc;
      }, 0);
    }
  }, [isAdding, wasAdding, addError, data]);

  return (
    <PageContent title={translate('ImportSeries')}>
      <PageContentBody>
        {isFetching && !isFetched ? <LoadingIndicator /> : null}

        {!isFetching && error ? (
          <Alert kind={kinds.DANGER}>{translate('RootFoldersLoadError')}</Alert>
        ) : null}

        {!error && isFetched && (
          <div>
            <div className={styles.header}>
              {translate('LibraryImportSeriesHeader')}
            </div>

            <div className={styles.tips}>
              {translate('LibraryImportTips')}
              <ul>
                <li className={styles.tip}>
                  <InlineMarkdown
                    data={translate(
                      'LibraryImportTipsQualityInEpisodeFilename'
                    )}
                  />
                </li>
                <li className={styles.tip}>
                  <InlineMarkdown
                    data={translate('LibraryImportTipsSeriesUseRootFolder', {
                      goodFolderExample,
                      badFolderExample,
                    })}
                  />
                </li>
                <li className={styles.tip}>
                  {translate('LibraryImportTipsDontUseDownloadsFolder')}
                </li>
              </ul>
            </div>

            {hasRootFolders ? (
              <div className={styles.recentFolders}>
                <FieldSet legend={translate('RootFolders')}>
                  <RootFolders />
                </FieldSet>
              </div>
            ) : null}

            {!isAdding && addError ? (
              <Alert className={styles.addErrorAlert} kind={kinds.DANGER}>
                {translate('AddRootFolderError')}

                <ul>
                  {Array.isArray(addError.statusBody) ? (
                    addError.statusBody.map((e, index) => {
                      return <li key={index}>{e.errorMessage}</li>;
                    })
                  ) : (
                    <li>{JSON.stringify(addError.statusBody)}</li>
                  )}
                </ul>
              </Alert>
            ) : null}

            <div className={hasRootFolders ? undefined : styles.startImport}>
              <Button
                kind={kinds.PRIMARY}
                size={sizes.LARGE}
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
        )}
      </PageContentBody>
    </PageContent>
  );
}

export default ImportSeriesSelectFolder;
