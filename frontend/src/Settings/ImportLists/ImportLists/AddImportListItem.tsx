import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import { sizes } from 'Helpers/Props';
import { selectImportListSchema } from 'Store/Actions/settingsActions';
import ImportList from 'typings/ImportList';
import translate from 'Utilities/String/translate';
import AddImportListPresetMenuItem from './AddImportListPresetMenuItem';
import styles from './AddImportListItem.css';

interface AddImportListItemProps {
  implementation: string;
  implementationName: string;
  minRefreshInterval: string;
  infoLink: string;
  presets?: ImportList[];
  onImportListSelect: () => void;
}

function AddImportListItem({
  implementation,
  implementationName,
  minRefreshInterval,
  infoLink,
  presets,
  onImportListSelect,
}: AddImportListItemProps) {
  const dispatch = useDispatch();
  const hasPresets = !!(presets && presets.length);

  const handleImportListSelect = useCallback(() => {
    dispatch(
      selectImportListSchema({
        implementation,
        implementationName,
      })
    );

    onImportListSelect();
  }, [implementation, implementationName, dispatch, onImportListSelect]);

  return (
    <div className={styles.list}>
      <Link className={styles.underlay} onPress={handleImportListSelect} />

      <div className={styles.overlay}>
        <div className={styles.name}>{implementationName}</div>

        <div className={styles.actions}>
          {hasPresets && (
            <span>
              <Button size={sizes.SMALL} onPress={handleImportListSelect}>
                {translate('Custom')}
              </Button>

              <Menu className={styles.presetsMenu}>
                <Button className={styles.presetsMenuButton} size={sizes.SMALL}>
                  {translate('Presets')}
                </Button>

                <MenuContent>
                  {presets.map((preset) => {
                    return (
                      <AddImportListPresetMenuItem
                        key={preset.name}
                        name={preset.name}
                        implementation={implementation}
                        implementationName={implementationName}
                        minRefreshInterval={minRefreshInterval}
                        onPress={onImportListSelect}
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

export default AddImportListItem;
