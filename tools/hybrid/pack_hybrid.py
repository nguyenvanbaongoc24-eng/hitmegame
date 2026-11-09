#!/usr/bin/env python3
"""
pack_hybrid.py — R2 Channel Packager for HIT ME Hybrid Web UI + Unity WebGL Release.
Channels:
  /             Stable channel (preserved unchanged unless --promote is explicitly passed)
  /next/        Candidate hybrid channel (Web UI from prep/codex + latest Unity WebGL Release)
  /previous/    Backup of the previous stable root / channel (restorable via --rollback)
  /ui-preview/  Web UI preview channel (web-ui/dist from prep/codex)
  /rc/          Unverified candidate channel (used only if deploy gate fails, with visible RC warning badge)
"""

from __future__ import annotations

import argparse
import hashlib
import io
import json
import os
import re
import shutil
import subprocess
import sys
import tarfile
from datetime import datetime, timezone
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]
DEPLOY_WEB_DIR = REPO_ROOT / "tools" / "deploy_web"
CATALOG_SRC_DIR = DEPLOY_WEB_DIR / "catalog"
HOME_BG_WEB_DIR = DEPLOY_WEB_DIR / "web_assets" / "bg_home"
DIST_DIR = DEPLOY_WEB_DIR / "dist"
BUILDS_DIR = REPO_ROOT / "Builds"
HOME_BG_DIR = REPO_ROOT / "Assets" / "Art" / "UI" / "HomeBg"
STAGING_DEPLOY_DIR = Path(os.environ.get("HITME_WEB_DIST_STAGING", str(REPO_ROOT.parent / "_web_dist_deploy")))
TEMP_EXTRACT_DIR = REPO_ROOT.parent / "_codex_extract"


