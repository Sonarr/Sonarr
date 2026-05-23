// Mirror values in Styles/Variables/variables.css so var(--foo) at runtime
// matches $foo at build time.

module.exports = {
  // Families
  defaultFontFamily: '"Inter", ui-sans-serif, system-ui, "Helvetica Neue", Helvetica, Arial, sans-serif',
  editorialFontFamily: '"Fraunces", "Iowan Old Style", Georgia, serif',
  monoSpaceFontFamily: '"JetBrains Mono", ui-monospace, "SFMono-Regular", Menlo, Monaco, Consolas, "Courier New", monospace',

  // Sizes — bumped 14 → 15 default; 11/13 small steps preserve label rhythm
  extraSmallFontSize: '11px',
  smallFontSize: '13px',
  defaultFontSize: '15px',
  intermediateFontSize: '16px',
  largeFontSize: '18px',

  lineHeight: '1.55'
};
