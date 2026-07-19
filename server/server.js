const express = require('express');
const http    = require('http');
const { Server } = require('socket.io');
const path    = require('path');
const os      = require('os');

const app    = express();
const server = http.createServer(app);
const io     = new Server(server, { cors: { origin: '*' } });

app.use(express.static(path.join(__dirname, '../demo')));
app.get('/', (_, res) => res.sendFile(path.join(__dirname, '../demo/index.html')));

// ════════════════════════════════════════
//  CONSTANTS
// ════════════════════════════════════════
const DEFAULT_MAP = 3500;
const MAX_FOOD    = 600;
const MAX_BOTS    = 12;

const AI_NAMES = ['SnakeKing','WormPro','CoilMaster','SlitherX','ZoneBoss','HissKing','BoomWorm','NeonSnek','GigaWorm','VenomBoss','TitanWorm','MegaSlither'];

// ════════════════════════════════════════
//  GLOBAL STATE
// ════════════════════════════════════════
const rooms   = {};   // { roomCode: RoomState }
const players = {};   // { socketId: { roomCode, name, skin } }

function rnd(a,b){ return a + Math.random()*(b-a); }

// Initialize Global Public Room
function getOrCreateGlobalRoom(){
  if(!rooms['GLOBAL']){
    rooms['GLOBAL'] = makeRoom('GLOBAL', true, DEFAULT_MAP);
  }
  return rooms['GLOBAL'];
}

// ════════════════════════════════════════
//  ROOM FACTORY
// ════════════════════════════════════════
function makeRoom(code, isPublic = true, mapSize = DEFAULT_MAP){
  const foods    = [];
  const powerups = [];
  const bots     = {};

  for(let i = 0; i < MAX_FOOD; i++) foods.push(makeFood(null, null, null, mapSize));

  const room = {
    code,
    isPublic,
    mapSize,
    players: {},          // { socketId: playerState }
    bots,                 // { botId: botState }
    foods,
    powerups,
    createdAt: Date.now(),
  };

  // Seed AI bots
  for(let i = 0; i < MAX_BOTS; i++){
    spawnBot(room, i);
  }

  return room;
}

function makeFood(x, y, val, mapSize = DEFAULT_MAP){
  return {
    id:  Math.random().toString(36).slice(2),
    x:   x  ?? rnd(50, mapSize-50),
    y:   y  ?? rnd(50, mapSize-50),
    val: val ?? (Math.random()<0.06?45 : Math.random()<0.2?20 : 8),
  };
}

function makePowerup(x, y, mapSize = DEFAULT_MAP){
  const TYPES = ['speed','magnet','shield','ghost'];
  return {
    id:   Math.random().toString(36).slice(2),
    x:    x ?? rnd(100, mapSize-100),
    y:    y ?? rnd(100, mapSize-100),
    type: TYPES[Math.floor(Math.random()*TYPES.length)],
  };
}

function genCode(){
  const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
  let c = '';
  for(let i=0;i<4;i++) c += chars[Math.floor(Math.random()*chars.length)];
  return (rooms[c] || c === 'GLOBAL') ? genCode() : c;
}

// ════════════════════════════════════════
//  SERVER-SIDE ADVANCED BOTS
// ════════════════════════════════════════
function spawnBot(room, index){
  const id = 'bot_' + Math.random().toString(36).slice(2,7);
  const name = AI_NAMES[index % AI_NAMES.length] || ('Bot' + index);
  const skin = Math.floor(Math.random()*12);
  const x = rnd(200, room.mapSize-200);
  const y = rnd(200, room.mapSize-200);
  const angle = Math.random() * Math.PI * 2;
  const initialSize = 15 + Math.floor(Math.random()*30); // Bots start decently big!

  const segs = [];
  for(let i = 0; i < initialSize; i++){
    segs.push({ x: x - Math.cos(angle)*i*11, y: y - Math.sin(angle)*i*11 });
  }

  room.bots[id] = {
    id, name, skin, isBot: true,
    x, y, angle,
    score: initialSize * 15,
    size: initialSize,
    segs,
    alive: true,
    target: null,
    aiTimer: 0,
    turnRate: 0.04 + Math.random()*0.03,
  };
}

