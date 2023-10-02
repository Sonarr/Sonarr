import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TableRow from 'Components/Table/TableRow';
import { icons, kinds } from 'Helpers/Props';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';
import styles from './Health.css';

function getInternalLink(source) {
  switch (source) {
    case 'IndexerRssCheck':
    case 'IndexerSearchCheck':
    case 'IndexerStatusCheck':
    case 'IndexerJackettAllCheck':
    case 'IndexerLongTermStatusCheck':
      return (
        <IconButton
          name={icons.SETTINGS}
          title={translate('Settings')}
          to="/settings/indexers"
        />
      );
    case 'DownloadClientCheck':
    case 'DownloadClientStatusCheck':
    case 'ImportMechanismCheck':
      return (
        <IconButton
          name={icons.SETTINGS}
          title={translate('Settings')}
          to="/settings/downloadclients"
        />
      );
    case 'NotificationStatusCheck':
      return (
        <IconButton
          name={icons.SETTINGS}
          title={translate('Settings')}
          to="/settings/connect"
        />
      );
    case 'RootFolderCheck':
      return (
        <IconButton
          name={icons.SERIES_CONTINUING}
          title={translate('SeriesEditor')}
          to="/serieseditor"
        />
      );
    case 'UpdateCheck':
      return (
        <IconButton
          name={icons.UPDATE}
          title={translate('Updates')}
          to="/system/updates"
        />
      );
    default:
      return;
  }
}

function getTestLink(source, props) {
  switch (source) {
    case 'IndexerStatusCheck':
    case 'IndexerLongTermStatusCheck':
      return (
        <SpinnerIconButton
          name={icons.TEST}
          title={translate('TestAll')}
          isSpinning={props.isTestingAllIndexers}
          onPress={props.dispatchTestAllIndexers}
        />
      );
    case 'DownloadClientCheck':
    case 'DownloadClientStatusCheck':
      return (
        <SpinnerIconButton
          name={icons.TEST}
          title={translate('TestAll')}
          isSpinning={props.isTestingAllDownloadClients}
          onPress={props.dispatchTestAllDownloadClients}
        />
      );

    default:
      break;
  }
}

const columns = [
  {
    className: styles.status,
    name: 'type',
    isVisible: true
  },
  {
    name: 'message',
    label: () => translate('Message'),
    isVisible: true
  },
  {
    name: 'actions',
    label: () => translate('Actions'),
    isVisible: true
  }
];

class Health extends Component {

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      items
    } = this.props;

    const healthIssues = !!items.length;

    return (
      <FieldSet
        legend={
          <div className={styles.legend}>
            {translate('Health')}

            {
              isFetching && isPopulated &&
                <LoadingIndicator
                  className={styles.loading}
                  size={20}
                />
            }
          </div>
        }
      >
        {
          isFetching && !isPopulated &&
            <LoadingIndicator />
        }

        {
          !healthIssues &&
            <div className={styles.healthOk}>
              {translate('NoIssuesWithYourConfiguration')}
            </div>
        }

        {
          healthIssues &&
            <Table
              columns={columns}
            >
              <TableBody>
                {
                  items.map((item) => {
                    const internalLink = getInternalLink(item.source);
                    const testLink = getTestLink(item.source, this.props);

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

                          {
                            internalLink
                          }

                          {
                            !!testLink &&
                            testLink
                          }
                        </TableRowCell>
                      </TableRow>
                    );
                  })
                }
              </TableBody>
            </Table>
        }
        {
          healthIssues &&
            <Alert kind={kinds.INFO}>
              <InlineMarkdown data={translate('HealthMessagesInfoBox', { link: '/system/logs/files' })} />
            </Alert>
        }
      </FieldSet>
    );
  }

}

Health.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  items: PropTypes.array.isRequired,
  isTestingAllDownloadClients: PropTypes.bool.isRequired,
  isTestingAllIndexers: PropTypes.bool.isRequired,
  dispatchTestAllDownloadClients: PropTypes.func.isRequired,
  dispatchTestAllIndexers: PropTypes.func.isRequired
};

export default Health;