HOME_VISUAL_PATCH_HTML = """
  <style id="hitme-home-bg-patch">
    /* B-2..B-5: Village Courtyard Home Background (u4_background.png) + Single Baked Lotus Pedestal */
    .home .backdrop {
      background-color: #d89658 !important;
      background-image: url('./assets/bg_home_1170.webp'), url('./assets/bg_home_lqip.webp') !important;
      background-image: -webkit-image-set(
        url('./assets/bg_home_750.webp') 1x,
        url('./assets/bg_home_1170.webp') 2x,
        url('./assets/bg_home_1536.webp') 3x
      ), url('./assets/bg_home_lqip.webp') !important;
      background-image: image-set(
        url('./assets/bg_home_750.webp') 1x,
        url('./assets/bg_home_1170.webp') 2x,
        url('./assets/bg_home_1536.webp') 3x
      ), url('./assets/bg_home_lqip.webp') !important;
      background-position: 50% 48% !important;
      background-size: cover !important;
      background-repeat: no-repeat !important;
    }
    .home .backdrop:before {
      content: "" !important;
      position: absolute !important;
      inset: 0 !important;
      height: 100% !important;
      background: linear-gradient(180deg, rgba(8, 15, 26, 0.45) 0%, rgba(8, 15, 26, 0.14) 14%, rgba(0, 0, 0, 0) 26%) !important;
      -webkit-mask-image: none !important;
      mask-image: none !important;
      pointer-events: none !important;
    }
    .home .backdrop:after {
      content: "" !important;
      position: absolute !important;
      inset: 0 !important;
      height: 100% !important;
      background: linear-gradient(180deg, rgba(0, 0, 0, 0) 53%, rgba(10, 18, 30, 0.25) 78%, rgba(8, 14, 24, 0.60) 100%) !important;
      -webkit-mask-image: none !important;
      mask-image: none !important;
      pointer-events: none !important;
    }
    /* Remove duplicate CSS-drawn lotus pedestal and bottom arena strip */
    .home .pedestal,
    .home .pedestal:before,
    .home .home-bottom-art {
      display: none !important;
    }
    /* Ground character feet onto the baked stone lotus pedestal at (fx=0.5, fy=0.4844) */
    .home .hero {
      top: calc(var(--pedestal-feet-y, 48.44%) - var(--hero-stand-h, 28%)) !important;
      height: var(--hero-stand-h, 28%) !important;
      left: 28% !important;
      width: 44% !important;
      object-fit: contain !important;
      object-position: center bottom !important;
      filter: drop-shadow(0 7px 6px rgba(28, 15, 6, 0.58)) !important;
    }
    /* Clean framing for character head avatars and weapon icons */
    .home .avatar {
      background: radial-gradient(circle at 50% 42%, #1e3a5f 0%, #0d1b2a 100%) !important;
      display: grid !important;
      place-items: center !important;
      padding: 2px !important;
    }
    .home .avatar img {
      width: 100% !important;
      height: 100% !important;
      object-fit: contain !important;
      object-position: center center !important;
    }
    .selection-card img,
    .choice-grid img,
    .live-card img,
    .live-rank img,
    .preview-character img {
      object-fit: contain !important;
      filter: drop-shadow(0 2px 3px rgba(0, 0, 0, 0.45));
    }
    @media (max-height: 660px) {
      .home .logo { top: 9.5% !important; height: 11.5% !important; }
      .home .selection-card { top: 53.5% !important; height: 8.2% !important; min-height: 44px !important; }
      .home .quick { top: 62.8% !important; height: 9.2% !important; min-height: 48px !important; }
      .home .blue, .home .green { top: 73.2% !important; height: 8.0% !important; min-height: 44px !important; }
      .home .practice { top: 82.2% !important; height: 7.4% !important; min-height: 44px !important; }
    }

    /* ============================================================================
       ROUND 38 G1–G8: In-Match HUD, Timer Ring + 5 Dots, Non-Blocking Interactive
       Tutorial, Phase Banners, Bot Setup Top Selector & Scroll, and Home Polish
       ============================================================================ */

    /* G7a: Hide stray server-dot circle O after 'Sắp ra mắt' on CHƠI NHANH */
    .home .quick .server-dot {
      display: none !important;
    }

    /* G7b + G7c: Widen weapon card so 'Dép Tổ Ong' stays on 1 line; hide reverse arrow when <= 1 weapon unlocked */
    .home .character-card {
      left: 3.5% !important;
      width: 44.5% !important;
      padding: 4px 8px !important;
    }
    .home .weapon-card {
      right: 3.5% !important;
      width: 47% !important;
      padding: 4px 8px !important;
    }
    .home .weapon-card b,
    .home .character-card b {
      white-space: nowrap !important;
      overflow: hidden !important;
      text-overflow: ellipsis !important;
      line-height: 1.28 !important;
      padding-bottom: 2px !important;
      max-height: none !important;
      font-size: clamp(12px, 3.2vw, 16px) !important;
    }
    .home .weapon-card.single-weapon > .icon.reverse {
      display: none !important;
    }

    /* G1: Adaptive Top Player Cards for N=2..6, larger circular avatars, clear hearts, unclipped diacritics ('Nữ Năng Động') */
    .live-top {
      padding: max(8px, env(safe-area-inset-top)) 8px 6px !important;
      gap: 6px !important;
    }
    .live-cards {
      display: flex !important;
      gap: 6px !important;
      width: 100% !important;
      padding: 0 2px !important;
      min-height: 66px !important;
      align-items: stretch !important;
    }
    .live-card {
      flex: 1 1 0 !important;
      min-width: 0 !important;
      padding: 5px 4px 4px !important;
      background: linear-gradient(180deg, #162b42f5 0%, #0d1b2bf5 100%) !important;
      border: 1.5px solid #d4af37 !important;
      border-radius: 12px !important;
      box-shadow: 0 3px 8px rgba(0, 0, 0, 0.45) !important;
      display: flex !important;
      flex-direction: column !important;
      align-items: center !important;
      justify-content: center !important;
      gap: 2px !important;
      position: relative !important;
    }
    .live-card.local {
      border: 2.5px solid #19d5d8 !important;
      background: linear-gradient(180deg, #173852f8 0%, #0e2438f8 100%) !important;
      box-shadow: 0 0 0 2px rgba(25, 213, 216, 0.28), 0 4px 10px rgba(0, 0, 0, 0.5) !important;
    }
    .live-card.out {
      opacity: 0.42 !important;
      filter: grayscale(0.85) !important;
    }
    .live-card img {
      width: 36px !important;
      height: 36px !important;
      border-radius: 50% !important;
      border: 2px solid rgba(255, 220, 67, 0.85) !important;
      background: radial-gradient(circle at 50% 40%, #24476e 0%, #0d1b2a 100%) !important;
      padding: 1px !important;
      object-fit: contain !important;
      flex: none !important;
    }
    .live-card.local img {
      border-color: #19d5d8 !important;
    }
    .live-cards[data-count="5"] .live-card img,
    .live-cards[data-count="6"] .live-card img {
      width: 30px !important;
      height: 30px !important;
    }
    .live-card div {
      width: 100% !important;
      overflow: visible !important;
    }
    .live-card b {
      display: block !important;
      font-size: clamp(9.5px, 2.55vw, 11px) !important;
      font-weight: 800 !important;
      line-height: 1.32 !important;
      padding-bottom: 2px !important;
      color: #fff8e8 !important;
      overflow: hidden !important;
      text-overflow: ellipsis !important;
      white-space: nowrap !important;
      letter-spacing: -0.02em !important;
    }
    .live-cards[data-count="2"] .live-card b,
    .live-cards[data-count="3"] .live-card b,
    .live-cards[data-count="4"] .live-card b {
      overflow: visible !important;
      text-overflow: clip !important;
      font-size: clamp(9.2px, 2.45vw, 10.8px) !important;
    }
    .live-cards[data-count="5"] .live-card b,
    .live-cards[data-count="6"] .live-card b {
      font-size: 9px !important;
    }
    .live-card.hold-reveal b {
      position: absolute !important;
      top: -24px !important;
      left: 50% !important;
      transform: translateX(-50%) !important;
      background: #0b1421f5 !important;
      border: 1px solid #ffcd38 !important;
      border-radius: 6px !important;
      padding: 2px 6px !important;
      font-size: 11px !important;
      z-index: 30 !important;
      white-space: nowrap !important;
      overflow: visible !important;
    }
    .live-hearts {
      display: inline-flex !important;
      align-items: center !important;
      justify-content: center !important;
      gap: 1px !important;
      color: #ff4a5e !important;
      font-size: 13px !important;
      line-height: 1.1 !important;
      text-shadow: 0 1px 2px rgba(0, 0, 0, 0.7) !important;
    }
    .live-hearts span {
      color: #64748b !important;
    }

    /* G2: Center-Top Countdown Progress Ring + 5 Round Dots */
    .live-summary {
      display: grid !important;
      grid-template-columns: 1fr auto 1fr !important;
      align-items: center !important;
      gap: 8px !important;
      min-height: 54px !important;
    }
    .r38-round-cluster {
      display: flex !important;
      flex-direction: column !important;
      align-items: flex-start !important;
      gap: 4px !important;
    }
    .r38-round-cluster b {
      font-size: 13px !important;
      font-weight: 900 !important;
      color: #ffe258 !important;
      letter-spacing: 0.03em !important;
    }
    .r38-round-dots {
      display: flex !important;
      align-items: center !important;
      gap: 5px !important;
    }
    .r38-round-dot {
      width: 9px !important;
      height: 9px !important;
      border-radius: 50% !important;
      border: 1.5px solid #ffd84d !important;
      background: rgba(15, 23, 42, 0.75) !important;
      transition: transform 0.2s ease, background 0.2s ease !important;
    }
    .r38-round-dot.past {
      background: #17b987 !important;
      border-color: #34d399 !important;
    }
    .r38-round-dot.current {
      background: #ffcd38 !important;
      border-color: #fff8e8 !important;
      transform: scale(1.28) !important;
      box-shadow: 0 0 6px rgba(255, 205, 56, 0.85) !important;
    }
    .r38-timer-ring {
      position: relative !important;
      width: 54px !important;
      height: 54px !important;
      border-radius: 50% !important;
      display: grid !important;
      place-items: center !important;
      background: conic-gradient(var(--timer-color, #19d5d8) var(--timer-pct, 100%), rgba(255, 255, 255, 0.14) 0) !important;
      box-shadow: 0 3px 10px rgba(0, 0, 0, 0.55) !important;
      justify-self: center !important;
    }
    .r38-timer-ring::before {
      content: "" !important;
      position: absolute !important;
      inset: 5px !important;
      border-radius: 50% !important;
      background: radial-gradient(circle at 50% 35%, #192b42 0%, #0b1421 100%) !important;
    }
    .r38-timer-ring.coin-style::before {
      background: radial-gradient(circle at 35% 30%, #ffe66d 0%, #d98212 85%, #7c3f06 100%) !important;
    }
    .r38-timer-ring span {
      position: relative !important;
      z-index: 2 !important;
      font-size: 17px !important;
      font-weight: 900 !important;
      color: #fff8e8 !important;
      line-height: 1 !important;
      text-shadow: 0 2px 3px rgba(0, 0, 0, 0.8) !important;
    }
    .r38-timer-ring.urgent {
      --timer-color: #f66a5d !important;
      animation: r38-timer-pulse 0.48s ease-in-out infinite !important;
    }
    .r38-timer-ring.urgent span {
      color: #ffdf55 !important;
    }
    @keyframes r38-timer-pulse {
      0%, 100% { transform: scale(1); }
      50% { transform: scale(1.13); box-shadow: 0 0 14px rgba(246, 106, 93, 0.85); }
    }
    .live-summary > button[data-action="settings"] {
      justify-self: end !important;
      width: 48px !important;
      height: 48px !important;
    }

    /* G3: Non-Blocking Interactive Tutorial (never covers the center court; pointer-events pass through) */
    .live-tutorial {
      left: 4% !important;
      right: 4% !important;
      bottom: calc(var(--live-bottom, 118px) + 6px) !important;
      padding: 8px 12px !important;
      background: rgba(11, 20, 33, 0.92) !important;
      border: 2px solid #19d5d8 !important;
      border-radius: 14px !important;
      display: flex !important;
      align-items: center !important;
      justify-content: space-between !important;
      gap: 10px !important;
      text-align: left !important;
      pointer-events: none !important;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.55) !important;
    }
    .live-tutorial b {
      font-size: 11px !important;
      color: #ffcd38 !important;
      display: block !important;
      white-space: nowrap !important;
    }
    .live-tutorial p {
      font-size: 13px !important;
      font-weight: 700 !important;
      margin: 2px 0 0 !important;
      line-height: 1.25 !important;
      color: #fff8e8 !important;
      flex: 1 !important;
    }
    .live-tutorial div {
      display: flex !important;
      gap: 6px !important;
      pointer-events: auto !important;
    }
    .live-tutorial button {
      pointer-events: auto !important;
      min-width: 48px !important;
      min-height: 48px !important;
      padding: 6px 10px !important;
      font-size: 12px !important;
    }
    .r38-hand-guide {
      position: fixed !important;
      left: 50% !important;
      top: 56% !important;
      width: 42px !important;
      height: 42px !important;
      margin-left: -21px !important;
      margin-top: -21px !important;
      z-index: 22 !important;
      pointer-events: none !important;
      filter: drop-shadow(0 3px 6px rgba(0, 0, 0, 0.7)) !important;
      animation: r38-hand-tap 1.35s ease-in-out infinite !important;
    }
    .r38-hand-guide.drag-mode {
      animation: r38-hand-drag 1.6s ease-in-out infinite !important;
    }
    @keyframes r38-hand-tap {
      0%, 100% { transform: translate(0, 0) scale(1); opacity: 0.92; }
      50% { transform: translate(0, -10px) scale(0.88); opacity: 1; }
    }
    @keyframes r38-hand-drag {
      0% { transform: translate(-24px, 14px) scale(0.95); opacity: 0.95; }
      65% { transform: translate(32px, -28px) scale(1); opacity: 1; }
      100% { transform: translate(-24px, 14px) scale(0.95); opacity: 0.85; }
    }

    /* G4: Phase Transition Banner (0.6s bounce-in + fade-out, non-blocking) */
    #r38-phase-banner {
      position: fixed !important;
      left: 50% !important;
      top: 38% !important;
      transform: translate(-50%, -50%) scale(0.7) !important;
      z-index: 45 !important;
      padding: 10px 26px !important;
      border-radius: 999px !important;
      background: linear-gradient(180deg, #ffdf4a 0%, #f59e0b 100%) !important;
      border: 3px solid #fff8e8 !important;
      color: #2b1206 !important;
      font-size: clamp(20px, 6vw, 30px) !important;
      font-weight: 900 !important;
      letter-spacing: 0.04em !important;
      text-transform: uppercase !important;
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.65) !important;
      pointer-events: none !important;
      opacity: 0 !important;
      white-space: nowrap !important;
    }
    #r38-phase-banner.active {
      animation: r38-banner-pop 0.62s cubic-bezier(0.2, 1.35, 0.35, 1) forwards !important;
    }
    @keyframes r38-banner-pop {
      0% { opacity: 0; transform: translate(-50%, -50%) scale(0.55); }
      28% { opacity: 1; transform: translate(-50%, -50%) scale(1.14); }
      48% { opacity: 1; transform: translate(-50%, -50%) scale(1.0); }
      82% { opacity: 1; transform: translate(-50%, -50%) scale(1.0); }
      100% { opacity: 0; transform: translate(-50%, -56%) scale(0.92); }
    }

    /* G6: Bot Practice Setup Screen — Top Bot Count Selector + Scrollable Weapon Grid + Bottom Padding >= Footer Height */
    [data-screen="bot"] .preview-scroll {
      display: flex !important;
      flex-direction: column !important;
      padding-bottom: 116px !important;
      overflow-y: auto !important;
      -webkit-overflow-scrolling: touch !important;
    }
    [data-screen="bot"] .preview-count {
      order: -2 !important;
      margin: 2px 0 10px !important;
      padding: 10px 12px !important;
      background: linear-gradient(135deg, #1b354f 0%, #112233 100%) !important;
      border: 2px solid #ffcd38 !important;
      border-radius: 14px !important;
      box-shadow: 0 4px 10px rgba(0, 0, 0, 0.4) !important;
    }
    [data-screen="bot"] .preview-weapons {
      display: grid !important;
      grid-template-columns: repeat(3, minmax(0, 1fr)) !important;
      gap: 8px !important;
    }
    [data-screen="bot"] .preview-weapon {
      height: auto !important;
      min-height: 88px !important;
      flex-direction: column !important;
      justify-content: center !important;
      text-align: center !important;
      padding: 8px 6px !important;
      gap: 4px !important;
    }
    [data-screen="bot"] .preview-weapon img {
      width: 44px !important;
      height: 44px !important;
    }
    [data-screen="bot"] .preview-weapon span {
      font-size: 12px !important;
      line-height: 1.2 !important;
      white-space: normal !important;
    }
    .r38-badge-tam {
      display: inline-block !important;
      margin-top: 2px !important;
      padding: 1px 6px !important;
      border-radius: 6px !important;
      background: rgba(245, 158, 11, 0.22) !important;
      border: 1px solid #f59e0b !important;
      color: #fde68a !important;
      font-size: 9.5px !important;
      font-weight: 800 !important;
      letter-spacing: 0.02em !important;
    }
    .r38-badge-owned {
      display: inline-block !important;
      margin-top: 2px !important;
      padding: 1px 6px !important;
      border-radius: 6px !important;
      background: rgba(23, 185, 135, 0.22) !important;
      border: 1px solid #17b987 !important;
      color: #6ee7b7 !important;
      font-size: 9.5px !important;
      font-weight: 800 !important;
    }

    /* E1: Living Home Screen (Hero Breathing, Tap Wave, Switch Bounce 220ms, Logo Shine, Courtyard Sway, Pooled Leaves, Coin Spin) */
    .home .hero {
      animation: r38-hero-breath 2.4s ease-in-out infinite !important;
      transform-origin: center bottom !important;
      cursor: pointer !important;
      will-change: transform !important;
    }
    @keyframes r38-hero-breath {
      0%, 100% { transform: translateY(0px) scale(1, 1); }
      50% { transform: translateY(-3.5px) scale(1.01, 1.018); }
    }
    .home .hero.tap-wave {
      animation: r38-hero-tap 0.28s cubic-bezier(0.22, 1.4, 0.36, 1) forwards !important;
    }
    @keyframes r38-hero-tap {
      0% { transform: scale(1, 1) rotate(0deg); }
      40% { transform: translateY(-8px) scale(1.06, 0.94) rotate(-4deg); }
      75% { transform: translateY(-3px) scale(0.98, 1.03) rotate(3deg); }
      100% { transform: translateY(0px) scale(1, 1) rotate(0deg); }
    }
    .home .hero.switch-bounce {
      animation: r38-hero-switch 0.22s cubic-bezier(0.2, 1.35, 0.38, 1) forwards !important;
    }
    @keyframes r38-hero-switch {
      0% { transform: scale(0.92); opacity: 0.82; }
      60% { transform: scale(1.04); opacity: 1; }
      100% { transform: scale(1.0); opacity: 1; }
    }
    #r38-pedestal-glow {
      position: fixed !important;
      left: 50% !important;
      top: var(--pedestal-feet-y, 48%) !important;
      width: 148px !important;
      height: 46px !important;
      transform: translate(-50%, -50%) scale(0.85) !important;
      border-radius: 50% !important;
      background: radial-gradient(ellipse at center, rgba(255, 226, 88, 0.78) 0%, rgba(25, 213, 216, 0.35) 55%, rgba(0, 0, 0, 0) 75%) !important;
      pointer-events: none !important;
      opacity: 0 !important;
      z-index: 2 !important;
    }
    #r38-pedestal-glow.flash {
      animation: r38-pedestal-flash 0.24s ease-out forwards !important;
    }
    @keyframes r38-pedestal-flash {
      0% { opacity: 0.95; transform: translate(-50%, -50%) scale(0.82); }
      55% { opacity: 1; transform: translate(-50%, -50%) scale(1.18); }
      100% { opacity: 0; transform: translate(-50%, -50%) scale(1.32); }
    }
    .home .logo,
    .home .brand-badge {
      position: relative !important;
      overflow: hidden !important;
    }
    .home .logo::after,
    .home .brand-badge::after {
      content: "" !important;
      position: absolute !important;
      top: -20% !important;
      left: -60% !important;
      width: 42% !important;
      height: 140% !important;
      background: linear-gradient(105deg, rgba(255,255,255,0) 0%, rgba(255,248,210,0.65) 50%, rgba(255,255,255,0) 100%) !important;
      transform: skewX(-22deg) !important;
      animation: r38-logo-shine 6.8s ease-in-out infinite !important;
      pointer-events: none !important;
    }
    @keyframes r38-logo-shine {
      0%, 76%, 100% { left: -65%; opacity: 0; }
      80% { opacity: 1; }
      92% { left: 135%; opacity: 0.9; }
    }
    .home .gold svg,
    .home .coin-pill::before,
    .home .coin-pill img,
    .home .coin-pill svg {
      animation: r38-coin-spin 3.6s ease-in-out infinite !important;
      display: inline-block !important;
    }
    @keyframes r38-coin-spin {
      0%, 70%, 100% { transform: rotateY(0deg) scale(1); filter: brightness(1); }
      85% { transform: rotateY(180deg) scale(1.12); filter: brightness(1.35); }
    }
    #r38-ambient-leaves {
      position: fixed !important;
      inset: 0 !important;
      pointer-events: none !important;
      z-index: 2 !important;
      overflow: hidden !important;
    }
    .r38-leaf {
      position: absolute !important;
      top: -18px !important;
      width: 9px !important;
      height: 9px !important;
      border-radius: 70% 15% 70% 15% !important;
      background: radial-gradient(circle at 30% 30%, #fff3a1 0%, #f59e0b 85%) !important;
      box-shadow: 0 0 6px rgba(255, 212, 71, 0.65) !important;
      opacity: 0.78 !important;
      animation: r38-leaf-fall linear infinite !important;
      will-change: transform, opacity !important;
    }
    @keyframes r38-leaf-fall {
      0% { transform: translate3d(0, -20px, 0) rotate(0deg); opacity: 0; }
      12% { opacity: 0.82; }
      85% { opacity: 0.75; }
      100% { transform: translate3d(var(--drift-x, 38px), 92vh, 0) rotate(320deg); opacity: 0; }
    }

    /* E2–E6: In-Match Place Ring + Dust, Hit Shake, Shattered Heart, Floating Callout ("TRÚNG!"/"HỤT!"), Confetti & Low-HP Vignette */
    #r38-place-fx {
      position: fixed !important;
      width: 68px !important;
      height: 68px !important;
      margin-left: -34px !important;
      margin-top: -34px !important;
      border-radius: 50% !important;
      border: 3px solid #19d5d8 !important;
      box-shadow: 0 0 14px rgba(25, 213, 216, 0.8), inset 0 0 10px rgba(255, 223, 74, 0.55) !important;
      pointer-events: none !important;
      z-index: 26 !important;
      opacity: 0 !important;
    }
    #r38-place-fx.active {
      animation: r38-place-pop 0.18s cubic-bezier(0.18, 1.35, 0.4, 1) forwards !important;
    }
    @keyframes r38-place-pop {
      0% { opacity: 1; transform: scale(1.18); }
      55% { opacity: 1; transform: scale(0.94); }
      100% { opacity: 0; transform: scale(1.38); }
    }
    .r38-screen-shake {
      animation: r38-shake-anim 0.12s ease-in-out !important;
    }
    @keyframes r38-shake-anim {
      0%, 100% { transform: translate3d(0, 0, 0); }
      25% { transform: translate3d(-3.5px, 2.5px, 0); }
      50% { transform: translate3d(3.5px, -2.5px, 0); }
      75% { transform: translate3d(-2.5px, -2px, 0); }
    }
    .live-card.r38-card-hit-shake {
      animation: r38-card-hit 0.34s cubic-bezier(0.36, 0.07, 0.19, 0.97) !important;
      border-color: #ff4a5e !important;
      box-shadow: 0 0 14px rgba(255, 74, 94, 0.9) !important;
    }
    @keyframes r38-card-hit {
      0%, 100% { transform: translateX(0) scale(1); }
      20% { transform: translateX(-5px) scale(1.06); background: rgba(220, 38, 38, 0.55); }
      45% { transform: translateX(5px) scale(0.96); }
      70% { transform: translateX(-3px) scale(1.02); }
    }
    #r38-hit-callout {
      position: fixed !important;
      left: 50% !important;
      top: 47% !important;
      transform: translate(-50%, -50%) scale(0.5) !important;
      z-index: 46 !important;
      padding: 6px 18px !important;
      border-radius: 999px !important;
      background: linear-gradient(180deg, #ff4a5e 0%, #b91c1c 100%) !important;
      border: 2.5px solid #fff8e8 !important;
      color: #fff8e8 !important;
      font-size: 22px !important;
      font-weight: 900 !important;
      letter-spacing: 0.04em !important;
      text-shadow: 0 2px 4px rgba(0, 0, 0, 0.85) !important;
      box-shadow: 0 6px 18px rgba(185, 28, 28, 0.75) !important;
      pointer-events: none !important;
      opacity: 0 !important;
    }
    #r38-hit-callout.miss {
      background: linear-gradient(180deg, #475569 0%, #1e293b 100%) !important;
      font-size: 17px !important;
    }
    #r38-hit-callout.active {
      animation: r38-callout-pop 0.58s cubic-bezier(0.2, 1.4, 0.35, 1) forwards !important;
    }
    @keyframes r38-callout-pop {
      0% { opacity: 0; transform: translate(-50%, -30%) scale(0.45); }
      30% { opacity: 1; transform: translate(-50%, -65%) scale(1.18); }
      65% { opacity: 1; transform: translate(-50%, -85%) scale(1.0); }
      100% { opacity: 0; transform: translate(-50%, -115%) scale(0.9); }
    }
    #r38-low-hp-vignette {
      position: fixed !important;
      inset: 0 !important;
      pointer-events: none !important;
      z-index: 19 !important;
      box-shadow: inset 0 0 36px rgba(246, 106, 93, 0.68) !important;
      opacity: 0 !important;
    }
    #r38-low-hp-vignette.active {
      animation: r38-low-hp-pulse 1.15s ease-in-out infinite !important;
    }
    @keyframes r38-low-hp-pulse {
      0%, 100% { opacity: 0.22; }
      50% { opacity: 0.72; }
    }
    #r38-confetti-pool {
      position: fixed !important;
      inset: 0 !important;
      pointer-events: none !important;
      z-index: 48 !important;
      overflow: hidden !important;
      display: none !important;
    }
    #r38-confetti-pool.active {
      display: block !important;
    }
    .r38-confetti {
      position: absolute !important;
      top: -16px !important;
      width: 10px !important;
      height: 14px !important;
      border-radius: 2px !important;
      animation: r38-confetti-drop 2.2s cubic-bezier(0.25, 0.46, 0.45, 0.94) infinite !important;
    }
    @keyframes r38-confetti-drop {
      0% { transform: translate3d(0, -20px, 0) rotate(0deg); opacity: 1; }
      100% { transform: translate3d(var(--cf-drift, 24px), 95vh, 0) rotate(540deg); opacity: 0.15; }
    }
    @media (prefers-reduced-motion: reduce) {
      .home .hero, .home .brand-badge::after, .r38-leaf, .r38-confetti, #r38-low-hp-vignette.active, .r38-timer-ring.urgent {
        animation: none !important;
      }
    }
    body.r38-reduced-motion .home .hero,
    body.r38-reduced-motion .home .brand-badge::after,
    body.r38-reduced-motion .r38-leaf,
    body.r38-reduced-motion .r38-confetti,
    body.r38-reduced-motion #r38-low-hp-vignette.active,
    body.r38-reduced-motion .r38-timer-ring.urgent {
      animation: none !important;
    }
  </style>
  <script>
    window.addEventListener("load", function() {
      setTimeout(function() {
        const preloadAssets = [
          "./assets/bg_home_750.webp",
          "./assets/bg_home_1170.webp",
          "./assets/nam_lang_portrait.png",
          "./assets/nu_nang_dong_portrait.png",
          "./assets/co_ba_portrait.png",
          "./assets/anh_teen_portrait.png",
          "./assets/nam_lang_head.png",
          "./assets/nu_nang_dong_head.png",
          "./assets/co_ba_head.png",
          "./assets/anh_teen_head.png",
          "./assets/quay_boy_head.png",
          "./assets/cho_vang_head.png",
          "./assets/meo_mun_head.png",
          "./assets/dep_to_ong_icon.png",
          "./assets/chao_chong_dinh_icon.png",
          "./assets/vot_pickleball_icon.png"
        ];
        preloadAssets.forEach(src => { const img = new Image(); img.src = src; });
      }, 600);
    });
    (function() {
      // Dynamic pedestal anchor alignment (B-4 formula from bg_home_meta.json)
      function syncPedestalAnchor() {
        const vv = window.visualViewport;
        const vw = vv ? vv.width : window.innerWidth;
        const vh = vv ? vv.height : window.innerHeight;
        const stageW = Math.min(vw, 430);
        const stageH = vh;
        const origW = 941, origH = 1672;
        const fy = 0.4844, posY = 0.48;
        const scale = Math.max(stageW / origW, stageH / origH);
        const renderH = origH * scale;
        const dy = (stageH - renderH) * posY;
        const feetY = Math.round(dy + fy * renderH);
        const heroH = Math.round(Math.max(110, Math.min(220, stageH * 0.278)));
        document.documentElement.style.setProperty("--pedestal-feet-y", feetY + "px");
        document.documentElement.style.setProperty("--hero-stand-h", heroH + "px");
      }
      window.addEventListener("resize", syncPedestalAnchor);
      if (window.visualViewport) {
        window.visualViewport.addEventListener("resize", syncPedestalAnchor);
        window.visualViewport.addEventListener("scroll", syncPedestalAnchor);
      }
      syncPedestalAnchor();

      // Round 38 G1–G8 DOM Enhancer (Timer Ring + 5 Round Dots + Phase Banners + Hold-to-View Name + Interactive Hand Guide)
      let _r38LastPhase = "";
      let _r38LastRound = 0;
      let _r38LastUrgentSec = -1;
      let _r38PlaceCountInTutorial = 0;
      let _r38CatalogWeaponsUnlocked = 1;
      let _r38PrevHpByPlayer = {};
      let _r38HapticsEnabled = true;
      try { _r38HapticsEnabled = localStorage.getItem("hitme_haptics") !== "0"; } catch (e) {}

      function tryHaptic(ms) {
        if (!_r38HapticsEnabled || document.body.classList.contains("r38-reduced-motion")) return;
        try { if (navigator && typeof navigator.vibrate === "function") navigator.vibrate(ms || 25); } catch (e) {}
      }

      function ensurePooledVfxContainers() {
        if (!document.getElementById("r38-ambient-leaves")) {
          const leaves = document.createElement("div");
          leaves.id = "r38-ambient-leaves";
          for (let i = 0; i < 16; i++) {
            const l = document.createElement("i");
            l.className = "r38-leaf";
            l.style.left = ((i * 6.25 + 3) % 96).toFixed(1) + "%";
            l.style.animationDuration = (5.2 + (i % 5) * 1.1).toFixed(1) + "s";
            l.style.animationDelay = (-i * 0.65).toFixed(2) + "s";
            l.style.setProperty("--drift-x", ((i % 2 === 0 ? 1 : -1) * (18 + (i % 4) * 9)) + "px");
            leaves.appendChild(l);
          }
          document.body.appendChild(leaves);
        }
        if (!document.getElementById("r38-pedestal-glow")) {
          const g = document.createElement("div");
          g.id = "r38-pedestal-glow";
          document.body.appendChild(g);
        }
        if (!document.getElementById("r38-place-fx")) {
          const p = document.createElement("div");
          p.id = "r38-place-fx";
          document.body.appendChild(p);
        }
        if (!document.getElementById("r38-hit-callout")) {
          const c = document.createElement("div");
          c.id = "r38-hit-callout";
          document.body.appendChild(c);
        }
        if (!document.getElementById("r38-low-hp-vignette")) {
          const v = document.createElement("div");
          v.id = "r38-low-hp-vignette";
          document.body.appendChild(v);
        }
        if (!document.getElementById("r38-confetti-pool")) {
          const cf = document.createElement("div");
          cf.id = "r38-confetti-pool";
          const colors = ["#ffcd38", "#19d5d8", "#f66a5d", "#17b987", "#fff8e8", "#a855f7"];
          for (let i = 0; i < 28; i++) {
            const p = document.createElement("i");
            p.className = "r38-confetti";
            p.style.left = ((i * 3.55 + 1.5) % 98).toFixed(1) + "%";
            p.style.background = colors[i % colors.length];
            p.style.animationDuration = (1.6 + (i % 5) * 0.28).toFixed(2) + "s";
            p.style.animationDelay = ((i % 7) * 0.14).toFixed(2) + "s";
            p.style.setProperty("--cf-drift", ((i % 2 === 0 ? 1 : -1) * (14 + (i % 5) * 8)) + "px");
            cf.appendChild(p);
          }
          document.body.appendChild(cf);
        }
      }

      function showHitCallout(text, isMiss) {
        if (document.body.classList.contains("r38-reduced-motion")) return;
        const c = document.getElementById("r38-hit-callout");
        if (!c) return;
        c.textContent = text;
        c.classList.toggle("miss", !!isMiss);
        c.classList.remove("active");
        void c.offsetWidth;
        c.classList.add("active");
      }

      function triggerScreenShake() {
        if (document.body.classList.contains("r38-reduced-motion")) return;
        const uCont = document.getElementById("unity-container");
        if (!uCont) return;
        uCont.classList.remove("r38-screen-shake");
        void uCont.offsetWidth;
        uCont.classList.add("r38-screen-shake");
      }

      fetch("./catalog.json").then(r => r.json()).then(cat => {
        if (cat && Array.isArray(cat.weapons)) {
          _r38CatalogWeaponsUnlocked = cat.weapons.filter(w => w.enabled).length;
        }
      }).catch(() => {});

      function showPhaseBanner(text) {
        if (!text) return;
        let banner = document.getElementById("r38-phase-banner");
        if (!banner) {
          banner = document.createElement("div");
          banner.id = "r38-phase-banner";
          banner.setAttribute("role", "status");
          banner.setAttribute("aria-live", "polite");
          document.body.appendChild(banner);
        }
        banner.textContent = text;
        banner.classList.remove("active");
        void banner.offsetWidth;
        banner.classList.add("active");
      }

      function enhanceRound38Dom() {
        const root = document.getElementById("ui-root");
        if (!root) return;

        // G7c: Toggle .single-weapon on Home weapon card when <= 1 weapon unlocked
        const weaponCard = root.querySelector(".home .weapon-card");
        if (weaponCard) {
          weaponCard.classList.toggle("single-weapon", _r38CatalogWeaponsUnlocked <= 1);
        }

        // U1: Annotate Character & Weapon cards in Bot Setup and Modal with SỞ HỮU / DÙNG THỬ • TẠM badges
        root.querySelectorAll('[data-action="select-character"], .preview-character').forEach(btn => {
          const cid = btn.dataset.id || "";
          const isTamPose = cid === "quay_boy" || cid === "cho_vang" || cid === "meo_mun";
          if (isTamPose && !btn.querySelector(".r38-badge-tam")) {
            const smallEl = btn.querySelector("small");
            if (smallEl) {
              smallEl.innerHTML = (smallEl.textContent.includes("ĐANG CHỌN") ? "ĐANG CHỌN • " : "CHỌN • ") + '<span class="r38-badge-tam">TẠM</span>';
            } else {
              const b = document.createElement("small");
              b.className = "r38-badge-tam";
              b.textContent = "MỞ KHÓA • TẠM";
              btn.appendChild(b);
            }
          }
        });

        root.querySelectorAll('[data-action="select-weapon"], .preview-weapon').forEach(btn => {
          const wid = btn.dataset.id || "";
          if (!btn.dataset.r38WeaponBadge) {
            btn.dataset.r38WeaponBadge = "1";
            const badge = document.createElement("small");
            if (wid === "dep_to_ong") {
              badge.className = "r38-badge-owned";
              badge.textContent = "SỞ HỮU";
            } else if (wid === "chao_chong_dinh") {
              badge.className = "r38-badge-tam";
              badge.textContent = "CHƯA SỞ HỮU (199 VÀNG • TẠM)";
            } else if (wid === "vot_pickleball") {
              badge.className = "r38-badge-tam";
              badge.textContent = "CHƯA SỞ HỮU (260 VÀNG • TẠM)";
            }
            if (badge.textContent) btn.appendChild(badge);
          }
        });

        // G1: Set data-count on .live-cards and wire hold-to-reveal for N > 4
        const liveCards = root.querySelector(".live-cards");
        if (liveCards) {
          const cards = liveCards.querySelectorAll(".live-card");
          liveCards.dataset.count = String(cards.length);
          cards.forEach(card => {
            const nameEl = card.querySelector("b");
            if (nameEl && !card.dataset.r38Bound) {
              card.dataset.r38Bound = "1";
              card.title = nameEl.textContent || "";
              card.addEventListener("pointerdown", () => card.classList.add("hold-reveal"));
              card.addEventListener("pointerup", () => card.classList.remove("hold-reveal"));
              card.addEventListener("pointercancel", () => card.classList.remove("hold-reveal"));
              card.addEventListener("pointerleave", () => card.classList.remove("hold-reveal"));
            }
          });
        }

        // G2: Enhance .live-summary with 5 Round Dots + Circular Countdown Ring
        const summary = root.querySelector(".live-summary");
        if (summary) {
          const roundB = summary.querySelector(":scope > b");
          const strongTimer = summary.querySelector(":scope > strong");
          const isEn = document.documentElement.lang === "en";
          const st = window.__hitmeLastMatchEvt || {};
          const roundNum = Math.max(1, Number(st.round) || 1);
          const phase = String(st.phase || "placing").toLowerCase();
          const secLeft = Math.max(0, Number(st.timeLeft) || 0);

          if (roundB && !summary.querySelector(".r38-round-cluster")) {
            const cluster = document.createElement("div");
            cluster.className = "r38-round-cluster";
            roundB.replaceWith(cluster);
            cluster.appendChild(roundB);
            const dots = document.createElement("div");
            dots.className = "r38-round-dots";
            dots.setAttribute("aria-label", isEn ? "5 rounds progress" : "Tiến độ 5 vòng");
            for (let i = 1; i <= 5; i++) {
              const d = document.createElement("i");
              d.className = "r38-round-dot" + (i < roundNum ? " past" : i === roundNum ? " current" : "");
              dots.appendChild(d);
            }
            cluster.appendChild(dots);
          } else {
            const dots = summary.querySelectorAll(".r38-round-dot");
            dots.forEach((d, idx) => {
              const i = idx + 1;
              d.className = "r38-round-dot" + (i < roundNum ? " past" : i === roundNum ? " current" : "");
            });
          }

          if (strongTimer) {
            let ring = summary.querySelector(".r38-timer-ring");
            if (!ring) {
              ring = document.createElement("div");
              ring.className = "r38-timer-ring";
              ring.dataset.name = "timer-ring";
              const span = document.createElement("span");
              ring.appendChild(span);
              strongTimer.replaceWith(ring);
            }
            const pct = phase === "placing" ? Math.max(0, Math.min(100, (secLeft / 5.0) * 100)) : 0;
            const urgent = phase === "placing" && secLeft > 0 && secLeft <= 2.0;
            ring.style.setProperty("--timer-pct", pct.toFixed(1) + "%");
            ring.classList.toggle("urgent", urgent);
            const timerStyle = new URLSearchParams(location.search).get("timerStyle") || "ring";
            ring.classList.toggle("coin-style", timerStyle === "coin");
            const span = ring.querySelector("span");
            if (span) {
              span.textContent = phase === "placing" ? Math.ceil(secLeft) + "s" : "—";
            }
          }

          // G4: Trigger phase transition banner when phase or round changes
          if (phase && (phase !== _r38LastPhase || roundNum !== _r38LastRound)) {
            _r38LastPhase = phase;
            _r38LastRound = roundNum;
            const bannerMapVi = {
              placing: "ĐẶT NHÂN VẬT",
              lock: "KHÓA!",
              reveal: "LỘ DIỆN!",
              throw: "NÉM!",
              throwing: "NÉM!",
              resolve: "KẾT QUẢ VÒNG",
              roundend: "KẾT QUẢ VÒNG"
            };
            const bannerMapEn = {
              placing: "PLACE CHARACTER",
              lock: "LOCKED!",
              reveal: "REVEAL!",
              throw: "THROW!",
              throwing: "THROW!",
              resolve: "ROUND RESULT",
              roundend: "ROUND RESULT"
            };
            const label = (isEn ? bannerMapEn : bannerMapVi)[phase];
            if (label) showPhaseBanner(label);
          }
        }

        // G3: Interactive Tutorial Hand Guide + step 3 copy update
        const tut = root.querySelector(".live-tutorial");
        let hand = document.getElementById("r38-hand-guide");
        if (tut) {
          const bText = tut.querySelector("b")?.textContent || "";
          const pEl = tut.querySelector("p");
          const isStep1 = bText.includes("1/3");
          const isStep2 = bText.includes("2/3");
          const isStep3 = bText.includes("3/3");
          if (isStep3 && pEl) {
            const isEn = document.documentElement.lang === "en";
            pEl.textContent = isEn
              ? "Tap another spot on the court to move your character before the 5s timer ends!"
              : "Chạm chỗ khác trên sân để đặt lại vị trí trước khi hết 5 giây!";
          }
          if (!hand) {
            hand = document.createElement("div");
            hand.id = "r38-hand-guide";
            hand.className = "r38-hand-guide";
            hand.innerHTML = `<svg viewBox="0 0 24 24" width="42" height="42" fill="#ffdf4a" stroke="#2b1206" stroke-width="1.8"><path d="M9 11V5a2 2 0 1 1 4 0v4h1a2 2 0 0 1 2 2v1h1a2 2 0 0 1 2 2v4a5 5 0 0 1-5 5H9a5 5 0 0 1-4.5-2.8L2.6 15a1.8 1.8 0 0 1 2.5-2.3L9 14.5V11z"/></svg>`;
            document.body.appendChild(hand);
          }
          hand.classList.toggle("drag-mode", isStep2);
          hand.style.display = "block";
        } else if (hand) {
          hand.style.display = "none";
        }

        // E1–E6: Sync pooled ambient leaves, low-HP vignette, HP-drop card shake + callout, and Victory confetti
        ensurePooledVfxContainers();
        const isHome = !!root.querySelector('[data-screen="home"]');
        const isMatch = !!root.querySelector('[data-screen="match"]');
        const isResult = !!root.querySelector('[data-screen="result"]');
        const leavesEl = document.getElementById("r38-ambient-leaves");
        if (leavesEl) leavesEl.style.display = isHome ? "block" : "none";

        const heroImg = root.querySelector(".home .hero");
        if (heroImg && !heroImg.dataset.r38TapBound) {
          heroImg.dataset.r38TapBound = "1";
          heroImg.addEventListener("pointerdown", () => {
            heroImg.classList.remove("tap-wave");
            void heroImg.offsetWidth;
            heroImg.classList.add("tap-wave");
            setTimeout(() => heroImg.classList.remove("tap-wave"), 300);
          });
        }

        // E4 & E6: Check player HP changes and local 1 HP vignette
        const stEvt = window.__hitmeLastMatchEvt;
        const lowHpVig = document.getElementById("r38-low-hp-vignette");
        if (isMatch && stEvt && Array.isArray(stEvt.players)) {
          let anyDamageThisTick = false;
          let localHp = 3;
          const cardEls = root.querySelectorAll(".live-cards .live-card");
          stEvt.players.forEach((p, idx) => {
            const pid = p.id || ("p" + idx);
            const hp = Number(p.hp ?? 3);
            if (pid === "you" || idx === 0) localHp = hp;
            if (_r38PrevHpByPlayer[pid] !== undefined && hp < _r38PrevHpByPlayer[pid]) {
              anyDamageThisTick = true;
              const cardEl = cardEls[idx];
              if (cardEl) {
                cardEl.classList.remove("r38-card-hit-shake");
                void cardEl.offsetWidth;
                cardEl.classList.add("r38-card-hit-shake");
              }
            }
            _r38PrevHpByPlayer[pid] = hp;
          });
          if (anyDamageThisTick) {
            triggerScreenShake();
            showHitCallout("BỐP! -1 ❤️", false);
            tryHaptic(25);
          }
          if (lowHpVig) lowHpVig.classList.toggle("active", localHp === 1);
        } else {
          if (!isMatch) _r38PrevHpByPlayer = {};
          if (lowHpVig) lowHpVig.classList.remove("active");
        }

        // E5: Confetti on Victory Result screen
        const cfPool = document.getElementById("r38-confetti-pool");
        if (cfPool) {
          const resultTitle = (root.querySelector('[data-screen="result"] h1, [data-screen="result"] h2')?.textContent || "").toUpperCase();
          const isWinResult = isResult && (resultTitle.includes("THẮNG") || resultTitle.includes("VICTORY") || resultTitle.includes("VÔ ĐỊCH"));
          cfPool.classList.toggle("active", isWinResult);
        }

        // E6: Inject Haptics toggle into Settings sheet & sync Reduced Motion class
        const settingsSheet = root.querySelector(".settings-sheet");
        if (settingsSheet) {
          const rmBtn = settingsSheet.querySelector('[data-action="reduced-motion"]');
          if (rmBtn) {
            const rmOn = (rmBtn.textContent || "").includes("Bật") || (rmBtn.textContent || "").includes("On");
            document.body.classList.toggle("r38-reduced-motion", rmOn);
          }
          if (!settingsSheet.querySelector('[data-action="r38-toggle-haptics"]')) {
            const hapBtn = document.createElement("button");
            hapBtn.type = "button";
            hapBtn.className = "setting-toggle";
            hapBtn.dataset.action = "r38-toggle-haptics";
            hapBtn.innerHTML = `<span>Rung xúc giác (Haptics)</span><b>${_r38HapticsEnabled ? "Bật" : "Tắt"}</b>`;
            hapBtn.addEventListener("click", (ev) => {
              ev.stopPropagation();
              _r38HapticsEnabled = !_r38HapticsEnabled;
              try { localStorage.setItem("hitme_haptics", _r38HapticsEnabled ? "1" : "0"); } catch (e) {}
              const b = hapBtn.querySelector("b");
              if (b) b.textContent = _r38HapticsEnabled ? "Bật" : "Tắt";
              if (_r38HapticsEnabled) tryHaptic(25);
            });
            const toggleGroup = settingsSheet.querySelector(".setting-toggles") || settingsSheet;
            toggleGroup.appendChild(hapBtn);
          }
        }
      }

      document.addEventListener("click", (e) => {
        const btn = e.target && e.target.closest ? e.target.closest("[data-action]") : null;
        if (!btn) return;
        const act = btn.dataset.action || "";
        if (act === "previous" || act === "next" || act === "prev-character" || act === "next-character" || act === "select-character") {
          setTimeout(() => {
            const hero = document.querySelector("#ui-root .home .hero");
            if (hero) {
              hero.classList.remove("switch-bounce");
              void hero.offsetWidth;
              hero.classList.add("switch-bounce");
              setTimeout(() => hero.classList.remove("switch-bounce"), 230);
            }
            const glow = document.getElementById("r38-pedestal-glow");
            if (glow) {
              glow.classList.remove("flash");
              void glow.offsetWidth;
              glow.classList.add("flash");
            }
          }, 15);
        } else if (act === "ready") {
          tryHaptic(25);
          showPhaseBanner("ĐÃ KHÓA!");
        }
      }, true);

      document.addEventListener("DOMContentLoaded", () => {
        const root = document.getElementById("ui-root");
        if (root && typeof MutationObserver !== "undefined") {
          const m = new MutationObserver(enhanceRound38Dom);
          m.observe(root, { childList: true, subtree: true });
        }
        const uCanvas = document.getElementById("unity-canvas");
        if (uCanvas) {
          uCanvas.addEventListener("pointerdown", (ev) => {
            const isMatch = !!document.querySelector('#ui-root [data-screen="match"]');
            const phase = String(window.__hitmeLastMatchEvt?.phase || "placing").toLowerCase();
            if (isMatch && phase === "placing") {
              ensurePooledVfxContainers();
              const fx = document.getElementById("r38-place-fx");
              if (fx) {
                fx.style.left = ev.clientX + "px";
                fx.style.top = ev.clientY + "px";
                fx.classList.remove("active");
                void fx.offsetWidth;
                fx.classList.add("active");
              }
              tryHaptic(18);
            }
          });
          uCanvas.addEventListener("pointerup", () => {
            const tut = document.querySelector("#ui-root .live-tutorial");
            if (tut && (tut.querySelector("b")?.textContent || "").includes("3/3")) {
              const nextBtn = tut.querySelector('[data-action="tutorial-next"]');
              if (nextBtn) setTimeout(() => nextBtn.click(), 180);
            }
          });
        }
      });
    })();
  </script>
"""


