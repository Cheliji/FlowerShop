<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElImageViewer } from 'element-plus'
import { ArrowLeft, ArrowRight, ShoppingCart } from '@element-plus/icons-vue'
import { productApi } from '@/api/product'
import { cartApi } from '@/api/cart'
import { useCartStore } from '@/stores/cart'
import type { ProductDetail, Sku } from '@/types/product'

const cartStore = useCartStore()

const route = useRoute()
const router = useRouter()
const productId = Number(route.params.id)

const product = ref<ProductDetail | null>(null)
const skus = ref<Sku[]>([])
const selectedSku = ref<Sku | null>(null)
const loading = ref(false)
const showViewer = ref(false)
const viewerIndex = ref(0)
const currentImageIndex = ref(0)

const currentPrice = computed(() => {
  return selectedSku.value ? selectedSku.value.price : product.value?.price ?? 0
})

const currentStock = computed(() => {
  return selectedSku.value ? selectedSku.value.stock : product.value?.stock ?? 0
})

const isLoggedIn = computed(() => !!localStorage.getItem('token'))

onMounted(() => {
  if (!productId || isNaN(productId)) {
    ElMessage.error('商品参数错误')
    router.replace('/')
    return
  }
  fetchProduct()
  fetchSkus()
  if (localStorage.getItem('token')) {
    cartStore.loadCart()
  }
})

async function fetchProduct() {
  loading.value = true
  try {
    product.value = await productApi.getById(productId)
  } catch {
    // error handled by interceptor
  } finally {
    loading.value = false
  }
}

async function fetchSkus() {
  try {
    skus.value = await productApi.getSkus(productId)
    if (skus.value.length > 0) {
      selectedSku.value = skus.value[0] ?? null
    }
  } catch {
    // error handled by interceptor
  }
}

function onSkuClick(sku: Sku) {
  selectedSku.value = sku
}

function onAddToCart() {
  if (!isLoggedIn.value) {
    router.push('/login')
    return
  }
  if (!selectedSku.value) {
    ElMessage.warning('请选择规格')
    return
  }
  cartApi
    .add({
      productId: productId,
      skuId: selectedSku.value.id,
      count: 1,
    })
    .then((res) => {
      ElMessage.success(`已加入购物车，当前购物车共 ${res.cartCount} 件`)
    })
}

async function onBuyNow() {
  if (!isLoggedIn.value) {
    router.push('/login')
    return
  }
  if (!selectedSku.value) {
    ElMessage.warning('请选择规格')
    return
  }

  try {
    await cartApi.add({
      productId: productId,
      skuId: selectedSku.value.id,
      count: 1,
    })
    const list = await cartApi.getList()
    const item = list.find(
      (i) => i.productId === productId && i.skuId === selectedSku.value!.id
    )
    if (item) {
      router.push({ path: '/checkout', query: { ids: String(item.id) } })
    } else {
      ElMessage.warning('未能定位购物车商品，请从购物车结算')
      router.push('/cart')
    }
  } catch {
    // error handled by interceptor
  }
}

function onImageClick() {
  viewerIndex.value = currentImageIndex.value
  showViewer.value = true
}

function prevImage() {
  if (!product.value || product.value.images.length === 0) return
  currentImageIndex.value =
    (currentImageIndex.value - 1 + product.value.images.length) % product.value.images.length
}

function nextImage() {
  if (!product.value || product.value.images.length === 0) return
  currentImageIndex.value = (currentImageIndex.value + 1) % product.value.images.length
}

function selectThumbnail(index: number) {
  currentImageIndex.value = index
}
</script>

