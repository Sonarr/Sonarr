import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import FilterBuilderRowValue from './FilterBuilderRowValue';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.qualityProfiles,
    (qualityProfiles) => {
      const tagList = qualityProfiles.items.map((qualityProfile) => {
        const {
          id,
          name
        } = qualityProfile;

        return {
          id,
          name
        };
      });

      return {
        tagList
      };
    }
  );
}

export default connect(createMapStateToProps)(FilterBuilderRowValue);
