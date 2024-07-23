export default function combinePath(
  isWindows: boolean,
  basePath: string,
  paths: string[] = []
) {
  const slash = isWindows ? '\\' : '/';

  return `${basePath}${slash}${paths.join(slash)}`;
}
