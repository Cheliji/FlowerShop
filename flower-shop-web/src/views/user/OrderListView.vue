<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { orderApi } from '@/api/order'
import type { Order } from '@/types/product'

const router = useRouter()

const orders = ref<Order[]>([])
const loading = ref(false)
const activeStatus = ref<number | undefined>(undefined)

const statusTabs = [
  { label: '全部', value: undefined },
  { label: '待付款', value: 0 },
  { label: '待发货', value: 1 },
  { label: '待收货', value: 2 },
  { label: '已完成', value: 3 },
  { label: '已取消', value: 4 },
]

const statusTypeMap: Record<number, string> = {
  0: 'warning',
  1: 'primary',
  2: 'primary',
  3: 'success',
  4: 'info',
}

const statusTextMap: Record<number, string> = {
  0: '待付款',
  1: '待发货',
  2: '待收货',
  3: '已完成',
  4: '已取消',
}

watch(activeStatus, () => {
  fetchOrders()
})

onMounted(() => {
  fetchOrders()
})

async function fetchOrders() {
  loading.value = true
  try {
    orders.value = await orderApi.getList({
      status: activeStatus.value,
      page: 1,
      pageSize: 50,
    })
  } catch {
    // error handled by interceptor
  } finally {
    loading.value = false
  }
}

function goPay(order: Order) {
  router.push(`/order/${order.id}`)
}

async function onReceive(order: Order) {
  try {
    await ElMessageBox.confirm('确认已收到该订单的商品？', '确认收货', {
      confirmButtonText: '确认',
      cancelButtonText: '取消',
      type: 'warning',
    })
    await orderApi.receive(order.id)
    ElMessage.success('确认收货成功')
    fetchOrders()
  } catch (err: any) {
    if (err !== 'cancel') {
      ElMessage.error(err?.message || '操作失败')
    }
  }
}

function goDetail(order: Order) {
  router.push(`/order/${order.id}`)
}
</script>

<template>
  <div class="order-list-page">
    <h2 class="page-title">我的订单</h2>

    <el-radio-group v-model="activeStatus" size="large" class="status-tabs">
      <el-radio-button
        v-for="tab in statusTabs"
        :key="String(tab.value)"
        :label="tab.value"
      >
        {{ tab.label }}
      </el-radio-button>
    </el-radio-group>

    <div v-loading="loading" class="order-list">
      <div v-if="orders.length === 0" class="empty-state">
        <el-empty description="暂无订单" />
      </div>

      <div v-for="order in orders" :key="order.id" class="order-card" @click="goDetail(order)">
        <div class="order-header">
          <span class="order-no">订单号：{{ order.orderNo }}</span>
          <span class="order-time">{{ order.createdAt }}</span>
          <el-tag :type="statusTypeMap[order.status] as any" size="small" effect="dark">
            {{ statusTextMap[order.status] || order.statusText }}
          </el-tag>
        </div>

        <div class="order-items">
          <div v-for="item in order.items.slice(0, 3)" :key="item.id" class="item-row">
            <el-image :src="item.flowerImage" fit="cover" class="item-img" />
            <div class="item-info">
              <span class="item-name">{{ item.flowerName }}</span>
              <span v-if="item.specName" class="item-spec">{{ item.specName }}</span>
            </div>
            <span class="item-count">x{{ item.quantity }}</span>
          </div>
          <div v-if="order.items.length > 3" class="more-items">
            等 {{ order.items.length }} 件商品
          </div>
        </div>

        <div class="order-footer">
          <span class="total-label">实付金额：</span>
          <span class="total-price">¥{{ order.totalAmount.toFixed(2) }}</span>
          <div class="order-actions" @click.stop>
            <el-button
              v-if="order.status === 0"
              type="danger"
              size="small"
              round
              @click="goPay(order)"
            >
              去支付
            </el-button>
            <el-button
              v-if="order.status === 2"
              type="primary"
              size="small"
              round
              @click="onReceive(order)"
            >
              确认收货
            </el-button>
            <el-button size="small" round @click="goDetail(order)">查看详情</el-button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.order-list-page {
  .page-title {
    font-size: 20px;
    font-weight: 600;
    color: #333;
    margin: 0 0 20px;
  }

  .status-tabs {
    margin-bottom: 20px;
  }

  .order-list {
    display: flex;
    flex-direction: column;
    gap: 16px;
  }

  .empty-state {
    padding: 40px 0;
  }

  .order-card {
    background: #fff;
    border-radius: 12px;
    padding: 16px;
    border: 1px solid #ebeef5;
    cursor: pointer;
    transition: box-shadow 0.2s;

    &:hover {
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
    }

    .order-header {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-bottom: 12px;
      padding-bottom: 12px;
      border-bottom: 1px solid #f0f0f0;

      .order-no {
        font-size: 13px;
        color: #666;
      }

      .order-time {
        font-size: 12px;
        color: #999;
        flex: 1;
      }
    }

    .order-items {
      display: flex;
      flex-direction: column;
      gap: 10px;
      margin-bottom: 12px;

      .item-row {
        display: flex;
        align-items: center;
        gap: 10px;

        .item-img {
          width: 60px;
          height: 60px;
          border-radius: 6px;
          flex-shrink: 0;
        }

        .item-info {
          flex: 1;
          min-width: 0;
          display: flex;
          flex-direction: column;
          gap: 2px;

          .item-name {
            font-size: 14px;
            color: #333;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
          }

          .item-spec {
            font-size: 12px;
            color: #999;
          }
        }

        .item-count {
          font-size: 13px;
          color: #666;
          flex-shrink: 0;
        }
      }

      .more-items {
        font-size: 12px;
        color: #999;
      }
    }

    .order-footer {
      display: flex;
      align-items: center;
      justify-content: flex-end;
      gap: 8px;
      padding-top: 12px;
      border-top: 1px solid #f0f0f0;

      .total-label {
        font-size: 13px;
        color: #666;
      }

      .total-price {
        font-size: 18px;
        font-weight: 700;
        color: #f56c6c;
      }

      .order-actions {
        display: flex;
        gap: 8px;
        margin-left: 12px;
      }
    }
  }
}

@media (max-width: 768px) {
  .order-list-page {
    .order-card {
      .order-header {
        flex-wrap: wrap;
      }

      .order-footer {
        flex-wrap: wrap;
        justify-content: space-between;

        .order-actions {
          width: 100%;
          justify-content: flex-end;
          margin-left: 0;
          margin-top: 8px;
        }
      }
    }
  }
}
</style>
