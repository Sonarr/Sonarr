import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TableRow from 'Components/Table/TableRow';
import { icons, kinds } from 'Helpers/Props';
import {
  testAllDownloadClients,
  testAllIndexers,
} from 'Store/Actions/settingsActions';
import { fetchHealth } from 'Store/Actions/systemActions';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';
import createHealthSelector from './createHealthSelector';
import HealthItemLink from './HealthItemLink';
import styles from './Health.css';

const columns: Column[] = [
  {
    className: styles.status,
    name: 'type',
    label: '',
    isVisible: true,
  },
  {
    name: 'message',
    label: () => translate('Message'),
    isVisible: true,
  },
  {
    name: 'actions',
    label: () => translate('Actions'),
    isVisible: true,
  },
];

function Health() {
  const dispatch = useDispatch();
  const { isFetching, isPopulated, items } = useSelector(
    createHealthSelector()
  );
  const isTestingAllDownloadClients = useSelector(
    (state: AppState) => state.settings.downloadClients.isTestingAll
  );
  const isTestingAllIndexers = useSelector(
    (state: AppState) => state.settings.indexers.isTestingAll
  );

  const healthIssues = !!items.length;

  const handleTestAllDownloadClientsPress = useCallback(() => {
    dispatch(testAllDownloadClients());
  }, [dispatch]);

  const handleTestAllIndexersPress = useCallback(() => {
    dispatch(testAllIndexers());
  }, [dispatch]);

  useEffect(() => {
    dispatch(fetchHealth());
  }, [dispatch]);

  return (
    <FieldSet
      legend={
        <div className={styles.legend}>
          {translate('Health')}

          {isFetching && isPopulated ? (
            <LoadingIndicator className={styles.loading} size={20} />
          ) : null}
        </div>
      }
    >
      {isFetching && !isPopulated ? <LoadingIndicator /> : null}

      {isPopulated && !healthIssues ? (
        <div className={styles.healthOk}>
          {translate('NoIssuesWithYourConfiguration')}
        </div>
      ) : null}

      {healthIssues ? (
        <>
          <Table columns={columns}>
            <TableBody>
              {items.map((item) => {
                const source = item.source;

                let kind = kinds.WARNING;
                switch (item.type.toLowerCase()) {
                  case 'error':
                    kind = kinds.DANGER;
                    break;
                  default:
                  case 'warning':
                    kind = kinds.WARNING;
                    break;
                  case 'notice':
                    kind = kinds.INFO;
                    break;
                }

                return (
                  <TableRow key={`health${item.message}`}>
                    <TableRowCell>
                      <Icon
                        name={icons.DANGER}
                        kind={kind}
                        title={titleCase(item.type)}
                      />
                    </TableRowCell>

                    <TableRowCell>{item.message}</TableRowCell>

                    <TableRowCell>
                      <IconButton
                        name={icons.WIKI}
                        to={item.wikiUrl}
                        title={translate('ReadTheWikiForMoreInformation')}
                      />

                      <HealthItemLink source={source} />

                      {source === 'IndexerStatusCheck' ||
                      source === 'IndexerLongTermStatusCheck' ? (
                        <SpinnerIconButton
                          name={icons.TEST}
                          title={translate('TestAll')}
                          isSpinning={isTestingAllIndexers}
                          onPress={handleTestAllIndexersPress}
                        />
                      ) : null}

                      {source === 'DownloadClientCheck' ||
                      source === 'DownloadClientStatusCheck' ? (
                        <SpinnerIconButton
                          name={icons.TEST}
                          title={translate('TestAll')}
                          isSpinning={isTestingAllDownloadClients}
                          onPress={handleTestAllDownloadClientsPress}
                        />
                      ) : null}
                    </TableRowCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>

          <Alert kind={kinds.INFO}>
            <InlineMarkdown
              data={translate('HealthMessagesInfoBox', {
                link: '/system/logs/files',
              })}
            />
          </Alert>
        </>
      ) : null}
    </FieldSet>
  );
}

export default Health;
