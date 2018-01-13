import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { DragDropContext } from 'react-dnd';
import HTML5Backend from 'react-dnd-html5-backend';
import { inputTypes } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormInputGroup from 'Components/Form/FormInputGroup';
import Modal from 'Components/Modal/Modal';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import TableOptionsColumn from './TableOptionsColumn';
import TableOptionsColumnDragSource from './TableOptionsColumnDragSource';
import TableOptionsColumnDragPreview from './TableOptionsColumnDragPreview';
import styles from './TableOptionsModal.css';

class TableOptionsModal extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasPageSize: !!props.pageSize,
      pageSize: props.pageSize,
      pageSizeError: null,
      dragIndex: null,
      dropIndex: null
    };
  }

  componentDidUpdate(prevProps) {
    if (prevProps.pageSize !== this.state.pageSize) {
      this.setState({ pageSize: this.props.pageSize });
    }
  }

  //
  // Listeners

  onPageSizeChange = ({ value }) => {
    let pageSizeError = null;

    if (value < 5) {
      pageSizeError = 'Page size must be at least 5';
    } else if (value > 250) {
      pageSizeError = 'Page size must not exceed 250';
    } else {
      this.props.onTableOptionChange({ pageSize: value });
    }

    this.setState({
      pageSize: value,
      pageSizeError
    });
  }

  onVisibleChange = ({ name, value }) => {
    const columns = _.cloneDeep(this.props.columns);

    const column = _.find(columns, { name });
    column.isVisible = value;

    this.props.onTableOptionChange({ columns });
  }

  onColumnDragMove = (dragIndex, dropIndex) => {
    if (this.state.dragIndex !== dragIndex || this.state.dropIndex !== dropIndex) {
      this.setState({
        dragIndex,
        dropIndex
      });
    }
  }

  onColumnDragEnd = ({ id }, didDrop) => {
    const {
      dragIndex,
      dropIndex
    } = this.state;

    if (didDrop && dropIndex !== null) {
      const columns = _.cloneDeep(this.props.columns);
      const items = columns.splice(dragIndex, 1);
      columns.splice(dropIndex, 0, items[0]);

      this.props.onTableOptionChange({ columns });
    }

    this.setState({
      dragIndex: null,
      dropIndex: null
    });
  }

  //
  // Render

  render() {
    const {
      isOpen,
      columns,
      canModifyColumns,
      onModalClose
    } = this.props;

    const {
      hasPageSize,
      pageSize,
      pageSizeError,
      dragIndex,
      dropIndex
    } = this.state;

    const isDragging = dropIndex !== null;
    const isDraggingUp = isDragging && dropIndex < dragIndex;
    const isDraggingDown = isDragging && dropIndex > dragIndex;

    return (
      <Modal
        isOpen={isOpen}
        onModalClose={onModalClose}
      >
        <ModalContent onModalClose={onModalClose}>
          <ModalHeader>
            Table Options
          </ModalHeader>

          <ModalBody>
            <Form>
              {
                hasPageSize &&
                  <FormGroup>
                    <FormLabel>Page Size</FormLabel>

                    <FormInputGroup
                      type={inputTypes.NUMBER}
                      name="pageSize"
                      value={pageSize || 0}
                      helpText="Number of items to show on each page"
                      errors={pageSizeError ? [{ message: pageSizeError }] : undefined}
                      onChange={this.onPageSizeChange}
                    />
                  </FormGroup>
              }

              {
                canModifyColumns &&
                  <FormGroup>
                    <FormLabel>Columns</FormLabel>

                    <div>
                      <FormInputHelpText
                        text="Choose which columns are isVisible and which order they appear in"
                      />

                      <div className={styles.columns}>
                        {
                          columns.map((column, index) => {
                            const {
                              name,
                              label,
                              columnLabel,
                              isVisible,
                              isModifiable
                            } = column;

                            if (isModifiable !== false) {
                              return (
                                <TableOptionsColumnDragSource
                                  key={name}
                                  name={name}
                                  label={label || columnLabel}
                                  isVisible={isVisible}
                                  isModifiable={true}
                                  index={index}
                                  isDragging={isDragging}
                                  isDraggingUp={isDraggingUp}
                                  isDraggingDown={isDraggingDown}
                                  onVisibleChange={this.onVisibleChange}
                                  onColumnDragMove={this.onColumnDragMove}
                                  onColumnDragEnd={this.onColumnDragEnd}
                                />
                              );
                            }

                            return (
                              <TableOptionsColumn
                                key={name}
                                name={name}
                                label={label || columnLabel}
                                isVisible={isVisible}
                                index={index}
                                isModifiable={false}
                                onVisibleChange={this.onVisibleChange}
                              />
                            );
                          })
                        }

                        <TableOptionsColumnDragPreview />
                      </div>
                    </div>
                  </FormGroup>
              }
            </Form>
          </ModalBody>
          <ModalFooter>
            <Button
              onPress={onModalClose}
            >
              Close
            </Button>
          </ModalFooter>
        </ModalContent>
      </Modal>
    );
  }
}

TableOptionsModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  pageSize: PropTypes.number,
  canModifyColumns: PropTypes.bool.isRequired,
  onTableOptionChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

TableOptionsModal.defaultProps = {
  canModifyColumns: true
};

export default DragDropContext(HTML5Backend)(TableOptionsModal);
