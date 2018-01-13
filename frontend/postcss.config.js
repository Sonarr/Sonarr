const reload = require('require-nocache')(module);

module.exports = (ctx, configPath, options) => {
  const config = {
    plugins: {
      'postcss-mixins': {
        mixinsDir: [
          'frontend/src/Styles/Mixins'
        ]
      },
      'postcss-simple-vars': {
        variables: () =>
          ctx.options.cssVarsFiles.reduce((acc, vars) => {
            return Object.assign(acc, reload(vars));
          }, {})
      },
      'postcss-nested': {},
      autoprefixer: {
        browsers: [
          'Chrome >= 30',
          'Firefox >= 30',
          'Safari >= 6',
          'Edge >= 12',
          'Explorer >= 11',
          'iOS >= 7',
          'Android >= 4.4'
        ]
      }
    }
  };

  return config;
};
