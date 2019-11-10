import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { sizes } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import AddImportListPresetMenuItem from './AddImportListPresetMenuItem';
import styles from './AddImportListItem.css';

class AddImportListItem extends Component {

  //
  // Listeners

  onImportListSelect = () => {
    const {
      implementation
    } = this.props;

    this.props.onImportListSelect({ implementation });
  }

  //
  // Render

  render() {
    const {
      implementation,
      implementationName,
      infoLink,
      presets,
      onImportListSelect
    } = this.props;

    const hasPresets = !!presets && !!presets.length;

    return (
      <div
        className={styles.list}
      >
        <Link
          className={styles.underlay}
          onPress={this.onImportListSelect}
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
                    onPress={this.onListSelect}
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
                        presets.map((preset) => {
                          return (
                            <AddImportListPresetMenuItem
                              key={preset.name}
                              name={preset.name}
                              implementation={implementation}
                              onPress={onImportListSelect}
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
              More info
            </Button>
          </div>
        </div>
      </div>
    );
  }
}

AddImportListItem.propTypes = {
  implementation: PropTypes.string.isRequired,
  implementationName: PropTypes.string.isRequired,
  infoLink: PropTypes.string.isRequired,
  presets: PropTypes.arrayOf(PropTypes.object),
  onImportListSelect: PropTypes.func.isRequired
};

export default AddImportListItem;
