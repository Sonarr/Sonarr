import React from 'react';
import NumberInput, { NumberInputChanged } from './NumberInput';

export interface FloatInputProps {
  name: string;
  value?: number | null;
  min?: number;
  max?: number;
  step?: number;
  placeholder?: string;
  className?: string;
  onChange: (change: NumberInputChanged) => void;
}

function FloatInput(props: FloatInputProps) {
  return <NumberInput {...props} isFloat={true} />;
}

export default FloatInput;
