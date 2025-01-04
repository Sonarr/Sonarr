import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import { sizes } from 'Helpers/Props';
import { selectNotificationSchema } from 'Store/Actions/settingsActions';
import Notification from 'typings/Notification';
import translate from 'Utilities/String/translate';
import AddNotificationPresetMenuItem from './AddNotificationPresetMenuItem';
import styles from './AddNotificationItem.css';

interface AddNotificationItemProps {
  implementation: string;
  implementationName: string;
  infoLink: string;
  presets?: Notification[];
  onNotificationSelect: () => void;
}

function AddNotificationItem({
  implementation,
  implementationName,
  infoLink,
  presets,
  onNotificationSelect,
}: AddNotificationItemProps) {
  const dispatch = useDispatch();
  const hasPresets = !!presets && !!presets.length;

  const handleNotificationSelect = useCallback(() => {
    dispatch(
      selectNotificationSchema({
        implementation,
        implementationName,
      })
    );

    onNotificationSelect();
  }, [implementation, implementationName, dispatch, onNotificationSelect]);

  return (
    <div className={styles.notification}>
      <Link className={styles.underlay} onPress={handleNotificationSelect} />

      <div className={styles.overlay}>
        <div className={styles.name}>{implementationName}</div>

        <div className={styles.actions}>
          {hasPresets ? (
            <span>
              <Button size={sizes.SMALL} onPress={handleNotificationSelect}>
                {translate('Custom')}
              </Button>

              <Menu className={styles.presetsMenu}>
                <Button className={styles.presetsMenuButton} size={sizes.SMALL}>
                  {translate('Presets')}
                </Button>

                <MenuContent>
                  {presets.map((preset) => {
                    return (
                      <AddNotificationPresetMenuItem
                        key={preset.name}
                        name={preset.name}
                        implementation={implementation}
                        implementationName={implementationName}
                        onPress={onNotificationSelect}
                      />
                    );
                  })}
                </MenuContent>
              </Menu>
            </span>
          ) : null}

          <Button to={infoLink} size={sizes.SMALL}>
            {translate('MoreInfo')}
          </Button>
        </div>
      </div>
    </div>
  );
}

export default AddNotificationItem;
