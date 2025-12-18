<template>
  <div class="pagination">
    <button 
      :disabled="currentPage === 1 || loading" 
      @click="$emit('page-change', currentPage - 1)"
      class="btn-nav"
    >
      Anterior
    </button>
    
    <div class="page-numbers">
      <button
        v-for="page in visiblePages"
        :key="page"
        @click="$emit('page-change', page)"
        :class="['btn-page', { active: page === currentPage, disabled: loading }]"
        :disabled="loading"
      >
        {{ page }}
      </button>
    </div>
    
    <button 
      :disabled="currentPage === totalPages || loading" 
      @click="$emit('page-change', currentPage + 1)"
      class="btn-nav"
    >
      Próxima
    </button>
  </div>
</template>

<script>
export default {
  props: {
    currentPage: {
      type: Number,
      required: true
    },
    totalPages: {
      type: Number,
      required: true
    },
    loading: {
      type: Boolean,
      default: false
    }
  },
  computed: {
    visiblePages() {
      const pages = []
      const maxVisible = 5
      
      if (this.totalPages <= maxVisible) {
        // Se tem 5 ou menos páginas, mostra todas
        for (let i = 1; i <= this.totalPages; i++) {
          pages.push(i)
        }
      } else {
        // Se tem mais de 5 páginas
        let startPage = 1
        let endPage = maxVisible
        
        if (this.currentPage > 1) {
          // Tenta centralizar a página atual
          const half = Math.floor(maxVisible / 2)
          startPage = Math.max(1, this.currentPage - half)
          endPage = Math.min(this.totalPages, startPage + maxVisible - 1)
          
          // Ajusta se chegou no final
          if (endPage === this.totalPages) {
            startPage = Math.max(1, this.totalPages - maxVisible + 1)
          }
          // Ajusta se está no início
          else if (startPage === 1) {
            endPage = maxVisible
          }
        }
        
        for (let i = startPage; i <= endPage; i++) {
          pages.push(i)
        }
      }
      
      return pages
    }
  }
}
</script>

<style scoped>
.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.page-numbers {
  display: flex;
  gap: 0.25rem;
}

.btn-nav,
.btn-page {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.2s ease;
  min-width: 40px;
}

.btn-nav {
  background-color: #0c66a3;
  color: white;
}

.btn-nav:hover:not(:disabled) {
  background-color: #095a8f;
  transform: translateY(-1px);
}

.btn-nav:disabled {
  background-color: #ccc;
  cursor: not-allowed;
  opacity: 0.6;
}

.btn-page {
  background-color: #f5f5f5;
  color: #333;
}

.btn-page:hover:not(:disabled):not(.active) {
  background-color: #e0e0e0;
  transform: translateY(-1px);
}

.btn-page.active {
  background-color: #0c66a3;
  color: white;
  font-weight: 600;
}

.btn-page.disabled {
  cursor: not-allowed;
  opacity: 0.6;
}
</style> 