import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import Measure from 'Components/Measure';
import PageSectionContent from 'Components/Page/PageSectionContent';
import Scroller from 'Components/Scroller/Scroller';
import { icons, scrollDirections } from 'Helpers/Props';
import DelayProfile from './DelayProfile';
import DelayProfileDragPreview from './DelayProfileDragPreview';
import DelayProfileDragSource from './DelayProfileDragSource';
import EditDelayProfileModalConnector from './EditDelayProfileModalConnector';
import styles from './DelayProfiles.css';

class DelayProfiles extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddDelayProfileModalOpen: false,
      width: 0
    };
  }

  //
  // Listeners

  onAddDelayProfilePress = () => {
    this.setState({ isAddDelayProfileModalOpen: true });
  };

  onModalClose = () => {
    this.setState({ isAddDelayProfileModalOpen: false });
  };

  onMeasure = ({ width }) => {
    this.setState({ width });
  };

  //
  // Render

  render() {
    const {
      defaultProfile,
      items,
      tagList,
      dragIndex,
      dropIndex,
      onConfirmDeleteDelayProfile,
      ...otherProps
    } = this.props;

    const {
      isAddDelayProfileModalOpen,
      width
    } = this.state;

    const isDragging = dropIndex !== null;
    const isDraggingUp = isDragging && dropIndex < dragIndex;
    const isDraggingDown = isDragging && dropIndex > dragIndex;

    return (
      <Measure onMeasure={this.onMeasure}>
        <FieldSet legend="Delay Profiles">
          <PageSectionContent
            errorMessage="Unable to load Delay Profiles"
            {...otherProps}
          >
            <Scroller
              className={styles.horizontalScroll}
              scrollDirection={
                scrollDirections.HORIZONTAL
              }
              autoFocus={false}
            >
              <div>
                <div className={styles.delayProfilesHeader}>
                  <div className={styles.column}>Preferred Protocol</div>
                  <div className={styles.column}>Usenet Delay</div>
                  <div className={styles.column}>Torrent Delay</div>
                  <div className={styles.tags}>Tags</div>
                </div>

                <div className={styles.delayProfiles}>
                  {
                    items.map((item, index) => {
                      return (
                        <DelayProfileDragSource
                          key={item.id}
                          tagList={tagList}
                          {...item}
                          {...otherProps}
                          index={index}
                          isDragging={isDragging}
                          isDraggingUp={isDraggingUp}
                          isDraggingDown={isDraggingDown}
                          onConfirmDeleteDelayProfile={onConfirmDeleteDelayProfile}
                        />
                      );
                    })
                  }

                  <DelayProfileDragPreview
                    width={width}
                  />
                </div>

                {
                  defaultProfile ?
                    <div>
                      <DelayProfile
                        tagList={tagList}
                        isDragging={false}
                        onConfirmDeleteDelayProfile={onConfirmDeleteDelayProfile}
                        {...defaultProfile}
                      />
                    </div> :
                    null
                }
              </div>
            </Scroller>

            <div className={styles.addDelayProfile}>
              <Link
                className={styles.addButton}
                onPress={this.onAddDelayProfilePress}
              >
                <Icon name={icons.ADD} />
              </Link>
            </div>

            <EditDelayProfileModalConnector
              isOpen={isAddDelayProfileModalOpen}
              onModalClose={this.onModalClose}
            />
          </PageSectionContent>
        </FieldSet>
      </Measure>
    );
  }
}

DelayProfiles.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  defaultProfile: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  tagList: PropTypes.arrayOf(PropTypes.object).isRequired,
  dragIndex: PropTypes.number,
  dropIndex: PropTypes.number,
  onConfirmDeleteDelayProfile: PropTypes.func.isRequired
};

export default DelayProfiles;
