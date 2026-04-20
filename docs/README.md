# Automated-E-Commerce-Deployment-Platform

## Archticture 

```mermaid
flowchart TB
    subgraph Databases["Databases"]
        IdentityDB[("IdentityDB<br/>PostgreSQL")]
        CatalogDB[("CatalogDB<br/>PostgreSQL")]
        OrderingDB[("OrderingDB<br/>PostgreSQL")]
        Redis[("Redis<br/>Cache")]
    end

    subgraph MessageBroker["Message Broker"]
        RabbitMQ[("RabbitMQ<br/>Event Bus")]
    end

    subgraph Services["Microservices"]
        IdentityAPI["Identity API"]
        CatalogAPI["Catalog API"]
        BasketAPI["Basket API"]
        OrderingAPI["Ordering API"]
        OrderProcessor["Order Processor"]
        PaymentProcessor["Payment Processor"]
        WebApp["WebApp"]
    end

    %% Database connections
    IdentityAPI --> IdentityDB
    CatalogAPI --> CatalogDB
    OrderingAPI --> OrderingDB
    OrderProcessor --> OrderingDB
    BasketAPI --> Redis

    %% Message broker connections
    CatalogAPI -->|"publish events"| RabbitMQ
    BasketAPI -->|"publish events"| RabbitMQ
    OrderingAPI -->|"publish events"| RabbitMQ
    
    RabbitMQ -->|"consume events"| OrderProcessor
    RabbitMQ -->|"consume events"| PaymentProcessor

    %% Direct API calls
    WebApp --> IdentityAPI
    WebApp --> CatalogAPI
    WebApp --> BasketAPI
    WebApp --> OrderingAPI
    
    BasketAPI --> IdentityAPI
    OrderingAPI --> IdentityAPI
    
    OrderProcessor --> OrderingAPI

    %% Updated GitHub-Compatible Styling
    classDef database fill:#ddf4ff,stroke:#0969da,stroke-width:2px,color:#0550ae
    classDef messagebus fill:#fff8ec,stroke:#bf4b00,stroke-width:2px,color:#953800
    classDef service fill:#dafbe1,stroke:#1a7f37,stroke-width:2px,color:#116329
    classDef frontend fill:#fbefff,stroke:#8250df,stroke-width:2px,color:#6639ba
    
    class IdentityDB,CatalogDB,OrderingDB,Redis database
    class RabbitMQ messagebus
    class IdentityAPI,CatalogAPI,BasketAPI,OrderingAPI,OrderProcessor,PaymentProcessor service
    class WebApp frontend
```

## Build And Run 
### Run With Shell Variables
#### 1. export required varaiables
```bash
export postgres_password=your_password 
export rabbitmq_password=your_password
```

#### 2. run with docker compose
```bash
docker compose up -d
```

### Or Use `.env` File
#### 1. create `.env` file 
`.env` example
```
postgres_password=your_password
rabbitmq_password=your_password
```

#### 2. run the project in docker
```bash
docker compose --env-file .env up -d 
```