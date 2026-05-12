from openai import AsyncOpenAI
from scenario.prompts import build_system_prompt
import os

client = AsyncOpenAI()
MOCK_MODE = os.getenv("MOCK_MODE", "false").lower() == "true"

async def get_ai_response(history: list, level: str, state: str):
    system_prompt = build_system_prompt("hotel_checkin", level, state)
    messages = [{"role": "system", "content": system_prompt}] + history
    
    if MOCK_MODE:
        # Respuesta fija para pruebas
        return "This is a mock AI response for testing purposes."
    
    # Código real (mantener por si se desactiva mock)
    response = await client.chat.completions.create(
        model="gpt-4o-mini",
        messages=messages,
        max_tokens=80
    )
    return response.choices[0].message.content
