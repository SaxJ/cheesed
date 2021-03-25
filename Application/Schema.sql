-- Your database schema. Use the Schema Designer at http://localhost:8001/ to add some tables.
CREATE TABLE cheeses (
    id UUID DEFAULT uuid_generate_v4() PRIMARY KEY NOT NULL,
    uid TEXT NOT NULL UNIQUE,
    count INT DEFAULT 0 NOT NULL,
    display_image TEXT DEFAULT NULL,
    name TEXT NOT NULL
);
