const API_URL = '/api'

export const bookService = {
  async getBooks(page, pageSize) {
    const response = await fetch(
      `${API_URL}/Book?page=${page}&pageSize=${pageSize}`
    )
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`)
    }
    const data = await response.json()
    return data
  },

  async createBook(book) {
    const response = await fetch(`${API_URL}/Book`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(book)
    })
    
    if (!response.ok) {
      let errorMessage = 'Erro ao criar livro'
      try {
        const errorData = await response.json()
        if (errorData.error) {
          errorMessage = errorData.error
        }
      } catch (e) {
        // Se não conseguir fazer parse do JSON, usa mensagem padrão
        console.error('Erro ao parsear resposta de erro:', e)
      }
      throw new Error(errorMessage)
    }
    
    try {
      const text = await response.text()
      if (text && text.trim()) {
        return JSON.parse(text)
      }
      return {}
    } catch (e) {
      console.error('Erro ao parsear resposta:', e)
      return {}
    }
  },

  async updateBook(id, book) {
    const response = await fetch(`${API_URL}/Book/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(book)
    })
    
    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.error || 'Erro ao editar livro')
    }
    
    return response.json()
  },

  async deleteBook(id) {
    const response = await fetch(`${API_URL}/Book/${id}`, {
      method: 'DELETE'
    })
    
    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.error || 'Erro ao excluir livro')
    }
  }
} 