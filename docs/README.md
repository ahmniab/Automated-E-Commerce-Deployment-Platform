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

    %% Message broker connections (event-driven communication)
    CatalogAPI -->|"publish events"| RabbitMQ
    BasketAPI -->|"publish events"| RabbitMQ
    OrderingAPI -->|"publish events"| RabbitMQ
    
    RabbitMQ -->|"consume events"| OrderProcessor
    RabbitMQ -->|"consume events"| PaymentProcessor

    %% Direct API calls (sync communication)
    WebApp --> IdentityAPI
    WebApp --> CatalogAPI
    WebApp --> BasketAPI
    WebApp --> OrderingAPI
    
    BasketAPI --> IdentityAPI
    OrderingAPI --> IdentityAPI
    
    OrderProcessor --> OrderingAPI

    %% Styling
    classDef database fill:#e1f5fe,stroke:#01579b,stroke-width:2px
    classDef messagebus fill:#fff3e0,stroke:#e65100,stroke-width:2px
    classDef service fill:#e8f5e9,stroke:#2e7d32,stroke-width:2px
    classDef frontend fill:#f3e5f5,stroke:#6a1b9a,stroke-width:2px
    
    class IdentityDB,CatalogDB,OrderingDB,Redis database
    class RabbitMQ messagebus
    class IdentityAPI,CatalogAPI,BasketAPI,OrderingAPI,OrderProcessor,PaymentProcessor service
    class WebApp frontend
```

## Build And Run 
### 1. create `.env` file 
`.env` example
```
postgres_password=your_password
rabbitmq_password=your_password
```

### 2. run the project in docker
```bash
docker compose --env-file .env up -d 
```