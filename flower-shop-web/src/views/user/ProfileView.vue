<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { useUserStore } from '@/stores/user'
import { userApi } from '@/api/user'

const userStore = useUserStore()
const loading = ref(false)
const saving = ref(false)

const form = ref({
  nickname: '',
  phone: '',
  avatar: '',
})

onMounted(() => {
  const u = userStore.user
  if (u) {
    form.value.nickname = u.nickname || ''
    form.value.phone = u.phone || ''
    form.value.avatar = u.avatar || ''
  }
})

async function onSave() {
  saving.value = true
  try {
    await userApi.updateMe({
      nickname: form.value.nickname || undefined,
      phone: form.value.phone || undefined,
      avatar: form.value.avatar || undefined,
    })
    ElMessage.success('保存成功')
    await userStore.fetchUser()
  } catch (err: any) {
    ElMessage.error(err?.message || '保存失败')
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <div class="profile-page" v-loading="loading">
    <h2 class="page-title">个人资料</h2>

    <el-form label-width="100px" class="profile-form">
      <el-form-item label="头像">
        <el-avatar :size="80" :src="form.avatar || '/images/banners/avatar.png'">
          {{ form.nickname?.[0] || userStore.user?.username?.[0] || 'U' }}
        </el-avatar>
      </el-form-item>

      <el-form-item label="用户名">
        <span class="readonly-text">{{ userStore.user?.username }}</span>
      </el-form-item>

      <el-form-item label="昵称">
        <el-input v-model="form.nickname" placeholder="请输入昵称" maxlength="50" show-word-limit />
      </el-form-item>

      <el-form-item label="手机号">
        <el-input v-model="form.phone" placeholder="请输入手机号" maxlength="20" />
      </el-form-item>

      <el-form-item>
        <el-button type="primary" :loading="saving" @click="onSave">保存修改</el-button>
      </el-form-item>
    </el-form>
  </div>
</template>

<style scoped lang="scss">
.profile-page {
  .page-title {
    font-size: 20px;
    font-weight: 600;
    color: #333;
    margin: 0 0 24px;
  }

  .profile-form {
    max-width: 480px;
  }

  .readonly-text {
    color: #666;
    font-size: 14px;
  }
}
</style>
