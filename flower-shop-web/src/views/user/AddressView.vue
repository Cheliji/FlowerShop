<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import { addressApi } from '@/api/address'
import type { Address } from '@/types/product'

const addresses = ref<Address[]>([])
const loading = ref(false)
const dialogVisible = ref(false)
const isEdit = ref(false)
const editId = ref<number | null>(null)

const form = ref({
  receiverName: '',
  phone: '',
  province: '',
  city: '',
  district: '',
  detailAddress: '',
  isDefault: false,
})

onMounted(() => {
  fetchAddresses()
})

async function fetchAddresses() {
  loading.value = true
  try {
    addresses.value = await addressApi.getList()
  } catch {
    // error handled by interceptor
  } finally {
    loading.value = false
  }
}

function openAdd() {
  isEdit.value = false
  editId.value = null
  form.value = {
    receiverName: '',
    phone: '',
    province: '',
    city: '',
    district: '',
    detailAddress: '',
    isDefault: false,
  }
  dialogVisible.value = true
}

function openEdit(addr: Address) {
  isEdit.value = true
  editId.value = addr.id
  form.value = {
    receiverName: addr.receiverName,
    phone: addr.phone,
    province: addr.province || '',
    city: addr.city || '',
    district: addr.district || '',
    detailAddress: addr.detailAddress,
    isDefault: addr.isDefault,
  }
  dialogVisible.value = true
}

async function onSubmit() {
  if (!form.value.receiverName.trim()) {
    ElMessage.warning('请输入收货人姓名')
    return
  }
  if (!form.value.phone.trim()) {
    ElMessage.warning('请输入手机号')
    return
  }
  if (!form.value.detailAddress.trim()) {
    ElMessage.warning('请输入详细地址')
    return
  }

  try {
    if (isEdit.value && editId.value) {
      await addressApi.update(editId.value, form.value)
      ElMessage.success('地址更新成功')
    } else {
      await addressApi.create(form.value)
      ElMessage.success('地址添加成功')
    }
    dialogVisible.value = false
    fetchAddresses()
  } catch (err: any) {
    ElMessage.error(err?.message || '操作失败')
  }
}

async function onDelete(addr: Address) {
  try {
    await ElMessageBox.confirm('确定要删除该地址吗？', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
    })
    await addressApi.remove(addr.id)
    ElMessage.success('删除成功')
    fetchAddresses()
  } catch (err: any) {
    if (err !== 'cancel') {
      ElMessage.error(err?.message || '删除失败')
    }
  }
}
</script>

<template>
  <div class="address-page">
    <div class="page-header">
      <h2 class="page-title">收货地址</h2>
      <el-button type="primary" :icon="Plus" @click="openAdd">新增地址</el-button>
    </div>

    <div v-loading="loading" class="address-list">
      <div v-if="addresses.length === 0" class="empty-state">
        <el-empty description="暂无收货地址" />
      </div>

      <div v-for="addr in addresses" :key="addr.id" class="address-card">
        <div class="addr-header">
          <span class="addr-name">{{ addr.receiverName }}</span>
          <span class="addr-phone">{{ addr.phone }}</span>
          <el-tag v-if="addr.isDefault" type="danger" size="small" effect="plain">默认</el-tag>
        </div>
        <p class="addr-detail">
          {{ addr.province }}{{ addr.city }}{{ addr.district }}{{ addr.detailAddress }}
        </p>
        <div class="addr-actions">
          <el-button text type="primary" size="small" @click="openEdit(addr)">编辑</el-button>
          <el-button text type="danger" size="small" @click="onDelete(addr)">删除</el-button>
        </div>
      </div>
    </div>

    <!-- 新增/编辑弹窗 -->
    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑地址' : '新增地址'" width="500px">
      <el-form label-width="90px">
        <el-form-item label="收货人">
          <el-input v-model="form.receiverName" placeholder="请输入收货人姓名" maxlength="50" />
        </el-form-item>
        <el-form-item label="手机号">
          <el-input v-model="form.phone" placeholder="请输入手机号" maxlength="20" />
        </el-form-item>
        <el-form-item label="所在地区">
          <div class="region-row">
            <el-input v-model="form.province" placeholder="省" />
            <el-input v-model="form.city" placeholder="市" />
            <el-input v-model="form.district" placeholder="区" />
          </div>
        </el-form-item>
        <el-form-item label="详细地址">
          <el-input v-model="form.detailAddress" type="textarea" :rows="2" placeholder="请输入详细地址" maxlength="200" />
        </el-form-item>
        <el-form-item>
          <el-checkbox v-model="form.isDefault">设为默认地址</el-checkbox>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="onSubmit">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped lang="scss">
.address-page {
  .page-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 20px;

    .page-title {
      font-size: 20px;
      font-weight: 600;
      color: #333;
      margin: 0;
    }
  }

  .address-list {
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .empty-state {
    padding: 40px 0;
  }

  .address-card {
    background: #fff;
    border-radius: 12px;
    padding: 16px;
    border: 1px solid #ebeef5;

    .addr-header {
      display: flex;
      align-items: center;
      gap: 10px;
      margin-bottom: 8px;

      .addr-name {
        font-size: 15px;
        font-weight: 600;
        color: #333;
      }

      .addr-phone {
        font-size: 13px;
        color: #666;
      }
    }

    .addr-detail {
      font-size: 13px;
      color: #666;
      margin: 0 0 12px;
      line-height: 1.5;
    }

    .addr-actions {
      display: flex;
      gap: 8px;
    }
  }

  .region-row {
    display: flex;
    gap: 8px;

    .el-input {
      flex: 1;
    }
  }
}
</style>
