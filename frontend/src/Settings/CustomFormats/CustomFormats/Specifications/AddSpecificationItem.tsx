import React, { useCallback } from 'react';
import Card from 'Components/Card';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import { icons, sizes } from 'Helpers/Props';
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
          <Menu className={styles.presetsMenu} alignMenu="right">
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
    </Card>
  );
}

export default AddSpecificationItem;
