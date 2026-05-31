<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'
import {
  User,
  Document,
  Location,
  ShoppingCart,
  ArrowLeft,
} from '@element-plus/icons-vue'

const route = useRoute()
const router = useRouter()
const userStore = useUserStore()

const activeMenu = computed(() => route.path)

onMounted(() => {
  if (!userStore.isLoggedIn) {
    router.push('/login')
    return
  }
  userStore.fetchUser()
})

const menus = [
  { path: '/user/profile', label: '个人资料', icon: User },
  { path: '/user/orders', label: '我的订单', icon: Document },
  { path: '/user/addresses', label: '收货地址', icon: Location },
  { path: '/cart', label: '我的购物车', icon: ShoppingCart },
]
</script>

<template>
  <div class="user-center-page">
    <div class="user-center-container">
      <!-- 侧边栏 -->
      <div class="sidebar">
        <div class="user-info">
          <el-avatar :size="64" :src="userStore.user?.avatar || '/images/banners/avatar.png'">
            {{ userStore.user?.nickname?.[0] || userStore.user?.username?.[0] || 'U' }}
          </el-avatar>
          <div class="user-name">{{ userStore.user?.nickname || userStore.user?.username || '用户' }}</div>
        </div>
        <el-menu
          :default-active="activeMenu"
          class="user-menu"
          router
        >
          <el-menu-item v-for="menu in menus" :key="menu.path" :index="menu.path">
            <el-icon><component :is="menu.icon" /></el-icon>
            <span>{{ menu.label }}</span>
          </el-menu-item>
        </el-menu>
      </div>

      <!-- 内容区 -->
      <div class="content">
        <div class="content-header">
          <el-button text :icon="ArrowLeft" @click="router.push('/')">返回首页</el-button>
        </div>
        <router-view />
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.user-center-page {
  min-height: 100vh;
  background: #f5f5f5;
  padding: 20px;
}

.user-center-container {
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  gap: 16px;
  background: #fff;
  border-radius: 12px;
  overflow: hidden;
  min-height: calc(100vh - 40px);
}

.sidebar {
  width: 220px;
  flex-shrink: 0;
  background: #fff;
  border-right: 1px solid #ebeef5;

  .user-info {
    padding: 24px 16px;
    text-align: center;
    border-bottom: 1px solid #ebeef5;

    .user-name {
      margin-top: 10px;
      font-size: 16px;
      font-weight: 500;
      color: #333;
    }
  }

  .user-menu {
    border-right: none;
  }
}

.content {
  flex: 1;
  padding: 24px;
  min-width: 0;
}

.content-header {
  margin-bottom: 16px;
}

@media (max-width: 768px) {
  .user-center-page {
    padding: 0;
  }

  .user-center-container {
    flex-direction: column;
    border-radius: 0;
  }

  .sidebar {
    width: 100%;
    border-right: none;
    border-bottom: 1px solid #ebeef5;

    .user-info {
      display: none;
    }

    .user-menu {
      display: flex;
      overflow-x: auto;

      :deep(.el-menu-item) {
        flex-shrink: 0;
      }
    }
  }

  .content {
    padding: 16px;
  }
}
</style>
