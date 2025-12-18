<template>
  <div class="app">
    <Navbar />
    <router-view></router-view>
  </div>
</template>

<script>
import Navbar from '@/components/common/Navbar.vue'

export default {
  components: {
    Navbar
  }
}
</script>

<style>
.app {
  font-family: Arial, sans-serif;
}
</style> 