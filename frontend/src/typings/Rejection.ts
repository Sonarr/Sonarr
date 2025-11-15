interface Rejection {
  message: string;
  reason: string;
  type: 'permanent' | 'temporary';
}

export default Rejection;