def git_rev_short(ref: str, cwd: Path = REPO_ROOT) -> str:
    res = subprocess.run(["git", "rev-parse", "--short=7", ref], cwd=cwd, capture_output=True, text=True, check=True)
    return res.stdout.strip()


def sha256_file(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def build_channel_file_manifest(channel_dir: Path, root_dir: Path) -> list[dict]:
    """
    Builds a deterministic list of required files for a channel (`index.html`, `version.json`,
    `app.js`, `style.css`, `catalog.json`, key assets, and referenced `Build/*` files).
    Paths are recorded relative to `channel_dir` (e.g. `index.html`, `../Build/WebGL_Release_...`).
    """
    entries: list[dict] = []
    seen_paths: set[str] = set()

    def add_entry(rel_path: str, abs_path: Path) -> None:
        if rel_path in seen_paths or not abs_path.exists() or not abs_path.is_file():
            return
        seen_paths.add(rel_path)
        entries.append({
            "path": rel_path,
            "size": abs_path.stat().st_size,
            "sha256": sha256_file(abs_path),
            "contentEncoding": "br" if abs_path.name.endswith(".br") else "identity",
        })

    for rel in (
        "index.html",
        "version.json",
        "config.json",
        "app.js",
        "style.css",
        "catalog.json",
        "assets/bg_home_750.webp",
        "assets/bg_home_1170.webp",
        "assets/bg_home_1536.webp",
        "assets/bg_home_lqip.webp",
        "assets/bg_home_meta.json",
        "assets/nam_lang_portrait.png",
        "assets/nu_nang_dong_portrait.png",
        "assets/co_ba_portrait.png",
        "assets/anh_teen_portrait.png",
        "assets/dep_to_ong_icon.png",
    ):
        add_entry(rel, channel_dir / rel)

    # Parse index.html to discover referenced WebGL Build files
    index_file = channel_dir / "index.html"
    if index_file.exists():
        html_text = index_file.read_text(encoding="utf-8", errors="ignore")
        build_url_match = re.search(r'const\s+buildUrl\s*=\s*"([^"]+)"', html_text)
        build_prefix = build_url_match.group(1).rstrip("/") if build_url_match else "./Build"
        for m in re.finditer(r'buildUrl\s*\+\s*"/([^"?]+)"', html_text):
            fname = m.group(1)
            rel_ref = f"{build_prefix}/{fname}"
            abs_build_file = (channel_dir / rel_ref).resolve()
            if not abs_build_file.exists() and (root_dir / "Build" / fname).exists():
                abs_build_file = (root_dir / "Build" / fname).resolve()
            add_entry(rel_ref, abs_build_file)

    return entries


def find_latest_release_build() -> Path:
    candidates = []
    if BUILDS_DIR.exists():
        for d in BUILDS_DIR.iterdir():
            if d.is_dir() and d.name.startswith("WebGL_Release_"):
                candidates.append(d)
    if not candidates:
        raise FileNotFoundError(f"No WebGL_Release_* builds found in {BUILDS_DIR}")
    candidates.sort(key=lambda p: p.stat().st_mtime, reverse=True)
    return candidates[0]


def extract_from_codex(ref: str, paths: list[str], dest_root: Path) -> None:
    if dest_root.exists():
        shutil.rmtree(dest_root)
    dest_root.mkdir(parents=True, exist_ok=True)
    cmd = ["git", "archive", "--format=tar", ref] + paths
    res = subprocess.run(cmd, cwd=REPO_ROOT, capture_output=True, check=True)
    with tarfile.open(fileobj=io.BytesIO(res.stdout)) as tf:
        tf.extractall(path=dest_root)


def sync_root_static_configs(dest_dir: Path) -> None:
    for cfg_name in ("vercel.json", "robots.txt"):
        src = DEPLOY_WEB_DIR / cfg_name
        dst = dest_dir / cfg_name
        if src.exists():
            if not dst.exists() or sha256_file(src) != sha256_file(dst):
                shutil.copy2(src, dst)


def preserve_stable_root() -> None:
    """Ensures / (root files, previous/, and Build/) matches the deployed stable root."""
    DIST_DIR.mkdir(parents=True, exist_ok=True)
    if STAGING_DEPLOY_DIR.exists() and (STAGING_DEPLOY_DIR / "index.html").exists():
        for item in STAGING_DEPLOY_DIR.iterdir():
            if item.name in (".git", "next", "rc", "ui-preview"):
                continue
            dst = DIST_DIR / item.name
            if item.is_dir():
                shutil.copytree(item, dst, dirs_exist_ok=True)
            else:
                shutil.copy2(item, dst)

    # Refresh files manifest in root release.json and previous/release.json if present
    for ch_dir in (DIST_DIR, DIST_DIR / "previous"):
        rel_path = ch_dir / "release.json"
        if rel_path.exists():
            try:
                rdata = json.loads(rel_path.read_text(encoding="utf-8"))
                rdata["files"] = build_channel_file_manifest(ch_dir, DIST_DIR)
                rel_path.write_text(json.dumps(rdata, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
            except Exception:
                pass


def sync_visual_assets(assets_dir: Path) -> None:
    """Copies cleaned catalog character/weapon PNGs and bg_home_* WebP/JSON files into assets_dir."""
    assets_dir.mkdir(parents=True, exist_ok=True)
    for bg_dir in (HOME_BG_WEB_DIR, HOME_BG_DIR):
        if bg_dir.exists():
            for f in bg_dir.iterdir():
                if f.is_file() and not f.name.endswith(".meta") and (f.name.startswith("bg_home_") or f.name == "bg_home_meta.json"):
                    shutil.copy2(f, assets_dir / f.name)
    if CATALOG_SRC_DIR.exists():
        for f in CATALOG_SRC_DIR.iterdir():
            if f.is_file() and f.suffix.lower() == ".png":
                shutil.copy2(f, assets_dir / f.name)
        cat_json = CATALOG_SRC_DIR / "catalog.json"
        if cat_json.exists() and (assets_dir.parent / "catalog.json").exists():
            shutil.copy2(cat_json, assets_dir.parent / "catalog.json")


def write_release_metadata(channel_dir: Path, version: str, build_id: str, unity_commit: str, webui_commit: str,
                           catalog_version: int, gate_passed: bool, failed_ids: list[str], notes: str) -> None:
    now_iso = datetime.now(timezone.utc).isoformat()
    version_data = {
        "version": version,
        "build": build_id,
        "unityCommit": unity_commit,
        "webUiCommit": webui_commit,
        "timestamp": now_iso
    }
    (channel_dir / "version.json").write_text(json.dumps(version_data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")

    files_manifest = build_channel_file_manifest(channel_dir, DIST_DIR)
    release_data = {
        "version": version,
        "buildId": build_id,
        "createdAt": now_iso,
        "unityCommit": unity_commit,
        "webUiCommit": webui_commit,
        "catalogVersion": catalog_version,
        "gate": {
            "passed": gate_passed,
            "failedIds": failed_ids
        },
        "files": files_manifest,
        "notes": notes
    }
    (channel_dir / "release.json").write_text(json.dumps(release_data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def generate_hybrid_index_html(channel_dir: Path, build_rel_url: str, build_files: dict[str, str],
                               version: str, build_id: str, is_rc: bool, failed_ids: list[str]) -> None:
    rc_banner = ""
    if is_rc:
        failed_str = ", ".join(failed_ids) if failed_ids else "chưa qua cổng"
        rc_banner = f"""
  <div id="rc-warning-badge" data-name="rc-badge" style="position:fixed;top:0;left:0;right:0;z-index:99999;background:#b91c1c;color:#fff8e8;font-size:12px;font-weight:800;text-align:center;padding:4px 8px;pointer-events:none;">
    RC - chưa qua kiểm thử ({failed_str})
  </div>"""

    html = f"""<!DOCTYPE html>
<html lang="vi">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, height=device-height, initial-scale=1.0, maximum-scale=1.0, user-scalable=no, viewport-fit=cover">
  <meta name="theme-color" content="#101b28">
  <title>HIT ME — Web UI + Unity Hybrid ({version})</title>
  <link rel="stylesheet" href="./style.css">{HOME_VISUAL_PATCH_HTML}
  <style>
    * {{ box-sizing: border-box; }}
    html, body {{
      width: 100%;
      height: 100%;
      height: 100dvh;
      margin: 0;
      padding: 0;
      overflow: hidden;
      background: #101b28;
      display: flex;
      justify-content: center;
      align-items: center;
      touch-action: none;
      user-select: none;
      -webkit-user-select: none;
      -webkit-touch-callout: none;
      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
    }}
    #unity-container {{
      position: fixed;
      inset: 0;
      width: 100vw;
      height: 100vh;
      height: 100dvh;
      margin: 0;
      padding: 0;
      overflow: hidden;
      display: flex;
      justify-content: center;
      align-items: center;
      z-index: 1;
    }}
    #unity-canvas {{
      width: 100% !important;
      height: 100% !important;
      outline: none;
      display: block;
      touch-action: none;
    }}
    #ui-root {{
      position: fixed;
      inset: 0;
      width: 100%;
      height: 100%;
      z-index: 20;
      pointer-events: none;
    }}
    #ui-root > * {{
      pointer-events: auto;
    }}
    #ui-root [data-screen="match"] {{
      pointer-events: none;
    }}
    #ui-root [data-screen="match"] .live-top,
    #ui-root [data-screen="match"] .live-bottom,
    #ui-root [data-screen="match"] .live-tutorial,
    #ui-root [data-screen="match"] .modal-shade,
    #ui-root [data-screen="match"] .modal,
    #ui-root [data-screen="match"] button {{
      pointer-events: auto;
    }}
    #unity-loading {{
      pointer-events: none;
      position: fixed;
      inset: 0;
      display: flex;
      flex-direction: column;
      justify-content: center;
      align-items: center;
      background: radial-gradient(circle at center, #141b2d 0%, #060709 100%);
      color: #ffe882;
      z-index: 100;
      padding: 24px;
      text-align: center;
    }}
    .loading-brand {{
      font-size: 32px;
      font-weight: 900;
      letter-spacing: 2px;
      color: #f5c518;
      margin-bottom: 6px;
      text-shadow: 0 2px 10px rgba(245,197,24,0.3);
    }}
    .loading-tagline {{
      font-size: 14px;
      color: #b0c4de;
      margin-bottom: 28px;
      letter-spacing: 1px;
    }}
    .progress-container {{
      width: 240px;
      max-width: 80vw;
      height: 10px;
      background: #1a2234;
      border-radius: 6px;
      overflow: hidden;
      border: 1px solid rgba(255,255,255,0.1);
      position: relative;
    }}
    .progress-fill {{
      width: 0%;
      height: 100%;
      background: linear-gradient(90deg, #ff9800, #ffeb3b);
      transition: width 0.15s ease-out;
    }}
    .loading-status {{
      margin-top: 14px;
      font-size: 13px;
      color: #90caf9;
      letter-spacing: 0.5px;
    }}
  </style>
</head>
<body>{rc_banner}
  <div id="unity-container">
    <canvas id="unity-canvas" aria-label="Sân chơi HIT ME"></canvas>
  </div>
  <div id="ui-root"></div>

  <div id="unity-loading">
    <div class="loading-brand">HIT ME</div>
    <div class="loading-tagline">Đang nạp sân chơi &amp; Web UI...</div>
    <div class="progress-container">
      <div class="progress-fill" id="progress-fill"></div>
    </div>
    <div class="loading-status" id="loading-status">Đang tải dữ liệu trò chơi… 0%</div>
  </div>

  <script>
    window.__hitmeConfig = {{ ui: "web" }};
    window.__hitmeReleaseInfo = {{ version: "{version}", buildId: "{build_id}" }};
    window.__hitmeBootT0 = performance.now();

    function checkAndEmitHomeShown() {{
      if (window.__hitmeHomeShown) return;
      const homeEl = document.querySelector('#ui-root [data-screen="home"]');
      const heroEl = document.querySelector('#ui-root .home .hero');
      if (homeEl && heroEl && window.unityInstance) {{
        window.__hitmeHomeShown = Math.round(performance.now() - (window.__hitmeBootT0 || 0));
        console.log("HOME_SHOWN " + window.__hitmeHomeShown + "ms");
      }}
    }}

    // Deduplicate 10 Hz Unity match ticks and guard active UI pointer interactions so app.js innerHTML never detaches buttons mid-click
    (function() {{
      let _realHitmeUi = null;
      let _lastMatchSig = "";
      let _tutorialHoldUntil = 0;
      let _interactHoldUntil = 0;
      let _deferredMatchEvt = null;
      let _flushTimer = null;

      function bumpInteractHold(ms) {{
        const until = Date.now() + ms;
        if (until > _interactHoldUntil) _interactHoldUntil = until;
      }}

      ["pointerdown", "pointerover", "mousedown", "touchstart"].forEach(function(evName) {{
        document.addEventListener(evName, function(e) {{
          const t = e.target;
          if (t && t.closest && t.closest("#ui-root button, #ui-root input, #ui-root .modal")) {{
            bumpInteractHold(550);
          }}
        }}, true);
      }});

      document.addEventListener("click", function(e) {{
        const btn = e.target && e.target.closest ? e.target.closest("[data-action]") : null;
        const act = btn ? btn.dataset.action : "";
        if (act) bumpInteractHold(450);
        if (act === "tutorial-reopen") _tutorialHoldUntil = Date.now() + 8000;
        else if (act === "tutorial-skip") _tutorialHoldUntil = 0;
      }}, true);

      Object.defineProperty(window, "hitmeUi", {{
        configurable: true,
        enumerable: true,
        get() {{ return _realHitmeUi; }},
        set(val) {{
          if (val && typeof val.onUnity === "function") {{
            const origOnUnity = val.onUnity.bind(val);
            function flushDeferred() {{
              _flushTimer = null;
              if (!_deferredMatchEvt) return;
              if (Date.now() < _interactHoldUntil) {{
                _flushTimer = setTimeout(flushDeferred, 120);
                return;
              }}
              const pending = _deferredMatchEvt;
              _deferredMatchEvt = null;
              origOnUnity(pending);
            }}
            val.onUnity = function(event) {{
              let evt = event;
              try {{ if (typeof event === "string") evt = JSON.parse(event); }} catch (e) {{}}
              if (window.__hitmeFreezeLiveMatch && evt && !evt.__fromTest) return;
              if (evt && typeof evt === "object") {{
                if (evt.type === "screen" || evt.type === "result") {{
                  _lastMatchSig = "";
                  _tutorialHoldUntil = 0;
                  _interactHoldUntil = 0;
                  _deferredMatchEvt = null;
                  if (_flushTimer) {{ clearTimeout(_flushTimer); _flushTimer = null; }}
                }} else if (evt.type === "error" && (evt.code === "character_locked" || evt.code === "weapon_locked")) {{
                  // Round 38 U1: All 7 characters and 3 cosmetic weapons are unlocked for offline Bot Practice
                  return;
                }} else if (evt.type === "match") {{
                  window.__hitmeLastMatchEvt = evt;
                  if (Date.now() < _tutorialHoldUntil && evt.phase !== "placing") {{
                    evt = Object.assign({{}}, evt, {{ phase: "placing", timeLeft: Math.max(2, Number(evt.timeLeft) || 2) }});
                    event = evt;
                  }}
                  const playersSig = Array.isArray(evt.players)
                    ? evt.players.map(p => `${{p.id}}:${{p.characterId}}:${{p.hp}}:${{p.alive}}`).join(",")
                    : "";
                  const sig = `${{evt.round}}|${{evt.phase}}|${{Math.ceil(Number(evt.timeLeft) || 0)}}|${{evt.toast || ""}}|${{playersSig}}`;
                  if (sig === _lastMatchSig) return;
                  if (_lastMatchSig !== "" && Date.now() < _interactHoldUntil) {{
                    _deferredMatchEvt = event;
                    if (!_flushTimer) _flushTimer = setTimeout(flushDeferred, 120);
                    return;
                  }}
                  _lastMatchSig = sig;
                }} else if (evt.type === "event" && Date.now() < _interactHoldUntil) {{
                  return;
                }}
              }}
              return origOnUnity(event);
            }};
          }}
          _realHitmeUi = val;
        }}
      }});
    }})();

    // Inject version + short buildId into Web Settings sheet (R2) without altering fingerprinted app.js
    const uiRootEl = document.getElementById("ui-root");
    if (uiRootEl && typeof MutationObserver !== "undefined") {{
      const obs = new MutationObserver(() => {{
        checkAndEmitHomeShown();
        const sheet = uiRootEl.querySelector(".settings-sheet");
        if (sheet && !sheet.querySelector(".release-version-badge")) {{
          const badge = document.createElement("small");
          badge.className = "release-version-badge";
          badge.dataset.name = "release-version";
          badge.style.cssText = "display:block;margin-top:10px;opacity:0.8;font-size:12px;text-align:center;color:#cbd5e1;";
          badge.textContent = "Phiên bản: {version} ({build_id})";
          sheet.appendChild(badge);
        }}
      }});
      obs.observe(uiRootEl, {{ childList: true, subtree: true }});
    }}

    const canvas = document.querySelector("#unity-canvas");
    const loading = document.querySelector("#unity-loading");
    const progressFill = document.querySelector("#progress-fill");
    const loadingStatus = document.querySelector("#loading-status");

    function updateCanvasResolution() {{
      const vv = window.visualViewport;
      const cssW = vv ? Math.round(vv.width) : window.innerWidth;
      const cssH = vv ? Math.round(vv.height) : window.innerHeight;
      const dpr = Math.min(window.devicePixelRatio || 1, 2.5);

      canvas.style.width = cssW + "px";
      canvas.style.height = cssH + "px";
      canvas.width = Math.round(cssW * dpr);
      canvas.height = Math.round(cssH * dpr);

      const container = document.querySelector("#unity-container");
      if (container && vv) {{
        container.style.width = cssW + "px";
        container.style.height = cssH + "px";
        container.style.top = (vv.offsetTop || 0) + "px";
        container.style.left = (vv.offsetLeft || 0) + "px";
      }}
    }}

    if (window.visualViewport) {{
      window.visualViewport.addEventListener("resize", updateCanvasResolution);
      window.visualViewport.addEventListener("scroll", updateCanvasResolution);
    }}
    window.addEventListener("resize", updateCanvasResolution);
    window.addEventListener("orientationchange", updateCanvasResolution);
    updateCanvasResolution();

    const buildUrl = "{build_rel_url}";
    const urlParams = new URLSearchParams(window.location.search);
    if (urlParams.get("e2e") === "1" && typeof WebAssembly !== "undefined" && WebAssembly.Memory) {{
      const OrigMem = WebAssembly.Memory;
      WebAssembly.Memory = function(desc) {{
        const mem = new OrigMem(desc);
        window.__hitmeWasmMemory = mem;
        return mem;
      }};
      WebAssembly.Memory.prototype = OrigMem.prototype;
    }}
    const buildVersion = urlParams.get("v") || "{build_id}";
    const versioned = path => path + "?v=" + encodeURIComponent(buildVersion);

    const config = {{
      arguments: [],
      dataUrl: versioned(buildUrl + "/{build_files['data']}"),
      frameworkUrl: versioned(buildUrl + "/{build_files['framework']}"),
      codeUrl: versioned(buildUrl + "/{build_files['wasm']}"),
      streamingAssetsUrl: "StreamingAssets",
      companyName: "HitMeStudio",
      productName: "HIT ME",
      productVersion: "{version}",
      matchWebGLToCanvasSize: false,
      devicePixelRatio: Math.min(window.devicePixelRatio || 1, 2.5)
    }};

    const script = document.createElement("script");
    script.src = versioned(buildUrl + "/{build_files['loader']}");
    script.onload = () => {{
      createUnityInstance(canvas, config, (progress) => {{
        const pct = Math.round(progress * 100);
        if (progressFill) progressFill.style.width = pct + "%";
        if (loadingStatus) loadingStatus.textContent = `Đang tải dữ liệu trò chơi… ${{pct}}%`;
      }}).then(instance => {{
        window.unityInstance = instance;
        if (loading) loading.style.display = "none";
        canvas.focus();
        checkAndEmitHomeShown();
        setTimeout(checkAndEmitHomeShown, 60);
        if (urlParams.get("autostart") === "practice") {{
          const autoChar = urlParams.get("char") || "nam_lang";
          const autoWeapon = urlParams.get("weapon") || "dep_to_ong";
          const autoBots = parseInt(urlParams.get("bots") || "3", 10) || 3;
          setTimeout(() => {{
            instance.SendMessage("WebBridge", "StartPractice", JSON.stringify({{
              characterId: autoChar,
              weaponId: autoWeapon === "dep" ? "dep_to_ong" : autoWeapon,
              botCount: autoBots
            }}));
          }}, 120);
        }}
      }}).catch(error => {{
        console.error("[Hybrid] Unity WebGL load error:", error);
        if (loadingStatus) loadingStatus.textContent = "Lỗi nạp trò chơi: " + error;
      }});
    }};
    document.body.appendChild(script);
  </script>
  <script type="module" src="./app.js"></script>
</body>
</html>
"""
    (channel_dir / "index.html").write_text(html, encoding="utf-8")


def promote_next_to_root() -> None:
    """
    Safely promotes `/next/` to root `/`, backing up the current root `/` into `/previous/`,
    while preserving all shared `Build/` and `ui-preview/` files using `dirs_exist_ok=True`.
    """
    next_dir = DIST_DIR / "next"
    if (STAGING_DEPLOY_DIR / "next" / "release.json").exists():
        if next_dir.exists():
            shutil.rmtree(next_dir)
        shutil.copytree(STAGING_DEPLOY_DIR / "next", next_dir)
    elif not next_dir.exists() or not (next_dir / "release.json").exists():
        raise FileNotFoundError("Cannot promote: dist/next/release.json does not exist")

    # 1. Always merge shared Build/ and ui-preview/ from STAGING_DEPLOY_DIR with dirs_exist_ok=True
    for shared_folder in ("Build", "ui-preview"):
        if (STAGING_DEPLOY_DIR / shared_folder).exists():
            shutil.copytree(
                STAGING_DEPLOY_DIR / shared_folder,
                DIST_DIR / shared_folder,
                dirs_exist_ok=True,
            )

    # 2. Backup current root / to /previous/ (including hybrid assets/app.js/style.css if present)
    prev_dir = DIST_DIR / "previous"
    if prev_dir.exists():
        shutil.rmtree(prev_dir)
    prev_dir.mkdir(parents=True, exist_ok=True)
    root_src_base = STAGING_DEPLOY_DIR if (STAGING_DEPLOY_DIR / "index.html").exists() else DIST_DIR
    for name in ("index.html", "version.json", "release.json", "config.json", "app.js", "style.css", "catalog.json"):
        src_root_file = root_src_base / name
        if src_root_file.exists() and src_root_file.is_file():
            shutil.copy2(src_root_file, prev_dir / name)
    if (root_src_base / "assets").exists():
        shutil.copytree(root_src_base / "assets", prev_dir / "assets", dirs_exist_ok=True)

    if (prev_dir / "index.html").exists():
        phtml = (prev_dir / "index.html").read_text(encoding="utf-8")
        phtml = phtml.replace('const buildUrl = "Build";', 'const buildUrl = "../Build";')
        phtml = phtml.replace('const buildUrl = "./Build";', 'const buildUrl = "../Build";')
        (prev_dir / "index.html").write_text(phtml, encoding="utf-8")
    if (prev_dir / "release.json").exists():
        pdata = json.loads((prev_dir / "release.json").read_text(encoding="utf-8"))
        pdata["files"] = build_channel_file_manifest(prev_dir, DIST_DIR)
        (prev_dir / "release.json").write_text(json.dumps(pdata, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(f"[+] Backed up current root / to {prev_dir}")

    # 3. Copy next/ contents to root /
    for item in next_dir.iterdir():
        if item.name == "Build":
            continue
        dst = DIST_DIR / item.name
        if item.is_dir():
            if dst.exists():
                shutil.rmtree(dst)
            shutil.copytree(item, dst)
        else:
            shutil.copy2(item, dst)
    if (DIST_DIR / "index.html").exists():
        rhtml = (DIST_DIR / "index.html").read_text(encoding="utf-8")
        rhtml = rhtml.replace('const buildUrl = "../Build";', 'const buildUrl = "./Build";')
        (DIST_DIR / "index.html").write_text(rhtml, encoding="utf-8")

    # 4. Refresh files manifest in root / and ui-preview/
    for ch_dir in (DIST_DIR, DIST_DIR / "ui-preview", DIST_DIR / "next"):
        rel_path = ch_dir / "release.json"
        if rel_path.exists():
            rdata = json.loads(rel_path.read_text(encoding="utf-8"))
            rdata["files"] = build_channel_file_manifest(ch_dir, DIST_DIR)
            rel_path.write_text(json.dumps(rdata, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")

    sync_root_static_configs(DIST_DIR)
    print("[+] Promoted /next/ to root /")


def rollback_previous_to_root() -> None:
    """Restores `/previous/` back to root `/` while keeping `Build/`, `/next/`, and `/ui-preview/` intact."""
    preserve_stable_root()
    for shared_folder in ("Build", "ui-preview", "next", "previous"):
        if (STAGING_DEPLOY_DIR / shared_folder).exists():
            shutil.copytree(STAGING_DEPLOY_DIR / shared_folder, DIST_DIR / shared_folder, dirs_exist_ok=True)

    prev_dir = DIST_DIR / "previous"
    if not prev_dir.exists() or not (prev_dir / "index.html").exists():
        raise FileNotFoundError("Cannot rollback: /previous/index.html does not exist")

    for item in prev_dir.iterdir():
        dst = DIST_DIR / item.name
        if item.is_dir():
            if dst.exists():
                shutil.rmtree(dst)
            shutil.copytree(item, dst)
        else:
            shutil.copy2(item, dst)

    if (DIST_DIR / "index.html").exists():
        rhtml = (DIST_DIR / "index.html").read_text(encoding="utf-8")
        rhtml = rhtml.replace('const buildUrl = "../Build";', 'const buildUrl = "./Build";')
        (DIST_DIR / "index.html").write_text(rhtml, encoding="utf-8")

    if (DIST_DIR / "release.json").exists():
        rdata = json.loads((DIST_DIR / "release.json").read_text(encoding="utf-8"))
        rdata["files"] = build_channel_file_manifest(DIST_DIR, DIST_DIR)
        (DIST_DIR / "release.json").write_text(json.dumps(rdata, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")

    sync_root_static_configs(DIST_DIR)
    print("[+] Rolled back root / from /previous/")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--channel", choices=["next", "rc"], default="next")
    parser.add_argument("--codex-ref", default="origin/prep/codex")
    parser.add_argument("--gate-passed", action="store_true", default=True)
    parser.add_argument("--gate-failed-ids", default="")
    parser.add_argument("--promote", action="store_true")
    parser.add_argument("--rollback", action="store_true")
    parser.add_argument("--notes", default="Hybrid Web UI (prep/codex) + Unity WebGL Release (feature/ui-ux-production)")
    args = parser.parse_args()

    if args.promote:
        promote_next_to_root()
        return 0
    if args.rollback:
        rollback_previous_to_root()
        return 0

    failed_ids = [x.strip() for x in args.gate_failed_ids.split(",") if x.strip()]
    gate_passed = (len(failed_ids) == 0) and (args.channel == "next")

    version_file = DEPLOY_WEB_DIR / "VERSION"
    version = version_file.read_text(encoding="utf-8").strip() if version_file.exists() else "0.1.0-beta.3"
    latest_build = find_latest_release_build()
    unity_commit = latest_build.name.rsplit("_", 1)[-1]
    subprocess.run(["git", "cat-file", "-e", f"{unity_commit}^{{commit}}"], cwd=REPO_ROOT, check=True)
    webui_commit = git_rev_short(args.codex_ref)
    build_id = f"{unity_commit}-{webui_commit}"

    print(f"[*] Packaging version={version}, buildId={build_id}, channel=/{args.channel}/")

    # 1. Preserve root / unchanged
    preserve_stable_root()

    # 2. Extract web-ui/dist and tools from prep/codex into temporary folder _codex_extract
    extract_from_codex(
        args.codex_ref,
        ["web-ui", "tools/blackbox", "tools/ui", "tools/webui_assets", "tools/supabase"],
        TEMP_EXTRACT_DIR
    )
    codex_webui_dist = TEMP_EXTRACT_DIR / "web-ui" / "dist"
    if not (codex_webui_dist / "app.js").exists():
        raise FileNotFoundError(f"Missing app.js in extracted {codex_webui_dist}")

    # Sync cleaned catalog avatars/portraits/icons into extracted web-ui/dist/assets
    sync_visual_assets(codex_webui_dist / "assets")
    sync_visual_assets(TEMP_EXTRACT_DIR / "web-ui" / "assets")

    catalog_data = json.loads((codex_webui_dist / "catalog.json").read_text(encoding="utf-8"))
    catalog_version = int(catalog_data.get("version", 1))

    # 3. Populate /ui-preview/
    ui_preview_dir = DIST_DIR / "ui-preview"
    if ui_preview_dir.exists():
        shutil.rmtree(ui_preview_dir)
    shutil.copytree(codex_webui_dist, ui_preview_dir)
    ui_prev_index = ui_preview_dir / "index.html"
    if ui_prev_index.exists():
        uhtml = ui_prev_index.read_text(encoding="utf-8")
        uhtml = uhtml.replace('<link rel="stylesheet" href="./style.css">', '<link rel="stylesheet" href="./style.css">' + HOME_VISUAL_PATCH_HTML)
        mock_boot = '<script>if(!new URLSearchParams(location.search).has("mock")){const u=new URL(location.href);u.searchParams.set("mock","1");history.replaceState(null,"",u);}</script>\n  <script type="module" src="./app.js"></script>'
        uhtml = uhtml.replace('<script type="module" src="./app.js"></script>', mock_boot)
        ui_prev_index.write_text(uhtml, encoding="utf-8")
    sync_visual_assets(ui_preview_dir / "assets")
    write_release_metadata(
        ui_preview_dir, version, build_id, unity_commit, webui_commit,
        catalog_version, gate_passed, failed_ids, args.notes
    )
    print(f"[+] Packaged /ui-preview/ ({ui_preview_dir})")

    # 4. Populate target channel (/next/ or /rc/)
    channel_dir = DIST_DIR / args.channel
    if channel_dir.exists():
        shutil.rmtree(channel_dir)
    shutil.copytree(codex_webui_dist, channel_dir)
    sync_visual_assets(channel_dir / "assets")

    # Copy latest Unity WebGL Release build into shared DIST_DIR/Build/ (preserving root / files alongside it)
    latest_build = find_latest_release_build()
    src_build = latest_build / "Build"
    shared_build = DIST_DIR / "Build"
    shared_build.mkdir(parents=True, exist_ok=True)
    shutil.copytree(src_build, shared_build, dirs_exist_ok=True)

    build_files = {}
    for f in src_build.iterdir():
        if f.name.endswith(".data.br"):
            build_files["data"] = f.name
        elif f.name.endswith(".framework.js.br"):
            build_files["framework"] = f.name
        elif f.name.endswith(".wasm.br"):
            build_files["wasm"] = f.name
        elif f.name.endswith(".loader.js"):
            build_files["loader"] = f.name

    if len(build_files) != 4:
        raise RuntimeError(f"Expected 4 WebGL build files in {src_build}, found: {build_files}")

    generate_hybrid_index_html(
        channel_dir, "../Build", build_files, version, build_id,
        is_rc=(args.channel == "rc"), failed_ids=failed_ids
    )
    write_release_metadata(
        channel_dir, version, build_id, unity_commit, webui_commit,
        catalog_version, gate_passed, failed_ids, args.notes
    )

    # 5. Copy vercel.json and robots.txt to DIST_DIR
    sync_root_static_configs(DIST_DIR)

    print(f"[SUCCESS] Packaged /{args.channel}/ and /ui-preview/ with buildId={build_id} (root / preserved unchanged)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
