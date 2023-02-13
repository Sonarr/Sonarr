// eslint-disable-next-line @typescript-eslint/no-var-requires
const fs = require('fs');
// eslint-disable-next-line @typescript-eslint/no-var-requires
const typescriptEslintRecommended = require('@typescript-eslint/eslint-plugin').configs.recommended;

const dirs = fs
  .readdirSync('frontend/src', { withFileTypes: true })
  .filter((dirent) => dirent.isDirectory())
  .map((dirent) => dirent.name)
  .join('|');

const frontendFolder = __dirname;

module.exports = {
  parser: '@babel/eslint-parser',

  env: {
    browser: true,
    commonjs: true,
    node: true,
    es6: true
  },

  globals: {
    expect: false,
    chai: false,
    sinon: false
  },

  parserOptions: {
    ecmaVersion: 6,
    sourceType: 'module',
    babelOptions: {
      configFile: `${frontendFolder}/babel.config.js`
    },
    ecmaFeatures: {
      modules: true,
      impliedStrict: true
    }
  },

  plugins: [
    'filenames',
    'react',
    'react-hooks',
    'simple-import-sort',
    'import',
    '@typescript-eslint'
  ],

  settings: {
    react: {
      version: 'detect'
    }
  },

  rules: {
    'filenames/match-exported': ['error'],

    // ECMAScript 6

    'arrow-body-style': [0],
    'arrow-parens': ['error', 'always'],
    'arrow-spacing': ['error', { before: true, after: true }],
    'constructor-super': 'error',
    'generator-star-spacing': 'off',
    'no-class-assign': 'error',
    'no-confusing-arrow': 'error',
    'no-const-assign': 'error',
    'no-dupe-class-members': 'error',
    'no-duplicate-imports': 'error',
    'no-new-symbol': 'error',
    'no-this-before-super': 'error',
    'no-useless-escape': 'error',
    'no-useless-computed-key': 'error',
    'no-useless-constructor': 'error',
    'no-var': 'warn',
    'object-shorthand': ['error', 'properties'],
    'prefer-arrow-callback': 'error',
    'prefer-const': 'warn',
    'prefer-reflect': 'off',
    'prefer-rest-params': 'off',
    'prefer-spread': 'warn',
    'prefer-template': 'error',
    'require-yield': 'off',
    'template-curly-spacing': ['error', 'never'],
    'yield-star-spacing': 'off',

    // Possible Errors

    'comma-dangle': 'error',
    'no-cond-assign': 'error',
    'no-console': 'off',
    'no-constant-condition': 'warn',
    'no-control-regex': 'error',
    'no-debugger': 'off',
    'no-dupe-args': 'error',
    'no-dupe-keys': 'error',
    'no-duplicate-case': 'error',
    'no-empty': 'warn',
    'no-empty-character-class': 'error',
    'no-ex-assign': 'error',
    'no-extra-boolean-cast': 'error',
    'no-extra-parens': ['error', 'functions'],
    'no-extra-semi': 'error',
    'no-func-assign': 'error',
    'no-inner-declarations': 'error',
    'no-invalid-regexp': 'error',
    'no-irregular-whitespace': 'error',
    'no-negated-in-lhs': 'error',
    'no-obj-calls': 'error',
    'no-regex-spaces': 'error',
    'no-sparse-arrays': 'error',
    'no-unexpected-multiline': 'error',
    'no-unreachable': 'warn',
    'no-unsafe-finally': 'error',
    'use-isnan': 'error',
    'valid-jsdoc': 'off',
    'valid-typeof': 'error',

    // Best Practices

    'accessor-pairs': 'off',
    'array-callback-return': 'warn',
    'block-scoped-var': 'warn',
    'consistent-return': 'off',
    curly: 'error',
    'default-case': 'error',
    'dot-location': ['error', 'property'],
    'dot-notation': 'error',
    eqeqeq: ['error', 'smart'],
    'guard-for-in': 'error',
    'no-alert': 'warn',
    'no-caller': 'error',
    'no-case-declarations': 'error',
    'no-div-regex': 'error',
    'no-else-return': 'error',
    'no-empty-function': ['error', { allow: ['arrowFunctions'] }],
    'no-empty-pattern': 'error',
    'no-eval': 'error',
    'no-extend-native': 'error',
    'no-extra-bind': 'error',
    'no-fallthrough': 'error',
    'no-floating-decimal': 'error',
    'no-implicit-coercion': ['error', {
      boolean: false,
      number: true,
      string: true,
      allow: [/* "!!", "~", "*", "+" */]
    }],
    'no-implicit-globals': 'error',
    'no-implied-eval': 'error',
    'no-invalid-this': 'off',
    'no-iterator': 'error',
    'no-labels': 'error',
    'no-lone-blocks': 'error',
    'no-loop-func': 'error',
    'no-magic-numbers': ['off', { ignoreArrayIndexes: true, ignore: [0, 1] }],
    'no-multi-spaces': 'error',
    'no-multi-str': 'error',
    'no-native-reassign': ['error', { exceptions: ['console'] }],
    'no-new': 'off',
    'no-new-func': 'error',
    'no-new-wrappers': 'error',
    'no-octal': 'error',
    'no-octal-escape': 'error',
    'no-param-reassign': 'off',
    'no-process-env': 'off',
    'no-proto': 'error',
    'no-redeclare': 'error',
    'no-return-assign': 'warn',
    'no-script-url': 'error',
    'no-self-assign': 'error',
    'no-self-compare': 'error',
    'no-sequences': 'error',
    'no-throw-literal': 'error',
    'no-unmodified-loop-condition': 'error',
    'no-unused-expressions': 'error',
    'no-unused-labels': 'error',
    'no-useless-call': 'error',
    'no-useless-concat': 'error',
    'no-void': 'error',
    'no-warning-comments': 'off',
    'no-with': 'error',
    radix: ['error', 'as-needed'],
    'vars-on-top': 'off',
    'wrap-iife': ['error', 'inside'],
    yoda: 'error',

    // Strict Mode

    strict: ['error', 'never'],

    // Variables

    'init-declarations': ['error', 'always'],
    'no-catch-shadow': 'error',
    'no-delete-var': 'error',
    'no-label-var': 'error',
    'no-restricted-globals': 'off',
    'no-shadow': 'error',
    'no-shadow-restricted-names': 'error',
    'no-undef': 'error',
    'no-undef-init': 'off',
    'no-undefined': 'off',
    'no-unused-vars': ['error', { args: 'none', ignoreRestSiblings: true }],
    'no-use-before-define': 'error',

    // Node.js and CommonJS

    'callback-return': 'warn',
    'global-require': 'error',
    'handle-callback-err': 'warn',
    'no-mixed-requires': 'error',
    'no-new-require': 'error',
    'no-path-concat': 'error',
    'no-process-exit': 'error',

    // Stylistic Issues

    'array-bracket-spacing': ['error', 'never'],
    'block-spacing': ['error', 'always'],
    'brace-style': ['error', '1tbs', { allowSingleLine: false }],
    camelcase: 'off',
    'comma-spacing': ['error', { before: false, after: true }],
    'comma-style': ['error', 'last'],
    'computed-property-spacing': ['error', 'never'],
    'consistent-this': ['error', 'self'],
    'eol-last': 'error',
    'func-names': 'off',
    'func-style': ['error', 'declaration', { allowArrowFunctions: true }],
    indent: ['error', 2, { SwitchCase: 1 }],
    'key-spacing': ['error', { beforeColon: false, afterColon: true }],
    'keyword-spacing': ['error', { before: true, after: true }],
    'lines-around-comment': ['error', { beforeBlockComment: true, afterBlockComment: false }],
    'max-depth': ['error', { maximum: 5 }],
    'max-nested-callbacks': ['error', 4],
    'max-statements': 'off',
    'max-statements-per-line': ['error', { max: 1 }],
    'new-cap': ['error', { capIsNewExceptions: ['$.Deferred', 'DragDropContext', 'DragLayer', 'DragSource', 'DropTarget'] }],
    'new-parens': 'error',
    'newline-after-var': 'off',
    'newline-before-return': 'off',
    'newline-per-chained-call': 'off',
    'no-array-constructor': 'error',
    'no-bitwise': 'error',
    'no-continue': 'error',
    'no-inline-comments': 'off',
    'no-lonely-if': 'warn',
    'no-mixed-spaces-and-tabs': 'error',
    'no-multiple-empty-lines': ['error', { max: 1 }],
    'no-negated-condition': 'warn',
    'no-nested-ternary': 'error',
    'no-new-object': 'error',
    'no-plusplus': 'off',
    'no-restricted-syntax': 'off',
    'no-spaced-func': 'error',
    'no-ternary': 'off',
    'no-trailing-spaces': 'error',
    'no-underscore-dangle': ['error', { allowAfterThis: true }],
    'no-unneeded-ternary': 'error',
    'no-whitespace-before-property': 'error',
    'object-curly-spacing': ['error', 'always'],
    'one-var': ['error', 'never'],
    'one-var-declaration-per-line': ['error', 'always'],
    'operator-assignment': ['off', 'never'],
    'operator-linebreak': ['error', 'after'],
    'quote-props': ['error', 'as-needed'],
    quotes: ['error', 'single'],
    'require-jsdoc': 'off',
    semi: 'error',
    'semi-spacing': ['error', { before: false, after: true }],
    'sort-vars': 'off',
    'space-before-blocks': ['error', 'always'],
    'space-before-function-paren': ['error', 'never'],
    'space-in-parens': 'off',
    'space-infix-ops': 'off',
    'space-unary-ops': 'off',
    'spaced-comment': 'error',
    'wrap-regex': 'error',

    // ImportSort

    'simple-import-sort/imports': 'error',
    'import/newline-after-import': 'error',

    // React

    'react/jsx-boolean-value': [2, 'always'],
    'react/jsx-uses-vars': 2,
    'react/jsx-closing-bracket-location': 2,
    'react/jsx-tag-spacing': ['error'],
    'react/jsx-curly-spacing': [2, 'never'],
    'react/jsx-equals-spacing': [2, 'never'],
    'react/jsx-indent-props': [2, 2],
    'react/jsx-indent': [2, 2, { indentLogicalExpressions: true }],
    'react/jsx-key': 2,
    'react/jsx-no-bind': [2, { allowArrowFunctions: true }],
    'react/jsx-no-duplicate-props': [2, { ignoreCase: true }],
    'react/jsx-max-props-per-line': [2, { maximum: 2 }],
    'react/jsx-handler-names': [2, { eventHandlerPrefix: '(on|dispatch)', eventHandlerPropPrefix: 'on' }],
    'react/jsx-no-undef': 2,
    'react/jsx-pascal-case': 2,
    'react/jsx-uses-react': 2,
    // Explicitly disabled in case we want to enable them again
    'react/no-did-mount-set-state': 0,
    'react/no-did-update-set-state': 0,
    'react/no-direct-mutation-state': 2,
    'react/no-multi-comp': [2, { ignoreStateless: true }],
    'react/no-unknown-property': 2,
    'react/prefer-es6-class': 2,
    'react/prop-types': 2,
    'react/react-in-jsx-scope': 2,
    'react/self-closing-comp': 2,
    'react/sort-comp': 2,
    'react/jsx-wrap-multilines': 2,
    'react-hooks/rules-of-hooks': 'error',
    'react-hooks/exhaustive-deps': 'error'
  },
  overrides: [
    {
      files: [
        '*.js'
      ],
      rules: {
        'simple-import-sort/imports': [
          'error',
          {
            groups: [
              // Packages
              // Absolute Paths
              // Relative Paths
              // Css
              ['^@?\\w', `^(${dirs})(/.*|$)`, '^\\.', '^\\..*css$']
            ]
          }
        ]
      }
    },
    {
      files: [
        '*.ts',
        '*.tsx'
      ],

      parser: '@typescript-eslint/parser',
      parserOptions: {
        project: './tsconfig.json'
      },

      rules: Object.assign(typescriptEslintRecommended.rules, {
        'no-shadow': 'off',
        // These should be enabled after cleaning things up
        '@typescript-eslint/no-unused-vars': 'warn',
        '@typescript-eslint/explicit-function-return-type': 'off',
        'react/prop-types': 'off',
        'simple-import-sort/imports': [
          'error',
          {
            groups: [
              // Packages
              // Absolute Paths
              // Relative Paths
              // Css
              ['^@?\\w', `^(${dirs})(/.*|$)`, '^\\.', '^\\..*css$']
            ]
          }
        ]
      })
    },
    {
      files: [
        '*.css.d.ts'
      ],
      rules: {
        'filenames/match-exported': 'off',
        'init-declarations': 'off',
        'prettier/prettier': 'off'
      }
    }
  ]
};
