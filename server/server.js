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
const DEFAULT_MAP = 5000;
const MAX_FOOD    = 1000;
const MAX_BOTS    = 25;

const AI_NAMES = ['SnakeKing','WormPro','CoilMaster','SlitherX','ZoneBoss','HissKing','BoomWorm','NeonSnek','GigaWorm','VenomBoss','TitanWorm','MegaSlither','ApexPredator','MonsterSnake','ShadowCoil','GoldenSlither','ViperKing','AlphaPython','HydraX','BehemothWorm'];

// ════════════════════════════════════════
//  GLOBAL STATE
// ════════════════════════════════════════
const rooms   = {};   // { roomCode: RoomState }
const players = {};   // { socketId: { roomCode, name, skin } }

function rnd(a,b){ return a + Math.random()*(b-a); }

function getOrCreateGlobalRoom(){
  if(!rooms['GLOBAL']){
    rooms['GLOBAL'] = makeRoom('GLOBAL', true, DEFAULT_MAP);
  }
  return rooms['GLOBAL'];
}

function makeRoom(code, isPublic = true, mapSize = DEFAULT_MAP){
  const foods    = [];
  const powerups = [];
  const bots     = {};

  for(let i = 0; i < MAX_FOOD; i++) foods.push(makeFood(null, null, null, mapSize));

  const room = {
    code,
    isPublic,
    mapSize,
    players: {},
    bots,
    foods,
    powerups,
    createdAt: Date.now(),
  };

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
    val: val ?? (Math.random()<0.08?50 : Math.random()<0.25?20 : 8),
  };
}

function makePowerup(x, y, mapSize = DEFAULT_MAP){
  const TYPES = ['speed','magnet','shield','ghost','zoom'];
  return {
    id:   Math.random().toString(36).slice(2),
    x:    x ?? rnd(150, mapSize-150),
    y:    y ?? rnd(150, mapSize-150),
    type: TYPES[Math.floor(Math.random()*TYPES.length)],
  };
}

function genCode(){
  const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
  let c = '';
  for(let i=0;i<4;i++) c += chars[Math.floor(Math.random()*chars.length)];
  return (rooms[c] || c === 'GLOBAL') ? genCode() : c;
}

function spawnBot(room, index, nearPlayerX, nearPlayerY, playerScore){
  const id = 'bot_' + Math.random().toString(36).slice(2,7);
  const name = AI_NAMES[index % AI_NAMES.length] || ('Bot' + index);
  const skin = Math.floor(Math.random()*10);
  const angle = Math.random() * Math.PI * 2;

  let x, y;
  if(nearPlayerX !== undefined && nearPlayerY !== undefined){
    const dist = 750 + Math.random()*350;
    const a = Math.random()*Math.PI*2;
    x = nearPlayerX + Math.cos(a) * dist;
    y = nearPlayerY + Math.sin(a) * dist;
    x = Math.max(300, Math.min(room.mapSize-300, x));
    y = Math.max(300, Math.min(room.mapSize-300, y));
  } else {
    x = rnd(300, room.mapSize-300);
    y = rnd(300, room.mapSize-300);
  }

  let initialSize = 25;
  if(playerScore !== undefined) {
    initialSize = 18 + Math.floor(Math.sqrt(playerScore) * 0.95);
  } else {
    if(index < 3) initialSize = 400 + Math.floor(Math.random()*300); // GIANT SNAKES!
    else if(index < 8) initialSize = 120 + Math.floor(Math.random()*150);
    else initialSize = 25 + Math.floor(Math.random()*40);
  }
  initialSize = Math.max(18, Math.min(600, initialSize));

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
    turnRate: 0.04 + Math.random()*0.02,
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
        bot.aiTimer = 0.4 + Math.random()*0.8;
        let best = null, bd = 99999;
        room.foods.forEach((f, idx)=>{
          if(idx % 4 === 0){
            const d = Math.hypot(f.x - bot.x, f.y - bot.y);
            if(d < bd){ bd = d; best = f; }
          }
        });
        bot.target = best;
      }

      const cx = room.mapSize / 2, cy = room.mapSize / 2;
      let ta = bot.angle;
      const margin = 200;
      if(bot.x < margin || bot.x > room.mapSize - margin || bot.y < margin || bot.y > room.mapSize - margin){
        ta = Math.atan2(cy - bot.y, cx - bot.x);
      } else if(bot.target){
        ta = Math.atan2(bot.target.y - bot.y, bot.target.x - bot.x);
      }

      let da = ta - bot.angle;
      while(da > Math.PI) da -= Math.PI*2;
      while(da < -Math.PI) da += Math.PI*2;
      bot.angle += Math.sign(da) * Math.min(Math.abs(da), bot.turnRate);

      const spd = 2.0;
      bot.x = Math.max(30, Math.min(room.mapSize-30, bot.x + Math.cos(bot.angle)*spd));
      bot.y = Math.max(30, Math.min(room.mapSize-30, bot.y + Math.sin(bot.angle)*spd));

      bot.segs.unshift({ x: bot.x, y: bot.y });
      bot.segs.pop();

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

// Replenish food
setInterval(()=>{
  Object.values(rooms).forEach(room => {
    const need = MAX_FOOD - room.foods.length;
    for(let i = 0; i < Math.min(need, 30); i++){
      const f = makeFood(null, null, null, room.mapSize);
      room.foods.push(f);
      io.to(room.code).emit('food_spawn', f);
    }
  });
}, 400);

