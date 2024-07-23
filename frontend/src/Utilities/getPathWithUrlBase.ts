export default function getPathWithUrlBase(path: string) {
  return `${window.Sonarr.urlBase}${path}`;
}
