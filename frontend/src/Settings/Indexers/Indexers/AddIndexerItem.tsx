import React, { useCallback } from 'react';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import { icons, sizes } from 'Helpers/Props';
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
      <Link
        className={styles.underlay}
        aria-label={translate('AddIndexerImplementation', {
          implementationName,
        })}
        onPress={handleIndexerSelect}
      />

      <div className={styles.overlay}>
        <div className={styles.name}>{implementationName}</div>

        <div className={styles.actions}>
          {hasPresets ? (
            <Menu className={styles.presetsMenu} alignMenu="right">
              <Button className={styles.presetsMenuButton} size={sizes.SMALL}>
                {translate('Presets')}
              </Button>

              <MenuContent>
                {presets.map((preset) => (
                  <AddIndexerPresetMenuItem
                    key={preset.name}
                    name={preset.name}
                    implementation={implementation}
                    implementationName={implementationName}
                    onPress={onIndexerSelect}
                  />
                ))}
              </MenuContent>
            </Menu>
          ) : null}

          <Link
            className={styles.infoLink}
            to={infoLink}
            title={translate('MoreInfo')}
            aria-label={translate('MoreInfo')}
          >
            <Icon name={icons.INFO} size={18} />
          </Link>
        </div>
      </div>
    </div>
  );
}

export default AddIndexerItem;
