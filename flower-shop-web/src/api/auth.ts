import request from './request'
import type { LoginResponse, UserProfile } from '@/types/user'

export interface RegisterRequest {
  username: string
  password: string
  confirmPassword: string
  phone?: string
  email?: string
}

export interface LoginRequest {
  username: string
  password: string
}

export const authApi = {
  register: (data: RegisterRequest) => request.post('/auth/register', data),
  login: (data: LoginRequest) => request.post<LoginResponse>('/auth/login', data),
  getProfile: () => request.get<UserProfile>('/auth/profile'),
}
