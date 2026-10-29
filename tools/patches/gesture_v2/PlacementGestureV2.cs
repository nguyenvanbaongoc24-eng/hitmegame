/* Integration (≤12 lines): copy into Assets/Scripts/Core/Phase1 after review.
 * Replace PlacementGestureState adapter calls with BeginRound/PointerDown/Move/Up.
 * Convert PointerSample screen coordinates to game coordinates via ScreenToGame first.
 * Supply minAimPixels as MmToPixels(4, Screen.dpi) at the Unity adapter boundary.
 * Only pass insideArena=true for a pointer that began in the arena; keep capture on move.
 * At placement timeout/Ready call Lock and submit Snapshot position plus aimActive/facing.
 * Port GestureV2Tests and replays to NUnit EditMode after adopting the file.
 * DefaultFacing is currently Vec2.Up. Promote it to one shared Core constant before
 * multiplayer work; the server must assert equality with that value in its tests.
 */
using System;
namespace HitMe.Core {
 public readonly struct GestureSnapshot { public readonly bool HasPlaced,AimActive,Locked; public readonly Vec2 Position,Facing; public readonly int GestureId; public GestureSnapshot(bool p,Vec2 pos,Vec2 f,bool a,bool l,int id){HasPlaced=p;Position=pos;Facing=f;AimActive=a;Locked=l;GestureId=id;} }
 public sealed class PlacementGestureV2 {
  public static readonly Vec2 DefaultFacing=Vec2.Up; int active=-1, id; bool started, placed,aim,locked; Vec2 previous,position,facing,downScreen,downGame,preAim;
  public static float MmToPixels(float mm,float dpi){ if(float.IsNaN(mm)||float.IsInfinity(mm)||mm<0)return 0; if(float.IsNaN(dpi)||float.IsInfinity(dpi)||dpi<=0)dpi=160; return mm*dpi/25.4f; }
  public void BeginRound(Vec2 previousPosition,Vec2 defaultFacing){ previous=previousPosition; position=previousPosition; facing=Finite(defaultFacing)&&defaultFacing.Length>0?defaultFacing.Normalized:DefaultFacing; active=-1;started=placed=aim=locked=false;id++; }
  public GestureSnapshot Snapshot()=>new GestureSnapshot(placed,position,facing,aim,locked,id);
  public void PointerDown(int pointerId,Vec2 screen,Vec2 game,bool insideArena,long timeMs){ if(locked||active>=0||!insideArena||!Finite(screen)||!Finite(game))return; active=pointerId;started=true;placed=true;aim=false;downScreen=screen;downGame=game;position=game;preAim=facing=DefaultFacing;id++; }
  public void PointerMove(int pointerId,Vec2 screen,Vec2 game,bool insideArena,long timeMs,float minAimPixels){ if(locked||pointerId!=active||!Finite(screen)||!Finite(game))return; var d=screen-downScreen; if(d.Length<minAimPixels){aim=false;facing=preAim;return;} var g=game-downGame; if(g.Length<=0.00001f)return; facing=g.Normalized;aim=true; }
  public void PointerUp(int pointerId,Vec2 screen,Vec2 game,bool insideArena,long timeMs){if(pointerId==active)active=-1;}
  public void Lock(){locked=true;active=-1;} static bool Finite(Vec2 p)=>!float.IsNaN(p.X)&&!float.IsNaN(p.Y)&&!float.IsInfinity(p.X)&&!float.IsInfinity(p.Y);
 }
}