// Spawn powerups
setInterval(()=>{
  Object.values(rooms).forEach(room => {
    if(room.powerups.length < 15){
      const pu = makePowerup(null, null, room.mapSize);
      room.powerups.push(pu);
      io.to(room.code).emit('powerup_spawn', pu);
    }
  });
}, 5000);

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

getOrCreateGlobalRoom();

io.on('connection', (socket)=>{
  socket.on('get_rooms', ()=>{
    const list = Object.values(rooms)
      .filter(r => r.isPublic)
      .map(r => ({
        code:    r.code,
        players: Object.keys(r.players).length + Object.keys(r.bots).length,
        max:     30,
      }));
    socket.emit('room_list', list);
  });

  socket.on('quick_play', (data)=>{
    const globalRoom = getOrCreateGlobalRoom();
    joinRoom(socket, globalRoom.code, data);
  });

  socket.on('create_room', (data)=>{
    const code = genCode();
    const mapSize = data.mapSize || DEFAULT_MAP;
    rooms[code] = makeRoom(code, data.isPublic ?? true, mapSize);
    joinRoom(socket, code, data);
  });

  socket.on('join_room', (data)=>{
    const code = (data.code||'GLOBAL').toUpperCase().trim();
    if(!rooms[code]){
      joinRoom(socket, 'GLOBAL', data);
      return;
    }
    joinRoom(socket, code, data);
  });

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

  socket.on('food_eaten', (foodId)=>{
    const pd = players[socket.id]; if(!pd) return;
    const room = rooms[pd.roomCode]; if(!room) return;
    const idx = room.foods.findIndex(f=>f.id===foodId);
    if(idx===-1) return;
    room.foods.splice(idx,1);
    io.to(pd.roomCode).emit('food_removed', foodId);
  });

  socket.on('powerup_collected', (puId)=>{
    const pd = players[socket.id]; if(!pd) return;
    const room = rooms[pd.roomCode]; if(!room) return;
    const idx = room.powerups.findIndex(p=>p.id===puId);
    if(idx===-1) return;
    const pu = room.powerups.splice(idx,1)[0];
    io.to(pd.roomCode).emit('powerup_removed', puId);
    socket.emit('powerup_apply', pu.type);
  });

  socket.on('player_died', (data)=>{
    const pd = players[socket.id]; if(!pd) return;
    const room = rooms[pd.roomCode]; if(!room) return;
    const p = room.players[socket.id];
    if(p) p.alive = false;
    const newFoods = [];
    (data.segs||[]).forEach((seg,i)=>{
      if(i%2===0){
        const f = makeFood(seg.x, seg.y, 15+Math.floor(Math.random()*30), room.mapSize);
        room.foods.push(f); newFoods.push(f);
      }
    });
    io.to(pd.roomCode).emit('player_dead',{ id:socket.id, newFoods });
    const pName = room.players[socket.id]?.name||'Someone';
    io.to(pd.roomCode).emit('kill_feed', `💀 ${pName} eliminated!`);
  });

  socket.on('kill_bot', (data)=>{
    const pd = players[socket.id]; if(!pd) return;
    const room = rooms[pd.roomCode]; if(!room) return;
    const bot = room.bots[data.botId];
    if(bot && bot.alive){
      bot.alive = false;
      const newFoods = [];
      (bot.segs || []).forEach((seg, i) => {
        if(i % 2 === 0){
          const f = makeFood(seg.x, seg.y, 20 + Math.floor(Math.random()*30), room.mapSize);
          room.foods.push(f);
          newFoods.push(f);
        }
      });
      io.to(pd.roomCode).emit('player_dead', { id: bot.id, newFoods });
      io.to(pd.roomCode).emit('kill_feed', `💥 ${bot.name} eliminated by ${room.players[socket.id]?.name || 'Player'}!`);
      
      // Respawn the bot after 500ms near the player who killed it
      const p = room.players[socket.id];
      setTimeout(() => {
        if(room.bots[data.botId] === bot) {
          spawnBot(room, Math.floor(Math.random()*AI_NAMES.length), p?.x, p?.y, p?.score);
          delete room.bots[data.botId];
        }
      }, 500);
    }
  });

  socket.on('ping_req', t => socket.emit('ping_res', t));
  socket.on('leave_room', ()=> leaveRoom(socket));
  socket.on('disconnect', ()=> leaveRoom(socket));
});

function joinRoom(socket, code, data){
  const room = rooms[code] || getOrCreateGlobalRoom();
  leaveRoom(socket);

  players[socket.id] = { roomCode: room.code, name: data.name||'Player', skin: data.skin||0 };

  const spawnX = rnd(300, room.mapSize-300), spawnY = rnd(300, room.mapSize-300);
  room.players[socket.id] = {
    id: socket.id, name: data.name||'Player', skin: data.skin||0,
    x: spawnX, y: spawnY, angle: Math.random()*Math.PI*2,
    score: 0, size: 20, alive: true,
  };

  socket.join(room.code);

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
}

function leaveRoom(socket){
  const pd = players[socket.id]; if(!pd) return;
  const room = rooms[pd.roomCode];
  if(room){
    io.to(pd.roomCode).emit('player_left', socket.id);
    delete room.players[socket.id];
  }
  socket.leave(pd.roomCode);
  delete players[socket.id];
}

const PORT = process.env.PORT || 3000;
server.listen(PORT, '0.0.0.0', ()=>{
  console.log(`\n🐛 WORMS ZONE SERVER RUNNING ON PORT ${PORT}\n`);
});
