import classNames from 'classnames';
import React, {
  Children,
  ComponentPropsWithoutRef,
  isValidElement,
  ReactNode,
  useId,
} from 'react';
import { Size } from 'Helpers/Props/sizes';
import { FormRowProvider } from './FormRowContext';
import styles from './FormRow.module.css';

export type FormRowLayout = 'right' | 'stacked';

interface FormRowProps extends ComponentPropsWithoutRef<'div'> {
  className?: string;
  children: ReactNode;
  size?: Extract<Size, keyof typeof styles>;
  layout?: FormRowLayout;
  advancedSettings?: boolean;
  isAdvanced?: boolean;
}

interface FormInputDetails {
  id?: string;
  name?: string;
  type?: string;
}

function getFormInputDetails(children: ReactNode): FormInputDetails {
  let details: FormInputDetails = {};

  Children.forEach(children, (child) => {
    if (
      !isValidElement<{
        children?: ReactNode;
        id?: string;
        name?: string;
        type?: string;
      }>(child)
    ) {
      return;
    }

    if (child.props.name) {
      details = {
        id: child.props.id,
        name: child.props.name,
        type: child.props.type,
      };
    }

    const childDetails = getFormInputDetails(child.props.children);

    if (childDetails.name) {
      details = childDetails;
    }
  });

  return details;
}

function FormRow({
  className = styles.row,
  children,
  size = 'small',
  layout = 'right',
  advancedSettings = false,
  isAdvanced = false,
  ...otherProps
}: FormRowProps) {
  const generatedId = useId();

  if (!advancedSettings && isAdvanced) {
    return null;
  }

  const { id: inputId, name, type } = getFormInputDetails(children);
  const id = type ? inputId ?? generatedId : undefined;

  return (
    <div
      className={classNames(className, styles[size], styles[layout])}
      {...otherProps}
    >
      <FormRowProvider id={id} name={name} type={type}>
        {children}
      </FormRowProvider>
    </div>
  );
}

export default FormRow;
