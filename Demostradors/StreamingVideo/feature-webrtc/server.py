import asyncio
import json
import logging
from aiohttp import web
from aiortc import RTCPeerConnection, RTCSessionDescription
from aiortc.contrib.media import MediaRelay

logging.basicConfig(level=logging.INFO)

pcs_publishers = set()
pcs_viewers = set()

relay = MediaRelay()
relay_video_track = None


async def index(request):
    # Sirve el HTML que tienes al lado del server.py
    return web.FileResponse("receiver.html")


async def publish_offer(request):
    global relay_video_track

    params = await request.json()
    offer = RTCSessionDescription(sdp=params["sdp"], type=params["type"])

    pc = RTCPeerConnection()
    pcs_publishers.add(pc)

    @pc.on("track")
    async def on_track(track):
        global relay_video_track
        logging.info("Track recibido del publisher: %s", track.kind)
        if track.kind == "video":
            relay_video_track = relay.subscribe(track)

    await pc.setRemoteDescription(offer)
    answer = await pc.createAnswer()
    await pc.setLocalDescription(answer)

    return web.json_response({"sdp": pc.localDescription.sdp, "type": pc.localDescription.type})


async def viewer_offer(request):
    global relay_video_track

    if relay_video_track is None:
        return web.json_response(
            {"error": "No hay vídeo disponible todavía. Arranca primero el publisher."},
            status=503
        )

    params = await request.json()
    offer = RTCSessionDescription(sdp=params["sdp"], type=params["type"])

    pc = RTCPeerConnection()
    pcs_viewers.add(pc)

    pc.addTrack(relay_video_track)

    await pc.setRemoteDescription(offer)
    answer = await pc.createAnswer()
    await pc.setLocalDescription(answer)

    return web.json_response({"sdp": pc.localDescription.sdp, "type": pc.localDescription.type})


async def on_shutdown(app):
    await asyncio.gather(*[pc.close() for pc in pcs_publishers | pcs_viewers])
    pcs_publishers.clear()
    pcs_viewers.clear()


# ✅ AQUÍ se crea app ANTES de usar app.router
app = web.Application()
app.on_shutdown.append(on_shutdown)

app.router.add_get("/", index)
app.router.add_post("/publish_offer", publish_offer)
app.router.add_post("/viewer_offer", viewer_offer)

if __name__ == "__main__":
    web.run_app(app, port=8080)
