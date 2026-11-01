import subprocess
import os

REPO = r"D:\HITME GAME\unity-client"

FILES = [
    ("tools/patches/net_client_v1/NetClientDtos.cs", "Assets/Scripts/Net/NetClientV1/NetClientDtos.cs"),
    ("tools/patches/net_client_v1/MatchTransports.cs", "Assets/Scripts/Net/NetClientV1/MatchTransports.cs"),
    ("tools/patches/net_client_v1/ClientMatchSession.cs", "Assets/Scripts/Net/NetClientV1/ClientMatchSession.cs"),
    ("tools/patches/net_client_v1/HitMeWebSocket.jslib", "Assets/Plugins/WebGL/HitMeWebSocket.jslib"),
    ("tools/patches/unity_online_glue/OnlineFlowController.cs", "Assets/Scripts/Net/OnlineGlue/OnlineFlowController.cs"),
    ("tools/patches/unity_online_glue/tests/OnlineFlowTests.cs", "Assets/Editor/Net/OnlineFlowTests.cs"),
]

for src, dst in FILES:
    full_dst = os.path.join(REPO, dst)
    os.makedirs(os.path.dirname(full_dst), exist_ok=True)
    cmd = ["git", "show", f"origin/prep/codex:{src}"]
    res = subprocess.run(cmd, cwd=REPO, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    if res.returncode == 0:
        with open(full_dst, "wb") as f:
            f.write(res.stdout)
        print(f"Extracted {src} -> {dst} ({len(res.stdout)} bytes)")
    else:
        print(f"Failed to extract {src}: {res.stderr.decode('utf-8', errors='ignore')}")

print("Extraction complete.")
