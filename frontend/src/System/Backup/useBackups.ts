import { useMutation, useQueryClient } from '@tanstack/react-query';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import Backup from 'typings/Backup';

const useBackups = () => {
  const result = useApiQuery<Backup[]>({
    path: '/system/backup',
    queryOptions: {
      staleTime: 30 * 1000, // 30 seconds
    },
  });

  return {
    ...result,
    data: result.data ?? [],
  };
};

export default useBackups;

export const useDeleteBackup = (id: number) => {
  const queryClient = useQueryClient();

  return useApiMutation<object, void>({
    path: `/system/backup/${id}`,
    method: 'DELETE',
    mutationOptions: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['/system/backup'] });
      },
    },
  });
};

interface RestoreBackupResponse {
  restartRequired: boolean;
}

export const useRestoreBackup = (id: number) => {
  const queryClient = useQueryClient();

  return useApiMutation<RestoreBackupResponse, void>({
    path: `/system/backup/restore/${id}`,
    method: 'POST',
    mutationOptions: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['/system/backup'] });
      },
    },
  });
};

export const useRestoreBackupUpload = () => {
  const queryClient = useQueryClient();

  return useMutation<RestoreBackupResponse, Error, FormData>({
    mutationFn: async (formData: FormData) => {
      const response = await fetch(
        `${window.Sonarr.urlBase}/api/v5/system/backup/restore/upload`,
        {
          method: 'POST',
          headers: {
            'X-Api-Key': window.Sonarr.apiKey,
            'X-Sonarr-Client': 'Sonarr',
            // Don't set Content-Type, let browser set it with boundary for multipart/form-data
          },
          body: formData,
        }
      );

      if (!response.ok) {
        throw new Error(`Failed to restore backup: ${response.statusText}`);
      }

      return response.json();
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['/system/backup'] });
    },
  });
};
