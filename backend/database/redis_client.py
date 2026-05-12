# Stub for Redis client
async def get_session(session_id: str):
    # Dummy session for now
    return {
        "history": [],
        "level": "B1",
        "scenario_state": "greeting"
    }

async def update_session(session_id: str, data: dict):
    pass
