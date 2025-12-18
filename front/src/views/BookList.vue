<template>
    <div class="book-list">
        <div v-if="error" class="error-message">
            {{ error }}
        </div>

        <div v-if="loading" class="loading">
            Carregando...
        </div>

        <div v-else>
            <div class="books-container">
                <div v-for="book in books" :key="book.id" class="book-card">
                    <h2>{{ book.title }}</h2>
                    <p><strong>Autor:</strong> {{ book.author }}</p>
                    <p>{{ book.description }}</p>
                </div>
            </div>

            <div v-if="books.length === 0" class="no-books">
                Nenhum livro encontrado.
            </div>

            <div class="pagination">
                <button :disabled="currentPage === 1 || loading" @click="changePage(currentPage - 1)">
                    Anterior
                </button>
                <span>Página {{ currentPage }}</span>
                <button :disabled="books.length < pageSize || loading" @click="changePage(currentPage + 1)">
                    Próxima
                </button>
            </div>
        </div>
    </div>
</template>

<script>
export default {
    data() {
        return {
            books: [],
            currentPage: 1,
            pageSize: 10,
            loading: false,
            error: null
        }
    },
    methods: {
        async fetchBooks() {
            this.loading = true
            this.error = null
            try {
                const response = await fetch(
                    `/api/Book?page=${this.currentPage}&pageSize=${this.pageSize}`
                )
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`)
                }
                this.books = await response.json()
            } catch (error) {
                console.error('Erro ao buscar livros:', error)
                this.error = 'Erro ao carregar os livros. Por favor, tente novamente.'
            } finally {
                this.loading = false
            }
        },
        changePage(page) {
            this.currentPage = page
            this.fetchBooks()
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

.books-container {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
    gap: 1.5rem;
    margin-bottom: 2rem;
}

.book-card {
    border: 1px solid #ddd;
    border-radius: 8px;
    padding: 1rem;
    background-color: white;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.book-card h2 {
    color: #0c66a3;
    margin-top: 0;
}

.pagination {
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 1rem;
}

.pagination button {
    padding: 0.5rem 1rem;
    background-color: #0c66a3;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
}

.pagination button:disabled {
    background-color: #ccc;
    cursor: not-allowed;
}

.pagination span {
    font-weight: bold;
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
</style>