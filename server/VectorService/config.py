from pydantic_settings import BaseSettings, SettingsConfigDict
from typing import Optional


class Settings(BaseSettings):
    model_config = SettingsConfigDict(
        env_file=".env", 
        env_file_encoding="utf-8",
        extra="ignore"  # Ignore extra fields like DATABASE_URL, RABBITMQ_URL from .env
    )

    # ML Model (required)
    model_name: str = "all-MiniLM-L6-v2"

    # Service
    service_name: str = "VectorService"
    log_level: str = "INFO"


settings = Settings()
