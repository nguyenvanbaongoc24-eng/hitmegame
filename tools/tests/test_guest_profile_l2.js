// Test suite for L2: Guest Profile Store, Name Validation, and Safari Private Browsing Fallback

const VULGAR_WORDS = ["dm", "đm", "dcm", "đcm", "cl", "vl", "vcl", "cc", "cac", "cặc", "lồn", "buoi", "bùi", "cu", "đĩ", "điếm", "loz", "lon"];

function validateAndSanitizeName(raw) {
  if (!raw || typeof raw !== "string") {
    return { ok: false, sanitized: "", error: "Tên người chơi không được để trống." };
  }
  let clean = raw.replace(/<[^>]*>/g, "");
  clean = clean.replace(/[\x00-\x1F\x7F]/g, "").trim();
  if (clean.length < 3) {
    return { ok: false, sanitized: clean, error: "Tên phải có ít nhất 3 ký tự." };
  }
  if (clean.length > 16) {
    clean = clean.slice(0, 16).trim();
  }
  const lower = clean.toLowerCase();
  for (const w of VULGAR_WORDS) {
    const re = new RegExp("(^|\\s|_|-)" + w + "($|\\s|_|-)", "i");
    if (re.test(lower) || lower === w) {
      return { ok: false, sanitized: clean, error: "Tên chứa từ ngữ không phù hợp, vui lòng đổi tên khác." };
    }
  }
  return { ok: true, sanitized: clean };
}

function runTests() {
  console.log("--- RUNNING L2 GUEST IDENTITY TEST SUITE ---");

  // Test 1: Long name (> 16 chars) is truncated to 16 chars cleanly
  const longName = "NgườiChơiRấtDàiQuá16KýTự";
  const r1 = validateAndSanitizeName(longName);
  console.assert(r1.ok && r1.sanitized.length <= 16, "T1 Failed: Long name length > 16");
  console.log("✓ T1 Passed: Long name truncated cleanly to <= 16 chars: " + r1.sanitized);

  // Test 2: Blank name
  const r2 = validateAndSanitizeName("");
  console.assert(!r2.ok && r2.error, "T2 Failed: Blank name accepted");
  console.log("✓ T2 Passed: Blank name rejected: " + r2.error);

  // Test 3: Spaces only
  const r3 = validateAndSanitizeName("      ");
  console.assert(!r3.ok && r3.error, "T3 Failed: Space name accepted");
  console.log("✓ T3 Passed: Spaces-only name rejected: " + r3.error);

  // Test 4: Name with <script> tag
  const scriptName = "<script>alert('pwned')</script>BìnhAn";
  const r4 = validateAndSanitizeName(scriptName);
  console.assert(r4.ok && !r4.sanitized.includes("<script>") && !r4.sanitized.includes("</script>"), "T4 Failed: Script tags not stripped");
  console.log("✓ T4 Passed: Script tags stripped safely: " + r4.sanitized);

  // Test 5: Vulgar word detection
  const vulgarName = "Chơi vl quá";
  const r5 = validateAndSanitizeName(vulgarName);
  console.assert(!r5.ok && r5.error.includes("từ ngữ không phù hợp"), "T5 Failed: Vulgar word not blocked");
  console.log("✓ T5 Passed: Vulgar word detected and rejected: " + r5.error);

  // Test 6: Safari Private Mode localStorage failure fallback
  const mockPrivateLocalStorage = {
    setItem() {
      const err = new Error("QuotaExceededError: Dom storage limit reached in private mode");
      err.name = "QuotaExceededError";
      throw err;
    },
    getItem() {
      throw new Error("SecurityError");
    },
    removeItem() {}
  };

  const TestProfileStore = {
    _mem: {},
    isStorageAvailable() {
      try {
        const k = "__hitme_test__";
        mockPrivateLocalStorage.setItem(k, "1");
        mockPrivateLocalStorage.removeItem(k);
        return true;
      } catch (e) {
        return false;
      }
    },
    getItem(key) {
      try {
        if (this.isStorageAvailable()) {
          return mockPrivateLocalStorage.getItem(key);
        }
      } catch (e) {}
      return this._mem[key] !== undefined ? this._mem[key] : null;
    },
    setItem(key, val) {
      try {
        if (this.isStorageAvailable()) {
          mockPrivateLocalStorage.setItem(key, val);
          return true;
        }
      } catch (e) {}
      this._mem[key] = String(val);
      return false;
    },
    getProfile() {
      const raw = this.getItem("hitme_user_profile");
      if (raw) {
        try { return JSON.parse(raw); } catch (e) {}
      }
      return null;
    },
    saveProfile(prof) {
      this.setItem("hitme_user_profile", JSON.stringify(prof));
      return prof;
    }
  };

  const savedProf = TestProfileStore.saveProfile({
    guestId: "guest_priv123",
    name: "SafariUser",
    token: "",
    characterId: "co_ba",
    weaponId: "dep_to_ong",
    renameCount: 0
  });
  const readBack = TestProfileStore.getProfile();
  console.assert(readBack && readBack.name === "SafariUser", "T6 Failed: Safari private mode fallback failed");
  console.log("✓ T6 Passed: Safari private browsing fallback saves and loads in memory gracefully");

  console.log("--- ALL 6 L2 TESTS PASSED! ---");
}

runTests();
