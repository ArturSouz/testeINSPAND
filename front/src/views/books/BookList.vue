<template>
  <div class="book-list">
    <!-- Botão criar livro -->
    <div class="header-actions">
      <button @click="openCreateModal" class="btn-create">
        ➕ Criar Novo Livro
      </button>
    </div>

    <!-- Mensagem de erro temporária -->
    <transition name="fade">
      <div v-if="errorMessage" class="error-popup">
        {{ errorMessage }}
      </div>
    </transition>

    <!-- Modal de confirmação de exclusão -->
    <transition name="fade">
      <div v-if="showDeleteModal" class="modal-overlay" @click="closeDeleteModal">
        <div class="delete-modal-content" @click.stop>
          <div class="delete-modal-icon">
            <svg width="64" height="64" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <circle cx="12" cy="12" r="10" fill="#ffebee"/>
              <path d="M9 3V4H4V6H5V19C5 20.1 5.9 21 7 21H17C18.1 21 19 20.1 19 19V6H20V4H15V3H9ZM7 6H17V19H7V6ZM9 8V17H11V8H9ZM13 8V17H15V8H13Z" fill="#f44336"/>
            </svg>
          </div>
          <h2 class="delete-modal-title">Confirmar Exclusão</h2>
          <p class="delete-modal-message">
            Tem certeza que deseja excluir o livro <strong>"{{ bookToDelete?.title }}"</strong>?
          </p>
          <p class="delete-modal-warning">
            Esta ação não pode ser desfeita.
          </p>
          <div class="delete-modal-actions">
            <button @click="closeDeleteModal" class="btn-delete-cancel">
              Cancelar
            </button>
            <button @click="executeDelete" class="btn-delete-confirm" :disabled="deleting">
              <span v-if="!deleting">Excluir</span>
              <span v-else>Excluindo...</span>
            </button>
          </div>
        </div>
      </div>
    </transition>

    <!-- Modal para criar/editar -->
    <div v-if="showModal" class="modal-overlay" @click="closeModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>{{ editingBook ? 'Editar Livro' : 'Criar Novo Livro' }}</h2>
          <button @click="closeModal" class="btn-close">✕</button>
        </div>
        
        <form @submit.prevent="saveBook" class="book-form">
          <!-- Título -->
          <div class="form-group">
            <label for="title">Título *</label>
            <input
              id="title"
              v-model="form.title"
              type="text"
              :class="['form-input', { 'invalid': !titleValid, 'valid': titleValid && form.title }]"
              @blur="validateTitle"
            />
            <div v-if="form.title" class="validation-message" :class="{ 'error': !titleValid, 'success': titleValid }">
              {{ titleValidationMessage }}
            </div>
          </div>

          <!-- Autor -->
          <div class="form-group">
            <label for="author">Autor *</label>
            <input
              id="author"
              v-model="form.author"
              type="text"
              :class="['form-input', { 'invalid': !authorValid, 'valid': authorValid && form.author }]"
              @blur="validateAuthor"
            />
            <div v-if="form.author" class="validation-message" :class="{ 'error': !authorValid, 'success': authorValid }">
              {{ authorValidationMessage }}
            </div>
          </div>

          <!-- Descrição -->
          <div class="form-group">
            <label for="description">Descrição</label>
            <textarea
              id="description"
              v-model="form.description"
              rows="5"
              :class="['form-input', { 'invalid': !descriptionValid, 'valid': descriptionValid && form.description }]"
              @blur="validateDescription"
            ></textarea>
            <div v-if="form.description" class="validation-message" :class="{ 'error': !descriptionValid, 'success': descriptionValid }">
              {{ descriptionValidationMessage }}
            </div>
          </div>

          <div class="form-actions">
            <button type="button" @click="closeModal" class="btn-cancel">
              Cancelar
            </button>
            <button type="submit" :disabled="!isFormValid" class="btn-save">
              {{ editingBook ? 'Salvar' : 'Criar' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Lista de livros -->
    <div v-if="error" class="error-message">
      {{ error }}
    </div>

    <div v-if="loading" class="loading">
      Carregando...
    </div>

    <div v-else>
      <div class="books-container">
        <BookCard 
          v-for="book in books" 
          :key="book.id" 
          :book="book"
          @edit="openEditModal"
          @delete="confirmDelete"
        />
      </div>

      <div v-if="books.length === 0" class="no-books">
        Nenhum livro encontrado.
      </div>

      <Pagination
        v-if="totalPages > 0"
        :current-page="currentPage"
        :total-pages="totalPages"
        :loading="loading"
        @page-change="changePage"
      />
    </div>
  </div>
</template>

<script>
import BookCard from '@/components/books/BookCard.vue'
import Pagination from '@/components/common/Pagination.vue'
import { bookService } from '@/services/api'

export default {
  components: {
    BookCard,
    Pagination
  },
  data() {
    return {
      books: [],
      currentPage: 1,
      pageSize: 10,
      totalPages: 0,
      loading: false,
      error: null,
      showModal: false,
      editingBook: null,
      form: {
        title: '',
        author: '',
        description: ''
      },
      errorMessage: null,
      errorPopupTimeout: 5000, // Valor parametrizável para timeout do popup de erro
      showDeleteModal: false,
      bookToDelete: null,
      deleting: false
    }
  },
  computed: {
    titleValid() {
      if (!this.form.title) return false
      return this.form.title.length >= 10 && this.form.title.length <= 100
    },
    titleValidationMessage() {
      if (!this.form.title) return ''
      const length = this.form.title.length
      if (length < 10) return `Título deve ter no mínimo 10 caracteres (${length}/10)`
      if (length > 100) return `Título deve ter no máximo 100 caracteres (${length}/100)`
      return `Título válido (${length} caracteres)`
    },
    authorValid() {
      if (!this.form.author) return false
      return this.form.author.length >= 10 && this.form.author.length <= 100
    },
    authorValidationMessage() {
      if (!this.form.author) return ''
      const length = this.form.author.length
      if (length < 10) return `Autor deve ter no mínimo 10 caracteres (${length}/10)`
      if (length > 100) return `Autor deve ter no máximo 100 caracteres (${length}/100)`
      return `Autor válido (${length} caracteres)`
    },
    descriptionValid() {
      if (!this.form.description) return true // Descrição é opcional
      return this.form.description.length <= 1024
    },
    descriptionValidationMessage() {
      if (!this.form.description) return ''
      const length = this.form.description.length
      if (length > 1024) return `Descrição deve ter no máximo 1024 caracteres (${length}/1024)`
      return `Descrição válida (${length}/1024 caracteres)`
    },
    isFormValid() {
      return this.titleValid && this.authorValid && this.descriptionValid
    }
  },
  methods: {
    async fetchBooks() {
      this.loading = true
      this.error = null
      try {
        const data = await bookService.getBooks(this.currentPage, this.pageSize)
        this.books = data.books || []
        this.totalPages = data.totalPages || 0
      } catch (error) {
        console.error('Erro ao buscar livros:', error)
        this.error = 'Erro ao carregar os livros. Por favor, tente novamente.'
        this.books = []
        this.totalPages = 0
      } finally {
        this.loading = false
      }
    },
    changePage(page) {
      this.currentPage = page
      this.fetchBooks()
    },
    openCreateModal() {
      this.editingBook = null
      this.form = {
        title: '',
        author: '',
        description: ''
      }
      this.showModal = true
    },
    openEditModal(book) {
      this.editingBook = book
      this.form = {
        title: book.title,
        author: book.author,
        description: book.description || ''
      }
      this.showModal = true
    },
    closeModal() {
      this.showModal = false
      this.editingBook = null
      this.form = {
        title: '',
        author: '',
        description: ''
      }
    },
    validateTitle() {
      // Validação já feita no computed
    },
    validateAuthor() {
      // Validação já feita no computed
    },
    validateDescription() {
      // Validação já feita no computed
    },
    async saveBook() {
      if (!this.isFormValid) {
        this.showError('Por favor, preencha todos os campos corretamente.')
        return
      }

      try {
        if (this.editingBook) {
          await bookService.updateBook(this.editingBook.id, this.form)
        } else {
          await bookService.createBook(this.form)
        }
        this.closeModal()
        this.fetchBooks()
      } catch (error) {
        this.showError(error.message)
      }
    },
    confirmDelete(book) {
      this.bookToDelete = book
      this.showDeleteModal = true
    },
    closeDeleteModal() {
      this.showDeleteModal = false
      this.bookToDelete = null
      this.deleting = false
    },
    async executeDelete() {
      if (!this.bookToDelete) return
      
      this.deleting = true
      try {
        await bookService.deleteBook(this.bookToDelete.id)
        this.closeDeleteModal()
        this.fetchBooks()
      } catch (error) {
        this.showError(error.message)
        this.deleting = false
      }
    },
    showError(message) {
      this.errorMessage = message
      setTimeout(() => {
        this.errorMessage = null
      }, this.errorPopupTimeout)
    }
  },
  mounted() {
    this.fetchBooks()
  }
}
</script>

<style scoped>
.book-list {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.header-actions {
  margin-bottom: 2rem;
  display: flex;
  justify-content: flex-end;
}

.btn-create {
  background-color: #0c66a3;
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 4px;
  font-size: 1rem;
  cursor: pointer;
  font-weight: 500;
  transition: background-color 0.2s;
}

.btn-create:hover {
  background-color: #095a8f;
}

/* Popup de erro */
.error-popup {
  position: fixed;
  top: 20px;
  right: 20px;
  background-color: #f44336;
  color: white;
  padding: 1rem 1.5rem;
  border-radius: 4px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  z-index: 10000;
  max-width: 400px;
}

.fade-enter-active, .fade-leave-active {
  transition: opacity 0.3s;
}

.fade-enter-from, .fade-leave-to {
  opacity: 0;
}

/* Modal */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  border-radius: 8px;
  width: 90%;
  max-width: 600px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid #ddd;
}

.modal-header h2 {
  margin: 0;
  color: #0c66a3;
}

.btn-close {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #666;
  padding: 0;
  width: 30px;
  height: 30px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-close:hover {
  color: #000;
}

.book-form {
  padding: 1.5rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: #333;
}

.form-input {
  width: 100%;
  padding: 0.75rem;
  border: 2px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
  box-sizing: border-box;
  transition: border-color 0.2s;
}

.form-input:focus {
  outline: none;
  border-color: #0c66a3;
}

.form-input.invalid {
  border-color: #f44336;
}

.form-input.valid {
  border-color: #4caf50;
}

.validation-message {
  margin-top: 0.5rem;
  font-size: 0.875rem;
}

.validation-message.error {
  color: #f44336;
}

.validation-message.success {
  color: #4caf50;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 2rem;
  padding-top: 1.5rem;
  border-top: 1px solid #ddd;
}

.btn-cancel,
.btn-save {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  font-size: 1rem;
  cursor: pointer;
  font-weight: 500;
  transition: background-color 0.2s;
}

.btn-cancel {
  background-color: #f5f5f5;
  color: #333;
}

.btn-cancel:hover {
  background-color: #e0e0e0;
}

.btn-save {
  background-color: #0c66a3;
  color: white;
}

.btn-save:hover:not(:disabled) {
  background-color: #095a8f;
}

.btn-save:disabled {
  background-color: #ccc;
  cursor: not-allowed;
}

.books-container {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.error-message {
  color: red;
  text-align: center;
  padding: 1rem;
  margin-bottom: 1rem;
  background-color: #ffebee;
  border-radius: 4px;
}

.loading {
  text-align: center;
  padding: 2rem;
  color: #666;
}

.no-books {
  text-align: center;
  padding: 2rem;
  color: #666;
}

/* Modal de confirmação de exclusão */
.delete-modal-content {
  background: white;
  border-radius: 12px;
  width: 90%;
  max-width: 480px;
  padding: 2rem;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
  text-align: center;
  animation: slideDown 0.3s ease-out;
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.delete-modal-icon {
  margin-bottom: 1rem;
  display: flex;
  justify-content: center;
  animation: pulse 0.6s ease-in-out;
}

@keyframes pulse {
  0%, 100% {
    transform: scale(1);
  }
  50% {
    transform: scale(1.1);
  }
}

.delete-modal-title {
  margin: 0 0 1rem 0;
  color: #333;
  font-size: 1.5rem;
  font-weight: 600;
}

.delete-modal-message {
  margin: 0 0 0.5rem 0;
  color: #666;
  font-size: 1rem;
  line-height: 1.5;
}

.delete-modal-message strong {
  color: #333;
  font-weight: 600;
}

.delete-modal-warning {
  margin: 0 0 2rem 0;
  color: #f44336;
  font-size: 0.875rem;
  font-weight: 500;
}

.delete-modal-actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
}

.btn-delete-cancel,
.btn-delete-confirm {
  padding: 0.75rem 2rem;
  border: none;
  border-radius: 6px;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
  min-width: 120px;
}

.btn-delete-cancel {
  background-color: #f5f5f5;
  color: #333;
}

.btn-delete-cancel:hover {
  background-color: #e0e0e0;
  transform: translateY(-1px);
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.btn-delete-confirm {
  background-color: #f44336;
  color: white;
}

.btn-delete-confirm:hover:not(:disabled) {
  background-color: #d32f2f;
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(244, 67, 54, 0.3);
}

.btn-delete-confirm:disabled {
  background-color: #ffcdd2;
  cursor: not-allowed;
  opacity: 0.7;
}
</style>