// AI Bot Loop
setInterval(()=>{
  const dt = 0.1;
  Object.values(rooms).forEach(room => {
    Object.values(room.bots).forEach(bot => {
      if(!bot.alive) return;

      bot.aiTimer -= dt;
      if(bot.aiTimer <= 0){
        bot.aiTimer = 0.5 + Math.random()*1.0;
        // Find nearest food or player
        let best = null, bd = 99999;
        room.foods.forEach((f, idx)=>{
          if(idx % 4 === 0){
            const d = Math.hypot(f.x - bot.x, f.y - bot.y);
            if(d < bd){ bd = d; best = f; }
          }
        });
        bot.target = best;
      }

      // Steering
      const cx = room.mapSize / 2, cy = room.mapSize / 2;
      let ta = bot.angle;
      const margin = 150;
      if(bot.x < margin || bot.x > room.mapSize - margin || bot.y < margin || bot.y > room.mapSize - margin){
        ta = Math.atan2(cy - bot.y, cx - bot.x);
      } else if(bot.target){
        ta = Math.atan2(bot.target.y - bot.y, bot.target.x - bot.x);
      }

      let da = ta - bot.angle;
      while(da > Math.PI) da -= Math.PI*2;
      while(da < -Math.PI) da += Math.PI*2;
      bot.angle += Math.sign(da) * Math.min(Math.abs(da), bot.turnRate);

      // Move bot (speed 2.6)
      const spd = 2.6;
      bot.x = Math.max(30, Math.min(room.mapSize-30, bot.x + Math.cos(bot.angle)*spd));
      bot.y = Math.max(30, Math.min(room.mapSize-30, bot.y + Math.sin(bot.angle)*spd));

      bot.segs.unshift({ x: bot.x, y: bot.y });
      bot.segs.pop();

      // Bot eats food & grows!
      for(let i = room.foods.length - 1; i >= 0; i--){
        const f = room.foods[i];
        if(Math.hypot(f.x - bot.x, f.y - bot.y) < 15 + bot.size*0.04){
          room.foods.splice(i, 1);
          bot.score += f.val;
          if(Math.random() < 0.35){
            const last = bot.segs[bot.segs.length-1];
            bot.segs.push({ x: last.x, y: last.y });
            bot.size = bot.segs.length;
          }
          io.to(room.code).emit('food_removed', f.id);
          break;
        }
      }

      // Broadcast bot movement to players in room
      io.to(room.code).emit('player_moved', {
        id: bot.id,
        x: bot.x, y: bot.y, angle: bot.angle,
        score: bot.score, size: bot.size,
        boosting: false,
        segs: bot.segs.filter((_, idx) => idx % 2 === 0).slice(0, 40),
      });
    });
  });
}, 100);

// ════════════════════════════════════════
//  PERIODIC TASKS
// ════════════════════════════════════════

// Replenish food in each room
setInterval(()=>{
  Object.values(rooms).forEach(room => {
    const need = MAX_FOOD - room.foods.length;
    for(let i = 0; i < Math.min(need, 20); i++){
      const f = makeFood(null, null, null, room.mapSize);
      room.foods.push(f);
      io.to(room.code).emit('food_spawn', f);
    }
  });
}, 500);

// Spawn power-ups
setInterval(()=>{
  Object.values(rooms).forEach(room => {
    if(room.powerups.length < 12){
      const pu = makePowerup(null, null, room.mapSize);
      room.powerups.push(pu);
      io.to(room.code).emit('powerup_spawn', pu);
    }
  });
}, 6000);

// Leaderboard broadcast
setInterval(()=>{
  Object.values(rooms).forEach(room => {
    const allParticipants = [
      ...Object.values(room.players),
      ...Object.values(room.bots)
    ];
    const lb = allParticipants
      .filter(p => p.alive)
      .map(p => ({ id: p.id, name: p.name, skin: p.skin, score: p.score, size: p.size }))
      .sort((a,b) => b.score - a.score)
      .slice(0, 10);
    if(lb.length > 0) io.to(room.code).emit('leaderboard', lb);
  });
}, 1500);

// Ensure Global room exists
getOrCreateGlobalRoom();

