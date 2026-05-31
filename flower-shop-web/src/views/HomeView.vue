<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Search, ShoppingCart, User } from '@element-plus/icons-vue'
import { categoryApi, productApi } from '@/api/product'
import { useCartStore } from '@/stores/cart'
import type { Category, Product, PagedResult } from '@/types/product'

const router = useRouter()
const cartStore = useCartStore()

const categories = ref<Category[]>([])
const activeCategoryId = ref<number | undefined>(undefined)

const searchKeyword = ref('')
const priceRange = ref<[number, number]>([0, 1000])

const products = ref<Product[]>([])
const loading = ref(false)
const pagination = reactive({
  page: 1,
  pageSize: 12,
  total: 0,
  totalPages: 0,
})

const isLoggedIn = ref(!!localStorage.getItem('token'))

const banners = ref([
  { image: '/images/banners/banner1.jpg' },
  { image: '/images/banners/banner2.jpg' },
])

async function loadCategories() {
  try {
    categories.value = await categoryApi.getList()
  } catch {
    // ignore
  }
}

async function loadProducts() {
  loading.value = true
  try {
    const res: PagedResult<Product> = await productApi.getList({
      categoryId: activeCategoryId.value,
      keyword: searchKeyword.value || undefined,
      minPrice: priceRange.value[0] > 0 ? priceRange.value[0] : undefined,
      maxPrice: priceRange.value[1] < 1000 ? priceRange.value[1] : undefined,
      page: pagination.page,
      pageSize: pagination.pageSize,
    })
    products.value = res.items
    pagination.total = res.total
    pagination.totalPages = res.totalPages
  } catch {
    products.value = []
  } finally {
    loading.value = false
  }
}

function onCategoryClick(id?: number) {
  activeCategoryId.value = id
  pagination.page = 1
  loadProducts()
}

function onSearch() {
  pagination.page = 1
  loadProducts()
}

function onPriceChange() {
  pagination.page = 1
  loadProducts()
}

function onPageChange(page: number) {
  pagination.page = page
  loadProducts()
}

function goLogin() {
  router.push('/login')
}

function goCart() {
  if (!isLoggedIn.value) {
    ElMessage.warning('请先登录')
    router.push('/login')
    return
  }
  router.push('/cart')
}

function goToDetail(id: number) {
  router.push(`/product/${id}`)
}

onMounted(() => {
  loadCategories()
  loadProducts()
  if (isLoggedIn.value) {
    cartStore.loadCart()
  }
})
</script>

<template>
  <div class="home-page">
    <!-- 顶部导航 -->
    <header class="home-header">
      <div class="header-inner">
        <div class="logo" @click="router.push('/')">
          <span class="logo-icon">🌸</span>
          <span class="logo-text">花语</span>
        </div>

        <nav class="header-nav">
          <a class="nav-item active">首页</a>
          <a class="nav-item">鲜花分类</a>
          <a class="nav-item">花礼指南</a>
          <a class="nav-item">企业团购</a>
        </nav>

        <div class="header-search">
          <el-input
            v-model="searchKeyword"
            placeholder="搜索鲜花"
            clearable
            @keyup.enter="onSearch"
          >
            <template #append>
              <el-button :icon="Search" @click="onSearch" />
            </template>
          </el-input>
        </div>

        <div class="header-actions">
          <el-badge :value="cartStore.totalCount" :hidden="cartStore.totalCount === 0" class="cart-badge">
            <el-button text :icon="ShoppingCart" @click="goCart">购物车</el-button>
          </el-badge>
          <el-button v-if="!isLoggedIn" type="primary" round @click="goLogin">登录</el-button>
          <el-button v-else text :icon="User" @click="router.push('/user')">我的</el-button>
        </div>
      </div>
    </header>

    <!-- 轮播图 -->
    <section class="banner-section">
      <el-carousel height="360px" arrow="always">
        <el-carousel-item v-for="(banner, index) in banners" :key="index">
          <div class="banner-slide">
            <img :src="banner.image" alt="" class="banner-img" />
          </div>
        </el-carousel-item>
      </el-carousel>
    </section>

    <!-- 主体内容 -->
    <main class="main-content">
      <!-- 分类 + 筛选 -->
      <div class="filter-bar">
        <div class="category-tags">
          <span
            class="category-tag"
            :class="{ active: activeCategoryId === undefined }"
            @click="onCategoryClick(undefined)"
          >
            全部
          </span>
          <span
            v-for="cat in categories"
            :key="cat.id"
            class="category-tag"
            :class="{ active: activeCategoryId === cat.id }"
            @click="onCategoryClick(cat.id)"
          >
            {{ cat.icon }} {{ cat.name }}
          </span>
        </div>

        <div class="price-filter">
          <span class="price-label">价格区间</span>
          <el-slider
            v-model="priceRange"
            range
            :max="1000"
            :step="10"
            style="width: 200px"
            @change="onPriceChange"
          />
          <span class="price-value">¥{{ priceRange[0] }} - ¥{{ priceRange[1] }}</span>
        </div>
      </div>

      <!-- 商品网格 -->
      <div v-loading="loading" class="product-section">
        <el-empty v-if="!loading && products.length === 0" description="暂无商品" />

        <el-row v-else :gutter="16">
          <el-col
            v-for="product in products"
            :key="product.id"
            :xs="12"
            :sm="8"
            :md="6"
            :lg="6"
            :xl="4"
          >
            <div class="product-card" @click="goToDetail(product.id)">
              <div class="product-image">
                <el-image :src="product.mainImage" fit="cover" lazy>
                  <template #error>
                    <div class="image-error">暂无图片</div>
                  </template>
                </el-image>
              </div>
              <div class="product-info">
                <h3 class="product-name">{{ product.name }}</h3>
                <p class="product-lang">{{ product.flowerLanguage || '' }}</p>
                <div class="product-footer">
                  <div class="product-price">
                    <span class="price-current">¥{{ product.price }}</span>
                    <span v-if="product.originalPrice > product.price" class="price-original">
                      ¥{{ product.originalPrice }}
                    </span>
                  </div>
                  <span class="product-sold">已售 {{ product.soldCount }}</span>
                </div>
              </div>
            </div>
          </el-col>
        </el-row>

        <div v-if="pagination.totalPages > 1" class="pagination-wrap">
          <el-pagination
            v-model:current-page="pagination.page"
            :page-size="pagination.pageSize"
            :total="pagination.total"
            layout="prev, pager, next"
            @current-change="onPageChange"
          />
        </div>
      </div>
    </main>
  </div>
