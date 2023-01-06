const loose = true;

module.exports = {
  plugins: [
    // Stage 1
    '@babel/plugin-proposal-export-default-from',
    ['@babel/plugin-proposal-optional-chaining', { loose }],
    ['@babel/plugin-proposal-nullish-coalescing-operator', { loose }],

    // Stage 2
    '@babel/plugin-proposal-export-namespace-from',

    // Stage 3
    ['@babel/plugin-proposal-class-properties', { loose }],
    '@babel/plugin-syntax-dynamic-import'
  ],
  env: {
    development: {
      presets: [
        ['@babel/preset-react', { development: true }],
        '@babel/preset-typescript'
      ],
      plugins: [
        'babel-plugin-inline-classnames'
      ]
    },
    production: {
      presets: [
        '@babel/preset-react',
        '@babel/preset-typescript'
      ],
      plugins: [
        'babel-plugin-transform-react-remove-prop-types'
      ]
    }
  }
};