// ════════════════════════════════════════
//  SOCKET EVENTS
// ════════════════════════════════════════
io.on('connection', (socket)=>{
  console.log(`+ connect ${socket.id}`);

  // ── Get room list ─────────────────────
  socket.on('get_rooms', ()=>{
    const list = Object.values(rooms)
      .filter(r => r.isPublic)
      .map(r => ({
        code:    r.code,
        players: Object.keys(r.players).length + Object.keys(r.bots).length,
        max:     25,
      }));
    socket.emit('room_list', list);
  });

  // ── Quick play (join global or public room) ──
  socket.on('quick_play', (data)=>{
    const globalRoom = getOrCreateGlobalRoom();
    joinRoom(socket, globalRoom.code, data);
  });

  // ── Create room ───────────────────────
  socket.on('create_room', (data)=>{
    const code = genCode();
    const mapSize = data.mapSize || DEFAULT_MAP;
    rooms[code] = makeRoom(code, data.isPublic ?? true, mapSize);
    console.log(`Room created: ${code} by ${data.name}`);
    joinRoom(socket, code, data);
  });

  // ── Join room by code ─────────────────
  socket.on('join_room', (data)=>{
    const code = (data.code||'GLOBAL').toUpperCase().trim();
    if(!rooms[code]){
      socket.emit('room_error', 'Room "'+code+'" nahi mila! Default room join kar rahe hain.');
      joinRoom(socket, 'GLOBAL', data);
      return;
    }
    joinRoom(socket, code, data);
  });

  // ── Player update (movement) ──────────
  socket.on('player_update', (data)=>{
    const pd = players[socket.id]; if(!pd) return;
    const room = rooms[pd.roomCode]; if(!room || !room.players[socket.id]?.alive) return;
    const p = room.players[socket.id];
    p.x=data.x; p.y=data.y; p.angle=data.angle;
    p.score=data.score; p.size=data.size;
    socket.to(pd.roomCode).emit('player_moved',{
      id:socket.id, x:data.x, y:data.y, angle:data.angle,
      score:data.score, size:data.size,
      boosting:data.boosting, segs:data.segs,
    });
  });

  // ── Food eaten ────────────────────────
  socket.on('food_eaten', (foodId)=>{
    const pd = players[socket.id]; if(!pd) return;
    const room = rooms[pd.roomCode]; if(!room) return;
    const idx = room.foods.findIndex(f=>f.id===foodId);
    if(idx===-1) return;
    room.foods.splice(idx,1);
    io.to(pd.roomCode).emit('food_removed', foodId);
  });

  // ── Power-up collected ────────────────
  socket.on('powerup_collected', (puId)=>{
    const pd = players[socket.id]; if(!pd) return;
    const room = rooms[pd.roomCode]; if(!room) return;
    const idx = room.powerups.findIndex(p=>p.id===puId);
    if(idx===-1) return;
    const pu = room.powerups.splice(idx,1)[0];
    io.to(pd.roomCode).emit('powerup_removed', puId);
    socket.emit('powerup_apply', pu.type);
  });

  // ── Player died ───────────────────────
  socket.on('player_died', (data)=>{
    const pd = players[socket.id]; if(!pd) return;
    const room = rooms[pd.roomCode]; if(!room) return;
    const p = room.players[socket.id];
    if(p) p.alive = false;
    const newFoods = [];
    (data.segs||[]).forEach((seg,i)=>{
      if(i%2===0){
        const f = makeFood(seg.x, seg.y, 12+Math.floor(Math.random()*25), room.mapSize);
        room.foods.push(f); newFoods.push(f);
      }
    });
    io.to(pd.roomCode).emit('player_dead',{ id:socket.id, newFoods });
    const pName = room.players[socket.id]?.name||'Someone';
    io.to(pd.roomCode).emit('kill_feed', `💀 ${pName} eliminated!`);
  });

  // ── Ping ─────────────────────────────
  socket.on('ping_req', t => socket.emit('ping_res', t));

  // ── Leave room ────────────────────────
  socket.on('leave_room', ()=> leaveRoom(socket));

  // ── Disconnect ────────────────────────
  socket.on('disconnect', ()=>{
    leaveRoom(socket);
    console.log(`- disconnect ${socket.id}`);
  });
});

// ════════════════════════════════════════
//  HELPERS
// ════════════════════════════════════════
function joinRoom(socket, code, data){
  const room = rooms[code] || getOrCreateGlobalRoom();

  leaveRoom(socket);

  players[socket.id] = { roomCode: room.code, name: data.name||'Player', skin: data.skin||0 };

  const spawnX = rnd(200, room.mapSize-200), spawnY = rnd(200, room.mapSize-200);
  room.players[socket.id] = {
    id: socket.id, name: data.name||'Player', skin: data.skin||0,
    x: spawnX, y: spawnY, angle: Math.random()*Math.PI*2,
    score: 0, size: 20, alive: true,
  };

  socket.join(room.code);

  // Combine bots and players for game init list
  const existingParticipants = [
    ...Object.values(room.players).filter(p=>p.id!==socket.id),
    ...Object.values(room.bots)
  ];

  socket.emit('game_init', {
    myId:     socket.id,
    roomCode: room.code,
    mapSize:  room.mapSize,
    foods:    room.foods,
    powerups: room.powerups,
    players:  existingParticipants,
    spawnX, spawnY,
  });

  socket.to(room.code).emit('player_joined', room.players[socket.id]);
  socket.to(room.code).emit('kill_feed', `🐛 ${data.name} joined!`);

  console.log(`${data.name} joined room ${room.code} | Total: ${Object.keys(room.players).length} players`);
}

function leaveRoom(socket){
  const pd = players[socket.id];
  if(!pd) return;
  const room = rooms[pd.roomCode];
  if(room){
    io.to(pd.roomCode).emit('player_left', socket.id);
    delete room.players[socket.id];
  }
  socket.leave(pd.roomCode);
  delete players[socket.id];
}

// ════════════════════════════════════════
//  START (Deployment Ready: process.env.PORT)
// ════════════════════════════════════════
const PORT = process.env.PORT || 3000;
server.listen(PORT, '0.0.0.0', ()=>{
  const ifaces = os.networkInterfaces();
  let ip = 'localhost';
  Object.values(ifaces).flat().forEach(a=>{ if(a.family==='IPv4'&&!a.internal) ip=a.address; });
  console.log(`\n🐛 WORMS ZONE GLOBAL SERVER RUNNING\n   Port:  ${PORT}\n   Mac:   http://localhost:${PORT}\n   Phone: http://${ip}:${PORT}\n`);
});
