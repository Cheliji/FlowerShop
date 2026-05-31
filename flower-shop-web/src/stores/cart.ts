import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { ElMessage } from 'element-plus'
import { cartApi } from '@/api/cart'
import type { CartItem } from '@/types/product'

export const useCartStore = defineStore('cart', () => {
  const items = ref<CartItem[]>([])
  const selectedIds = ref<Set<number>>(new Set())
  const isLoading = ref(false)

  const totalCount = computed(() =>
    items.value.reduce((sum, item) => sum + item.count, 0)
  )

  const selectedItems = computed(() =>
    items.value.filter((item) => selectedIds.value.has(item.id))
  )

  const selectedTotalAmount = computed(() =>
    selectedItems.value.reduce((sum, item) => sum + item.price * item.count, 0)
  )

  const selectedTotalCount = computed(() =>
    selectedItems.value.reduce((sum, item) => sum + item.count, 0)
  )

  const isAllSelected = computed(() =>
    items.value.length > 0 && items.value.every((item) => selectedIds.value.has(item.id))
  )

  const loadCart = async () => {
    isLoading.value = true
    try {
      const res = await cartApi.getList()
      items.value = res
      // 保留仍然存在的选中项
      const validIds = new Set(res.map((i) => i.id))
      selectedIds.value = new Set([...selectedIds.value].filter((id) => validIds.has(id)))
    } catch {
      items.value = []
    } finally {
      isLoading.value = false
    }
  }

  const addToCart = async (productId: number, skuId: number, count = 1) => {
    try {
      const res = await cartApi.add({ productId, skuId, count })
      ElMessage.success('已加入购物车')
      await loadCart()
      return res
    } catch (err: any) {
      ElMessage.error(err?.message || '加入购物车失败')
      throw err
    }
  }

  let debounceTimer: ReturnType<typeof setTimeout> | null = null

  const updateCount = async (id: number, count: number) => {
    const item = items.value.find((i) => i.id === id)
    if (!item) return
    item.count = count

    if (debounceTimer) clearTimeout(debounceTimer)
    debounceTimer = setTimeout(async () => {
      try {
        await cartApi.updateCount(id, count)
      } catch (err: any) {
        ElMessage.error(err?.message || '更新数量失败')
        await loadCart()
      }
    }, 500)
  }

  const removeItem = async (id: number) => {
    try {
      await cartApi.remove(id)
      selectedIds.value.delete(id)
      await loadCart()
    } catch (err: any) {
      ElMessage.error(err?.message || '删除失败')
    }
  }

  const clearCart = async () => {
    try {
      await cartApi.clear()
      selectedIds.value.clear()
      await loadCart()
    } catch (err: any) {
      ElMessage.error(err?.message || '清空失败')
    }
  }

  const toggleSelect = (id: number) => {
    if (selectedIds.value.has(id)) {
      selectedIds.value.delete(id)
    } else {
      selectedIds.value.add(id)
    }
  }

  const selectAll = (checked: boolean) => {
    if (checked) {
      selectedIds.value = new Set(items.value.map((i) => i.id))
    } else {
      selectedIds.value.clear()
    }
  }

  return {
    items,
    selectedIds,
    isLoading,
    totalCount,
    selectedItems,
    selectedTotalAmount,
    selectedTotalCount,
    isAllSelected,
    loadCart,
    addToCart,
    updateCount,
    removeItem,
    clearCart,
    toggleSelect,
    selectAll,
  }
})
