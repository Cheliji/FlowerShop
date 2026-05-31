import request from './request'
import type { Address } from '@/types/product'

export interface AddressForm {
  receiverName: string
  phone: string
  province?: string
  city?: string
  district?: string
  detailAddress: string
  isDefault: boolean
}

export const addressApi = {
  getList(): Promise<Address[]> {
    return request.get('/v1/useraddress')
  },
  create(data: AddressForm): Promise<void> {
    return request.post('/v1/useraddress', data)
  },
  update(id: number, data: AddressForm): Promise<void> {
    return request.put(`/v1/useraddress/${id}`, data)
  },
  remove(id: number): Promise<void> {
    return request.delete(`/v1/useraddress/${id}`)
  },
}
