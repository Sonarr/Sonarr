const growableColumns = [
  'network',
  'qualityProfileId',
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
