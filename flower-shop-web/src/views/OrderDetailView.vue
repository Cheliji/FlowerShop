<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { ArrowLeft } from '@element-plus/icons-vue'
import { orderApi } from '@/api/order'
import type { Order } from '@/types/product'

const router = useRouter()
const route = useRoute()
const orderId = Number(route.params.id)

const order = ref<Order | null>(null)
const loading = ref(false)
const paying = ref(false)
const cancelling = ref(false)

const isPending = computed(() => order.value?.status === 0)
const statusType = computed(() => {
  switch (order.value?.status) {
    case 0:
      return 'warning'
    case 1:
      return 'success'
    case 4:
      return 'info'
    default:
      return ''
  }
})

onMounted(() => {
  if (!orderId || isNaN(orderId)) {
    ElMessage.error('订单参数错误')
    router.replace('/')
    return
  }
  fetchOrder()
})

async function fetchOrder() {
  loading.value = true
  try {
    order.value = await orderApi.getDetail(orderId)
  } catch {
    // handled by interceptor
  } finally {
    loading.value = false
  }
}

async function onPay() {
  if (!order.value) return
  try {
    await ElMessageBox.confirm('确定模拟支付该订单吗？', '模拟支付', {
      confirmButtonText: '确认支付',
      cancelButtonText: '取消',
      type: 'warning',
    })
    paying.value = true
    await orderApi.pay(order.value.id)
    ElMessage.success('支付成功')
    await fetchOrder()
  } catch (err: any) {
    if (err !== 'cancel') {
      ElMessage.error(err?.message || '支付失败')
    }
  } finally {
    paying.value = false
  }
}

async function onCancel() {
  if (!order.value) return
  try {
    await ElMessageBox.confirm('确定取消该订单吗？', '取消订单', {
      confirmButtonText: '确定取消',
      cancelButtonText: '再想想',
      type: 'warning',
    })
    cancelling.value = true
    await orderApi.cancel(order.value.id)
    ElMessage.success('订单已取消')
    await fetchOrder()
  } catch (err: any) {
    if (err !== 'cancel') {
      ElMessage.error(err?.message || '取消失败')
    }
  } finally {
    cancelling.value = false
  }
}

function goBack() {
  router.back()
}
</script>

<template>
  <div class="order-detail-page" v-loading="loading">
    <div class="detail-header">
      <el-button :icon="ArrowLeft" text circle @click="goBack" />
      <h2>订单详情</h2>
    </div>

    <div v-if="order" class="detail-body">
      <!-- 状态卡片 -->
      <div class="status-card">
        <div class="status-main">
          <el-tag :type="statusType" size="large" effect="dark">
            {{ order.statusText }}
          </el-tag>
          <span class="order-no">订单号：{{ order.orderNo }}</span>
        </div>
        <div v-if="isPending" class="status-actions">
          <el-button type="danger" size="large" round :loading="paying" @click="onPay">
            模拟支付
          </el-button>
          <el-button size="large" round :loading="cancelling" @click="onCancel">
            取消订单
          </el-button>
        </div>
      </div>

      <!-- 收货信息 -->
      <div class="info-card">
        <div class="card-title">收货信息</div>
        <div class="info-row">
          <span class="info-label">收货人</span>
          <span class="info-value">{{ order.receiverName }} {{ order.receiverPhone }}</span>
        </div>
        <div class="info-row">
          <span class="info-label">收货地址</span>
          <span class="info-value">{{ order.receiverAddress }}</span>
        </div>
        <div v-if="order.deliveryDate" class="info-row">
          <span class="info-label">配送日期</span>
          <span class="info-value">{{ order.deliveryDate }} {{ order.deliveryTimeSlot }}</span>
        </div>
        <div v-if="order.cardMessage" class="info-row">
          <span class="info-label">贺卡留言</span>
          <span class="info-value message-value">{{ order.cardMessage }}</span>
        </div>
        <div v-if="order.remark" class="info-row">
          <span class="info-label">订单备注</span>
          <span class="info-value">{{ order.remark }}</span>
        </div>
      </div>

      <!-- 商品清单 -->
      <div class="info-card">
        <div class="card-title">商品清单</div>
        <div class="goods-list">
          <div v-for="item in order.items" :key="item.id" class="goods-item">
            <el-image :src="item.flowerImage" fit="cover" class="goods-img" />
            <div class="goods-info">
              <h4 class="goods-name">{{ item.flowerName }}</h4>
              <p v-if="item.specName" class="goods-spec">{{ item.specName }}</p>
            </div>
            <div class="goods-price">
              <span class="price">¥{{ item.unitPrice.toFixed(2) }}</span>
              <span class="count">x{{ item.quantity }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- 金额汇总 -->
      <div class="info-card total-card">
        <div class="total-row">
          <span>商品总额</span>
          <span>¥{{ order.totalAmount.toFixed(2) }}</span>
        </div>
        <div class="total-row">
          <span>配送费</span>
          <span class="free">免运费</span>
        </div>
        <div class="total-row grand-total">
          <span>应付总额</span>
          <span class="total-price">¥{{ order.totalAmount.toFixed(2) }}</span>
        </div>
        <div v-if="order.paidAt" class="total-row">
          <span>支付时间</span>
          <span>{{ order.paidAt }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.order-detail-page {
  min-height: 100vh;
  padding: 16px 16px 24px;
  background: #f5f5f5;
}

.detail-header {
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

.detail-body {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.status-card {
  background: #fff;
  border-radius: 12px;
  padding: 20px;
  text-align: center;

  .status-main {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 10px;
    margin-bottom: 16px;

    .order-no {
      font-size: 13px;
      color: #999;
    }
  }

  .status-actions {
    display: flex;
    justify-content: center;
    gap: 12px;
  }
}

.info-card {
  background: #fff;
  border-radius: 12px;
  padding: 16px;

  .card-title {
    font-size: 15px;
    font-weight: 600;
    color: #333;
    margin-bottom: 12px;
  }

  .info-row {
    display: flex;
    align-items: flex-start;
    gap: 12px;
    margin-bottom: 8px;
    font-size: 14px;

    &:last-child {
      margin-bottom: 0;
    }

    .info-label {
      color: #999;
      min-width: 70px;
      flex-shrink: 0;
    }

    .info-value {
      color: #333;
      flex: 1;
      word-break: break-all;

      &.message-value {
        color: #f56c6c;
        font-style: italic;
      }
    }
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

.total-card {
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
</style>
