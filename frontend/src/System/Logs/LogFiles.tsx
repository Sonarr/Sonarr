import React from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { icons, kinds } from 'Helpers/Props';
import LogFile from 'typings/LogFile';
import combinePath from 'Utilities/String/combinePath';
import translate from 'Utilities/String/translate';
import LogFilesTableRow from './LogFilesTableRow';
import LogsNavMenu from './LogsNavMenu';

const columns: Column[] = [
  {
    name: 'filename',
    label: () => translate('Filename'),
    isVisible: true,
  },
  {
    name: 'lastWriteTime',
    label: () => translate('LastWriteTime'),
    isVisible: true,
  },
  {
    name: 'download',
    label: '',
    isVisible: true,
  },
];

type LogFileType = 'app' | 'update';

interface LogFilesProps {
  isFetching: boolean;
  items: LogFile[];
  isDeleteFilesExecuting: boolean;
  type: LogFileType;
  onRefreshPress: () => void;
  onDeleteFilesPress: () => void;
}

function LogFiles({
  isFetching,
  items,
  isDeleteFilesExecuting,
  type,
  onRefreshPress,
  onDeleteFilesPress,
  ...otherProps
}: LogFilesProps) {
  const { appData, isWindows } = useSelector(
    (state: AppState) => state.system.status.item
  );

  const currentLogView =
    type === 'update' ? translate('UpdaterLogFiles') : translate('LogFiles');

  const location = combinePath(isWindows, appData, [
    type === 'update' ? 'UpdateLogs' : 'logs',
  ]);

  return (
    <PageContent title={translate('LogFiles')}>
      <PageToolbar>
        <PageToolbarSection>
          <LogsNavMenu current={currentLogView} />

          <PageToolbarSeparator />

          <PageToolbarButton
            label={translate('Refresh')}
            iconName={icons.REFRESH}
            spinningName={icons.REFRESH}
            isSpinning={isFetching}
            onPress={onRefreshPress}
          />

          <PageToolbarButton
            label={translate('Clear')}
            iconName={icons.CLEAR}
            isSpinning={isDeleteFilesExecuting}
            onPress={onDeleteFilesPress}
          />
        </PageToolbarSection>
      </PageToolbar>
      <PageContentBody>
        <Alert>
          <div>
            {translate('LogFilesLocation', {
              location,
            })}
          </div>

          {currentLogView === 'Log Files' ? (
            <div>
              <InlineMarkdown data={translate('TheLogLevelDefault')} />
            </div>
          ) : null}
        </Alert>

        {isFetching ? <LoadingIndicator /> : null}

        {!isFetching && items.length ? (
          <div>
            <Table columns={columns} {...otherProps}>
              <TableBody>
                {items.map((item) => {
                  return <LogFilesTableRow key={item.id} {...item} />;
                })}
              </TableBody>
            </Table>
          </div>
        ) : null}

        {!isFetching && !items.length ? (
          <Alert kind={kinds.INFO}>{translate('NoLogFiles')}</Alert>
        ) : null}
      </PageContentBody>
    </PageContent>
  );
}

export default LogFiles;
