<template>
  <div class="auth-page">
    <div class="auth-container">
      <div class="auth-left">
        <div class="brand-text">
          <h1>花语</h1>
          <p>新用户专享 · 首单8折</p>
          <p class="sub-text">用鲜花传递每一份真挚情感</p>
        </div>
      </div>
      <div class="auth-right">
        <div class="auth-card">
          <div class="auth-header">
            <span class="flower-icon">🌸</span>
            <h2>欢迎回来</h2>
          </div>
          <el-form :model="form" :rules="rules" ref="formRef" class="auth-form">
            <el-form-item prop="username">
              <el-input
                v-model="form.username"
                placeholder="请输入用户名"
                :prefix-icon="User"
                size="large"
              />
            </el-form-item>
            <el-form-item prop="password">
              <el-input
                v-model="form.password"
                type="password"
                placeholder="请输入密码"
                :prefix-icon="Lock"
                size="large"
                show-password
              />
            </el-form-item>
            <el-form-item>
              <el-button
                type="primary"
                class="submit-btn"
                size="large"
                :loading="loading"
                @click="handleLogin"
              >
                登录
              </el-button>
            </el-form-item>
          </el-form>
          <div class="auth-footer">
            <span>还没有账号？</span>
            <router-link to="/register" class="link">立即注册</router-link>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { User, Lock } from '@element-plus/icons-vue'
import { useUserStore } from '@/stores/user'
import { authApi } from '@/api/auth'
import type { LoginResponse } from '@/types/user'
import type { FormInstance } from 'element-plus'

const router = useRouter()
const userStore = useUserStore()
const formRef = ref<FormInstance>()
const loading = ref(false)

const form = reactive({
  username: '',
  password: '',
})

const rules = {
  username: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
  password: [{ required: true, message: '请输入密码', trigger: 'blur' }],
}

const handleLogin = async () => {
  if (!formRef.value) return
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return

  loading.value = true
  try {
    const res = await authApi.login({
      username: form.username,
      password: form.password,
    })
    const data = res as LoginResponse
    userStore.setToken(data.token)
    await userStore.fetchUser()
    ElMessage.success('登录成功')
    router.push('/')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped lang="scss">
.auth-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #fff0f3 0%, #ffe4e9 100%);
}

.auth-container {
  display: flex;
  width: 900px;
  min-height: 560px;
  background: #fff;
  border-radius: 24px;
  box-shadow: 0 20px 60px rgba(255, 105, 180, 0.15);
  overflow: hidden;
}

.auth-left {
  flex: 1;
  background: linear-gradient(160deg, #ff9a9e 0%, #fecfef 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px;
  color: #fff;

  .brand-text {
    text-align: center;

    h1 {
      font-size: 48px;
      margin-bottom: 20px;
      font-weight: 600;
      letter-spacing: 4px;
    }

    p {
      font-size: 18px;
      margin-bottom: 12px;
      opacity: 0.95;
    }

    .sub-text {
      font-size: 14px;
      opacity: 0.8;
    }
  }
}

.auth-right {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px;
}

.auth-card {
  width: 100%;
  max-width: 360px;
}

.auth-header {
  text-align: center;
  margin-bottom: 32px;

  .flower-icon {
    font-size: 40px;
    display: block;
    margin-bottom: 12px;
  }

  h2 {
    font-size: 24px;
    color: #333;
    font-weight: 500;
  }
}

.auth-form {
  :deep(.el-input__wrapper) {
    border-radius: 12px;
    box-shadow: 0 0 0 1px #e4e7ed inset;
    padding: 4px 15px;
  }

  .submit-btn {
    width: 100%;
    border-radius: 12px;
    background: linear-gradient(135deg, #ff9a9e 0%, #f06292 100%);
    border: none;
    font-size: 16px;
    height: 44px;
    margin-top: 8px;

    &:hover {
      background: linear-gradient(135deg, #ff8589 0%, #ec407a 100%);
    }
  }
}

.auth-footer {
  text-align: center;
  margin-top: 24px;
  font-size: 14px;
  color: #666;

  .link {
    color: #f06292;
    text-decoration: none;
    margin-left: 4px;
    font-weight: 500;

    &:hover {
      color: #e91e63;
    }
  }
}

@media (max-width: 768px) {
  .auth-container {
    flex-direction: column;
    width: 90%;
    min-height: auto;
  }

  .auth-left {
    padding: 32px 20px;
    min-height: 180px;
  }
}
</style>
