import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import LoginView from '../views/LoginView.vue'
import RegisterView from '../views/RegisterView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/login',
      name: 'login',
      component: LoginView,
    },
    {
      path: '/register',
      name: 'register',
      component: RegisterView,
    },
    {
      path: '/about',
      name: 'about',
      component: () => import('../views/AboutView.vue'),
    },
    {
      path: '/product/:id',
      name: 'productDetail',
      component: () => import('../views/ProductDetailView.vue'),
    },
    {
      path: '/cart',
      name: 'cart',
      component: () => import('../views/CartView.vue'),
    },
    {
      path: '/checkout',
      name: 'checkout',
      component: () => import('../views/CheckoutView.vue'),
    },
    {
      path: '/order/:id',
      name: 'orderDetail',
      component: () => import('../views/OrderDetailView.vue'),
    },
    {
      path: '/user',
      component: () => import('../views/user/UserCenterView.vue'),
      redirect: '/user/profile',
      children: [
        {
          path: 'profile',
          name: 'userProfile',
          component: () => import('../views/user/ProfileView.vue'),
        },
        {
          path: 'orders',
          name: 'userOrders',
          component: () => import('../views/user/OrderListView.vue'),
        },
        {
          path: 'addresses',
          name: 'userAddresses',
          component: () => import('../views/user/AddressView.vue'),
        },
      ],
    },
  ],
})

router.beforeEach((to, from, next) => {
  const publicPages = ['/', '/about', '/login', '/register']
  const authRequired = !publicPages.some((p) => to.path === p || to.path.startsWith('/product/'))
  const token = localStorage.getItem('token')

  if (authRequired && !token) {
    return next('/login')
  }

  next()
})

export default router