<template>
  <div class="product-detail-page" v-loading="loading">
    <!-- 顶部返回 -->
    <div class="top-bar">
      <el-button :icon="ArrowLeft" text circle @click="router.back()">
        <span style="margin-left: 4px">返回</span>
      </el-button>
    </div>

    <div v-if="product" class="detail-body">
      <!-- 左侧图片区 -->
      <div class="left-section">
        <div class="main-image-wrap">
          <el-image
            :src="product.images[currentImageIndex] || product.mainImage"
            fit="cover"
            class="main-image"
            @click="onImageClick"
          />
          <button class="img-arrow img-arrow-left" @click.stop="prevImage">
            <el-icon><ArrowLeft /></el-icon>
          </button>
          <button class="img-arrow img-arrow-right" @click.stop="nextImage">
            <el-icon><ArrowRight /></el-icon>
          </button>
        </div>

        <!-- 缩略图 -->
        <div class="thumbnail-list">
          <div
            v-for="(img, index) in product.images"
            :key="index"
            class="thumbnail-item"
            :class="{ active: currentImageIndex === index }"
            @click="selectThumbnail(index)"
          >
            <el-image :src="img" fit="cover" />
          </div>
        </div>
      </div>

      <!-- 右侧信息区 -->
      <div class="right-section">
        <div class="info-content">
          <!-- 价格 -->
          <div class="price-block">
            <span class="current-price">¥{{ currentPrice.toFixed(2) }}</span>
            <span v-if="product.originalPrice > currentPrice" class="original-price">
              ¥{{ product.originalPrice.toFixed(2) }}
            </span>
          </div>

          <!-- 名称 + 副标题 -->
          <h1 class="product-name">{{ product.name }}</h1>
          <p v-if="product.subtitle" class="product-subtitle">{{ product.subtitle }}</p>

          <!-- 销量库存 -->
          <div class="meta-row">
            <el-tag type="danger" effect="light" round>已售{{ product.soldCount }}件</el-tag>
            <span class="stock-text">库存 {{ currentStock }} 件</span>
          </div>

          <!-- 标签 -->
          <div class="tag-row">
            <span v-if="product.flowerLanguage" class="tag tag-red">{{ product.flowerLanguage }}</span>
            <span v-if="product.suitableFor" class="tag tag-green">适合：{{ product.suitableFor }}</span>
          </div>

          <!-- SKU -->
          <div class="sku-block">
            <div class="block-title">选择规格</div>
            <div class="sku-options">
              <div
                v-for="sku in skus"
                :key="sku.id"
                class="sku-card"
                :class="{ active: selectedSku?.id === sku.id }"
                @click="onSkuClick(sku)"
              >
                <div class="sku-name">{{ sku.specName }}</div>
                <div class="sku-price">¥{{ sku.price.toFixed(2) }}</div>
              </div>
              <span v-if="skus.length === 0" class="no-sku">暂无规格选项</span>
            </div>
          </div>

          <!-- 配送说明 -->
          <div class="delivery-block">
            <div class="block-title">配送说明</div>
            <p class="delivery-text">
              {{ product.deliveryDesc || '同城2小时送达，全国冷链配送，确保鲜花新鲜。' }}
            </p>
          </div>

          <!-- 商品详情 -->
          <div class="desc-block">
            <div class="block-title">商品详情</div>
            <p class="desc-text">{{ product.description || '暂无描述' }}</p>
          </div>
        </div>
      </div>
    </div>

    <!-- 底部固定操作栏 -->
    <div class="bottom-action-bar">
      <div class="action-left">
        <el-badge :value="cartStore.totalCount" :hidden="cartStore.totalCount === 0" class="cart-badge">
          <el-button :icon="ShoppingCart" circle @click="router.push('/cart')" />
        </el-badge>
      </div>
      <div class="action-right">
        <el-button class="btn-cart" size="large" @click="onAddToCart">加入购物车</el-button>
        <el-button class="btn-buy" size="large" @click="onBuyNow">立即购买</el-button>
      </div>
    </div>

    <!-- 图片预览 -->
    <el-image-viewer
      v-if="showViewer"
      :url-list="product?.images ?? []"
      :initial-index="viewerIndex"
      @close="showViewer = false"
    />
  </div>
</template>

<style scoped lang="scss">
.product-detail-page {
  min-height: 100vh;
  padding-bottom: 80px;
  background: #fff;
}

:deep(.cart-badge .el-badge__content) {
  top: 6px;
  right: 6px;
}

.top-bar {
  padding: 12px 24px;
  border-bottom: 1px solid #f0f0f0;

  .el-button {
    font-size: 14px;
    color: #666;
  }
}

.detail-body {
  display: flex;
  max-width: 1200px;
  margin: 0 auto;
  padding: 24px;
  gap: 40px;
}

/* 左侧图片区 */
.left-section {
  flex: 1;
  min-width: 0;

  .main-image-wrap {
    position: relative;
    width: 100%;
    aspect-ratio: 1 / 1;
    border-radius: 12px;
    overflow: hidden;
    background: #f8f8f8;

    .main-image {
      width: 100%;
      height: 100%;
      cursor: pointer;
    }

    .img-arrow {
      position: absolute;
      top: 50%;
      transform: translateY(-50%);
      width: 36px;
      height: 36px;
      border-radius: 50%;
      border: none;
      background: rgba(255, 255, 255, 0.85);
      color: #666;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: background 0.2s;

      &:hover {
        background: rgba(255, 255, 255, 1);
      }

      &.img-arrow-left {
        left: 12px;
      }

      &.img-arrow-right {
        right: 12px;
      }
    }
  }

  .thumbnail-list {
    display: flex;
    gap: 10px;
    margin-top: 16px;

    .thumbnail-item {
      width: 72px;
      height: 72px;
      border-radius: 8px;
      overflow: hidden;
      cursor: pointer;
      border: 2px solid transparent;
      transition: border-color 0.2s;

      .el-image {
        width: 100%;
        height: 100%;
      }

      &.active {
        border-color: #f56c6c;
      }
    }
  }
}

