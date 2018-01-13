import $ from 'jquery';
import 'signalr';
import PropTypes from 'prop-types';
import { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { repopulatePage } from 'Utilities/pagePopulator';
import { updateCommand, finishCommand } from 'Store/Actions/commandActions';
import { setAppValue, setVersion } from 'Store/Actions/appActions';
import { update, updateItem, removeItem } from 'Store/Actions/baseActions';
import { fetchHealth } from 'Store/Actions/systemActions';
import { fetchQueue, fetchQueueDetails } from 'Store/Actions/queueActions';

function getState(status) {
  switch (status) {
    case 0:
      return 'connecting';
    case 1:
      return 'connected';
    case 2:
      return 'reconnecting';
    case 4:
      return 'disconnected';
    default:
      throw new Error(`invalid status ${status}`);
  }
}

function isAppDisconnected(disconnectedTime) {
  if (!disconnectedTime) {
    return false;
  }

  return Math.floor(new Date().getTime() / 1000) - disconnectedTime > 180;
}

function createMapStateToProps() {
  return createSelector(
    (state) => state.app.isReconnecting,
    (state) => state.app.isDisconnected,
    (state) => state.queue.paged.isPopulated,
    (isReconnecting, isDisconnected, isQueuePopulated) => {
      return {
        isReconnecting,
        isDisconnected,
        isQueuePopulated
      };
    }
  );
}

const mapDispatchToProps = {
  updateCommand,
  finishCommand,
  setAppValue,
  setVersion,
  update,
  updateItem,
  removeItem,
  fetchHealth,
  fetchQueue,
  fetchQueueDetails
};

class SignalRConnector extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.signalRconnectionOptions = { transport: ['webSockets', 'longPolling'] };
    this.signalRconnection = null;
    this.retryInterval = 1;
    this.retryTimeoutId = null;
    this.disconnectedTime = null;
  }

  componentDidMount() {
    console.log('Starting signalR');

    this.signalRconnection = $.connection('/signalr', { apiKey: window.Sonarr.apiKey });

    this.signalRconnection.stateChanged(this.onStateChanged);
    this.signalRconnection.received(this.onReceived);
    this.signalRconnection.reconnecting(this.onReconnecting);
    this.signalRconnection.disconnected(this.onDisconnected);

    this.signalRconnection.start(this.signalRconnectionOptions);
  }

  componentWillUnmount() {
    this.signalRconnection.stop();
    this.signalRconnection = null;
  }

  //
  // Control

  retryConnection = () => {
    if (isAppDisconnected(this.disconnectedTime)) {
      this.setState({
        isDisconnected: true
      });
    }

    this.retryTimeoutId = setTimeout(() => {
      this.signalRconnection.start(this.signalRconnectionOptions);
      this.retryInterval = Math.min(this.retryInterval + 1, 10);
    }, this.retryInterval * 1000);
  }

  handleMessage = (message) => {
    const {
      name,
      body
    } = message;

    if (name === 'calendar') {
      this.handleCalendar(body);
      return;
    }

    if (name === 'command') {
      this.handleCommand(body);
      return;
    }

    if (name === 'episode') {
      this.handleEpisode(body);
      return;
    }

    if (name === 'episodefile') {
      this.handleEpisodeFile(body);
      return;
    }

    if (name === 'health') {
      this.handleHealth(body);
      return;
    }

    if (name === 'series') {
      this.handleSeries(body);
      return;
    }

    if (name === 'queue') {
      this.handleQueue(body);
      return;
    }

    if (name === 'queue/details') {
      this.handleQueueDetails(body);
      return;
    }

    if (name === 'queue/status') {
      this.handleQueueStatus(body);
      return;
    }

    if (name === 'version') {
      this.handleVersion(body);
      return;
    }

    if (name === 'wanted/cutoff') {
      this.handleWantedCutoff(body);
      return;
    }

    if (name === 'wanted/missing') {
      this.handleWantedMissing(body);
      return;
    }
  }

  handleCalendar = (body) => {
    if (body.action === 'updated') {
      this.props.updateItem({
        section: 'calendar',
        updateOnly: true,
        ...body.resource
      });
    }
  }

  handleCommand = (body) => {
    const resource = body.resource;
    const state = resource.state;

    // Both sucessful and failed commands need to be
    // completed, otherwise they spin until they timeout.

    if (state === 'completed' || state === 'failed') {
      this.props.finishCommand(resource);
    } else {
      this.props.updateCommand(resource);
    }
  }

  handleEpisode = (body) => {
    if (body.action === 'updated') {
      this.props.updateItem({
        section: 'episodes',
        updateOnly: true,
        ...body.resource
      });
    }
  }

  handleEpisodeFile = (body) => {
    const section = 'episodeFiles';

    if (body.action === 'updated') {
      this.props.updateItem({ section, ...body.resource });
    } else if (body.action === 'deleted') {
      this.props.removeItem({ section, id: body.resource.id });
    }
  }

  handleHealth = (body) => {
    this.props.fetchHealth();
  }

  handleSeries = (body) => {
    const action = body.action;
    const section = 'series';

    if (action === 'updated') {
      this.props.updateItem({ section, ...body.resource });
    } else if (action === 'deleted') {
      this.props.removeItem({ section, id: body.resource.id });
    }
  }

  handleQueue = (body) => {
    if (this.props.isQueuePopulated) {
      this.props.fetchQueue();
    }
  }

  handleQueueDetails = (body) => {
    this.props.fetchQueueDetails();
  }

  handleQueueStatus = (body) => {
    this.props.update({ section: 'queue.status', data: body.resource });
  }

  handleVersion = (body) => {
    const version = body.Version;

    this.props.setVersion({ version });
  }

  handleWantedCutoff = (body) => {
    if (body.action === 'updated') {
      this.props.updateItem({
        section: 'cutoffUnmet',
        updateOnly: true,
        ...body.resource
      });
    }
  }

  handleWantedMissing = (body) => {
    if (body.action === 'updated') {
      this.props.updateItem({
        section: 'missing',
        updateOnly: true,
        ...body.resource
      });
    }
  }

  //
  // Listeners

  onStateChanged = (change) => {
    const state = getState(change.newState);
    console.log(`SignalR: ${state}`);

    if (state === 'connected') {
      // Clear disconnected time
      this.disconnectedTime = null;

      // Repopulate the page (if a repopulator is set) to ensure things
      // are in sync after reconnecting.

      if (this.props.isReconnecting || this.props.isDisconnected) {
        repopulatePage();
      }

      this.props.setAppValue({
        isConnected: true,
        isReconnecting: false,
        isDisconnected: false,
        isRestarting: false
      });

      this.retryInterval = 5;

      if (this.retryTimeoutId) {
        clearTimeout(this.retryTimeoutId);
      }
    }
  }

  onReceived = (message) => {
    console.debug('SignalR: received', message.name, message.body);

    this.handleMessage(message);
  }

  onReconnecting = () => {
    if (window.Sonarr.unloading) {
      return;
    }

    if (!this.disconnectedTime) {
      this.disconnectedTime = Math.floor(new Date().getTime() / 1000);
    }

    this.props.setAppValue({
      isReconnecting: true
    });
  }

  onDisconnected = () => {
    if (window.Sonarr.unloading) {
      return;
    }

    if (!this.disconnectedTime) {
      this.disconnectedTime = Math.floor(new Date().getTime() / 1000);
    }

    this.props.setAppValue({
      isConnected: false,
      isReconnecting: true,
      isDisconnected: isAppDisconnected(this.disconnectedTime)
    });

    this.retryConnection();
  }

  //
  // Render

  render() {
    return null;
  }
}

SignalRConnector.propTypes = {
  isReconnecting: PropTypes.bool.isRequired,
  isDisconnected: PropTypes.bool.isRequired,
  isQueuePopulated: PropTypes.bool.isRequired,
  updateCommand: PropTypes.func.isRequired,
  finishCommand: PropTypes.func.isRequired,
  setAppValue: PropTypes.func.isRequired,
  setVersion: PropTypes.func.isRequired,
  update: PropTypes.func.isRequired,
  updateItem: PropTypes.func.isRequired,
  removeItem: PropTypes.func.isRequired,
  fetchHealth: PropTypes.func.isRequired,
  fetchQueue: PropTypes.func.isRequired,
  fetchQueueDetails: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SignalRConnector);
