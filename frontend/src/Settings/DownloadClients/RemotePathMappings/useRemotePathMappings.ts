import ModelBase from 'App/ModelBase';
import {
  useDeleteProvider,
  useManageProviderSettings,
  useProviderSettings,
} from 'Settings/useProviderSettings';

export interface RemotePathMappingModel extends ModelBase {
  host: string;
  localPath: string;
  remotePath: string;
}

const PATH = '/remotepathmapping';

const NEW_REMOTE_PATH_MAPPING: RemotePathMappingModel = {
  id: 0,
  host: '',
  remotePath: '',
  localPath: '',
};

export const useRemotePathMapping = () => {};

export const useRemotePathMappings = () => {
  return useProviderSettings<RemotePathMappingModel>({ path: PATH });
};

export const useManageRemotePathMappings = (id: number) => {
  return useManageProviderSettings<RemotePathMappingModel>(
    id,
    NEW_REMOTE_PATH_MAPPING,
    PATH
  );
};

export const useDeleteRemotePathMapping = (id: number) => {
  const result = useDeleteProvider<RemotePathMappingModel>(id, PATH);

  return {
    ...result,
    deleteRemotePathMapping: result.deleteProvider,
  };
};
