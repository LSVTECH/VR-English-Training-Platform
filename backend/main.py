from fastapi import FastAPI 
from fastapi.middleware.cors import CORSMiddleware 
from fastapi.responses import HTMLResponse
from fastapi.staticfiles import StaticFiles
from dotenv import load_dotenv
import os

load_dotenv()

from routers import conversation, session, health, demo 
 
app = FastAPI( 
    title="VR Training Platform API", 
    version="1.0.0" 
) 
 
app.add_middleware( 
    CORSMiddleware, 
    allow_origins=["*"], 
    allow_methods=["*"], 
    allow_headers=["*"], 
) 
 
app.include_router(health.router) 
app.include_router(session.router, prefix="/api/v1") 
app.include_router(conversation.router, prefix="/api/v1") 
app.include_router(demo.router)
os.makedirs("static/audio", exist_ok=True)
app.mount("/static", StaticFiles(directory="static"), name="static")

@app.get("/test", response_class=HTMLResponse)
async def test_page():
    with open("test.html", "r", encoding="utf-8") as f:
        return f.read()

@app.get("/api/status")
@app.get("/api/v1/status")
async def connection_status():
    return {"status": "ok", "message": "Connected to VR Training Backend"}
