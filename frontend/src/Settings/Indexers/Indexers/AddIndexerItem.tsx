import React, { useCallback } from 'react';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import { sizes } from 'Helpers/Props';
import { SelectedSchema } from 'Settings/useProviderSchema';
import translate from 'Utilities/String/translate';
import { IndexerModel } from '../useIndexers';
import AddIndexerPresetMenuItem from './AddIndexerPresetMenuItem';
import styles from './AddIndexerItem.css';

interface AddIndexerItemProps {
  implementation: string;
  implementationName: string;
  infoLink: string;
  presets?: IndexerModel[];
  onIndexerSelect: (selectedSchema: SelectedSchema) => void;
}

function AddIndexerItem({
  implementation,
  implementationName,
  infoLink,
  presets,
  onIndexerSelect,
}: AddIndexerItemProps) {
  const hasPresets = !!presets && !!presets.length;

  const handleIndexerSelect = useCallback(() => {
    onIndexerSelect({ implementation, implementationName });
  }, [implementation, implementationName, onIndexerSelect]);

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
