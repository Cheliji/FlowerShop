import request from './request'
import type { Order } from '@/types/product'

export const orderApi = {
  create(data: {
    addressId: number
    deliveryDate?: string
    deliveryTimeSlot?: string
    cardMessage?: string
    remark?: string
    cartItemIds: number[]
  }): Promise<Order> {
    return request.post('/v1/orders', data)
  },
  getList(params?: { status?: number; page?: number; pageSize?: number }): Promise<Order[]> {
    return request.get('/v1/orders', { params })
  },
  getDetail(id: number): Promise<Order> {
    return request.get(`/v1/orders/${id}`)
  },
  pay(id: number): Promise<void> {
    return request.post(`/v1/orders/${id}/pay`)
  },
  receive(id: number): Promise<void> {
    return request.post(`/v1/orders/${id}/receive`)
  },
  cancel(id: number): Promise<void> {
    return request.post(`/v1/orders/${id}/cancel`)
  },
}
