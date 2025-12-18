# INSPAND Developers Exam - Full Stack

## Visão Geral

Projeto de avaliação full-stack implementando uma plataforma de gerenciamento de livros com operações CRUD. Algumas funcionalidades foram intencionalmente deixadas não implementadas para você completar.

## Stack Tecnológica

### Backend
- **.NET 10** (SDK 10.0.100)
- **Clean Architecture** com 3 camadas:
  - `Domain` - Lógica de negócio e entidades
  - `Infrastructure` - Acesso a dados e serviços externos
  - `WebAPI` - Endpoints HTTP e controllers
- **Entity Framework Core 10** com SQL Server
- **MediatR** para CQRS e eventos
- **FluentValidation** para validação de domínio

### Frontend
- **Vue 3.5.13** com Options API
- **Vue Router 4.5.0** para roteamento
- **Vite 6** como build tool
- **ESLint** para qualidade de código

## Começando

### Pré-requisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js](https://nodejs.org/) (v18+ recomendado)
- SQL Server 2017+ (ou use Docker - veja abaixo)

### Configuração do Banco de Dados

**Opção 1: Docker (Recomendado)**
```bash
cd back
docker-compose up -d
```

Isso sobe o SQL Server 2017 em `localhost:1433`:
- **Database**: `INSPAND_Exam`
- **Usuário**: `sa`
- **Senha**: `5Gi3re61sXehuSGxMqHX`

**Opção 2: SQL Server Existente**
Atualize a connection string em `back/src/WebAPI/appsettings.json` para apontar para sua instância.

### Rodando o Backend
```bash
cd back
dotnet restore
dotnet build
cd src/WebAPI
dotnet run
```

Backend roda em: `http://localhost:5240`

### Rodando o Frontend
```bash
cd front
npm install
npm run dev
```

Frontend roda em: `http://localhost:5173`

Chamadas à API em `/api/*` são automaticamente redirecionadas para o backend.


## Feedback Esperado

Junto com sua implementação, forneça seu entendimento sobre a arquitetura:

1. **Qual o papel da camada Domain?**
   A camada Domain é o núcleo da aplicação e contém a lógica de negócio pura, sem dependências externas. Ela é responsável por:
   - **Entidades de domínio**: Representam os objetos de negócio (ex: `Book`) com suas propriedades e comportamentos
   - **Validações de negócio**: Utiliza FluentValidation para garantir que as regras de negócio sejam respeitadas (ex: título entre 10-100 caracteres)
   - **Eventos de domínio**: Define eventos que representam algo importante que aconteceu em algum objeto de negócio.
   - **Interfaces**: Define contratos que serão implementados por outras camadas.
   - **Regras de negócio**: Contém a lógica essencial.

2. **Qual o papel da camada Infrastructure?**
   A camada Infrastructure implementa os detalhes técnicos e de infraestrutura necessários para a aplicação funcionar:
   - **Acesso a dados**: Implementa o `SqlDbContext`, mapeamentos de entidades para tabelas do banco
   - **Serviços externos**: Implementa serviços como `EmailService` que interagem com sistemas externos
   - **Handlers de eventos**: Processa eventos de domínio (ex: `BookCreatedEventHandler`)
   - **Migrations**: Gerencia as migrações do banco de dados
   Esta camada depende do Domain, mas o Domain não conhece a Infrastructure (inversão de dependência).

3. **Qual o papel da camada WebAPI?**
   A camada WebAPI é a camada de apresentação e ponto de entrada da aplicação:
   - **Controllers**: Expõe endpoints HTTP RESTful (ex: `BookController` com GET, POST, PUT, DELETE)
   - **DTOs (Data Transfer Objects)**: Define objetos para transferência de dados entre cliente e servidor (ex: `BookDto`)
   - **Configuração HTTP**: Configura CORS, serialização JSON, pipeline de requisições
   - **Orquestração**: Coordena as chamadas entre Domain e Infrastructure, transformando requisições HTTP em operações de domínio
   - **Validação de entrada**: Valida dados recebidos antes de passar para o domínio
   Esta camada depende tanto do Domain quanto da Infrastructure.

4. **Aponte um ponto de melhoria** que considere relevante para o projeto.
   **Separação de responsabilidades nos Controllers**: Atualmente, o `BookController` está fazendo muito mais do que deveria - ele está acessando diretamente o `SqlDbContext`, fazendo validações de negócio (verificação de título duplicado), e orquestrando a lógica. 
   
   **Sugestão de melhoria**: Implementar uma camada de **Application Services** ou usar **CQRS com MediatR** (que já está configurado no projeto) para separar as responsabilidades:
   - Criar **Commands** e **Queries** (ex: `CreateBookCommand`, `GetBooksQuery`)
   - Mover a lógica de negócio (como verificação de duplicatas) para **Handlers** ou **Application Services**
   - Os Controllers ficariam apenas recebendo requisições HTTP e delegando para os handlers
   - Isso facilitaria testes unitários, reutilização de código e manutenção
   - Exemplo: A verificação de título duplicado poderia ser uma regra de negócio no Domain ou uma validação em um Application Service, não no Controller

  **Permitir adicionar e excluir múltiplos livros**: Esta funcionalidade melhoraria significativamente a experiência do usuário e a eficiência operacional:
   
   - **Adicionar múltiplos livros**: Criar um endpoint `POST /Book/batch` que aceite um array de livros, permitindo importação em lote. Isso seria útil para:
     - Importação de catálogos de livros de arquivos CSV/Excel
     - Migração de dados de outros sistemas
     - Cadastro inicial de grandes volumes de livros
     - Implementação: Validar todos os livros antes de salvar, usar transação para garantir atomicidade (tudo ou nada), e retornar relatório detalhado de sucessos e falhas
   
   - **Excluir múltiplos livros**: Criar um endpoint `DELETE /Book/batch` que aceite um array de IDs, permitindo exclusão em lote. Benefícios:
     - Remoção rápida de múltiplos registros sem precisar fazer várias requisições
     - Melhor performance ao processar grandes volumes
     - Interface mais intuitiva com seleção múltipla no frontend
     - Implementação: Validar existência de todos os IDs antes de excluir, usar transação, e retornar status de cada exclusão
   
   - **Melhorias no Frontend**: 
     - Adicionar checkboxes para seleção múltipla na lista de livros
     - Botões "Excluir Selecionados" e "Adicionar Múltiplos" 
     - Modal para upload/cadastro em lote com validação visual
     - Feedback detalhado sobre operações em lote (quantos foram processados com sucesso/falha)
   
   Esta funcionalidade se alinha bem com o uso de **CQRS/MediatR** já configurado, onde poderiam ser criados `BatchCreateBooksCommand` e `BatchDeleteBooksCommand` com seus respectivos handlers, mantendo a separação de responsabilidades e facilitando testes.

*Powered by INSPAND Developers*