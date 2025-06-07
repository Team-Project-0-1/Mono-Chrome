# UI System

This folder contains the runtime user interface scripts for MONOCHROME. The main entry point is `UIController`, which should reside here as per the guidelines in `CLAUDE.md`.

Debug and test utilities live under `Debug/` and are wrapped in `#if UNITY_EDITOR` so they are excluded from builds.
