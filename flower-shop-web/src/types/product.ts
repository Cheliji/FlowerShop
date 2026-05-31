export interface Category {
  id: number
  name: string
  icon?: string
}

export interface Product {
  id: number
  name: string
  subtitle?: string
  flowerLanguage?: string
  price: number
  originalPrice: number
  mainImage: string
  soldCount: number
  categoryId: number
  categoryName: string
}

export interface ProductDetail {
  id: number
  name: string
  subtitle?: string
  flowerLanguage?: string
  description?: string
  suitableFor?: string
  deliveryDesc?: string
  price: number
  originalPrice: number
  stock: number
  soldCount: number
  mainImage: string
  images: string[]
  priceOptions: {
    quantity: number
    label: string
    price: number
  }[]
  categoryId: number
  categoryName: string
}

export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}

export interface Sku {
  id: number
  specName: string
  price: number
  stock: number
}

export interface CartCountResponse {
  cartCount: number
}

export interface CartItem {
  id: number
  productId: number
  skuId: number
  productName: string
  productImage: string
  specName: string
  price: number
  count: number
  stock: number
}

export interface ProductListParams {
  categoryId?: number
  keyword?: string
  minPrice?: number
  maxPrice?: number
  page?: number
  pageSize?: number
}

export interface OrderItem {
  id: number
  flowerId: number
  flowerName: string
  flowerImage?: string
  specName?: string
  quantity: number
  unitPrice: number
  subTotal: number
}

export interface Order {
  id: number
  orderNo: string
  status: number
  statusText: string
  totalAmount: number
  receiverName: string
  receiverPhone: string
  receiverAddress: string
  deliveryDate?: string
  deliveryTimeSlot?: string
  cardMessage?: string
  remark?: string
  createdAt: string
  paidAt?: string
  items: OrderItem[]
}

export interface Address {
  id: number
  receiverName: string
  phone: string
  province?: string
  city?: string
  district?: string
  detailAddress: string
  isDefault: boolean
}
