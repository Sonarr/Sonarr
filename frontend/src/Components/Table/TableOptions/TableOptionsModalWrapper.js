import PropTypes from 'prop-types';
import React, { Component, Fragment } from 'react';
import TableOptionsModal from './TableOptionsModal';

class TableOptionsModalWrapper extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isTableOptionsModalOpen: false
    };
  }

  //
  // Listeners

  onTableOptionsPress = () => {
    this.setState({ isTableOptionsModalOpen: true });
  };

  onTableOptionsModalClose = () => {
    this.setState({ isTableOptionsModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      columns,
      children,
      ...otherProps
    } = this.props;

    return (
      <Fragment>
        {
          React.cloneElement(children, { onPress: this.onTableOptionsPress })
        }

        <TableOptionsModal
          {...otherProps}
          isOpen={this.state.isTableOptionsModalOpen}
          columns={columns}
          onModalClose={this.onTableOptionsModalClose}
        />
      </Fragment>
    );
  }
}

TableOptionsModalWrapper.propTypes = {
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  children: PropTypes.node.isRequired
};

export default TableOptionsModalWrapper;
