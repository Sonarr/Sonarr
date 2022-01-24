import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import { sizes } from 'Helpers/Props';
import AddSpecificationPresetMenuItem from './AddSpecificationPresetMenuItem';
import styles from './AddSpecificationItem.css';

class AddSpecificationItem extends Component {

  //
  // Listeners

  onSpecificationSelect = () => {
    const {
      implementation
    } = this.props;

    this.props.onSpecificationSelect({ implementation });
  };

  //
  // Render

  render() {
    const {
      implementation,
      implementationName,
      infoLink,
      presets,
      onSpecificationSelect
    } = this.props;

    const hasPresets = !!presets && !!presets.length;

    return (
      <div
        className={styles.specification}
      >
        <Link
          className={styles.underlay}
          onPress={this.onSpecificationSelect}
        />

        <div className={styles.overlay}>
          <div className={styles.name}>
            {implementationName}
          </div>

          <div className={styles.actions}>
            {
              hasPresets &&
                <span>
                  <Button
                    size={sizes.SMALL}
                    onPress={this.onSpecificationSelect}
                  >
                    Custom
                  </Button>

                  <Menu className={styles.presetsMenu}>
                    <Button
                      className={styles.presetsMenuButton}
                      size={sizes.SMALL}
                    >
                      Presets
                    </Button>

                    <MenuContent>
                      {
                        presets.map((preset, index) => {
                          return (
                            <AddSpecificationPresetMenuItem
                              key={index}
                              name={preset.name}
                              implementation={implementation}
                              onPress={onSpecificationSelect}
                            />
                          );
                        })
                      }
                    </MenuContent>
                  </Menu>
                </span>
            }

            <Button
              to={infoLink}
              size={sizes.SMALL}
            >
              More Info
            </Button>
          </div>
        </div>
      </div>
    );
  }
}

AddSpecificationItem.propTypes = {
  implementation: PropTypes.string.isRequired,
  implementationName: PropTypes.string.isRequired,
  infoLink: PropTypes.string.isRequired,
  presets: PropTypes.arrayOf(PropTypes.object),
  onSpecificationSelect: PropTypes.func.isRequired
};

export default AddSpecificationItem;
