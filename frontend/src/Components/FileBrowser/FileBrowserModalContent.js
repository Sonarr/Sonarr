import PropTypes from 'prop-types';
import React, { Component } from 'react';
import ReactDOM from 'react-dom';
import { scrollDirections } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import Scroller from 'Components/Scroller/Scroller';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import PathInput from 'Components/Form/PathInput';
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

  componentDidUpdate(prevProps) {
    const {
      currentPath
    } = this.props;

    if (currentPath !== this.state.currentPath) {
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
  }

  //
  // Listeners

  onPathInputChange = ({ value }) => {
    this.setState({ currentPath: value });
  }

  onRowPress = (path) => {
    this.props.onFetchPaths(path);
  }

  onOkPress = () => {
    this.props.onChange({
      name: this.props.name,
      value: this.state.currentPath
    });

    this.props.onClearPaths();
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    const {
      parent,
      directories,
      files,
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
          >
            <Table columns={columns}>
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
          </Scroller>
        </ModalBody>

        <ModalFooter>
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
  parent: PropTypes.string,
  currentPath: PropTypes.string.isRequired,
  directories: PropTypes.arrayOf(PropTypes.object).isRequired,
  files: PropTypes.arrayOf(PropTypes.object).isRequired,
  onFetchPaths: PropTypes.func.isRequired,
  onClearPaths: PropTypes.func.isRequired,
  onChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default FileBrowserModalContent;
