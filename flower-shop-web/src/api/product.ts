import request from './request'
import type { Category, Product, ProductDetail, Sku, PagedResult, ProductListParams } from '@/types/product'

export const categoryApi = {
  getList(): Promise<Category[]> {
    return request.get('/v1/categories')
  },
}

export const productApi = {
  getList(params?: ProductListParams): Promise<PagedResult<Product>> {
    return request.get('/v1/products', { params })
  },
  getById(id: number): Promise<ProductDetail> {
    return request.get(`/v1/products/${id}`)
  },
  getSkus(id: number): Promise<Sku[]> {
    return request.get(`/v1/products/${id}/skus`)
  },
}
