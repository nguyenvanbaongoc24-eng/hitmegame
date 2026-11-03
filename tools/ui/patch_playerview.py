import sys
from pathlib import Path

path = Path('unity-client/Assets/Scripts/Game/PlayerView.cs')
raw = path.read_text(encoding='utf-8')
code = raw.replace('\r\n', '\n')

# 1. Update PlayAnimation: keep default sprite during throw
old_throw = """            else if (stateName == "Throw")
            {
                _throwTimer = 0.35f;
                if (_throwSprites != null && idx < _throwSprites.Length && _throwSprites[idx] != null)
                {
                    _body.sprite = _throwSprites[idx];
                    return;
                }
            }"""

new_throw = """            else if (stateName == "Throw")
            {
                _throwTimer = 0.35f;
                if (_defaultSprites != null && idx < _defaultSprites.Length && _defaultSprites[idx] != null)
                {
                    _body.sprite = _defaultSprites[idx];
                }
                return;
            }"""

if old_throw in code:
    code = code.replace(old_throw, new_throw)
    print("Patched Throw animation to keep cute idle chibi sprite.")
else:
    print("WARNING: old_throw not found")

# 2. Update mesh triangles, UVs and sharedMesh
old_mesh = """                if (_aimConeMesh == null)
                {
                    _aimConeMesh = new Mesh();
                    _aimConeMesh.name = "AimConeMesh";
                    if (_aimConeFilter != null) _aimConeFilter.mesh = _aimConeMesh;
                }
                _aimConeMesh.Clear();
                _aimConeMesh.vertices = new Vector3[] { localBaseLeft, localBaseRight, localTipLeft, localTipRight };
                Color startCol = new Color(1f, 1f, 1f, 0.12f);
                Color endCol = new Color(0f, 0.898f, 1f, 0.38f);
                _aimConeMesh.colors = new Color[] { startCol, startCol, endCol, endCol };
                _aimConeMesh.triangles = new int[] { 0, 1, 2, 2, 1, 3 };
                _aimConeMesh.RecalculateBounds();"""

new_mesh = """                if (_aimConeMesh == null)
                {
                    _aimConeMesh = new Mesh();
                    _aimConeMesh.name = "AimConeMesh";
                    if (_aimConeFilter != null) _aimConeFilter.sharedMesh = _aimConeMesh;
                }
                _aimConeMesh.Clear();
                _aimConeMesh.vertices = new Vector3[] { localBaseLeft, localBaseRight, localTipLeft, localTipRight };
                _aimConeMesh.uv = new Vector2[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) };
                Color startCol = new Color(1f, 1f, 1f, 0.12f);
                Color endCol = new Color(0f, 0.898f, 1f, 0.38f);
                _aimConeMesh.colors = new Color[] { startCol, startCol, endCol, endCol };
                _aimConeMesh.triangles = new int[] { 0, 2, 1, 1, 2, 3 };
                _aimConeMesh.RecalculateBounds();
                if (_aimConeFilter != null) _aimConeFilter.sharedMesh = _aimConeMesh;"""

if old_mesh in code:
    code = code.replace(old_mesh, new_mesh)
    print("Patched AimConeMesh sharedMesh, UVs and triangles.")
else:
    print("WARNING: old_mesh not found")

# 3. Ensure sharedMesh in EnsureAimComponents
old_ensure = """                if (cGo == null)
                {
                    var go = new GameObject("AimCone");
                    go.transform.SetParent(transform, false);
                    _aimConeFilter = go.AddComponent<MeshFilter>();
                    _aimConeRenderer = go.AddComponent<MeshRenderer>();
                }
                else
                {
                    _aimConeFilter = cGo.GetComponent<MeshFilter>();
                    _aimConeRenderer = cGo.GetComponent<MeshRenderer>();
                }"""

new_ensure = """                if (cGo == null)
                {
                    var go = new GameObject("AimCone");
                    go.transform.SetParent(transform, false);
                    _aimConeFilter = go.AddComponent<MeshFilter>();
                    _aimConeRenderer = go.AddComponent<MeshRenderer>();
                }
                else
                {
                    _aimConeFilter = cGo.GetComponent<MeshFilter>();
                    _aimConeRenderer = cGo.GetComponent<MeshRenderer>();
                }
                if (_aimConeFilter != null && _aimConeMesh != null)
                {
                    _aimConeFilter.sharedMesh = _aimConeMesh;
                }"""

if old_ensure in code:
    code = code.replace(old_ensure, new_ensure)
    print("Patched EnsureAimComponents sharedMesh hook.")
else:
    print("WARNING: old_ensure not found")

# 4. Uniform breathing in Aim state
old_aim_motion = """            else if (_currentState == "Aim")
            {
                // Breathing + subtle sway
                float breath = 1.0f + Mathf.Sin(Time.time * 4.0f) * 0.03f;
                _body.transform.localScale = new Vector3(_baseBodyScale.x * breath, _baseBodyScale.y, 1f);

                // Tilt slightly in aim direction
                float leanAngle = -_currentAimDirection.x * 7.0f;
                _body.transform.localRotation = Quaternion.Euler(0f, 0f, leanAngle);
                _body.transform.localPosition = new Vector3(_currentAimDirection.x * 0.04f, _sinkOffsetY + _currentAimDirection.y * 0.02f, 0f);
            }"""

new_aim_motion = """            else if (_currentState == "Aim")
            {
                // Uniform breathing + subtle lean in aim direction
                float breath = 1.0f + Mathf.Sin(Time.time * 4.0f) * 0.03f;
                _body.transform.localScale = new Vector3(_baseBodyScale.x * breath, _baseBodyScale.y * breath, 1f);

                float leanAngle = -_currentAimDirection.x * 6.0f;
                _body.transform.localRotation = Quaternion.Euler(0f, 0f, leanAngle);
                _body.transform.localPosition = new Vector3(_currentAimDirection.x * 0.03f, _sinkOffsetY, 0f);
            }"""

if old_aim_motion in code:
    code = code.replace(old_aim_motion, new_aim_motion)
    print("Patched Aim motion uniform breathing.")
else:
    print("WARNING: old_aim_motion not found")

path.write_text(code, encoding='utf-8')
print("Finished patching PlayerView.cs")
