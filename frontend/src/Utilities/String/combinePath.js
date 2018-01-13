export default function combinePath(isWindows, basePath, paths = []) {
  const slash = isWindows ? '\\' : '/';

  return `${basePath}${slash}${paths.join(slash)}`;
}
