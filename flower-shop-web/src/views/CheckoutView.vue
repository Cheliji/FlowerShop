<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { ArrowLeft, Location } from '@element-plus/icons-vue'
import { cartApi } from '@/api/cart'
import { addressApi } from '@/api/address'
import { orderApi } from '@/api/order'
import type { CartItem, Address } from '@/types/product'

const router = useRouter()
const route = useRoute()

const cartItems = ref<CartItem[]>([])
const addresses = ref<Address[]>([])
const selectedAddressId = ref<number | undefined>(undefined)
const deliveryDate = ref<string | undefined>(undefined)
const deliveryTimeSlot = ref<string | undefined>(undefined)
const cardMessage = ref('')
const remark = ref('')
const loading = ref(false)
const submitting = ref(false)

const timeSlots = [
  { label: '上午 9:00 - 12:00', value: '09:00-12:00' },
  { label: '下午 14:00 - 18:00', value: '14:00-18:00' },
  { label: '晚上 18:00 - 21:00', value: '18:00-21:00' },
]

const totalAmount = computed(() =>
  cartItems.value.reduce((sum, item) => sum + item.price * item.count, 0)
)

const selectedAddress = computed(() =>
  addresses.value.find((a) => a.id === selectedAddressId.value)
)

const disabledDate = (time: Date) => {
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  const maxDate = new Date(today)
  maxDate.setDate(maxDate.getDate() + 7)
  return time.getTime() < today.getTime() || time.getTime() > maxDate.getTime()
}

onMounted(async () => {
  const idsParam = route.query.ids?.toString()
  const ids = idsParam ? idsParam.split(',').map(Number).filter((n) => !isNaN(n)) : []

  if (ids.length === 0) {
    ElMessage.warning('请先选择商品')
    router.replace('/cart')
    return
  }

  loading.value = true
  try {
    const [cartRes, addrRes] = await Promise.all([
      cartApi.getList(),
      addressApi.getList(),
    ])
    cartItems.value = cartRes.filter((item) => ids.includes(item.id))
    addresses.value = addrRes

    if (cartItems.value.length === 0) {
      ElMessage.warning('选择的商品已失效，请重新选择')
      router.replace('/cart')
      return
    }

    const defaultAddr = addrRes.find((a) => a.isDefault)
    if (defaultAddr) {
      selectedAddressId.value = defaultAddr.id
    } else if (addrRes.length > 0) {
      selectedAddressId.value = addrRes[0]!.id
    }
  } catch {
    // error handled by interceptor
  } finally {
    loading.value = false
  }
})

async function onSubmit() {
  if (!selectedAddressId.value) {
    ElMessage.warning('请选择收货地址')
    return
  }
  if (!deliveryDate.value) {
    ElMessage.warning('请选择配送日期')
    return
  }
  if (!deliveryTimeSlot.value) {
    ElMessage.warning('请选择配送时段')
    return
  }

  submitting.value = true
  try {
    const order = await orderApi.create({
      addressId: selectedAddressId.value,
      deliveryDate: deliveryDate.value,
      deliveryTimeSlot: deliveryTimeSlot.value,
      cardMessage: cardMessage.value || undefined,
      remark: remark.value || undefined,
      cartItemIds: cartItems.value.map((i) => i.id),
    })
    ElMessage.success('订单创建成功')
    router.push(`/order/${order.id}`)
  } catch (err: any) {
    ElMessage.error(err?.message || '创建订单失败')
  } finally {
    submitting.value = false
  }
}

function goBack() {
  router.back()
}
</script>

