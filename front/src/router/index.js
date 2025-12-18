import { createRouter, createWebHistory } from 'vue-router'
import BookList from '../views/books/BookList.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.VITE_BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: BookList
    }
  ]
})

export default router 