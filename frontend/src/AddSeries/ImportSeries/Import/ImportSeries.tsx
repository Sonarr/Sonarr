import React, { useEffect, useMemo, useRef } from 'react';
import { useSelector } from 'react-redux';
import { useParams } from 'react-router';
import {
  setAddSeriesOption,
  useAddSeriesOption,
} from 'AddSeries/addSeriesOptionsStore';
import { SelectProvider } from 'App/Select/SelectContext';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { kinds } from 'Helpers/Props';
import useRootFolders, { useRootFolder } from 'RootFolder/useRootFolders';
import translate from 'Utilities/String/translate';
import ImportSeriesFooter from './ImportSeriesFooter';
import { clearImportSeries } from './importSeriesStore';
import ImportSeriesTable from './ImportSeriesTable';

function ImportSeries() {
  const { rootFolderId: rootFolderIdString } = useParams<{
    rootFolderId: string;
  }>();
  const rootFolderId = parseInt(rootFolderIdString);

  const {
    isFetching: rootFoldersFetching,
    isFetched: rootFoldersFetched,
    error: rootFoldersError,
    data: rootFolders,
  } = useRootFolders();

  useRootFolder(rootFolderId, false);

  const { path, unmappedFolders } = useMemo(() => {
    const rootFolder = rootFolders.find((r) => r.id === rootFolderId);

    return {
      path: rootFolder?.path ?? '',
      unmappedFolders:
        rootFolder?.unmappedFolders.map((unmappedFolders) => {
          return {
            ...unmappedFolders,
            id: unmappedFolders.name,
          };
        }) ?? [],
    };
  }, [rootFolders, rootFolderId]);

  const qualityProfiles = useSelector(
    (state: AppState) => state.settings.qualityProfiles.items
  );

  const defaultQualityProfileId = useAddSeriesOption('qualityProfileId');

  const scrollerRef = useRef<HTMLDivElement>(null);

  const items = useMemo(() => {
    return unmappedFolders.map((unmappedFolder) => {
      return {
        ...unmappedFolder,
        id: unmappedFolder.name,
      };
    });
  }, [unmappedFolders]);

  useEffect(() => {
    return () => {
      clearImportSeries();
    };
  }, [rootFolderId]);

  useEffect(() => {
    if (
      !defaultQualityProfileId ||
      !qualityProfiles.some((p) => p.id === defaultQualityProfileId)
    ) {
      setAddSeriesOption('qualityProfileId', qualityProfiles[0].id);
    }
  }, [defaultQualityProfileId, qualityProfiles]);

  return (
    <SelectProvider items={items}>
      <PageContent title={translate('ImportSeries')}>
        <PageContentBody ref={scrollerRef}>
          {rootFoldersFetching && !rootFoldersFetched ? (
            <LoadingIndicator />
          ) : null}

          {!rootFoldersFetching && !!rootFoldersError ? (
            <Alert kind={kinds.DANGER}>
              {translate('RootFoldersLoadError')}
            </Alert>
          ) : null}

          {!rootFoldersError &&
          !rootFoldersFetching &&
          rootFoldersFetched &&
          !unmappedFolders.length ? (
            <Alert kind={kinds.INFO}>
              {translate('AllSeriesInRootFolderHaveBeenImported', { path })}
            </Alert>
          ) : null}

          {!rootFoldersError &&
          rootFoldersFetched &&
          !!unmappedFolders.length &&
          scrollerRef.current ? (
            <ImportSeriesTable items={items} scrollerRef={scrollerRef} />
          ) : null}
        </PageContentBody>

        {!rootFoldersError && rootFoldersFetched && !!unmappedFolders.length ? (
          <ImportSeriesFooter />
        ) : null}
      </PageContent>
    </SelectProvider>
  );
}

export default ImportSeries;
