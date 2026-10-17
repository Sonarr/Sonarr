interface Language {
  id: number;
  name: string;
}

export const DEFAULT_LANGUAGE: Language = Object.freeze({
  id: 1,
  name: 'English',
});

export default Language;
