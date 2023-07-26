import PropTypes from 'prop-types';
import React, { useCallback } from 'react';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import { sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import AddSpecificationPresetMenuItem from './AddSpecificationPresetMenuItem';
import styles from './AddSpecificationItem.css';

export default function AddSpecificationItem(props) {
  const {
    implementation,
    implementationName,
    infoLink,
    presets,
    onSpecificationSelect
  } = props;

  const onWrappedSpecificationSelect = useCallback(() => {
    onSpecificationSelect({ implementation });
  }, [implementation, onSpecificationSelect]);

  const hasPresets = !!presets && !!presets.length;

  return (
    <div
      className={styles.specification}
    >
      <Link
        className={styles.underlay}
        onPress={onWrappedSpecificationSelect}
      />

      <div className={styles.overlay}>
        <div className={styles.name}>
          {implementationName}
        </div>

        <div className={styles.actions}>
          {
            hasPresets ?
              <span>
                <Button
                  size={sizes.SMALL}
                  onPress={onWrappedSpecificationSelect}
                >
                  {translate('Custom')}
                </Button>

                <Menu className={styles.presetsMenu}>
                  <Button
                    className={styles.presetsMenuButton}
                    size={sizes.SMALL}
                  >
                    {translate('Presets')}
                  </Button>

                  <MenuContent>
                    {
                      presets.map((preset, index) => {
                        return (
                          <AddSpecificationPresetMenuItem
                            key={index}
                            name={preset.name}
                            implementation={implementation}
                            onPress={onWrappedSpecificationSelect}
                          />
                        );
                      })
                    }
                  </MenuContent>
                </Menu>
              </span> :
              null
          }

          {
            infoLink ?
              <Button
                to={infoLink}
                size={sizes.SMALL}
              >
                {translate('MoreInfo')}
              </Button> :
              null
          }
        </div>
      </div>
    </div>
  );
}

AddSpecificationItem.propTypes = {
  implementation: PropTypes.string.isRequired,
  implementationName: PropTypes.string.isRequired,
  infoLink: PropTypes.string,
  presets: PropTypes.arrayOf(PropTypes.object),
  onSpecificationSelect: PropTypes.func.isRequired
};
