import { useSelector } from 'react-redux';
import createTagsSelector from 'Store/Selectors/createTagsSelector';

const useTags = () => {
  return useSelector(createTagsSelector());
};

export default useTags;
