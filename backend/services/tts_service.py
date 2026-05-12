from openai import AsyncOpenAI
import os
MOCK_MODE = os.getenv("MOCK_MODE", "false").lower() == "true"

client = AsyncOpenAI()

async def text_to_speech(text: str, session_id: str):
    os.makedirs("static/audio", exist_ok=True)
    if MOCK_MODE:
        mock_path = "static/audio/mock.mp3"
        if not os.path.isfile(mock_path):
            open(mock_path, "ab").close()
        return "/static/audio/mock.mp3"
    filepath = f"static/audio/{session_id}.mp3"
    
    response = await client.audio.speech.create(
        model="tts-1",
        voice="alloy",
        input=text
    )
    
    response.stream_to_file(filepath)
    return f"/static/audio/{session_id}.mp3"

async def text_to_speech_stream(text: str, session_id: str, ws_manager): 
    async with client.audio.speech.with_streaming_response.create( 
        model="tts-1", 
        voice="alloy", 
        input=text, 
        response_format="pcm", 
    ) as response: 
        async for chunk in response.iter_bytes(chunk_size=4096): 
            await ws_manager.send_audio_chunk(session_id, chunk) 
    await ws_manager.send_audio_complete(session_id) 
