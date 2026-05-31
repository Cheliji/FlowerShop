<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Delete, ShoppingCart, Goods, ArrowLeft } from '@element-plus/icons-vue'
import { useCartStore } from '@/stores/cart'
import { useUserStore } from '@/stores/user'

const router = useRouter()
const cartStore = useCartStore()
const userStore = useUserStore()

const isEmpty = computed(() => cartStore.items.length === 0)

onMounted(() => {
  if (!userStore.isLoggedIn) {
    router.push('/login')
    return
  }
  cartStore.loadCart()
})

function onDelete(id: number) {
  ElMessageBox.confirm('确定要删除该商品吗？', '提示', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning',
  })
    .then(() => {
      cartStore.removeItem(id)
    })
    .catch(() => {})
}

function onClear() {
  if (cartStore.items.length === 0) return
  ElMessageBox.confirm('确定要清空购物车吗？', '提示', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning',
  })
    .then(() => {
      cartStore.clearCart()
    })
    .catch(() => {})
}

function onCheckout() {
  if (cartStore.selectedItems.length === 0) {
    ElMessage.warning('请选择要结算的商品')
    return
  }
  const ids = cartStore.selectedItems.map((i) => i.id).join(',')
  router.push({ path: '/checkout', query: { ids } })
}

function goShopping() {
  router.push('/')
}
</script>

<template>
  <div class="cart-page" v-loading="cartStore.isLoading">
    <div class="cart-header">
      <div class="header-left">
        <el-button text :icon="ArrowLeft" @click="router.back()">返回</el-button>
        <h2>购物车</h2>
      </div>
      <el-button v-if="!isEmpty" type="danger" text :icon="Delete" @click="onClear">
        清空
      </el-button>
    </div>

    <!-- 空购物车 -->
    <div v-if="isEmpty" class="empty-cart">
      <el-icon class="empty-icon"><ShoppingCart /></el-icon>
      <p class="empty-text">购物车还是空的</p>
      <el-button type="primary" :icon="Goods" @click="goShopping">去逛逛</el-button>
    </div>

    <!-- 购物车列表 -->
    <div v-else class="cart-list">
      <div
        v-for="item in cartStore.items"
        :key="item.id"
        class="cart-item"
        :class="{ selected: cartStore.selectedIds.has(item.id) }"
      >
        <div class="item-select">
          <el-checkbox
            :model-value="cartStore.selectedIds.has(item.id)"
            @change="cartStore.toggleSelect(item.id)"
            size="large"
          />
        </div>

        <div class="item-image" @click="router.push(`/product/${item.productId}`)">
          <el-image :src="item.productImage" fit="cover" />
        </div>

        <div class="item-info">
          <div class="info-top">
            <h3 class="product-name" @click="router.push(`/product/${item.productId}`)">
              {{ item.productName }}
            </h3>
            <el-button
              type="danger"
              link
              :icon="Delete"
              class="delete-btn"
              @click="onDelete(item.id)"
            />
          </div>

          <p class="spec-name">{{ item.specName }}</p>

          <div class="info-bottom">
            <span class="unit-price">¥{{ item.price.toFixed(2) }}</span>
            <el-input-number
              v-model="item.count"
              :min="1"
              :max="item.stock"
              size="small"
              @change="(val: number) => cartStore.updateCount(item.id, val)"
            />
          </div>
        </div>

        <div class="item-subtotal">
          <span class="subtotal-label">小计</span>
          <span class="subtotal-price">¥{{ (item.price * item.count).toFixed(2) }}</span>
        </div>
      </div>
    </div>

    <!-- 底部结算栏 -->
    <div v-if="!isEmpty" class="checkout-bar">
      <div class="bar-left">
        <el-checkbox
          :model-value="cartStore.isAllSelected"
          @change="(val: boolean) => cartStore.selectAll(val)"
          size="large"
        >
          全选
        </el-checkbox>
      </div>
      <div class="bar-right">
        <div class="total-info">
          <span class="total-label">合计：</span>
          <span class="total-price">¥{{ cartStore.selectedTotalAmount.toFixed(2) }}</span>
          <span class="total-count">({{ cartStore.selectedTotalCount }}件)</span>
        </div>
        <el-button type="danger" size="large" class="checkout-btn" @click="onCheckout">
          结算
        </el-button>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.cart-page {
  min-height: 100vh;
  padding: 16px 16px 100px;
  background: #f5f5f5;
}

