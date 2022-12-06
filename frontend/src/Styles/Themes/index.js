import * as dark from './dark';
import * as light from './light';

const defaultDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
const auto = defaultDark ? { ...dark } : { ...light };

export default {
  auto,
  light,
  dark
};