/* 右侧信息区 */
.right-section {
  flex: 1;
  min-width: 0;

  .info-content {
    padding-top: 8px;
  }

  .price-block {
    display: flex;
    align-items: baseline;
    gap: 10px;
    margin-bottom: 12px;

    .current-price {
      font-size: 32px;
      font-weight: 700;
      color: #f56c6c;
    }

    .original-price {
      font-size: 16px;
      color: #999;
      text-decoration: line-through;
    }
  }

  .product-name {
    font-size: 22px;
    font-weight: 600;
    color: #333;
    margin: 0 0 6px;
    line-height: 1.4;
  }

  .product-subtitle {
    font-size: 14px;
    color: #999;
    margin: 0 0 16px;
  }

  .meta-row {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 16px;

    .stock-text {
      font-size: 13px;
      color: #999;
    }
  }

  .tag-row {
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
    margin-bottom: 24px;

    .tag {
      display: inline-block;
      padding: 4px 12px;
      border-radius: 4px;
      font-size: 13px;

      &.tag-red {
        background: #fef0f0;
        color: #f56c6c;
      }

      &.tag-green {
        background: #f0f9eb;
        color: #67c23a;
      }
    }
  }

  .sku-block,
  .delivery-block,
  .desc-block {
    margin-bottom: 24px;

    .block-title {
      font-size: 14px;
      font-weight: 600;
      color: #333;
      margin-bottom: 12px;
    }
  }

  .sku-options {
    display: flex;
    flex-wrap: wrap;
    gap: 12px;

    .sku-card {
      min-width: 100px;
      padding: 10px 16px;
      border: 1px solid #e0e0e0;
      border-radius: 8px;
      cursor: pointer;
      text-align: center;
      transition: all 0.2s;

      .sku-name {
        font-size: 13px;
        color: #333;
        margin-bottom: 4px;
      }

      .sku-price {
        font-size: 14px;
        font-weight: 600;
        color: #f56c6c;
      }

      &:hover {
        border-color: #f56c6c;
      }

      &.active {
        border-color: #f56c6c;
        background: #fef0f0;
      }
    }

    .no-sku {
      font-size: 13px;
      color: #999;
    }
  }

  .delivery-text,
  .desc-text {
    font-size: 13px;
    color: #666;
    line-height: 1.8;
    margin: 0;
  }
}

/* 底部操作栏 */
.bottom-action-bar {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  z-index: 100;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 24px;
  background: #fff;
  box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.06);

  .action-left {
    .el-button {
      width: 44px;
      height: 44px;
      font-size: 18px;
      color: #666;
      border: 1px solid #e0e0e0;
    }
  }

  .action-right {
    display: flex;
    gap: 12px;

    .btn-cart {
      min-width: 140px;
      border-radius: 24px;
      background: #ff8a8a;
      border-color: #ff8a8a;
      color: #fff;
      font-size: 15px;

      &:hover {
        background: #ff7070;
        border-color: #ff7070;
      }
    }

    .btn-buy {
      min-width: 140px;
      border-radius: 24px;
      background: #f56c6c;
      border-color: #f56c6c;
      color: #fff;
      font-size: 15px;

      &:hover {
        background: #e64c4c;
        border-color: #e64c4c;
      }
    }
  }
}

/* 响应式 */
@media (max-width: 768px) {
  .detail-body {
    flex-direction: column;
    padding: 16px;
    gap: 20px;
  }

  .left-section {
    .thumbnail-list {
      .thumbnail-item {
        width: 56px;
        height: 56px;
      }
    }
  }

  .right-section {
    .price-block {
      .current-price {
        font-size: 26px;
      }
    }

    .product-name {
      font-size: 18px;
    }
  }

  .bottom-action-bar {
    padding: 10px 16px;

    .action-right {
      .btn-cart,
      .btn-buy {
        min-width: 120px;
        font-size: 14px;
      }
    }
  }
}
</style>
