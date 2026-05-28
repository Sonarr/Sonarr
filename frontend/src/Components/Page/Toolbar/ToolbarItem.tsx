import React, { type ReactElement, type ReactNode, useEffect } from 'react';
import { InternalOverflowItem } from './Overflow';
import { useToolbarRegistry } from './PageToolbar';
import PageToolbarButton, {
  type PageToolbarButtonProps,
} from './PageToolbarButton';
import styles from './ToolbarItem.css';

type ToolbarItemLayoutProps = {
  id: string;
  priority?: number;
  pinned?: boolean;
  groupId?: string;
};

type ToolbarItemButtonModeProps = ToolbarItemLayoutProps &
  Partial<PageToolbarButtonProps> & {
    label: string;
    iconName: PageToolbarButtonProps['iconName'];
    children?: never;
    renderButton?: (props: PageToolbarButtonProps) => ReactNode;
    renderOverflow?: (base: PageToolbarButtonProps) => ReactNode;
  };

type ToolbarItemChildrenModeProps = ToolbarItemLayoutProps & {
  children: ReactElement;
  label?: never;
  iconName?: never;
  isSpinning?: never;
  isDisabled?: never;
  spinningName?: never;
  onPress?: never;
  renderButton?: never;
  renderOverflow?: never;
};

type ToolbarItemProps =
  | ToolbarItemButtonModeProps
  | ToolbarItemChildrenModeProps;

function ToolbarItem(props: ToolbarItemProps) {
  if (props.children !== undefined) {
    return <ToolbarItemChildrenMode {...props} />;
  }

  return <ToolbarItemButtonMode {...props} />;
}

function ToolbarItemButtonMode(props: ToolbarItemButtonModeProps) {
  const {
    id,
    priority,
    pinned,
    groupId,
    label,
    iconName,
    isSpinning,
    isDisabled,
    spinningName,
    onPress,
    renderButton,
    renderOverflow,
    ...rest
  } = props;
  const registry = useToolbarRegistry();

  useEffect(() => {
    registry.register({
      id,
      label,
      iconName,
      isSpinning,
      isDisabled,
      spinningName,
      onPress,
      renderOverflow,
      priority,
      groupId,
    });

    return () => {
      registry.unregister(id);
    };
  }, [
    registry,
    id,
    label,
    iconName,
    isSpinning,
    isDisabled,
    spinningName,
    onPress,
    renderOverflow,
    priority,
    groupId,
  ]);

  const buttonProps: PageToolbarButtonProps = {
    ...rest,
    label,
    iconName,
    isSpinning,
    isDisabled,
    spinningName,
    onPress,
  };

  const content = renderButton ? (
    renderButton(buttonProps)
  ) : (
    <PageToolbarButton {...buttonProps} />
  );

  return (
    <InternalOverflowItem
      id={id}
      priority={priority}
      pinned={pinned}
      groupId={groupId}
    >
      <div className={styles.slot}>{content as ReactElement}</div>
    </InternalOverflowItem>
  );
}

function ToolbarItemChildrenMode(props: ToolbarItemChildrenModeProps) {
  const { id, priority, pinned, groupId, children } = props;

  return (
    <InternalOverflowItem
      id={id}
      priority={priority}
      pinned={pinned}
      groupId={groupId}
    >
      <div className={styles.slot}>{children}</div>
    </InternalOverflowItem>
  );
}

export default ToolbarItem;
