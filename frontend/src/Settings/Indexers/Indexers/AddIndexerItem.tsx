import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import { sizes } from 'Helpers/Props';
import { selectIndexerSchema } from 'Store/Actions/settingsActions';
import Indexer from 'typings/Indexer';
import translate from 'Utilities/String/translate';
import AddIndexerPresetMenuItem from './AddIndexerPresetMenuItem';
import styles from './AddIndexerItem.css';

interface AddIndexerItemProps {
  implementation: string;
  implementationName: string;
  infoLink: string;
  presets?: Indexer[];
  onIndexerSelect: () => void;
}

function AddIndexerItem({
  implementation,
  implementationName,
  infoLink,
  presets,
  onIndexerSelect,
}: AddIndexerItemProps) {
  const dispatch = useDispatch();
  const hasPresets = !!presets && !!presets.length;

  const handleIndexerSelect = useCallback(() => {
    dispatch(
      selectIndexerSchema({
        implementation,
        implementationName,
      })
    );

    onIndexerSelect();
  }, [implementation, implementationName, dispatch, onIndexerSelect]);

  return (
    <div className={styles.indexer}>
      <Link className={styles.underlay} onPress={handleIndexerSelect} />

      <div className={styles.overlay}>
        <div className={styles.name}>{implementationName}</div>

        <div className={styles.actions}>
          {hasPresets && (
            <span>
              <Button size={sizes.SMALL} onPress={handleIndexerSelect}>
                {translate('Custom')}
              </Button>

              <Menu className={styles.presetsMenu}>
                <Button className={styles.presetsMenuButton} size={sizes.SMALL}>
                  {translate('Presets')}
                </Button>

                <MenuContent>
                  {presets.map((preset) => {
                    return (
                      <AddIndexerPresetMenuItem
                        key={preset.name}
                        name={preset.name}
                        implementation={implementation}
                        implementationName={implementationName}
                        onPress={onIndexerSelect}
                      />
                    );
                  })}
                </MenuContent>
              </Menu>
            </span>
          )}

          <Button to={infoLink} size={sizes.SMALL}>
            {translate('MoreInfo')}
          </Button>
        </div>
      </div>
    </div>
  );
}

export default AddIndexerItem;
