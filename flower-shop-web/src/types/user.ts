export interface UserProfile {
  id: number;
  username: string;
  nickname?: string;
  avatar?: string;
  gender: number;
  phone?: string;
  email?: string;
}

export interface LoginResponse {
  token: string;
  user: UserProfile;
}
