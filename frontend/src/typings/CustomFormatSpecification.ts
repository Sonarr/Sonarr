import Provider from './Provider';

interface CustomFormatSpecification extends Provider {
  negate: boolean;
  required: boolean;
}

export default CustomFormatSpecification;
