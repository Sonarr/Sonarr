import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import PathInput from 'Components/Form/PathInput';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Scroller from 'Components/Scroller/Scroller';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { kinds, scrollDirections } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import InlineMarkdown from '../Markdown/InlineMarkdown';
import FileBrowserRow from './FileBrowserRow';
import styles from './FileBrowserModalContent.css';

const columns = [
  {
    name: 'type',
    label: () => translate('Type'),
    isVisible: true
  },
  {
    name: 'name',
    label: () => translate('Name'),
    isVisible: true
  }
];

class FileBrowserModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._scrollerRef = React.createRef();

    this.state = {
      isFileBrowserModalOpen: false,
      currentPath: props.value
    };
  }

  componentDidUpdate(prevProps, prevState) {
    const {
      currentPath
    } = this.props;

    if (
      currentPath !== this.state.currentPath &&
      currentPath !== prevState.currentPath
    ) {
      this.setState({ currentPath });
      this._scrollerRef.current.scrollTop = 0;
    }
  }

  //
  // Listeners

  onPathInputChange = ({ value }) => {
    this.setState({ currentPath: value });
  };

  onRowPress = (path) => {
    this.props.onFetchPaths(path);
  };

  onOkPress = () => {
    this.props.onChange({
      name: this.props.name,
      value: this.state.currentPath
    });

    this.props.onClearPaths();
    this.props.onModalClose();
  };

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      parent,
      directories,
      files,
      isWindowsService,
      onModalClose,
      ...otherProps
    } = this.props;

    const emptyParent = parent === '';

    return (
      <ModalContent
        onModalClose={onModalClose}
      >
        <ModalHeader>
          {translate('FileBrowser')}
        </ModalHeader>

        <ModalBody
          className={styles.modalBody}
          scrollDirection={scrollDirections.NONE}
        >
          {
            isWindowsService &&
              <Alert
                className={styles.mappedDrivesWarning}
                kind={kinds.WARNING}
              >
                <InlineMarkdown data={translate('MappedNetworkDrivesWindowsService')} />
              </Alert>
          }

          <PathInput
            className={styles.pathInput}
            placeholder={translate('FileBrowserPlaceholderText')}
            hasFileBrowser={false}
            {...otherProps}
            value={this.state.currentPath}
            onChange={this.onPathInputChange}
          />

          <Scroller
            ref={this._scrollerRef}
            className={styles.scroller}
            scrollDirection={scrollDirections.BOTH}
          >
            {
              !!error &&
                <div>{translate('ErrorLoadingContents')}</div>
            }

            {
              isPopulated && !error &&
                <Table
                  horizontalScroll={false}
                  columns={columns}
                >
                  <TableBody>
                    {
                      emptyParent &&
                        <FileBrowserRow
                          type="computer"
                          name={translate('MyComputer')}
                          path={parent}
                          onPress={this.onRowPress}
                        />
                    }

                    {
                      !emptyParent && parent &&
                        <FileBrowserRow
                          type="parent"
                          name="..."
                          path={parent}
                          onPress={this.onRowPress}
                        />
                    }

                    {
                      directories.map((directory) => {
                        return (
                          <FileBrowserRow
                            key={directory.path}
                            type={directory.type}
                            name={directory.name}
                            path={directory.path}
                            onPress={this.onRowPress}
                          />
                        );
                      })
                    }

                    {
                      files.map((file) => {
                        return (
                          <FileBrowserRow
                            key={file.path}
                            type={file.type}
                            name={file.name}
                            path={file.path}
                            onPress={this.onRowPress}
                          />
                        );
                      })
                    }
                  </TableBody>
                </Table>
            }
          </Scroller>
        </ModalBody>

        <ModalFooter>
          {
            isFetching &&
              <LoadingIndicator
                className={styles.loading}
                size={20}
              />
          }

          <Button
            onPress={onModalClose}
          >
            {translate('Cancel')}
          </Button>

          <Button
            onPress={this.onOkPress}
          >
            {translate('Ok')}
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

FileBrowserModalContent.propTypes = {
  name: PropTypes.string.isRequired,
  value: PropTypes.string.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  parent: PropTypes.string,
  currentPath: PropTypes.string.isRequired,
  directories: PropTypes.arrayOf(PropTypes.object).isRequired,
  files: PropTypes.arrayOf(PropTypes.object).isRequired,
  isWindowsService: PropTypes.bool.isRequired,
  onFetchPaths: PropTypes.func.isRequired,
  onClearPaths: PropTypes.func.isRequired,
  onChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default FileBrowserModalContent;
