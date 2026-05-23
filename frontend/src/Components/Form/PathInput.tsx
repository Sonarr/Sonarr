import React, { useCallback, useState } from 'react';
import usePaths from 'Path/usePaths';
import { InputChanged } from 'typings/inputs';
import { PathInputInternal } from './PathInputInternal';

export interface PathInputProps {
  className?: string;
  name: string;
  value?: string;
  placeholder?: string;
  includeFiles: boolean;
  hasButton?: boolean;
  hasFileBrowser?: boolean;
  onChange: (change: InputChanged<string>) => void;
}

function PathInput(props: PathInputProps) {
  const { includeFiles, value = '' } = props;
  const [currentPath, setCurrentPath] = useState(value);

  const { data } = usePaths({
    path: currentPath,
    includeFiles,
  });

  const handleFetchPaths = useCallback((path: string) => {
    setCurrentPath(path);
  }, []);

  const handleClearPaths = useCallback(() => {
    // No-op for React Query implementation as we don't need to clear
  }, []);

  return (
    <PathInputInternal
      {...props}
      paths={data.paths}
      onFetchPaths={handleFetchPaths}
      onClearPaths={handleClearPaths}
    />
  );
}

export default PathInput;
