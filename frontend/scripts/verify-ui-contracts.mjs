import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { readFileSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const require = createRequire(import.meta.url);
const scriptDirectory = dirname(fileURLToPath(import.meta.url));
const repositoryRoot = resolve(scriptDirectory, '../..');

function read(relativePath) {
  return readFileSync(resolve(repositoryRoot, relativePath), 'utf8');
}

function assertIncludes(relativePath, expected, message) {
  assert.ok(
    read(relativePath).includes(expected),
    `${message} (${relativePath})`
  );
}

const light = require(resolve(
  repositoryRoot,
  'frontend/src/Styles/Themes/light.js'
));
const dark = require(resolve(
  repositoryRoot,
  'frontend/src/Styles/Themes/dark.js'
));
const oled = require(resolve(
  repositoryRoot,
  'frontend/src/Styles/Themes/oled.js'
));

const lightKeys = Object.keys(light).sort();
const darkKeys = Object.keys(dark).sort();
const oledKeys = Object.keys(oled).sort();

assert.deepEqual(darkKeys, lightKeys, 'Dark and light theme tokens must match');
assert.deepEqual(oledKeys, lightKeys, 'OLED and light theme tokens must match');
assert.equal(oled.pageBackground, '#000000', 'OLED canvas must use true black');
assert.equal(oled.pageHeaderBackgroundColor, '#050505');
assert.equal(oled.sidebarBackgroundColor, '#050505');
assert.ok(
  Object.values(oled).every((value) => typeof value === 'string'),
  'All OLED theme values must be CSS strings'
);

assertIncludes(
  'frontend/src/Styles/Themes/index.js',
  "['auto', 'light', 'dark', 'oled']",
  'Theme registry must expose Auto, Light, Dark, and OLED'
);
assertIncludes(
  'frontend/src/Settings/UI/useUiSettings.ts',
  "'oled'",
  'UI settings type must include OLED'
);
assertIncludes(
  'frontend/src/App/ApplyTheme.tsx',
  'themeStorageKey',
  'Runtime theme application must persist the explicit selection'
);
assertIncludes(
  'frontend/src/index.ejs',
  "var storageKey = 'sonarr.ui.theme'",
  'Initial app theme must be read before the first visible paint'
);
assertIncludes(
  'frontend/src/login.html',
  '<option value="oled">OLED</option>',
  'Login appearance selector must include OLED'
);
assertIncludes(
  'src/Sonarr.Http/Frontend/Mappers/IndexHtmlMapper.cs',
  'Replace("__THEME__"',
  'The configured theme must be injected into the app shell'
);
assertIncludes(
  'src/NzbDrone.Core/Configuration/UiTheme.cs',
  'public const string Oled = "oled";',
  'Backend theme validation must include OLED'
);
assertIncludes(
  'frontend/src/Components/Page/Header/PageHeader.tsx',
  'aria-expanded={isSidebarVisible}',
  'Mobile navigation trigger must expose its expanded state'
);
assertIncludes(
  'frontend/src/Components/Page/Sidebar/PageSidebar.tsx',
  'focusTarget?.focus',
  'Mobile navigation must restore focus'
);
assertIncludes(
  'frontend/src/Components/Modal/Modal.tsx',
  'aria-modal="true"',
  'Modals must expose modal dialog semantics'
);
assertIncludes(
  'frontend/src/Components/Table/Table.tsx',
  "aria-label={horizontalScroll ? translate('Table') : undefined}",
  'Scrollable data tables must be keyboard discoverable'
);
assertIncludes(
  'frontend/src/Styles/Themes/index.js',
  'documentElement.style.backgroundColor = themeVariables.pageBackground;',
  'Live theme changes must update the root canvas color'
);
assertIncludes(
  'frontend/src/login.html',
  'document.documentElement.style.backgroundColor = finalTheme.pageBackground;',
  'Login theme changes must update the root canvas color'
);
assertIncludes(
  'frontend/src/Components/Menu/MenuContent.tsx',
  "style={isOpen ? style : { ...style, display: 'none' }}",
  'Closed menus must not paint an empty popper shell'
);
assertIncludes(
  'frontend/src/Components/Menu/Menu.tsx',
  "event.key !== 'ArrowDown'",
  'Menus must support arrow-key navigation'
);
assertIncludes(
  'frontend/src/Components/Menu/MenuItem.tsx',
  'tabIndex = -1,',
  'Menu items must use a managed tab stop'
);
assertIncludes(
  'frontend/src/Components/Link/Link.tsx',
  'event.preventDefault();',
  'Disabled links must not navigate'
);

assert.ok(
  !read('frontend/src/Components/Page/PageContent.css').includes(
    'overflow-x: hidden'
  ),
  'PageContent must not mask horizontal overflow globally'
);
assert.ok(
  !read('frontend/src/Styles/scaffolding.css').includes('*:focus {\n  outline: none;'),
  'Global focus outlines must not be removed'
);
assertIncludes(
  'frontend/src/Components/Form/Input.css',
  'font-size: 16px;',
  'Mobile form controls must avoid browser input zoom'
);

console.log(
  `UI contract verification passed: ${lightKeys.length} theme tokens, true-black OLED, persistence, responsive shell, and accessibility markers.`
);
