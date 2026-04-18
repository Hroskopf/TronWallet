CREATE TABLE users (
    id            UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    email         VARCHAR(255) NOT NULL UNIQUE,
    username      VARCHAR(100) NOT NULL UNIQUE,
    password_hash TEXT         NOT NULL,   -- BCrypt hash
    created_at    TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at    TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    is_active     BOOLEAN      NOT NULL DEFAULT TRUE
);

CREATE TABLE wallets (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id         UUID        NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    tron_address    VARCHAR(64) NOT NULL UNIQUE,  -- Base58Check, починається з 'T'
    private_key_enc TEXT        NOT NULL,          -- AES-256-GCM зашифрований hex private key
    public_key      TEXT        NOT NULL,           -- hex public key
    network         VARCHAR(20) NOT NULL DEFAULT 'shasta',
    is_primary      BOOLEAN     NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_wallets_user_id      ON wallets(user_id);
CREATE INDEX idx_wallets_tron_address ON wallets(tron_address);

CREATE TABLE transactions (
    id            UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    wallet_id     UUID         NOT NULL REFERENCES wallets(id),
    tx_hash       VARCHAR(128) UNIQUE,
    direction     VARCHAR(4)   NOT NULL CHECK (direction IN ('IN','OUT')),
    from_address  VARCHAR(64)  NOT NULL,
    to_address    VARCHAR(64)  NOT NULL,
    amount_sun    BIGINT       NOT NULL,    -- 1 TRX = 1_000_000 SUN
    fee_sun       BIGINT       NOT NULL DEFAULT 0,
    status        VARCHAR(20)  NOT NULL DEFAULT 'PENDING'
                               CHECK (status IN ('PENDING','BROADCASTING','CONFIRMED','FAILED')),
    block_number  BIGINT,
    block_time    TIMESTAMPTZ,
    raw_data      JSONB,                   -- повна відповідь з TronGrid
    created_at    TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    confirmed_at  TIMESTAMPTZ
);

CREATE INDEX idx_tx_from ON transactions(from_address);
CREATE INDEX idx_tx_to ON transactions(to_address);
CREATE INDEX idx_tx_created ON transactions(created_at DESC);
CREATE INDEX idx_transactions_tx_hash   ON transactions(tx_hash);
CREATE INDEX idx_transactions_status    ON transactions(status) WHERE status = 'PENDING';

CREATE TABLE refresh_tokens (
    id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id     UUID        NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token_hash  TEXT        NOT NULL UNIQUE,  -- SHA-256 хеш токена, не сам токен
    expires_at  TIMESTAMPTZ NOT NULL,
    revoked_at  TIMESTAMPTZ,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    ip_address  VARCHAR(45),
    user_agent  TEXT
);

CREATE INDEX idx_refresh_tokens_user_id ON refresh_tokens(user_id);