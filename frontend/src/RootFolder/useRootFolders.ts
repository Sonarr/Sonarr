import { useQueryClient } from '@tanstack/react-query';
import ModelBase from 'App/ModelBase';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';

export interface UnmappedFolder {
  name: string;
  path: string;
  relativePath: string;
}

export interface RootFolder extends ModelBase {
  id: number;
  path: string;
  accessible: boolean;
  isEmpty: boolean;
  freeSpace?: number;
  unmappedFolders: UnmappedFolder[];
}

interface AddRootFolder {
  path: string;
}

const DEFAULT_ROOT_FOLDERS: RootFolder[] = [];

const useRootFolders = () => {
  const result = useApiQuery<RootFolder[]>({
    path: '/rootFolder',
  });

  return {
    ...result,
    data: result.data ?? DEFAULT_ROOT_FOLDERS,
  };
};

export const useRootFolder = (id: number, timeout: boolean) => {
  const result = useApiQuery<RootFolder>({
    path: `/rootFolder/${id}`,
    queryParams: { timeout },
    queryOptions: {
      // Disable refetch on window focus to prevent refetching when the user switch tabs
      refetchOnWindowFocus: false,
    },
  });

  return {
    ...result,
    data: result.data,
  };
};

export default useRootFolders;

export const useDeleteRootFolder = (id: number) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error } = useApiMutation<unknown, void>({
    path: `/rootFolder/${id}`,
    method: 'DELETE',
    mutationOptions: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['/rootFolder'] });
      },
    },
  });

  return {
    deleteRootFolder: mutate,
    isDeleting: isPending,
    deleteError: error,
  };
};

export const useAddRootFolder = () => {
  const queryClient = useQueryClient();

  const { mutate, isPending, error, data } = useApiMutation<
    RootFolder,
    AddRootFolder
  >({
    path: '/rootFolder',
    method: 'POST',
    mutationOptions: {
      onSuccess: (newRootFolder) => {
        queryClient.setQueryData<RootFolder[]>(
          ['/rootFolder'],
          (oldRootFolders = []) => {
            return [...oldRootFolders, newRootFolder];
          }
        );
      },
    },
  });

  return {
    addRootFolder: mutate,
    isAdding: isPending,
    addError: error,
    newRootFolder: data,
  };
};
