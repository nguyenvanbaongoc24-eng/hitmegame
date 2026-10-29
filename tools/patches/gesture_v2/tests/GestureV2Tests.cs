using System; using System.IO; using HitMe.Core; using HitMe.GestureV2; using Xunit;
public class GestureV2Tests { static PlacementGestureV2 G(){var g=new PlacementGestureV2();g.BeginRound(new Vec2(2,2),Vec2.Up);return g;} static void D(PlacementGestureV2 g,int id=1,float x=1,float y=1,bool inside=true)=>g.PointerDown(id,new Vec2(x,y),new Vec2(x,y),inside,0); static void M(PlacementGestureV2 g,float sx,float sy,float gx,float gy)=>g.PointerMove(1,new Vec2(sx,sy),new Vec2(gx,gy),true,0,10);
[Fact] public void TouchPlacesAndDefaults(){var g=G();D(g);var s=g.Snapshot();Assert.True(s.HasPlaced);Assert.Equal(Vec2.Up,s.Facing);}
[Fact] public void DragAimsWithoutMoving(){var g=G();D(g);M(g,20,1,4,1);var s=g.Snapshot();Assert.Equal(new Vec2(1,0),s.Facing);Assert.Equal(new Vec2(1,1),s.Position);}
[Fact] public void BelowThresholdKeepsFacing(){var g=G();D(g);M(g,5,1,2,1);Assert.Equal(Vec2.Up,g.Snapshot().Facing);}
[Fact] public void UpKeepsState(){var g=G();D(g);M(g,20,1,4,1);g.PointerUp(1,Vec2.Zero,Vec2.Zero,true,0);Assert.True(g.Snapshot().AimActive);}
[Fact] public void NewTouchResetsAim(){var g=G();D(g);M(g,20,1,4,1);g.PointerUp(1,new Vec2(20,1),new Vec2(4,1),true,0);D(g,2,5,5);Assert.Equal(Vec2.Up,g.Snapshot().Facing);}
[Fact] public void NewTouchThenAimIndependent(){var g=G();D(g);M(g,20,1,4,1);g.PointerUp(1,new Vec2(20,1),new Vec2(4,1),true,0);D(g,2,5,5);g.PointerMove(2,new Vec2(5,30),new Vec2(5,8),true,0,10);Assert.Equal(new Vec2(0,1),g.Snapshot().Facing);}
[Fact] public void ReverseCancels(){var g=G();D(g);M(g,20,1,4,1);M(g,2,1,1.1f,1);Assert.Equal(Vec2.Up,g.Snapshot().Facing);}
[Fact] public void SecondFingerIgnored(){var g=G();D(g);D(g,2,9,9);Assert.Equal(new Vec2(1,1),g.Snapshot().Position);}
[Fact] public void OutsideStartIgnored(){var g=G();D(g,1,1,1,false);Assert.False(g.Snapshot().HasPlaced);}
[Fact] public void MoveOutsideStillAims(){var g=G();D(g);g.PointerMove(1,new Vec2(20,1),new Vec2(4,1),false,0,10);Assert.True(g.Snapshot().AimActive);}
[Fact] public void LockBlocksEvents(){var g=G();D(g);g.Lock();M(g,20,1,4,1);Assert.True(g.Snapshot().Locked);Assert.Equal(Vec2.Up,g.Snapshot().Facing);}
[Fact] public void NoTouchUsesPrevious(){var g=G();Assert.Equal(new Vec2(2,2),g.Snapshot().Position);}
[Fact] public void NewRoundResetsFacing(){var g=G();D(g);M(g,20,1,4,1);g.BeginRound(g.Snapshot().Position,Vec2.Up);Assert.Equal(Vec2.Up,g.Snapshot().Facing);}
[Fact] public void MmFallback(){Assert.Equal(25.19685f,PlacementGestureV2.MmToPixels(4,160),3);}
[Fact] public void InvalidCoordinatesIgnored(){var g=G();g.PointerDown(1,new Vec2(float.NaN,0),Vec2.Zero,true,0);Assert.False(g.Snapshot().HasPlaced);}
[Fact] public void ReplayWorks(){var s=GestureReplay.Play("{\"events\":[{\"Kind\":\"down\",\"PointerId\":1,\"Sx\":1,\"Sy\":1,\"Gx\":1,\"Gy\":1,\"Inside\":true},{\"Kind\":\"move\",\"PointerId\":1,\"Sx\":20,\"Sy\":1,\"Gx\":4,\"Gy\":1,\"Inside\":true}],\"expected\":{}}");Assert.True(s.AimActive);}
[Fact] public void RequiredReplayFixturesMatchExpectedState(){var folder=Path.Combine(AppContext.BaseDirectory,"replays");var files=Directory.GetFiles(folder,"*.json");Assert.Equal(6,files.Length);foreach(var file in files){var fixture=GestureReplay.Load(File.ReadAllText(file));var s=GestureReplay.Play(fixture);var e=fixture.Expected;Assert.Equal(e.HasPlaced,s.HasPlaced);Assert.Equal(e.AimActive,s.AimActive);Assert.Equal(e.Locked,s.Locked);Assert.Equal(e.PositionX,s.Position.X,3);Assert.Equal(e.PositionY,s.Position.Y,3);Assert.Equal(e.FacingX,s.Facing.X,3);Assert.Equal(e.FacingY,s.Facing.Y,3);}}
}
