import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '@/api/auth'
import type { UserProfile } from '@/types/user'

export const useUserStore = defineStore('user', () => {
  const token = ref<string>(localStorage.getItem('token') || '')
  const user = ref<UserProfile | null>(null)

  const isLoggedIn = computed(() => !!token.value)

  const setToken = (newToken: string) => {
    token.value = newToken
    localStorage.setItem('token', newToken)
  }

  const clearToken = () => {
    token.value = ''
    user.value = null
    localStorage.removeItem('token')
  }

  const fetchUser = async () => {
    if (!token.value) return
    try {
      const res = await authApi.getProfile()
      user.value = res
    } catch {
      clearToken()
    }
  }

  const logout = () => {
    clearToken()
    window.location.href = '/login'
  }

  return {
    token,
    user,
    isLoggedIn,
    setToken,
    clearToken,
    fetchUser,
    logout,
  }
})
