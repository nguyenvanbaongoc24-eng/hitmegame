import subprocess
import sys
import time

cmd = [
    r"D:\App\UNITY\6000.6.4f1\Editor\Unity.exe",
    "-quit",
    "-batchmode",
    "-projectPath", r"D:\HITME GAME\unity-client",
    "-executeMethod", "HitMe.Editor.WebBuildPipeline.BuildWebRelease",
    "-logFile", r"D:\HITME GAME\unity-client\unity-web-build.log"
]

print("Starting Unity WebGL Release build...")
t0 = time.time()
res = subprocess.run(cmd)
print(f"Unity build finished with exit code {res.returncode} in {time.time()-t0:.1f}s")
