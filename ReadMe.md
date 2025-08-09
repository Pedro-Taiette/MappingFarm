# 🌾 Farm ERP — Módulo de Desenho e Armazenamento de Áreas (Mock)

Este projeto demonstra como implementar a **funcionalidade de desenhar áreas geográficas** (polígonos) em um mapa e salvá-las no backend usando **.NET 8 + Entity Framework Core + NetTopologySuite** com armazenamento no SQL Server (`geography`, SRID 4326).

A solução inclui:

- **Backend**: API REST em .NET 8 com EF Core e suporte a tipos espaciais.
- **Frontend**: Mapa interativo com Leaflet + OpenStreetMap (gratuito), permitindo desenhar, editar e salvar polígonos.

> Este mock pode ser incorporado futuramente como um módulo/feature em um ERP rural ou sistema GIS.

---

## 📂 Estrutura do projeto

```
farm-erp-polygon/
├── backend/
│   └── FarmErp.Api/
│       ├── Controllers/         # Controllers da API
│       ├── Data/                # DbContext e configuração EF Core
│       ├── Dtos/                # DTOs para transporte de dados
│       ├── Models/              # Entidades do domínio
│       ├── Program.cs           # Configuração da aplicação
│       ├── appsettings.*.json   # Configurações (connection strings etc.)
│       └── Dockerfile           # Build container da API
└── frontend/
    ├── css/                     # Estilos do mapa e UI
    ├── js/                      # Lógica do mapa (Leaflet + API)
    └── index.html               # Página principal do mock
```

---

## 🛠 Backend — FarmErp.Api

### **Stack**
- .NET 8 Web API
- Entity Framework Core + SQL Server
- NetTopologySuite (tipos espaciais)
- CORS liberado (para dev)
- Swagger para documentação

### **Principais Endpoints**
| Método  | Rota                | Descrição |
|---------|---------------------|-----------|
| POST    | `/api/farms`        | Cria uma fazenda com polígono |
| GET     | `/api/farms`        | Lista todas as fazendas (id e nome) |
| GET     | `/api/farms/{id}`   | Retorna uma fazenda com suas coordenadas |
| PUT     | `/api/farms/{id}`   | Atualiza nome/polígono de uma fazenda |
| DELETE  | `/api/farms/{id}`   | Remove uma fazenda |

**Formato do payload para criação/atualização:**
```json
{
  "name": "Fazenda Teste",
  "coordinates": [
    { "lat": -15.0, "lng": -47.0 },
    { "lat": -15.1, "lng": -47.1 },
    { "lat": -15.2, "lng": -47.0 }
  ]
}
```

---

### **Configuração e execução**
1. **Instalar dependências**
   ```bash
   dotnet restore
   ```

2. **Configurar conexão**
   - Editar `appsettings.Development.json`:
     ```json
     {
       "ConnectionStrings": {
         "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FarmErpDb;Trusted_Connection=True;"
       }
     }
     ```

3. **Gerar e aplicar migrations**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. **Rodar API**
   ```bash
   dotnet run --urls "http://localhost:5272"
   ```
   Acesse o Swagger em: `http://localhost:5272/swagger`

---

## 🗺 Frontend — Leaflet + OpenStreetMap

### **Stack**
- Leaflet.js (mapa interativo)
- Leaflet.draw (ferramentas de desenho/edição)
- OpenStreetMap (tiles gratuitos)
- Turf.js (cálculo de área e perímetro)
- HTML + CSS + JS puro (sem build step)

### **Funcionalidades**
- Desenhar e editar polígonos no mapa.
- Calcular área e perímetro geodésicos.
- Salvar polígono na API.
- Listar fazendas e carregar ao clicar.
- Exportar/importar polígonos em formato GeoJSON.

---

### **Uso do frontend**
1. **Servir o frontend** (ou abrir o HTML direto no navegador)
   - Com Node.js:
     ```bash
     npx http-server frontend
     ```
     ou
     ```bash
     npx live-server frontend
     ```
2. **Configurar API base**
   - No topo da tela, digitar a URL da API (ex.: `http://localhost:5272`) e clicar **Salvar**.
3. **Fluxo básico**
   - Clique **Desenhar** → trace o polígono → clique em **Salvar na API**.
   - Clique **Listar fazendas** → selecione uma para carregar no mapa.

---

## 🧩 Integração futura

Quando for integrar este mock a um projeto real:
- **Autenticação/Autorização**: vincular fazendas a usuários/tenants.
- **Validação de polígonos**: verificar interseções e duplicidades.
- **Consultas espaciais**: filtros por proximidade ou interseção.
- **Exportação**: suporte a Shapefile, KML, GPX.
- **Frontend**: unificar estilo com o design system do sistema principal.

---

## 🚀 Executar tudo junto (Docker)
Na pasta `backend/FarmErp.Api`:
```bash
docker build -t farm-api .
docker run -p 8080:8080 farm-api
```
No frontend, aponte para `http://localhost:8080` como API base.

---

## 📜 Licença
Este mock é para uso interno/demonstrativo.  
Atribuição obrigatória ao [OpenStreetMap](https://www.openstreetmap.org/copyright) quando exibindo tiles.
