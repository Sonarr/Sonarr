import Quality from 'Quality/Quality';
import { QualityProfileFormatItem } from './CustomFormat';

export interface QualityProfileQualityItem {
  quality: Quality;
  allowed: boolean;
  minSize: number | null;
  maxSize: number | null;
  preferredSize: number | null;
}

export interface QualityProfileGroup {
  id: number;
  items: QualityProfileQualityItem[];
  allowed: boolean;
  name: string;
}

export type QualityProfileItems = (
  | QualityProfileQualityItem
  | QualityProfileGroup
)[];

interface QualityProfile {
  name: string;
  upgradeAllowed: boolean;
  cutoff: number;
  items: QualityProfileItems;
  minFormatScore: number;
  cutoffFormatScore: number;
  minUpgradeFormatScore: number;
  formatItems: QualityProfileFormatItem[];
  id: number;
}

export default QualityProfile;
