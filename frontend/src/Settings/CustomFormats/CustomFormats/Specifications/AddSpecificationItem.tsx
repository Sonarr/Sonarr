import React, { useCallback } from 'react';
import Card from 'Components/Card';
import Button from 'Components/Link/Button';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import { sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import { CustomFormatSpecification } from '../useCustomFormats';
import AddSpecificationPresetMenuItem from './AddSpecificationPresetMenuItem';
import styles from './AddSpecificationItem.css';

interface AddSpecificationItemProps {
  implementation: string;
  implementationName: string;
  infoLink: string;
  presets?: CustomFormatSpecification[];
  onSpecificationSelect: (selected: {
    implementation: string;
    presetName?: string;
  }) => void;
}

function AddSpecificationItem({
  implementation,
  implementationName,
  infoLink,
  presets,
  onSpecificationSelect,
}: AddSpecificationItemProps) {
  const hasPresets = !!presets && !!presets.length;

  const handleCustomSelect = useCallback(() => {
    onSpecificationSelect({ implementation });
  }, [implementation, onSpecificationSelect]);

  return (
    <Card
      className={styles.specification}
      overlayClassName={styles.overlay}
      overlayContent={true}
      aria-label={translate('AddConditionImplementation', {
        implementationName,
      })}
      onPress={handleCustomSelect}
    >
      <div className={styles.name}>{implementationName}</div>

      <div className={styles.actions}>
        {hasPresets ? (
          <span>
            <Button size={sizes.SMALL} onPress={handleCustomSelect}>
              {translate('Custom')}
            </Button>

            <Menu className={styles.presetsMenu}>
              <Button className={styles.presetsMenuButton} size={sizes.SMALL}>
                {translate('Presets')}
              </Button>

              <MenuContent>
                {presets.map((preset) => (
                  <AddSpecificationPresetMenuItem
                    key={preset.name}
                    name={preset.name}
                    implementation={implementation}
                    onPress={onSpecificationSelect}
                  />
                ))}
              </MenuContent>
            </Menu>
          </span>
        ) : null}

        <Button to={infoLink} size={sizes.SMALL}>
          {translate('MoreInfo')}
        </Button>
      </div>
    </Card>
  );
}

export default AddSpecificationItem;
