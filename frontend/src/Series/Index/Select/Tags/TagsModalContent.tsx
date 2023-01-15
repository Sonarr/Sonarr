import { concat, uniq } from 'lodash';
import React, { useCallback, useMemo, useState } from 'react';
import { useSelector } from 'react-redux';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Label from 'Components/Label';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import styles from './TagsModalContent.css';

interface TagsModalContentProps {
  seriesIds: number[];
  onApplyTagsPress: (tags: number[], applyTags: string) => void;
  onModalClose: () => void;
}

function TagsModalContent(props: TagsModalContentProps) {
  const { seriesIds, onModalClose, onApplyTagsPress } = props;

  const allSeries = useSelector(createAllSeriesSelector());
  const tagList = useSelector(createTagsSelector());

  const [tags, setTags] = useState<number[]>([]);
  const [applyTags, setApplyTags] = useState('add');

  const seriesTags = useMemo(() => {
    const series = seriesIds.map((id) => {
      return allSeries.find((s) => s.id === id);
    });

    return uniq(concat(...series.map((s) => s.tags)));
  }, [seriesIds, allSeries]);

  const onTagsChange = useCallback(
    ({ value }) => {
      setTags(value);
    },
    [setTags]
  );

  const onApplyTagsChange = useCallback(
    ({ value }) => {
      setApplyTags(value);
    },
    [setApplyTags]
  );

  const onApplyPress = useCallback(() => {
    onApplyTagsPress(tags, applyTags);
  }, [tags, applyTags, onApplyTagsPress]);

  const applyTagsOptions = [
    { key: 'add', value: 'Add' },
    { key: 'remove', value: 'Remove' },
    { key: 'replace', value: 'Replace' },
  ];

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>Tags</ModalHeader>

      <ModalBody>
        <Form>
          <FormGroup>
            <FormLabel>Tags</FormLabel>

            <FormInputGroup
              type={inputTypes.TAG}
              name="tags"
              value={tags}
              onChange={onTagsChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Apply Tags</FormLabel>

            <FormInputGroup
              type={inputTypes.SELECT}
              name="applyTags"
              value={applyTags}
              values={applyTagsOptions}
              helpTexts={[
                'How to apply tags to the selected series',
                'Add: Add the tags the existing list of tags',
                'Remove: Remove the entered tags',
                'Replace: Replace the tags with the entered tags (enter no tags to clear all tags)',
              ]}
              onChange={onApplyTagsChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Result</FormLabel>

            <div className={styles.result}>
              {seriesTags.map((id) => {
                const tag = tagList.find((t) => t.id === id);

                if (!tag) {
                  return null;
                }

                const removeTag =
                  (applyTags === 'remove' && tags.indexOf(id) > -1) ||
                  (applyTags === 'replace' && tags.indexOf(id) === -1);

                return (
                  <Label
                    key={tag.id}
                    title={removeTag ? 'Removing tag' : 'Existing tag'}
                    kind={removeTag ? kinds.INVERSE : kinds.INFO}
                    size={sizes.LARGE}
                  >
                    {tag.label}
                  </Label>
                );
              })}

              {(applyTags === 'add' || applyTags === 'replace') &&
                tags.map((id) => {
                  const tag = tagList.find((t) => t.id === id);

                  if (!tag) {
                    return null;
                  }

                  if (seriesTags.indexOf(id) > -1) {
                    return null;
                  }

                  return (
                    <Label
                      key={tag.id}
                      title={'Adding tag'}
                      kind={kinds.SUCCESS}
                      size={sizes.LARGE}
                    >
                      {tag.label}
                    </Label>
                  );
                })}
            </div>
          </FormGroup>
        </Form>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>Cancel</Button>

        <Button kind={kinds.PRIMARY} onPress={onApplyPress}>
          Apply
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default TagsModalContent;