.cart-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;

  .header-left {
    display: flex;
    align-items: center;
    gap: 8px;
  }

  h2 {
    font-size: 20px;
    font-weight: 600;
    color: #333;
    margin: 0;
  }
}

.empty-cart {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 80px 20px;
  background: #fff;
  border-radius: 12px;

  .empty-icon {
    font-size: 80px;
    color: #ddd;
    margin-bottom: 16px;
  }

  .empty-text {
    font-size: 16px;
    color: #999;
    margin: 0 0 24px;
  }
}

.cart-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.cart-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px;
  background: #fff;
  border-radius: 12px;
  transition: box-shadow 0.2s;

  &.selected {
    box-shadow: 0 0 0 2px #f56c6c inset;
  }

  .item-select {
    flex-shrink: 0;
  }

  .item-image {
    flex-shrink: 0;
    width: 100px;
    height: 100px;
    border-radius: 8px;
    overflow: hidden;
    cursor: pointer;

    .el-image {
      width: 100%;
      height: 100%;
    }
  }

  .item-info {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    gap: 6px;

    .info-top {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 8px;

      .product-name {
        font-size: 15px;
        font-weight: 500;
        color: #333;
        margin: 0;
        line-height: 1.4;
        cursor: pointer;
        flex: 1;

        &:hover {
          color: #f56c6c;
        }
      }

      .delete-btn {
        flex-shrink: 0;
        padding: 4px;
      }
    }

    .spec-name {
      font-size: 13px;
      color: #999;
      margin: 0;
    }

    .info-bottom {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-top: 4px;

      .unit-price {
        font-size: 16px;
        font-weight: 600;
        color: #f56c6c;
      }
    }
  }

  .item-subtotal {
    flex-shrink: 0;
    text-align: right;
    min-width: 80px;

    .subtotal-label {
      font-size: 12px;
      color: #999;
      display: block;
      margin-bottom: 4px;
    }

    .subtotal-price {
      font-size: 16px;
      font-weight: 600;
      color: #f56c6c;
    }
  }
}

.checkout-bar {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  z-index: 100;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 16px;
  background: #fff;
  box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.06);

  .bar-left {
    :deep(.el-checkbox__label) {
      font-size: 14px;
      color: #666;
    }
  }

  .bar-right {
    display: flex;
    align-items: center;
    gap: 16px;

    .total-info {
      display: flex;
      align-items: baseline;
      gap: 4px;

      .total-label {
        font-size: 14px;
        color: #666;
      }

      .total-price {
        font-size: 20px;
        font-weight: 700;
        color: #f56c6c;
      }

      .total-count {
        font-size: 13px;
        color: #999;
      }
    }

    .checkout-btn {
      min-width: 120px;
      border-radius: 24px;
      font-size: 16px;
      font-weight: 500;
    }
  }
}

@media (max-width: 768px) {
  .cart-item {
    flex-wrap: wrap;
    gap: 10px;

    .item-image {
      width: 80px;
      height: 80px;
    }

    .item-subtotal {
      width: 100%;
      text-align: left;
      padding-left: 28px;
      display: flex;
      align-items: center;
      gap: 8px;

      .subtotal-label {
        display: inline;
        margin-bottom: 0;
      }
    }
  }

  .checkout-bar {
    .bar-right {
      gap: 10px;

      .checkout-btn {
        min-width: 100px;
        font-size: 14px;
      }
    }
  }
}
</style>
