import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createProfileInUseSelector from 'Store/Selectors/createProfileInUseSelector';
import createProviderSettingsSelector from 'Store/Selectors/createProviderSettingsSelector';
import { fetchQualityProfileSchema, setQualityProfileValue, saveQualityProfile } from 'Store/Actions/settingsActions';
import EditQualityProfileModalContent from './EditQualityProfileModalContent';

function getQualityItemGroupId(qualityProfile) {
  // Get items with an `id` and filter out null/undefined values
  const ids = _.filter(_.map(qualityProfile.items.value, 'id'), (i) => i != null);

  return Math.max(1000, ...ids) + 1;
}

function parseIndex(index) {
  const split = index.split('.');

  if (split.length === 1) {
    return [
      null,
      parseInt(split[0]) - 1
    ];
  }

  return [
    parseInt(split[0]) - 1,
    parseInt(split[1]) - 1
  ];
}

function createQualitiesSelector() {
  return createSelector(
    createProviderSettingsSelector('qualityProfiles'),
    (qualityProfile) => {
      const items = qualityProfile.item.items;
      if (!items || !items.value) {
        return [];
      }

      return _.reduceRight(items.value, (result, { allowed, id, name, quality }) => {
        if (allowed) {
          if (id) {
            result.push({
              key: id,
              value: name
            });
          } else {
            result.push({
              key: quality.id,
              value: quality.name
            });
          }
        }

        return result;
      }, []);
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    createProviderSettingsSelector('qualityProfiles'),
    createQualitiesSelector(),
    createProfileInUseSelector('qualityProfileId'),
    (qualityProfile, qualities, isInUse) => {
      return {
        qualities,
        ...qualityProfile,
        isInUse
      };
    }
  );
}

const mapDispatchToProps = {
  fetchQualityProfileSchema,
  setQualityProfileValue,
  saveQualityProfile
};

class EditQualityProfileModalContentConnector extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      dragQualityIndex: null,
      dropQualityIndex: null,
      dropPosition: null,
      editGroups: false
    };
  }

  componentDidMount() {
    if (!this.props.id && !this.props.isPopulated) {
      this.props.fetchQualityProfileSchema();
    }
  }

  componentDidUpdate(prevProps, prevState) {
    if (prevProps.isSaving && !this.props.isSaving && !this.props.saveError) {
      this.props.onModalClose();
    }
  }

  //
  // Control

  ensureCutoff = (qualityProfile) => {
    const cutoff = qualityProfile.cutoff.value;

    const cutoffItem = _.find(qualityProfile.items.value, (i) => {
      if (!cutoff) {
        return false;
      }

      return i.id === cutoff || (i.quality && i.quality.id === cutoff);
    });

    // If the cutoff isn't allowed anymore or there isn't a cutoff set one
    if (!cutoff || !cutoffItem || !cutoffItem.allowed) {
      const firstAllowed = _.find(qualityProfile.items.value, { allowed: true });
      let cutoffId = null;

      if (firstAllowed) {
        cutoffId = firstAllowed.quality ? firstAllowed.quality.id : firstAllowed.id;
      }

      this.props.setQualityProfileValue({ name: 'cutoff', value: cutoffId });
    }
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setQualityProfileValue({ name, value });
  }

  onCutoffChange = ({ name, value }) => {
    const id = parseInt(value);
    const item = _.find(this.props.item.items.value, (i) => {
      if (i.quality) {
        return i.quality.id === id;
      }

      return i.id === id;
    });

    const cutoffId = item.quality ? item.quality.id : item.id;

    this.props.setQualityProfileValue({ name, value: cutoffId });
  }

  onSavePress = () => {
    this.props.saveQualityProfile({ id: this.props.id });
  }

  onQualityProfileItemAllowedChange = (id, allowed) => {
    const qualityProfile = _.cloneDeep(this.props.item);
    const items = qualityProfile.items.value;
    const item = _.find(qualityProfile.items.value, (i) => i.quality && i.quality.id === id);

    item.allowed = allowed;

    this.props.setQualityProfileValue({
      name: 'items',
      value: items
    });

    this.ensureCutoff(qualityProfile);
  }

  onItemGroupAllowedChange = (id, allowed) => {
    const qualityProfile = _.cloneDeep(this.props.item);
    const items = qualityProfile.items.value;
    const item = _.find(qualityProfile.items.value, (i) => i.id === id);

    item.allowed = allowed;

    // Update each item in the group (for consistency only)
    item.items.forEach((i) => {
      i.allowed = allowed;
    });

    this.props.setQualityProfileValue({
      name: 'items',
      value: items
    });

    this.ensureCutoff(qualityProfile);
  }

  onItemGroupNameChange = (id, name) => {
    const qualityProfile = _.cloneDeep(this.props.item);
    const items = qualityProfile.items.value;
    const group = _.find(items, (i) => i.id === id);

    group.name = name;

    this.props.setQualityProfileValue({
      name: 'items',
      value: items
    });
  }

  onCreateGroupPress = (id) => {
    const qualityProfile = _.cloneDeep(this.props.item);
    const items = qualityProfile.items.value;
    const item = _.find(items, (i) => i.quality && i.quality.id === id);
    const index = items.indexOf(item);
    const groupId = getQualityItemGroupId(qualityProfile);

    const group = {
      id: groupId,
      name: item.quality.name,
      allowed: item.allowed,
      items: [
        item
      ]
    };

    // Add the group in the same location the quality item was in.
    items.splice(index, 1, group);

    this.props.setQualityProfileValue({
      name: 'items',
      value: items
    });

    this.ensureCutoff(qualityProfile);
  }

  onDeleteGroupPress = (id) => {
    const qualityProfile = _.cloneDeep(this.props.item);
    const items = qualityProfile.items.value;
    const group = _.find(items, (i) => i.id === id);
    const index = items.indexOf(group);

    // Add the items in the same location the group was in
    items.splice(index, 1, ...group.items);

    this.props.setQualityProfileValue({
      name: 'items',
      value: items
    });

    this.ensureCutoff(qualityProfile);
  }

  onQualityProfileItemDragMove = (options) => {
    const {
      dragQualityIndex,
      dropQualityIndex,
      dropPosition
    } = options;

    const [dragGroupIndex, dragItemIndex] = parseIndex(dragQualityIndex);
    const [dropGroupIndex, dropItemIndex] = parseIndex(dropQualityIndex);

    if (
      (dropPosition === 'below' && dropItemIndex - 1 === dragItemIndex) ||
      (dropPosition === 'above' && dropItemIndex + 1 === dragItemIndex)
    ) {
      if (
        this.state.dragQualityIndex != null &&
        this.state.dropQualityIndex != null &&
        this.state.dropPosition != null
      ) {
        this.setState({
          dragQualityIndex: null,
          dropQualityIndex: null,
          dropPosition: null
        });
      }

      return;
    }

    let adjustedDropQualityIndex = dropQualityIndex;

    // Correct dragging out of a group to the position above
    if (
      dropPosition === 'above' &&
      dragGroupIndex !== dropGroupIndex &&
      dropGroupIndex != null
    ) {
      // Add 1 to the group index and 2 to the item index so it's inserted above in the correct group
      adjustedDropQualityIndex = `${dropGroupIndex + 1}.${dropItemIndex + 2}`;
    }

    // Correct inserting above outside a group
    if (
      dropPosition === 'above' &&
      dragGroupIndex !== dropGroupIndex &&
      dropGroupIndex == null
    ) {
      // Add 2 to the item index so it's entered in the correct place
      adjustedDropQualityIndex = `${dropItemIndex + 2}`;
    }

    // Correct inserting below a quality within the same group (when moving a lower item)
    if (
      dropPosition === 'below' &&
      dragGroupIndex === dropGroupIndex &&
      dropGroupIndex != null &&
      dragItemIndex < dropItemIndex
    ) {
      // Add 1 to the group index leave the item index
      adjustedDropQualityIndex = `${dropGroupIndex + 1}.${dropItemIndex}`;
    }

    // Correct inserting below a quality outside a group (when moving a lower item)
    if (
      dropPosition === 'below' &&
      dragGroupIndex === dropGroupIndex &&
      dropGroupIndex == null &&
      dragItemIndex < dropItemIndex
    ) {
      // Leave the item index so it's inserted below the item
      adjustedDropQualityIndex = `${dropItemIndex}`;
    }

    if (
      dragQualityIndex !== this.state.dragQualityIndex ||
      adjustedDropQualityIndex !== this.state.dropQualityIndex ||
      dropPosition !== this.state.dropPosition
    ) {
      this.setState({
        dragQualityIndex,
        dropQualityIndex: adjustedDropQualityIndex,
        dropPosition
      });
    }
  }

  onQualityProfileItemDragEnd = (didDrop) => {
    const {
      dragQualityIndex,
      dropQualityIndex
    } = this.state;

    if (didDrop && dropQualityIndex != null) {
      const qualityProfile = _.cloneDeep(this.props.item);
      const items = qualityProfile.items.value;
      const [dragGroupIndex, dragItemIndex] = parseIndex(dragQualityIndex);
      const [dropGroupIndex, dropItemIndex] = parseIndex(dropQualityIndex);

      let item = null;
      let dropGroup = null;

      // Get the group before moving anything so we know the correct place to drop it.
      if (dropGroupIndex != null) {
        dropGroup = items[dropGroupIndex];
      }

      if (dragGroupIndex == null) {
        item = items.splice(dragItemIndex, 1)[0];
      } else {
        const group = items[dragGroupIndex];
        item = group.items.splice(dragItemIndex, 1)[0];

        // If the group is now empty, destroy it.
        if (!group.items.length) {
          items.splice(dragGroupIndex, 1);
        }
      }

      if (dropGroupIndex == null) {
        items.splice(dropItemIndex, 0, item);
      } else {
        dropGroup.items.splice(dropItemIndex, 0, item);
      }

      this.props.setQualityProfileValue({
        name: 'items',
        value: items
      });

      this.ensureCutoff(qualityProfile);
    }

    this.setState({
      dragQualityIndex: null,
      dropQualityIndex: null,
      dropPosition: null
    });
  }

  onToggleEditGroupsMode = () => {
    this.setState({ editGroups: !this.state.editGroups });
  }

  //
  // Render

  render() {
    if (_.isEmpty(this.props.item.items) && !this.props.isFetching) {
      return null;
    }

    return (
      <EditQualityProfileModalContent
        {...this.state}
        {...this.props}
        onSavePress={this.onSavePress}
        onInputChange={this.onInputChange}
        onCutoffChange={this.onCutoffChange}
        onCreateGroupPress={this.onCreateGroupPress}
        onDeleteGroupPress={this.onDeleteGroupPress}
        onQualityProfileItemAllowedChange={this.onQualityProfileItemAllowedChange}
        onItemGroupAllowedChange={this.onItemGroupAllowedChange}
        onItemGroupNameChange={this.onItemGroupNameChange}
        onQualityProfileItemDragMove={this.onQualityProfileItemDragMove}
        onQualityProfileItemDragEnd={this.onQualityProfileItemDragEnd}
        onToggleEditGroupsMode={this.onToggleEditGroupsMode}
      />
    );
  }
}

EditQualityProfileModalContentConnector.propTypes = {
  id: PropTypes.number,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  setQualityProfileValue: PropTypes.func.isRequired,
  fetchQualityProfileSchema: PropTypes.func.isRequired,
  saveQualityProfile: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditQualityProfileModalContentConnector);
