import PropTypes from 'prop-types';
import React, { Component } from 'react';
import titleCase from 'Utilities/String/titleCase';
import { icons, kinds } from 'Helpers/Props';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FieldSet from 'Components/FieldSet';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import styles from './Health.css';

function getInternalLink(source) {
  switch (source) {
    case 'IndexerRssCheck':
    case 'IndexerSearchCheck':
    case 'IndexerStatusCheck':
      return (
        <IconButton
          name={icons.SETTINGS}
          title="Settings"
          to="/settings/indexers"
        />
      );
    case 'DownloadClientCheck':
    case 'ImportMechanismCheck':
      return (
        <IconButton
          name={icons.SETTINGS}
          title="Settings"
          to="/settings/downloadclients"
        />
      );
    case 'RootFolderCheck':
      return (
        <IconButton
          name={icons.SERIES}
          title="Series Editor"
          to="/serieseditor"
        />
      );
    case 'UpdateCheck':
      return (
        <IconButton
          name={icons.UPDATE}
          title="Updates"
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
      return (
        <SpinnerIconButton
          name={icons.TEST}
          title="Test All"
          isSpinning={props.isTestingAllIndexers}
          onPress={props.dispatchTestAllIndexers}
        />
      );
    case 'DownloadClientCheck':
      return (
        <SpinnerIconButton
          name={icons.TEST}
          title="Test All"
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
    label: 'Message',
    isVisible: true
  },
  {
    name: 'actions',
    label: 'Actions',
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
            Health

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
              No issues with your configuration
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

                    return (
                      <TableRow key={`health${item.message}`}>
                        <TableRowCell>
                          <Icon
                            name={icons.DANGER}
                            kind={item.type.toLowerCase() === 'error' ? kinds.DANGER : kinds.WARNING}
                            title={titleCase(item.type)}
                          />
                        </TableRowCell>

                        <TableRowCell>{item.message}</TableRowCell>

                        <TableRowCell>
                          <IconButton
                            name={icons.WIKI}
                            to={item.wikiUrl}
                            title="Read the Wiki for more information"
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
