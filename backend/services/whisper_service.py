from openai import AsyncOpenAI
import io
import os

# Detect mock mode
MOCK_MODE = os.getenv("MOCK_MODE", "false").lower() == "true"

client = AsyncOpenAI()

async def transcribe_audio(audio_bytes: bytes):
    if MOCK_MODE:
        return "mock transcription of the provided audio"
    audio_file = io.BytesIO(audio_bytes)
    audio_file.name = "audio.wav"
    response = await client.audio.transcriptions.create(
        model="whisper-1",
        file=audio_file
    )
    return response.text
