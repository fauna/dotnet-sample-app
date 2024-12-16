# Fauna .NET sample app

## Overview

This sample app shows how to use [Fauna](https://fauna.com) in a production application.

The app uses .NET and the [Fauna v10 .NET
driver](https://github.com/fauna/fauna-dotnet) to create HTTP API endpoints for an
e-commerce store. You can use the app's API endpoints to manage products,
customers, and orders for the store.

The app uses Fauna schemas and queries to:

- Read and write data with strong consistency.

- Define and handle relationships between resources, such as linking orders
  to products and customers.

- Validate data changes against business logic.

The app's source code includes comments that highlight Fauna best practices.


## Highlights

The sample app uses the following Fauna features:

- **[Document type
  enforcement](https://docs.fauna.com/fauna/current/learn/schema/#type-enforcement):**
  Collection schemas enforce a structure for the app's documents. Fauna rejects
  document writes that don't conform to the schema, ensuring data consistency.
  [Zero-downtime
  migrations](https://docs.fauna.com/fauna/current/learn/schema/#schema-migrations)
  let you safely change the schemas at any time.

- **[Relationships](https://docs.fauna.com/fauna/current/learn/query/relationships/):**
  Normalized references link documents across collections. The app's queries use
  [projection](https://docs.fauna.com/fauna/current/reference/fql/projection/)
  to dynamically retrieve linked documents, even when deeply nested. No complex
  joins, aggregations, or duplication needed.

- **[Computed
  fields](https://docs.fauna.com/fauna/current/learn/schema/#computed-fields):**
  Computed fields dynamically calculate their values at query time. For example,
  each customer's `orders` field uses a query to fetch a set of filtered orders.
  Similarly, each order's `total` is calculated at query time based on linked
  product prices and quantity.

- **[Constraints](https://docs.fauna.com/fauna/current/learn/schema/#unique-constraints):**
  The app uses constraints to ensure field values are valid. For example, the
  app uses unique constraints to ensure each customer has a unique email address
  and each product has a unique name. Similarly, check constraints ensure each
  customer has only one cart at a time and that product prices are not negative.

- **[User-defined functions
  (UDFs)](https://docs.fauna.com/fauna/current/learn/data-model/user-defined-functions/):**
  The app uses UDFs to store business logic as reusable queries. For example,
  the app uses a `checkout()` UDF to process order updates. `checkout()` calls
  another UDF, `validateOrderStatusTransition()`, to validate `status`
  transitions for orders.


## Requirements

To run the app, you'll need:

- A [Fauna account](https://dashboard.fauna.com/register). You can sign up for a
  free account at https://dashboard.fauna.com/register.

- .NET 8.0 or later.

- [Fauna CLI](https://docs.fauna.com/fauna/current/tools/shell/) 4.0.0-beta or later.
    - [Node.js](https://nodejs.org/en/download/) v20.x or later.

  To install the CLI, run:

    ```sh
    npm install -g fauna-shell@">=4.0.0-beta"
    ```

## Setup

1. Clone the repo and navigate to the `dotnet-sample-app` directory:

    ```sh
    git clone git@github.com:fauna/dotnet-sample-app.git
    cd dotnet-sample-app
    ```

2. If you haven't already, log in to Fauna using the Fauna CLI:

    ```sh
    fauna login
    ```

3. Use the Fauna CLI to create the `EcommerceDotnet` database:

    ```sh
    # Replace 'us' with your preferred Region Group:
    # 'us' (United States), 'eu' (Europe), or `global`.
    fauna database create \
      --name EcommerceDotnet \
      --database us
    ```

4.  Push the `.fsl` files in the `schema` directory to the `EcommerceDotnet`
    database:

    ```sh
    # Replace 'us' with your Region Group identifier.
    fauna schema push \
      --database us/EcommerceDotnet \
      --dir ./schema
    ```

    When prompted, accept and stage the schema.

5.  Check the status of the staged schema:

    ```sh
    fauna schema status \
      --database us/EcommerceDotnet
    ```

6.  When the status is `ready`, commit the staged schema to the database:

    ```sh
    fauna schema commit \
      --database us/EcommerceDotnet
    ```

    The commit applies the staged schema to the database. The commit creates the
    collections and user-defined functions (UDFs) defined in the `.fsl` files of the
    `schema` directory.

7. Create a key with the `server` role for the `EcommerceDotnet` database:

    ```sh
    fauna query "Key.create({ role: 'server' })" \
      --database us/EcommerceDotnet
    ```

    Copy the returned `secret`. The app can use the key's secret to authenticate
    requests to the database.

8.  Make a copy of the `.env.example` file and name the copy `.env`. For example:

    ```sh
    cp .env.example .env
    ```

9.  In `.env`, set the `FAUNA_SECRET` environment variable to the secret you
    copied earlier:

    ```
    ...
    FAUNA_SECRET=fn...
    ...
    ```


## Run the app

The app runs an HTTP API server. From the `DotNetSampleApp` directory, run:

```sh
export $(grep -v '^#' .env | xargs) && \
FAUNA_SECRET=$FAUNA_SECRET dotnet run
```

Once started, the local server is available at http://localhost:5049.

### Docker

You can also run the app in a Docker container. From the root directory, run:

```sh
docker build -t dotnet-sample-app .
export $(grep -v '^#' .env | xargs) && \
docker run -p 5049:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e FAUNA_SECRET=$FAUNA_SECRET \
  dotnet-sample-app
```

Once started, the local server is available at http://localhost:5049.

## Sample data

The app includes seed data that's populated when you make a successful request to any API endpoint.

## HTTP API endpoints

The app's HTTP API endpoints are defined in the `sample-app/Controllers` directory.

An OpenAPI spec and Swagger UI docs for the endpoints are available at:

* OpenAPI spec: http://localhost:5049/swagger/v1/swagger.json
* Swagger UI: http://localhost:5049/swagger/index.html

### Make API requests

You can use the endpoints to make API requests that read and write data from
the `EcommerceDotnet` database.

For example, with the local server running in a separate terminal tab, run the
following curl request to the `POST /products` endpoint. The request creates a
`Product` collection document in the `EcommerceDotnet` database.

```sh
curl -v \
  http://localhost:5049/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "The Old Man and the Sea",
    "price": 899,
    "description": "A book by Ernest Hemingway",
    "stock": 10,
    "category": "books"
  }' | jq .
```

## Expand the app

You can further expand the app by adding fields and endpoints.

As an example, the following steps adds a computed `totalPurchaseAmt` field to
Customer documents and related API responses:

1. If the app server is running, stop the server by pressing Ctrl+C.

2. In `schema/collections.fsl`, add the following `totalPurchaseAmt` computed
  field definition to the `Customer` collection:

    ```diff
    collection Customer {
      ...
      // Use a computed field to get the set of Orders for a customer.
      compute orders: Set<Order> = (customer => Order.byCustomer(customer))

    + // Use a computed field to calculate the customer's cumulative purchase total.
    + // The field sums purchase `total` values from the customer's linked Order documents.
    + compute totalPurchaseAmt: Number = (customer => customer.orders.fold(0, (sum, order) => {
    +   let order: Any = order
    +   sum + order.total
    + }))
      ...
    }
    ...
    ```

    Save `schema/collections.fsl`.

3. In `sample-app/Controllers/QuerySnippets.cs`, add the `totalPurchaseAmt` field to the
   `CustomerResponse` method's projection:

    ```diff
    ...
    customer {
        id,
        name,
        email,
    +   address,
    +   totalPurchaseAmt
    }
    ...
    ```

4.  Push the updated schema to the `EcommerceDotnet` database:

    ```sh
    fauna schema push \
      --database us/EcommerceDotnet \
      --role admin \
      --dir ./schema
    ```

    When prompted, accept and stage the schema.

5.  Check the status of the staged schema:

    ```sh
    fauna schema status \
      --database us/EcommerceDotnet
    ```

6.  When the status is `ready`, commit the staged schema changes to the
    database:

    ```sh
    fauna schema commit \
      --database us/EcommerceDotnet
    ```

7. In `sample-app/Models/Customer.cs`, add the
   `totalPurchaseAmt` field to the `Customer` class:

    ```diff
    public class Customer
    {
        /// <summary>
        /// Document ID
        /// </summary>
        [Id]
        public string? Id { get; init; }

        ...

        /// <summary>
        /// Address
        /// </summary>
        [Field]
        public required Address Address { get; init; }

    +   /// <summary>
    +   /// Total Purchase Amount
    +   /// </summary>
    +   [Field]
    +   public required int TotalPurchaseAmt { get; init; }
    }
    ```

    Save `sample-app/Models/Customer.cs`.

   Customer-related endpoints use this template to project Customer
   document fields in responses.

8. Start the app server:

    ```sh
    export $(grep -v '^#' .env | xargs) && \
    FAUNA_SECRET=$FAUNA_SECRET dotnet run
    ```

    If using Docker, run:

    ```sh
    docker build -t dotnet-sample-app .
    export $(grep -v '^#' .env | xargs) && \
    docker run -p 5049:8080 \
      -e ASPNETCORE_ENVIRONMENT=Development \
      -e FAUNA_SECRET=$FAUNA_SECRET \
      dotnet-sample-app
    ```

9. With the local server running in a separate terminal tab, run the
   following curl request to the `POST /customers` endpoint:

    ```sh
    curl -v http://localhost:5049/customers/999 | jq .
    ```

    The response includes the computed `totalPurchaseAmt` field:

    ```json
    {
      "id": "999",
      "name": "Valued Customer",
      "email": "fake@fauna.com",
      "address": {
        "street": "123 Main St",
        "city": "San Francisco",
        "state": "CA",
        "postalCode": "12345",
        "country": "United States"
      },
      "totalPurchaseAmt": 36000
    }
    ```
## Development

### Local Testing
1. Install the v3 version of the Fauna CLI: `npm install -g fauna-shell@3`
2. Start Fauna in a container: `docker run --rm --name fauna -p 8443:8443 -p 8084:8084 fauna/faunadb`
3. Configure the schema: `./setup-local.sh`
4. Run tests: `dotnet test`

### dotnet run with local Fauna
1. Start Fauna in a container: `docker run --rm --name fauna -p 8443:8443 -p 8084:8084 fauna/faunadb`
2. Configure the schema: `./setup-local.sh`
2. Copy the secret returned from running `./setup-local.sh`
2. `cd DotNetSampleApp`
3. `FAUNA_SECRET="<SECRET>" FAUNA_ENDPOINT="http://localhost:8443" dotnet run`
