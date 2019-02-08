const growableColumns = [
  'network',
  'qualityProfileId',
  'languageProfileId',
  'path',
  'tags'
];

export default function hasGrowableColumns(columns) {
  return columns.some((column) => {
    const {
      name,
      isVisible
    } = column;

    return growableColumns.includes(name) && isVisible;
  });
}
