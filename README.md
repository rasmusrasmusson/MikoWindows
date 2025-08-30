
# MikoMe (WinUI 3 starter)

A clean, offline-first spaced repetition app to study Chinese on Windows 11.

## Features
- Add items with **English / 简体中文 / Pinyin**
- Practice sessions in two directions:
  - **中→英** (Hanzi + optional Pinyin → recall English)
  - **英→中** (English → recall Hanzi, answer shows Hanzi + Pinyin)
- **Show → Know / Didn’t know** flow, using a simple SM‑2 pass/fail scheduler
- Keyboard shortcuts for speed
- Local **SQLite** database stored at `Documents\MikoMe\MikoMe.db` (syncs with OneDrive)

## Keyboard shortcuts
Global:
- **Ctrl+H** Home
- **Ctrl+N** Add word
- **Ctrl+L** Start session (中→英)
- **Ctrl+Shift+L** Start session (英→中)

In Session:
- **Space** Show / (after reveal) advance as **Know**
- **K** Know
- **J** Didn’t know
- **P** Toggle Pinyin
- **S** Speak Hanzi
- **C** Switch to 中→英
- **E** Switch to 英→中

## Getting started
1. Install Visual Studio 2022 with “.NET desktop development”
2. Open `src/MikoMe/MikoMe.csproj`
3. Restore NuGet packages and press **F5**

> First run seeds a few sample cards.
