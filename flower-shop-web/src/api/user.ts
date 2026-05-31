import request from './request'
import type { UserProfile } from '@/types/user'

export const userApi = {
  getMe(): Promise<UserProfile> {
    return request.get('/v1/users/me')
  },
  updateMe(data: { nickname?: string; avatar?: string; phone?: string }): Promise<void> {
    return request.put('/v1/users/me', data)
  },
}
