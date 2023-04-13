export enum RejectionType {
  Permanent = 'permanent',
  Temporary = 'temporary',
}

interface Rejection {
  reason: string;
  type: RejectionType;
}

export default Rejection;
