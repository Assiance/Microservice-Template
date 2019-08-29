CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

                CREATE TABLE IF NOT EXISTS products (
                    id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
                    name varchar(256) NOT NULL,
                    price NUMERIC NOT NULL,
                    quantity integer NOT NULL,
                    created_date timestamp with time zone NOT NULL,
                    modified_date timestamp with time zone,
                    created_by varchar(256) NOT NULL,
                    modified_by varchar(256)
                ); 
			    
			    CREATE TABLE IF NOT EXISTS orders (
                    id serial PRIMARY KEY,
                    product_id uuid references products(id),
                    quantity integer NOT NULL,
                    created_date timestamp with time zone NOT NULL,
                    modified_date timestamp with time zone,
                    created_by varchar(256) NOT NULL,
                    modified_by varchar(256)
                );
			    
			    CREATE INDEX IF NOT EXISTS idx_orders_product_id
                    ON orders(product_id);