<template>
  <div class="checkout-page" v-loading="loading">
    <div class="checkout-header">
      <el-button :icon="ArrowLeft" text circle @click="goBack" />
      <h2>确认订单</h2>
    </div>

    <div v-if="cartItems.length > 0" class="checkout-body">
      <!-- 收货地址 -->
      <div class="section address-section">
        <div class="section-title">
          <el-icon><Location /></el-icon>
          <span>收货地址</span>
        </div>
        <div v-if="addresses.length === 0" class="empty-address">
          <p>暂无收货地址，请先去个人中心添加</p>
        </div>
        <el-radio-group v-else v-model="selectedAddressId" class="address-list">
          <el-radio
            v-for="addr in addresses"
            :key="addr.id"
            :label="addr.id"
            class="address-card"
            border
          >
            <div class="address-info">
              <div class="address-top">
                <span class="addr-name">{{ addr.receiverName }}</span>
                <span class="addr-phone">{{ addr.phone }}</span>
                <el-tag v-if="addr.isDefault" type="danger" size="small" effect="plain">默认</el-tag>
              </div>
              <p class="addr-detail">
                {{ addr.province }}{{ addr.city }}{{ addr.district }}{{ addr.detailAddress }}
              </p>
            </div>
          </el-radio>
        </el-radio-group>
      </div>

      <!-- 商品清单 -->
      <div class="section">
        <div class="section-title">商品清单</div>
        <div class="goods-list">
          <div v-for="item in cartItems" :key="item.id" class="goods-item">
            <el-image :src="item.productImage" fit="cover" class="goods-img" />
            <div class="goods-info">
              <h4 class="goods-name">{{ item.productName }}</h4>
              <p class="goods-spec">{{ item.specName }}</p>
            </div>
            <div class="goods-price">
              <span class="price">¥{{ item.price.toFixed(2) }}</span>
              <span class="count">x{{ item.count }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- 配送信息 -->
      <div class="section">
        <div class="section-title">配送信息</div>
        <div class="form-row">
          <label class="form-label">配送日期</label>
          <el-date-picker
            v-model="deliveryDate"
            type="date"
            placeholder="选择配送日期"
            format="YYYY-MM-DD"
            value-format="YYYY-MM-DD"
            :disabled-date="disabledDate"
            style="width: 200px"
          />
        </div>
        <div class="form-row">
          <label class="form-label">配送时段</label>
          <el-radio-group v-model="deliveryTimeSlot">
            <el-radio-button
              v-for="slot in timeSlots"
              :key="slot.value"
              :label="slot.value"
            >
              {{ slot.label }}
            </el-radio-button>
          </el-radio-group>
        </div>
      </div>

      <!-- 贺卡留言 -->
      <div class="section">
        <div class="section-title">贺卡留言</div>
        <el-input
          v-model="cardMessage"
          type="textarea"
          :rows="3"
          maxlength="200"
          show-word-limit
          placeholder="写下你想对收花人说的祝福..."
        />
      </div>

      <!-- 备注 -->
      <div class="section">
        <div class="section-title">订单备注</div>
        <el-input
          v-model="remark"
          type="textarea"
          :rows="2"
          maxlength="200"
          show-word-limit
          placeholder="如有特殊要求请在此说明..."
        />
      </div>

      <!-- 金额汇总 -->
      <div class="section total-section">
        <div class="total-row">
          <span>商品总额</span>
          <span>¥{{ totalAmount.toFixed(2) }}</span>
        </div>
        <div class="total-row">
          <span>配送费</span>
          <span class="free">免运费</span>
        </div>
        <div class="total-row grand-total">
          <span>应付总额</span>
          <span class="total-price">¥{{ totalAmount.toFixed(2) }}</span>
        </div>
      </div>
    </div>

    <!-- 底部提交栏 -->
    <div class="submit-bar">
      <div class="submit-info">
        <span class="submit-label">应付：</span>
        <span class="submit-price">¥{{ totalAmount.toFixed(2) }}</span>
      </div>
      <el-button
        type="danger"
        size="large"
        class="submit-btn"
        :loading="submitting"
        @click="onSubmit"
      >
        提交订单
      </el-button>
    </div>
  </div>
</template>

<style scoped lang="scss">
.checkout-page {
  min-height: 100vh;
  padding: 16px 16px 90px;
  background: #f5f5f5;
}

.checkout-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;

  h2 {
    font-size: 18px;
    font-weight: 600;
    color: #333;
    margin: 0;
  }
}

.checkout-body {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.section {
  background: #fff;
  border-radius: 12px;
  padding: 16px;

  .section-title {
    font-size: 15px;
    font-weight: 600;
    color: #333;
    margin-bottom: 12px;
    display: flex;
    align-items: center;
    gap: 6px;
  }
}

.address-section {
  .address-list {
    display: flex;
    flex-direction: column;
    gap: 10px;
    width: 100%;

    .address-card {
      width: 100%;
      height: auto;
      padding: 12px;
      margin-right: 0;

      :deep(.el-radio__input) {
        align-self: flex-start;
        margin-top: 4px;
      }

      :deep(.el-radio__label) {
        padding-left: 8px;
        white-space: normal;
        width: 100%;
      }

      .address-info {
        .address-top {
          display: flex;
          align-items: center;
          gap: 8px;
          margin-bottom: 4px;

          .addr-name {
            font-weight: 600;
            color: #333;
          }

          .addr-phone {
            color: #666;
            font-size: 13px;
          }
        }

        .addr-detail {
          font-size: 13px;
          color: #999;
          margin: 0;
          line-height: 1.5;
        }
      }
    }
  }

  .empty-address {
    text-align: center;
    color: #999;
    padding: 20px 0;
  }
}

.goods-list {
  display: flex;
  flex-direction: column;
  gap: 12px;

  .goods-item {
    display: flex;
    align-items: center;
    gap: 12px;

    .goods-img {
      width: 80px;
      height: 80px;
      border-radius: 8px;
      flex-shrink: 0;
    }

    .goods-info {
      flex: 1;
      min-width: 0;

      .goods-name {
        font-size: 14px;
        font-weight: 500;
        color: #333;
        margin: 0 0 4px;
        line-height: 1.4;
      }

      .goods-spec {
        font-size: 12px;
        color: #999;
        margin: 0;
      }
    }

    .goods-price {
      text-align: right;
      flex-shrink: 0;

      .price {
        display: block;
        font-size: 14px;
        font-weight: 600;
        color: #f56c6c;
      }

      .count {
        font-size: 12px;
        color: #999;
      }
    }
  }
}

.form-row {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;

  &:last-child {
    margin-bottom: 0;
  }

  .form-label {
    font-size: 14px;
    color: #666;
    min-width: 70px;
    flex-shrink: 0;
  }
}

.total-section {
  .total-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 14px;
    color: #666;
    margin-bottom: 8px;

    &.grand-total {
      margin-top: 12px;
      padding-top: 12px;
      border-top: 1px dashed #e0e0e0;
      font-size: 16px;
      font-weight: 600;
      color: #333;

      .total-price {
        font-size: 22px;
        color: #f56c6c;
      }
    }

    .free {
      color: #67c23a;
    }
  }
}

.submit-bar {
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

  .submit-info {
    .submit-label {
      font-size: 14px;
      color: #666;
    }

    .submit-price {
      font-size: 22px;
      font-weight: 700;
      color: #f56c6c;
    }
  }

  .submit-btn {
    min-width: 140px;
    border-radius: 24px;
    font-size: 16px;
    font-weight: 500;
  }
}
</style>
