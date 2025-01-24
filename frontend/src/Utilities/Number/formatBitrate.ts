import { filesize } from 'filesize';

function formatBitrate(input: string | number) {
  const size = Number(input);

  if (isNaN(size)) {
    return '';
  }

  const { value, symbol } = filesize(size, {
    base: 10,
    round: 1,
    output: 'object',
  });

  return `${value} ${symbol}/s`;
}

export default formatBitrate;
