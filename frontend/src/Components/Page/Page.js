import PropTypes from 'prop-types';
import React, { Component } from 'react';
import locationShape from 'Helpers/Props/Shapes/locationShape';
import SignalRConnector from 'Components/SignalRConnector';
import AppUpdatedModalConnector from 'App/AppUpdatedModalConnector';
import ConnectionLostModalConnector from 'App/ConnectionLostModalConnector';
import PageHeader from './Header/PageHeader';
import PageSidebar from './Sidebar/PageSidebar';
import styles from './Page.css';

class Page extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isUpdatedModalOpen: false,
      isConnectionLostModalOpen: false
    };
  }

  componentDidMount() {
    window.addEventListener('resize', this.onResize);
  }

  componentDidUpdate(prevProps) {
    const {
      isDisconnected,
      isUpdated
    } = this.props;

    if (!prevProps.isUpdated && isUpdated) {
      this.setState({ isUpdatedModalOpen: true });
    }

    if (prevProps.isDisconnected !== isDisconnected) {
      this.setState({ isConnectionLostModalOpen: isDisconnected });
    }
  }

  componentWillUnmount() {
    window.removeEventListener('resize', this.onResize);
  }

  //
  // Listeners

  onResize = () => {
    this.props.onResize({
      width: window.innerWidth,
      height: window.innerHeight
    });
  }

  onUpdatedModalClose = () => {
    this.setState({ isUpdatedModalOpen: false });
  }

  onConnectionLostModalClose = () => {
    this.setState({ isConnectionLostModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      className,
      location,
      children,
      isSmallScreen,
      isSidebarVisible,
      onSidebarToggle,
      onSidebarVisibleChange
    } = this.props;

    return (
      <div className={className}>
        <SignalRConnector />

        <PageHeader
          onSidebarToggle={onSidebarToggle}
        />

        <div className={styles.main}>
          <PageSidebar
            location={location}
            isSmallScreen={isSmallScreen}
            isSidebarVisible={isSidebarVisible}
            onSidebarVisibleChange={onSidebarVisibleChange}
          />

          {children}
        </div>

        <AppUpdatedModalConnector
          isOpen={this.state.isUpdatedModalOpen}
          onModalClose={this.onUpdatedModalClose}
        />

        <ConnectionLostModalConnector
          isOpen={this.state.isConnectionLostModalOpen}
          onModalClose={this.onConnectionLostModalClose}
        />
      </div>
    );
  }
}

Page.propTypes = {
  className: PropTypes.string,
  location: locationShape.isRequired,
  children: PropTypes.node.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  isSidebarVisible: PropTypes.bool.isRequired,
  isUpdated: PropTypes.bool.isRequired,
  isDisconnected: PropTypes.bool.isRequired,
  onResize: PropTypes.func.isRequired,
  onSidebarToggle: PropTypes.func.isRequired,
  onSidebarVisibleChange: PropTypes.func.isRequired
};

Page.defaultProps = {
  className: styles.page
};

export default Page;
