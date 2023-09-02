import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import LogsNavMenu from '../LogsNavMenu';
import LogFilesTableRow from './LogFilesTableRow';

const columns = [
  {
    name: 'filename',
    label: () => translate('Filename'),
    isVisible: true
  },
  {
    name: 'lastWriteTime',
    label: () => translate('LastWriteTime'),
    isVisible: true
  },
  {
    name: 'download',
    isVisible: true
  }
];

class LogFiles extends Component {

  //
  // Render

  render() {
    const {
      isFetching,
      items,
      deleteFilesExecuting,
      currentLogView,
      location,
      onRefreshPress,
      onDeleteFilesPress,
      ...otherProps
    } = this.props;

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
              isSpinning={deleteFilesExecuting}
              onPress={onDeleteFilesPress}
            />
          </PageToolbarSection>
        </PageToolbar>
        <PageContentBody>
          <Alert>
            <div>
              {translate('LogFilesLocation', {
                location
              })}
            </div>

            {
              currentLogView === 'Log Files' &&
                <div>
                  <InlineMarkdown data={translate('TheLogLevelDefault')} />
                </div>
            }
          </Alert>

          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            !isFetching && !!items.length &&
              <div>
                <Table
                  columns={columns}
                  {...otherProps}
                >
                  <TableBody>
                    {
                      items.map((item) => {
                        return (
                          <LogFilesTableRow
                            key={item.id}
                            {...item}
                          />
                        );
                      })
                    }
                  </TableBody>
                </Table>
              </div>
          }

          {
            !isFetching && !items.length &&
              <Alert kind={kinds.INFO}>
                {translate('NoLogFiles')}
              </Alert>
          }
        </PageContentBody>
      </PageContent>
    );
  }

}

LogFiles.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  items: PropTypes.array.isRequired,
  deleteFilesExecuting: PropTypes.bool.isRequired,
  currentLogView: PropTypes.string.isRequired,
  location: PropTypes.string.isRequired,
  onRefreshPress: PropTypes.func.isRequired,
  onDeleteFilesPress: PropTypes.func.isRequired
};

export default LogFiles;
