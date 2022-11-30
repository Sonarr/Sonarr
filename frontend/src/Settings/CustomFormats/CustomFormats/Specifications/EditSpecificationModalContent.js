import PropTypes from 'prop-types';
import React from 'react';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import ProviderFieldFormGroup from 'Components/Form/ProviderFieldFormGroup';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import styles from './EditSpecificationModalContent.css';

function EditSpecificationModalContent(props) {
  const {
    advancedSettings,
    item,
    onInputChange,
    onFieldChange,
    onCancelPress,
    onSavePress,
    onDeleteSpecificationPress,
    ...otherProps
  } = props;

  const {
    id,
    implementationName,
    name,
    negate,
    required,
    fields
  } = item;

  return (
    <ModalContent onModalClose={onCancelPress}>
      <ModalHeader>
        {`${id ? 'Edit' : 'Add'} Condition - ${implementationName}`}
      </ModalHeader>

      <ModalBody>
        <Form
          {...otherProps}
        >
          {
            fields && fields.some((x) => x.label === 'Regular Expression') &&
              <Alert kind={kinds.INFO}>
                <div>
                  <div dangerouslySetInnerHTML={{ __html: 'This condition matches using Regular Expressions. Note that the characters <code>\\^$.|?*+()[{</code> have special meanings and need escaping with a <code>\\</code>' }} />
                  {'More details'} <Link to="https://www.regular-expressions.info/tutorial.html">{'Here'}</Link>
                </div>
                <div>
                  {'Regular expressions can be tested '}
                  <Link to="http://regexstorm.net/tester">Here</Link>
                </div>
              </Alert>
          }

          <FormGroup>
            <FormLabel>
              Name
            </FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT}
              name="name"
              {...name}
              onChange={onInputChange}
            />
          </FormGroup>

          {
            fields && fields.map((field) => {
              return (
                <ProviderFieldFormGroup
                  key={field.name}
                  advancedSettings={advancedSettings}
                  provider="specifications"
                  providerData={item}
                  {...field}
                  onChange={onFieldChange}
                />
              );
            })
          }

          <FormGroup>
            <FormLabel>
              Negate
            </FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="negate"
              {...negate}
              helpText={`If checked, the custom format will not apply if this ${implementationName} condition matches.`}
              onChange={onInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>
              Required
            </FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="required"
              {...required}
              helpText={`This ${implementationName} condition must match for the custom format to apply.  Otherwise a single ${implementationName} match is sufficient.`}
              onChange={onInputChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>
      <ModalFooter>
        {
          id &&
            <Button
              className={styles.deleteButton}
              kind={kinds.DANGER}
              onPress={onDeleteSpecificationPress}
            >
              Delete
            </Button>
        }

        <Button
          onPress={onCancelPress}
        >
          Cancel
        </Button>

        <SpinnerErrorButton
          isSpinning={false}
          onPress={onSavePress}
        >
          Save
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

EditSpecificationModalContent.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  item: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onFieldChange: PropTypes.func.isRequired,
  onCancelPress: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onDeleteSpecificationPress: PropTypes.func
};

export default EditSpecificationModalContent;
