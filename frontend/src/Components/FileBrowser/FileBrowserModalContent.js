import PropTypes from 'prop-types';
import React, { Component } from 'react';
import ReactDOM from 'react-dom';
import Alert from 'Components/Alert';
import PathInput from 'Components/Form/PathInput';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Scroller from 'Components/Scroller/Scroller';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { kinds, scrollDirections } from 'Helpers/Props';
import FileBrowserRow from './FileBrowserRow';
import styles from './FileBrowserModalContent.css';

const columns = [
  {
    name: 'type',
    label: 'Type',
    isVisible: true
  },
  {
    name: 'name',
    label: 'Name',
    isVisible: true
  }
];

class FileBrowserModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._scrollerNode = null;

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
      this._scrollerNode.scrollTop = 0;
    }
  }

  //
  // Control

  setScrollerRef = (ref) => {
    if (ref) {
      this._scrollerNode = ReactDOM.findDOMNode(ref);
    } else {
      this._scrollerNode = null;
    }
  };

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
          File Browser
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
                Mapped network drives are not available when running as a Windows Service, see the <Link className={styles.faqLink} to="https://wiki.servarr.com/sonarr/faq#why-cant-sonarr-see-my-files-on-a-remote-server">FAQ</Link> for more information.
              </Alert>
          }

          <PathInput
            className={styles.pathInput}
            placeholder="Start typing or select a path below"
            hasFileBrowser={false}
            {...otherProps}
            value={this.state.currentPath}
            onChange={this.onPathInputChange}
          />

          <Scroller
            ref={this.setScrollerRef}
            className={styles.scroller}
            scrollDirection={scrollDirections.BOTH}
          >
            {
              !!error &&
                <div>Error loading contents</div>
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
                          name="My Computer"
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
            Cancel
          </Button>

          <Button
            onPress={this.onOkPress}
          >
            Ok
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
