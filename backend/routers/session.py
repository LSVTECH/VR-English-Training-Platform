from fastapi import APIRouter

router = APIRouter()

@router.post("/session/init")
async def init_session():
    return {"session_id": "test-session", "ai_greeting": "Welcome to the Grand Palace Hotel!"}
