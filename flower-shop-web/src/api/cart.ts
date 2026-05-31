import request from './request'
import type { CartCountResponse, CartItem } from '@/types/product'

export const cartApi = {
  add(data: { productId: number; skuId: number; count: number }): Promise<CartCountResponse> {
    return request.post('/v1/cart', data)
  },
  getList(): Promise<CartItem[]> {
    return request.get('/v1/cart')
  },
  updateCount(id: number, count: number): Promise<void> {
    return request.put(`/v1/cart/${id}`, { count })
  },
  remove(id: number): Promise<void> {
    return request.delete(`/v1/cart/${id}`)
  },
  clear(): Promise<void> {
    return request.delete('/v1/cart')
  },
}