</template>

<style scoped lang="scss">
.home-page {
  min-height: 100vh;
  background: #f5f5f5;
}

.home-header {
  background: #fff;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  position: sticky;
  top: 0;
  z-index: 100;

  .header-inner {
    max-width: 1400px;
    margin: 0 auto;
    padding: 0 16px;
    height: 64px;
    display: flex;
    align-items: center;
    gap: 24px;
  }

  .logo {
    display: flex;
    align-items: center;
    gap: 8px;
    cursor: pointer;
    flex-shrink: 0;

    .logo-icon {
      font-size: 28px;
    }

    .logo-text {
      font-size: 22px;
      font-weight: 700;
      color: #e91e63;
      letter-spacing: 2px;
    }
  }

  .header-nav {
    display: flex;
    gap: 24px;
    flex: 1;

    .nav-item {
      font-size: 14px;
      color: #666;
      cursor: pointer;
      transition: color 0.2s;

      &:hover,
      &.active {
        color: #e91e63;
      }
    }
  }

  .header-search {
    width: 260px;
    flex-shrink: 0;
  }

  .header-actions {
    display: flex;
    align-items: center;
    gap: 8px;
    flex-shrink: 0;
  }
}

.banner-section {
    .banner-slide {
      position: relative;
      height: 100%;
      width: 100%;
      overflow: hidden;

      .banner-img {
        width: 100%;
        height: 100%;
        object-fit: cover;
        display: block;
      }
    }
  }

.main-content {
  max-width: 1400px;
  margin: 0 auto;
  padding: 16px;
}

.filter-bar {
  background: #fff;
  border-radius: 8px;
  padding: 16px;
  margin-bottom: 16px;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 16px;

  .category-tags {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
    flex: 1;

    .category-tag {
      padding: 6px 14px;
      border-radius: 16px;
      font-size: 13px;
      color: #666;
      background: #f5f5f5;
      cursor: pointer;
      transition: all 0.2s;
      user-select: none;

      &:hover {
        background: #fce4ec;
        color: #e91e63;
      }

      &.active {
        background: #e91e63;
        color: #fff;
      }
    }
  }

  .price-filter {
    display: flex;
    align-items: center;
    gap: 12px;
    flex-shrink: 0;

    .price-label {
      font-size: 13px;
      color: #666;
    }

    .price-value {
      font-size: 13px;
      color: #e91e63;
      min-width: 100px;
    }
  }
}

.product-section {
  background: #fff;
  border-radius: 8px;
  padding: 16px;
  min-height: 400px;
}

.product-card {
  border-radius: 8px;
  overflow: hidden;
  background: #fff;
  transition: transform 0.2s, box-shadow 0.2s;
  cursor: pointer;
  margin-bottom: 16px;
  border: 1px solid #f0f0f0;

  &:hover {
    transform: translateY(-4px);
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.08);
  }

  .product-image {
    width: 100%;
    aspect-ratio: 1 / 1;
    overflow: hidden;

    .el-image {
      width: 100%;
      height: 100%;
    }

    .image-error {
      width: 100%;
      height: 100%;
      display: flex;
      align-items: center;
      justify-content: center;
      background: #f5f5f5;
      color: #999;
      font-size: 13px;
    }
  }

  .product-info {
    padding: 12px;

    .product-name {
      font-size: 14px;
      font-weight: 600;
      color: #333;
      margin-bottom: 4px;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .product-lang {
      font-size: 12px;
      color: #999;
      margin-bottom: 8px;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .product-footer {
      display: flex;
      align-items: center;
      justify-content: space-between;

      .product-price {
        display: flex;
        align-items: baseline;
        gap: 6px;

        .price-current {
          font-size: 16px;
          font-weight: 700;
          color: #e91e63;
        }

        .price-original {
          font-size: 12px;
          color: #bbb;
          text-decoration: line-through;
        }
      }

      .product-sold {
        font-size: 12px;
        color: #999;
      }
    }
  }
}

.pagination-wrap {
  display: flex;
  justify-content: center;
  padding: 24px 0 8px;
}

@media (max-width: 768px) {
  .home-header {
    .header-inner {
      flex-wrap: wrap;
      height: auto;
      padding: 8px 12px;
      gap: 8px;
    }

    .header-nav {
      display: none;
    }

    .header-search {
      width: 100%;
      order: 3;
    }
  }

  .banner-section {
    .banner-slide {
      .banner-img {
        object-fit: cover;
      }
    }
  }
}
</style>
